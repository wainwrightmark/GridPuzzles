namespace Sudoku.Variants;

public partial class PalindromeVariantBuilder<T> : VariantBuilder<T> where T : notnull
{
    private PalindromeVariantBuilder()
    {
    }

    public static PalindromeVariantBuilder<T> Instance { get; } = new ();
    public override string Name => "Palindrome";

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments => new VariantBuilderArgument[]
    {
        PositionArgument
    };

    private static readonly ListPositionArgument PositionArgument = new("Positions",
        3, 81);
        

    public override Result<IReadOnlyCollection<IClueBuilder<T>>> TryGetClueBuilders1(IReadOnlyDictionary<string, string> arguments)
    {
        var pr = PositionArgument.TryGetFromDictionary(arguments);
        if (pr.IsFailure) return pr.ConvertFailure<IReadOnlyCollection<IClueBuilder<T>>>();

        var l = new List<IClueBuilder<T>>
        {
            new PalindromeClueBuilder(pr.Value.ToImmutableList())
        };

        return l;
    }

    [Equatable]
    private partial record PalindromeClueBuilder([property:OrderedEquality] ImmutableList<Position> Positions) : IClueBuilder<T>
    {

        /// <inheritdoc />
        public string Name => "Palindrome " + Positions.ToDelimitedString(", ");

        /// <inheritdoc />
        public int Level => 2;

        /// <inheritdoc />
        public IEnumerable<IClue<T>> CreateClues(Position minPosition, Position maxPosition, IValueSource<T> valueSource,
            IReadOnlyCollection<IClue<T>> lowerLevelClues)
        {
            foreach (var (first, second) in Positions.Zip(Positions.Reverse()).Take(Positions.Count / 2))
            {
                yield return new RelationshipClue<T>(first, second, AreEqualConstraint<T>.Instance);
            }
        }

        /// <param name="minPosition"></param>
        /// <param name="maxPosition"></param>
        /// <inheritdoc />
        public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
        {
            if (Positions.AreContiguous(minPosition, maxPosition, true))
                yield return new LineCellOverlay(Positions, Color.DarkSlateGray);
            else
            {
                foreach (var position in Positions)
                {
                    yield return new CellColorOverlay(Color.DarkSlateGray, position);
                }
            }
        }
    }
}
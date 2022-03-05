namespace Sudoku.Variants;

public partial class ConsecutiveGroupVariantBuilder : VariantBuilder
{
    private ConsecutiveGroupVariantBuilder()
    {
    }

    public static VariantBuilder Instance { get; } = new ConsecutiveGroupVariantBuilder();

    /// <inheritdoc />
    public override string Name => "Consecutive Group";


    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder>> TryGetClueBuilders1(
        IReadOnlyDictionary<string, string> arguments)
    {
        var pr = PositionArgument.TryGetFromDictionary(arguments);
        if (pr.IsFailure) return pr.ConvertFailure<IReadOnlyCollection<IClueBuilder>>();


        var l = new List<IClueBuilder>
        {
            new ConsecutiveGroupClueBuilder(pr.Value.ToImmutableArray())
        };

        return l;
    }
        

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments => new VariantBuilderArgument[]
    {
        PositionArgument
    };

    private static readonly ListPositionArgument PositionArgument = new("Positions",
        2,
        8);

    [Equatable]
    public partial record ConsecutiveGroupClueBuilder([property:OrderedEquality] ImmutableArray<Position> Positions) : IClueBuilder
    {

        /// <inheritdoc />
        public string Name => "Consecutive Group";

        /// <inheritdoc />
        public int Level => 1;

        /// <inheritdoc />
        public IEnumerable<IClue<int, IntCell>> CreateClues(Position minPosition, Position maxPosition,
            IValueSource valueSource,
            IReadOnlyCollection<IClue<int, IntCell>> lowerLevelClues)
        {
            var amount = Positions.Length - 1;
            var constraint = new DifferByAtMostConstraint(amount);

            yield return new UniquenessClue<int, IntCell>(Positions.ToImmutableSortedSet(), "Consecutive Group");

            foreach (var (p, q) in Positions.SelectMany(p => Positions.Select(q => (p, q))).Where(x => x.p != x.q))
            {
                yield return new RelationshipClue(p, q, constraint);
            }
        }

        /// <param name="minPosition"></param>
        /// <param name="maxPosition"></param>
        /// <inheritdoc />
        public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
        {
            var path = Positions.BuildContiguousPath(true);

            if (path.HasValue)
            {
                yield return new LineCellOverlay(path.Value.ToList(), Color.Violet);
            }
            else
            {
                foreach (var position in Positions)
                {
                    yield return new CellColorOverlay(Color.Violet, position);
                }
            }
        }
    }
}
using Sudoku.Clues;

namespace Sudoku.Variants;

public partial class MagicSquareVariantBuilder : VariantBuilder<int>
{
    private MagicSquareVariantBuilder()
    {
    }

    public static VariantBuilder<int> Instance { get; } = new MagicSquareVariantBuilder();

    /// <inheritdoc />
    public override string Name => "Magic Square";

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder<int>>> TryGetClueBuilders1(
        IReadOnlyDictionary<string, string> arguments)
    {
        var cr = CentreArgument.TryGetFromDictionary(arguments);
        if (!cr.IsSuccess) return cr.ConvertFailure<IReadOnlyCollection<IClueBuilder<int>>>();

        var clues = CreateMagicSquaresClues(GetCellsAround(cr.Value)).ToList();

        return clues;

    }

    public static readonly SinglePositionArgument CentreArgument = new("Center of Magic Square");

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments => new[] {CentreArgument};
        

    /// <inheritdoc />
    public override bool IsValid(Position maxPosition) => maxPosition.Row >= 9 && maxPosition.Column >= 9;

    private static Position[] GetCellsAround(Position position)
    {
        var cells = new[]
        {
            new Position(position.Column - 1, position.Row - 1),
            new Position(position.Column, position.Row - 1),
            new Position(position.Column + 1, position.Row - 1),

            new Position(position.Column - 1, position.Row),
            new Position(position.Column, position.Row),
            new Position(position.Column + 1, position.Row),

            new Position(position.Column - 1, position.Row + 1),
            new Position(position.Column, position.Row + 1),
            new Position(position.Column + 1, position.Row + 1),
        };
        return cells;
    }

    private static IReadOnlyCollection<IClueBuilder<int>> CreateMagicSquaresClues(IReadOnlyList<Position> positions) => new []{new MagicSquareClueBuilder(positions.ToImmutableArray())};


    [Equatable]
    public partial record MagicSquareClueBuilder([property:OrderedEquality] ImmutableArray<Position> Positions) : IClueBuilder<int>
    {

        /// <inheritdoc />
        public string Name => "Magic Square";

        /// <inheritdoc />
        public int Level => 2;

        /// <inheritdoc />
        public IEnumerable<IClue<int>> CreateClues(Position minPosition, Position maxPosition, IValueSource<int> valueSource,
            IReadOnlyCollection<IClue<int>> lowerLevelClues)
        {
            yield return new UniqueCompleteClue<int>("Magic Square", Positions);

            yield return new RestrictedValuesClue<int>("Centre of Magic Square", new[] { Positions[4] }, new[] { 5 });
            yield return new RestrictedValuesClue<int>("Corner of Magic Square", new[] { Positions[0], Positions[2], Positions[6], Positions[8] }, new[] { 2, 4, 6, 8 });
            yield return new RestrictedValuesClue<int>("Edge of Magic Square", new[] { Positions[1], Positions[3], Positions[5], Positions[7] }, new[] { 1, 3, 7, 9 });


            var triples = new List<(int a, int b, int c)>()
            {
                //Rows
                (0,1,2),
                (3,4,5),
                (6,7,8),
                //Columns
                (0,3,6),
                (1,4,7),
                (2,5,8),
                //Diagonals
                (0,4,8),
                (2,4,6)
            };

            var fifteenSum = ImmutableSortedSet.Create(15);
            var tenSum = ImmutableSortedSet.Create(10);

            foreach (var (a, b, c) in triples)
            {
                var multipliers =
                    new[] { Positions[a], Positions[b], Positions[c] }.Select(x => new KeyValuePair<Position, int>(x, 1))
                        .ToImmutableDictionary();


                yield return SumClue.Create("Magic Square", fifteenSum, true, multipliers);
            }

            var pairs = new List<(int a, int b)>
            {
                (0, 8), (1, 7), (2, 6), (5, 3)
            };

            foreach (var (a, b) in pairs)
            {
                var multipliers =
                    new[] { Positions[a], Positions[b] }.Select(x => new KeyValuePair<Position, int>(x, 1))
                        .ToImmutableDictionary();


                yield return SumClue.Create("Magic Square", tenSum, true, multipliers);
            }
        }

        /// <param name="minPosition"></param>
        /// <param name="maxPosition"></param>
        /// <inheritdoc />
        public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
        {
            foreach (var position in Positions)
            {
                yield return new CellColorOverlay(ClueColors.MagicSquareColor, position);
            }
        }
    }

}
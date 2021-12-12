using System.Drawing;
using CSharpFunctionalExtensions;
using Generator.Equals;
using GridPuzzles.Clues.Constraints;
using GridPuzzles.Overlays;
using GridPuzzles.VariantBuilderArguments;
using MoreLinq;

namespace Sudoku.Variants;

public partial class LockoutVariantBuilder : VariantBuilder<int>
{
    private LockoutVariantBuilder()
    {
    }

    public static VariantBuilder<int> Instance { get; } = new LockoutVariantBuilder();

    /// <inheritdoc />
    public override string Name => "Lockout";

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder<int>>> TryGetClueBuilders1(
        IReadOnlyDictionary<string, string> arguments)
    {
        var pr = Positions.TryGetFromDictionary(arguments);
        if (pr.IsFailure) return pr.ConvertFailure<IReadOnlyCollection<IClueBuilder<int>>>();

        var md = MinimumDifference.TryGetFromDictionary(arguments);
        if (md.IsFailure) return md.ConvertFailure<IReadOnlyCollection<IClueBuilder<int>>>();

        return new List<IClueBuilder<int>>
        {
            new LockoutClueBuilder(pr.Value.ToImmutableList(), md.Value)
        };
    }

    public static readonly IntArgument MinimumDifference = new("Minimum Difference", 0, 9, Maybe<int>.From(4));
    public static readonly ListPositionArgument Positions = new("Positions", 3, 9);


    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments => new VariantBuilderArgument[]
    {
        MinimumDifference,
        Positions
    };

    [Equatable]
    public partial record LockoutClueBuilder([property:OrderedEquality] ImmutableList<Position> AllPositions, int MinDifference) : IClueBuilder<int>
    {

        /// <inheritdoc />
        public string Name => "Lockout";

        /// <inheritdoc />
        public int Level => 4;

        /// <inheritdoc />
        public IEnumerable<IClue<int>> CreateClues(Position minPosition, Position maxPosition,
            IValueSource<int> valueSource,
            IReadOnlyCollection<IClue<int>> lowerLevelClues)
        {
            if (AllPositions.Count > 2)
            {
                yield return new RelationshipClue<int>(AllPositions[0], AllPositions.Last(),
                    new DifferByAtLeastConstraint(MinDifference));

                yield return new LockoutClue(AllPositions[0], AllPositions.Last(),
                    AllPositions.Skip(1).SkipLast(1).ToImmutableSortedSet(), MinDifference);
            }
        }


        /// <param name="minPosition"></param>
        /// <param name="maxPosition"></param>
        /// <inheritdoc />
        public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
        {
            if (!AllPositions.Any()) yield break;

            yield return new InsideRectCellOverlay(AllPositions[0], Color.Yellow);
            yield return new InsideRectCellOverlay(AllPositions.Last(), Color.Yellow);

            if (AllPositions.Pairwise((a, b) => PositionExtensions.IsAdjacent(a, b)).All(x => x))
            {
                yield return new LineCellOverlay(AllPositions, Color.DeepSkyBlue);
                yield break;
            }

            foreach (var position in AllPositions.Skip(1).SkipLast(1))
            {
                yield return new CellColorOverlay(Color.DeepSkyBlue, position);
            }
        }
    }
}
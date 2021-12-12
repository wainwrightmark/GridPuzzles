using System.Drawing;
using CSharpFunctionalExtensions;
using Generator.Equals;
using GridPuzzles.Overlays;
using GridPuzzles.VariantBuilderArguments;
using MoreLinq;

namespace Sudoku.Variants;

public partial class NexusVariantBuilder : VariantBuilder<int>
{
    private NexusVariantBuilder()
    {
    }

    public static VariantBuilder<int> Instance { get; } = new NexusVariantBuilder();

    /// <inheritdoc />
    public override string Name => "Nexus";

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder<int>>> TryGetClueBuilders1(
        IReadOnlyDictionary<string, string> arguments)
    {
        var pr = Positions.TryGetFromDictionary(arguments);
        if (pr.IsFailure) return pr.ConvertFailure<IReadOnlyCollection<IClueBuilder<int>>>();

        return new List<IClueBuilder<int>>
        {
            new NexusClueBuilder(pr.Value.ToImmutableList())
        };
    }

    public static readonly ListPositionArgument Positions = new("Positions", 3, 9);


    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments => new VariantBuilderArgument[] { Positions };

    [Equatable]
    public partial record NexusClueBuilder(ImmutableList<Position> AllPositions) : IClueBuilder<int>
    {

        /// <inheritdoc />
        public string Name => "Nexus";

        /// <inheritdoc />
        public int Level => 2;

        /// <inheritdoc />
        public IEnumerable<IClue<int>> CreateClues(Position minPosition, Position maxPosition,
            IValueSource<int> valueSource,
            IReadOnlyCollection<IClue<int>> lowerLevelClues)
        {
            if (AllPositions.Count > 2)
            {
                yield return new BetweenClue(AllPositions[0], AllPositions.Last(),
                    AllPositions.Skip(1).SkipLast(1).ToImmutableSortedSet());
            }
        }


        /// <param name="minPosition"></param>
        /// <param name="maxPosition"></param>
        /// <inheritdoc />
        public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
        {
            if (!AllPositions.Any()) yield break;

            yield return new InsideCircleCellOverlay(AllPositions[0], Color.LightPink);
            yield return new InsideCircleCellOverlay(AllPositions.Last(), Color.LightPink);

            if (AllPositions.Pairwise((a, b) => a.IsAdjacent(b)).All(x => x))
            {
                yield return new LineCellOverlay(AllPositions, Color.Purple);
                yield break;
            }

            foreach (var position in AllPositions.Skip(1).SkipLast(1))
            {
                yield return new CellColorOverlay(Color.Purple, position);
            }
        }
    }
}
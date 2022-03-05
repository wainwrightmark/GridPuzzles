namespace Sudoku.Variants;

public partial class NexusVariantBuilder : VariantBuilder
{
    private NexusVariantBuilder()
    {
    }

    public static VariantBuilder Instance { get; } = new NexusVariantBuilder();

    /// <inheritdoc />
    public override string Name => "Nexus";

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder>> TryGetClueBuilders1(
        IReadOnlyDictionary<string, string> arguments)
    {
        var pr = Positions.TryGetFromDictionary(arguments);
        if (pr.IsFailure) return pr.ConvertFailure<IReadOnlyCollection<IClueBuilder>>();

        return new List<IClueBuilder>
        {
            new NexusClueBuilder(pr.Value.ToImmutableArray())
        };
    }

    public static readonly ListPositionArgument Positions = new("Positions", 3, 9);


    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments => new VariantBuilderArgument[] { Positions };

    [Equatable]
    public partial record NexusClueBuilder([property:OrderedEquality] IReadOnlyList<Position> AllPositions) : IClueBuilder
    {

        /// <inheritdoc />
        public string Name => "Nexus";

        /// <inheritdoc />
        public int Level => 2;

        /// <inheritdoc />
        public IEnumerable<IClue<int, IntCell>> CreateClues(Position minPosition, Position maxPosition,
            IValueSource valueSource,
            IReadOnlyCollection<IClue<int, IntCell>> lowerLevelClues)
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
﻿
namespace Sudoku.Variants;

public partial class WhispersVariantBuilder : VariantBuilder
{
    private WhispersVariantBuilder() { }

    public static VariantBuilder Instance { get; } = new WhispersVariantBuilder();

    /// <inheritdoc />
    public override string Name => "Whispers";

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder>> TryGetClueBuilders1(
        IReadOnlyDictionary<string, string> arguments)
    {
        var pr = Positions.TryGetFromDictionary(arguments);
        if (pr.IsFailure) return pr.ConvertFailure<IReadOnlyCollection<IClueBuilder>>();

        var md = MinDistance.TryGetFromDictionary(arguments);
        if (md.IsFailure) return md.ConvertFailure<IReadOnlyCollection<IClueBuilder>>();

        return new List<IClueBuilder>
        {
            new WhispersClueBuilder(pr.Value.ToImmutableArray(), md.Value)
        };
    }

    public static readonly IntArgument MinDistance = new("Minimum Distance", 1,9, 5);
    public static readonly ListPositionArgument Positions = new("Positions", 2,9);
        

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments => new VariantBuilderArgument[]  {MinDistance, Positions};

    [Equatable]
    public partial record  WhispersClueBuilder([property:OrderedEquality] IReadOnlyList<Position> Positions, int MinimumDistance) : IClueBuilder
    {

        /// <inheritdoc />
        public string Name  => "Whispers";

        /// <inheritdoc />
        public int Level => 2;

        /// <inheritdoc />
        public IEnumerable<IClue<int, IntCell>> CreateClues(Position minPosition, Position maxPosition, IValueSource valueSource,
            IReadOnlyCollection<IClue<int, IntCell>> lowerLevelClues)
        {

            foreach (var (p1, p2) in Positions.Pairwise((a,b)=>(a,b)))
            {
                yield return RelationshipClue.Create(p1, p2, new DifferByAtLeastConstraint(MinimumDistance));   
            }
        }


        /// <param name="minPosition"></param>
        /// <param name="maxPosition"></param>
        /// <inheritdoc />
        public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
        {
            if (Positions.Pairwise((a,b)=> a.IsAdjacent(b)).All(x=>x))
            {
                yield return new LineCellOverlay(Positions, Color.Orange);
                yield break;
            }

            var pairs = Positions.Pairwise((a, b) => (a, b)).Select(x => (x.a, x.b));

            foreach (var (a, b) in pairs)
            {
                var overlay = CellOverlays.TryCreateTwoPositionText(a, b, $"|{MinimumDistance}|");
                if (overlay.HasValue)
                    yield return overlay.Value;
                else
                {
                    yield return new CellColorOverlay(Color.Orange, a);
                    yield return new CellColorOverlay(Color.Orange, b);
                }
            }
        }
    }
}
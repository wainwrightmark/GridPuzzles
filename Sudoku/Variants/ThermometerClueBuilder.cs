using System;
using GridPuzzles.Enums;
using Sudoku.Clues;
using Sudoku.Overlays;

namespace Sudoku.Variants;

public partial class ThermometerVariantBuilder : VariantBuilder
{
    private ThermometerVariantBuilder()
    {
    }

    public static VariantBuilder Instance { get; } = new ThermometerVariantBuilder();

    /// <inheritdoc />
    public override string Name => "Thermometer";

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder>> TryGetClueBuilders1(
        IReadOnlyDictionary<string, string> arguments)
    {
        var pr = Positions.TryGetFromDictionary(arguments);
        if (pr.IsFailure) return pr.ConvertFailure<IReadOnlyCollection<IClueBuilder>>();

        return new List<IClueBuilder>
        {
            new ThermometerClueBuilder(pr.Value.ToImmutableArray())
        };
    }

    public static readonly ListPositionArgument Positions = new("Positions", 2, 9);

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments => new[] { Positions };

    [Equatable]
    public partial record ThermometerClueBuilder([property:OrderedEquality] IReadOnlyList<Position> Positions) : IClueBuilder
    {

        /// <inheritdoc />
        public string Name => "Thermometer";

        /// <inheritdoc />
        public int Level => 2;

        /// <inheritdoc />
        public IEnumerable<IClue<int, IntCell>> CreateClues(Position minPosition, Position maxPosition,
            IValueSource valueSource,
            IReadOnlyCollection<IClue<int, IntCell>> lowerLevelClues)
        {
            for (var i = 0; i < Positions.Count - 1; i++)
            {
                var p1 = Positions[i];
                for (var j = i + 1; j < Positions.Count; j++)
                {
                    var p2 = Positions[j];
                    yield return RelationshipClue.Create(p1, p2, new XLessThanConstraint(j - i));
                }
            }
        }


        /// <param name="minPosition"></param>
        /// <param name="maxPosition"></param>
        /// <inheritdoc />
        public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
        {
            if (Positions.Count > 2 && Positions.Pairwise((a, b) => a.IsAdjacent(b)).All(x => x))
            {
                yield return new ThermometerCellOverlay(Positions, Color.LightGray);
                yield break;
            }

            var pairs = Positions.Pairwise((a, b) => (a, b)).Select(x => (x.a, x.b));

            foreach (var (a, b) in pairs)
            {
                var overlay = CellOverlays.TryCreateTwoPositionText(a, b, GetSymbol);
                if (overlay.HasValue)
                    yield return overlay.Value;
                else
                {
                    yield return new CellColorOverlay(ClueColors.ThermometerHeadColor, a);
                    yield return new CellColorOverlay(ClueColors.ThermometerColor, b);
                }
            }

            static (string symbol, int? rotation) GetSymbol(CompassDirection compassDirection)
            {
                return compassDirection switch
                {
                    CompassDirection.North => (">", -90),
                    CompassDirection.NorthEast => (">", -45),
                    CompassDirection.East => (">", null),
                    CompassDirection.SouthEast => (">", 45),
                    CompassDirection.South => ("<", -90),
                    CompassDirection.SouthWest => ("<", -45),
                    CompassDirection.West => ("<", null),
                    CompassDirection.NorthWest => ("<", 45),

                    _ => throw new ArgumentOutOfRangeException(nameof(compassDirection), compassDirection, null)
                };
            }
        }
    }
}
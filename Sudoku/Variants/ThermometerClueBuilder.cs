using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using CSharpFunctionalExtensions;
using GridPuzzles;
using GridPuzzles.Clues;
using GridPuzzles.Clues.Constraints;
using GridPuzzles.Enums;
using GridPuzzles.Overlays;
using GridPuzzles.VariantBuilderArguments;
using MoreLinq;
using Sudoku.Clues;
using Sudoku.Overlays;

namespace Sudoku.Variants;

public class ThermometerVariantBuilder : VariantBuilder<int>
{
    private ThermometerVariantBuilder()
    {
    }

    public static VariantBuilder<int> Instance { get; } = new ThermometerVariantBuilder();

    /// <inheritdoc />
    public override string Name => "Thermometer";

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder<int>>> TryGetClueBuilders1(
        IReadOnlyDictionary<string, string> arguments)
    {
        var pr = Positions.TryGetFromDictionary(arguments);
        if (pr.IsFailure) return pr.ConvertFailure<IReadOnlyCollection<IClueBuilder<int>>>();

        return new List<IClueBuilder<int>>
        {
            new ThermometerClueBuilder(pr.Value.ToImmutableList())
        };
    }

    public static readonly ListPositionArgument Positions = new("Positions", 2, 9);

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments => new[] { Positions };

    public class ThermometerClueBuilder : IClueBuilder<int>
    {
        /// <summary>
        /// Create a thermometer. The fist position will have the lowest value.
        /// </summary>
        /// <param name="positions"></param>
        public ThermometerClueBuilder(ImmutableList<Position> positions)
        {
            Positions = positions;
        }

        /// <inheritdoc />
        public string Name => "Thermometer";

        /// <inheritdoc />
        public int Level => 2;

        public ImmutableList<Position> Positions { get; }

        /// <inheritdoc />
        public IEnumerable<IClue<int>> CreateClues(Position minPosition, Position maxPosition,
            IValueSource<int> valueSource,
            IReadOnlyCollection<IClue<int>> lowerLevelClues)
        {
            for (var i = 0; i < Positions.Count - 1; i++)
            {
                var p1 = Positions[i];
                for (var j = i + 1; j < Positions.Count; j++)
                {
                    var p2 = Positions[j];
                    yield return RelationshipClue<int>.Create(p1, p2, new XLessThanConstraint(j - i));
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
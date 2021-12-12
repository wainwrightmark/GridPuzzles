﻿using Sudoku.Overlays;

namespace Sudoku.Variants;

public partial class UniqueGroupVariantBuilder<T> : VariantBuilder<T> where T : notnull
{
    private UniqueGroupVariantBuilder()
    {
    }

    public static UniqueGroupVariantBuilder<T> Instance { get; } = new();
    public override string Name => "UniqueGroup";

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments => new VariantBuilderArgument[]
    {
        PositionArgument
    };

    private static readonly ListPositionArgument PositionArgument = new("Positions",
        2, 9);

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder<T>>> TryGetClueBuilders1(IReadOnlyDictionary<string, string> arguments)
    {
        var pr = PositionArgument.TryGetFromDictionary(arguments);
        if (pr.IsFailure) return pr.ConvertFailure<IReadOnlyCollection<IClueBuilder<T>>>();

        var l = new List<IClueBuilder<T>>
        {
            new UniqueClueBuilder(pr.Value.ToImmutableSortedSet())
        };

        return l;
    }

    [Equatable]
    private partial record UniqueClueBuilder([property:SetEquality] ImmutableSortedSet<Position> Positions) : IClueBuilder<T>
    {

        /// <inheritdoc />
        public string Name => "Unique Group " + Positions.ToDelimitedString(", ");

        /// <inheritdoc />
        public int Level => 1;

        /// <inheritdoc />
        public IEnumerable<IClue<T>> CreateClues(Position minPosition, Position maxPosition,
            IValueSource<T> valueSource,
            IReadOnlyCollection<IClue<T>> lowerLevelClues)
        {

            yield return new UniquenessClue<T>(Positions, "Unique Group");
        }

        /// <param name="minPosition"></param>
        /// <param name="maxPosition"></param>
        /// <inheritdoc />
        public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
        {
            var shapeCellOverlay = ShapeCellOverlay.TryMake(Positions, null);
            if (shapeCellOverlay.HasValue)
            {
                yield return shapeCellOverlay.Value;
                yield break;
            }

            //TODO dashed line box
            foreach (var position in Positions)
            {
                yield return new CellColorOverlay(Color.LightYellow, position);
            }
        }
    }
}
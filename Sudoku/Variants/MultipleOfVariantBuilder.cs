using System.Drawing;
using CSharpFunctionalExtensions;
using GridPuzzles.Clues.Constraints;
using GridPuzzles.Overlays;
using GridPuzzles.VariantBuilderArguments;

namespace Sudoku.Variants;

public class MultipleOfVariantBuilder : VariantBuilder<int>
{
    private MultipleOfVariantBuilder() { }

    public static VariantBuilder<int> Instance { get; } = new MultipleOfVariantBuilder();

    /// <inheritdoc />
    public override string Name => "Multiple";

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder<int>>> TryGetClueBuilders1(
        IReadOnlyDictionary<string, string> arguments)
    {
        var sr = AmountArgument.TryGetFromDictionary(arguments);
        if (sr.IsFailure) return sr.ConvertFailure<IReadOnlyCollection<IClueBuilder<int>>>();
        var pr = PositionArgument.TryGetFromDictionary(arguments);
        if (pr.IsFailure) return pr.ConvertFailure<IReadOnlyCollection<IClueBuilder<int>>>();

        var l = new List<IClueBuilder<int>>
        {
            new MultipleOfClueBuilder(pr.Value.Min(), pr.Value.Max(), sr.Value)
        };

        return l;
    }

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments => new VariantBuilderArgument[]
    {
        AmountArgument,
        PositionArgument
    };

    private static readonly IntArgument AmountArgument = new("Amount",
        2,
        4, 2);

    private static readonly ListPositionArgument PositionArgument = new("Positions",
        2,
        2);
        

    private record MultipleOfClueBuilder(Position Position1,Position Position2,int Amount ) : IClueBuilder<int>
    {
        /// <inheritdoc />
        public string Name => "Multiple";

        /// <inheritdoc />
        public int Level => 2;

        /// <inheritdoc />
        public IEnumerable<IClue<int>> CreateClues(Position minPosition, Position maxPosition, IValueSource<int> valueSource,
            IReadOnlyCollection<IClue<int>> lowerLevelClues)
        {
            yield return RelationshipClue<int>.Create(Position1, Position2, new XTimesConstraint(Amount));
        }

        /// <param name="minPosition"></param>
        /// <param name="maxPosition"></param>
        /// <inheritdoc />
        public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
        {
            if (Amount == 2 && CellOverlays.TryCreateTwoPositionText(Position1, Position2,  "⚫").TryExtract(out var co))
            {
                yield return co;
            }
            else
            {
                yield return new CellColorOverlay(Color.Orange, Position1);
                yield return new CellColorOverlay(Color.Orange, Position2);
            }
        }
    }
}
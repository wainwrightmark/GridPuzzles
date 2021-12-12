using System.Drawing;
using CSharpFunctionalExtensions;
using Generator.Equals;
using GridPuzzles.Enums;
using GridPuzzles.Overlays;
using GridPuzzles.VariantBuilderArguments;
using Sudoku.Clues;
using Sudoku.Overlays;

namespace Sudoku.Variants;

public partial class SumVariantBuilder : VariantBuilder<int>
{
    private SumVariantBuilder() {}

    public static VariantBuilder<int> Instance { get; } = new SumVariantBuilder();

    /// <inheritdoc />
    public override string Name => "Sum";

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder<int>>> TryGetClueBuilders1(
        IReadOnlyDictionary<string, string> arguments)
    {
        var sr = SumArgument.TryGetFromDictionary(arguments);
        if (sr.IsFailure) return sr.ConvertFailure<IReadOnlyCollection<IClueBuilder<int>>>();
        var pr = PositionArgument.TryGetFromDictionary(arguments);
        if (pr.IsFailure) return pr.ConvertFailure<IReadOnlyCollection<IClueBuilder<int>>>();
        var ur = UniqueArgument.TryGetFromDictionary(arguments).Match(x => x, _ => false);

        var l = new List<IClueBuilder<int>>
        {
            new SumClueBuilder(sr.Value, pr.Value.ToImmutableSortedSet(), ur)
        };

        return l;
    }

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments => new VariantBuilderArgument[]
    {
        SumArgument,
        UniqueArgument,
        PositionArgument
    };

    private static readonly IntArgument SumArgument = new("Sum",
        1,
        100, 10);

    private static readonly ListPositionArgument PositionArgument = new("Positions",
        2,
        9);

    private static readonly BoolArgument UniqueArgument = new("Unique", Maybe<bool>.From(true));

    [Equatable]
    public partial record SumClueBuilder(int Sum,[property:OrderedEquality] ImmutableSortedSet<Position> Positions, bool Unique) : IClueBuilder<int>
    {
        /// <inheritdoc />
        public string Name => $"Sum to {Sum}";

        /// <inheritdoc />
        public int Level => 2;

        /// <inheritdoc />
        public IEnumerable<IClue<int>> CreateClues(Position minPosition, Position maxPosition,
            IValueSource<int> valueSource,
            IReadOnlyCollection<IClue<int>> lowerLevelClues)
        {
            var dict = Positions.Select(x => new KeyValuePair<Position, int>(x, 1)).ToImmutableDictionary();

            if (Sum == valueSource.AllValues.Sum()) //Special case for sums to 45
            {
                var unique = Unique;
                if (!unique)
                {
                    var clueHelper = new UniquenessClueHelper<int>(lowerLevelClues);
                    unique =  clueHelper.ArePositionsMutuallyUnique(Positions);
                }

                yield return new UniqueCompleteClue<int>($"Sum to {Sum}", Positions);
                yield break;
            }

            yield return SumClue.Create($"Sum to {Sum}", ImmutableSortedSet.Create(Sum), true, dict);

            if (Unique)
                yield return new UniquenessClue<int>(Positions, Name);
        }

        /// <param name="minPosition"></param>
        /// <param name="maxPosition"></param>
        /// <inheritdoc />
        public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
        {
            if (Positions.Count == 2 && RomanNumeralDictionary.TryGetValue(Sum, out var numeral) &&
                Positions.Min.IsOrthogonal(Positions.Max))
            {
                var overlay = CellOverlays.TryCreateTwoPositionText(Positions.Min, Positions.Max, numeral);
                if (overlay.HasValue)
                    yield return overlay.Value;
            }
            else if (CircledNumberDictionary.TryGetValue(Sum, out var circledNumber) &&
                     CellOverlays.TryCreateSquareText(Positions, circledNumber).TryExtract(out var co))
            {
                yield return co;
            }
            else
            {
                if (Unique)
                {
                    var shapeCellOverlay = ShapeCellOverlay.TryMake(Positions, Sum.ToString());
                    if (shapeCellOverlay.HasValue)
                    {
                        yield return shapeCellOverlay.Value;
                        yield break;
                    }
                }
                else //not unique - try little killer
                {
                    var possibleDirections = new List<CompassDirection>()
                    {
                        CompassDirection.NorthEast, CompassDirection.SouthEast, CompassDirection.SouthWest
                    };

                    foreach (var direction in possibleDirections)
                    {
                        var outsideCell = Positions.Min.Move(direction.Opposite());
                        if (!outsideCell.IsWithin(minPosition, maxPosition))
                        {
                            var line = Positions.BuildStraightLine(outsideCell, direction);
                            if (line.HasValue)
                            {
                                //make sure the line goes all the way
                                var finalOutside = line.Value.Last().Move(direction);
                                if (!finalOutside.IsWithin(minPosition, maxPosition))
                                {
                                    //TODO find a way to stop arrows clashing
                                    yield return new TextCellOverlay(outsideCell, 1, 1,
                                        direction.GetArrowSymbol() + Sum, Color.Black, Color.Black);
                                    yield break;
                                }
                            }
                        }
                    }
                }

                foreach (var position in Positions)
                {
                    yield return new CellColorOverlay(ClueColors.GetUniqueSumColor(Sum), position);
                }
            }
        }

        private static readonly IReadOnlyDictionary<int, string> CircledNumberDictionary =
            "⓪①②③④⑤⑥⑦⑧⑨⑩⑪⑫⑬⑭⑮⑯⑰⑱⑲⑳㉑㉒㉓㉔㉕㉖㉗㉘㉙㉚㉛㉜㉝㉞㉟㊱㊲㊳㊴㊵㊶㊷㊸㊹㊺㊻㊼㊽㊾㊿"
                .Select((c, i) => (i, c)).ToDictionary(x => x.i, x => x.c.ToString());


        private static readonly IReadOnlyDictionary<int, string> RomanNumeralDictionary =
            "0 Ⅰ Ⅱ Ⅲ Ⅳ Ⅴ Ⅵ Ⅶ Ⅷ Ⅸ Ⅹ Ⅺ Ⅻ ⅫⅠ ⅩⅣ ⅩⅤ ⅩⅥ ⅩⅦ ⅩⅧ ⅩⅨ ⅩⅩ".Split(' ')
                .Select((s, i) => (s, i)).ToDictionary(x => x.i, x => x.s);
    }
}
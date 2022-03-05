using System;
using System.ComponentModel;

namespace Sudoku.Variants;

public class SandwichVariantBuilder : VariantBuilder
{
    private SandwichVariantBuilder() {}

    public static VariantBuilder Instance { get; } = new SandwichVariantBuilder();

    /// <inheritdoc />
    public override string Name => "Sandwich";

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder>> TryGetClueBuilders1(
        IReadOnlyDictionary<string, string> arguments)
    {
        var pir = ParallelIndexArgument.TryGetFromDictionary(arguments);
        var dr = DirectionArgument.TryGetFromDictionary(arguments);
        var sr = SumArgument.TryGetFromDictionary(arguments);

        if (pir.IsFailure) return pir.ConvertFailure<IReadOnlyCollection<IClueBuilder>>();
        if (dr.IsFailure) return dr.ConvertFailure<IReadOnlyCollection<IClueBuilder>>();
        if (sr.IsFailure) return sr.ConvertFailure<IReadOnlyCollection<IClueBuilder>>();


        return new List<IClueBuilder>() {new SandwichClueBuilder(pir.Value, dr.Value, sr.Value)};
    }

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments => new List<VariantBuilderArgument>()
    {
        ParallelIndexArgument,
        DirectionArgument,
        SumArgument
    };
        

    public readonly IntArgument ParallelIndexArgument = new("Index", 1, 9, Maybe<int>.None);

    public readonly EnumArgument<Parallel> DirectionArgument =
        new("Parallel", Maybe<Parallel>.From(Parallel.Column));

    public readonly IntArgument SumArgument = new("Sandwich Sum", 0, 45, Maybe<int>.None);
        

    public record SandwichClueBuilder(int ParallelIndex, Parallel Parallel, int SandwichSum) : IClueBuilder
    {

        /// <inheritdoc />
        public string Name => "Sandwich";

        /// <inheritdoc />
        public int Level => 2;

        /// <inheritdoc />
        public IEnumerable<IClue<int, IntCell>> CreateClues(Position minPosition, Position maxPosition,
            IValueSource valueSource,
            IReadOnlyCollection<IClue<int, IntCell>> lowerLevelClues)
        {
            var positions = Parallel switch
            {
                Parallel.Row => Enumerable.Range(minPosition.Column, maxPosition.Column)
                    .Select(c => new Position(c, ParallelIndex))
                    .ToArray(),
                Parallel.Column => Enumerable.Range(minPosition.Row, maxPosition.Row)
                    .Select(r => new Position(ParallelIndex, r))
                    .ToArray(),
                _ => throw new InvalidEnumArgumentException(nameof(Parallel))
            };

            var min = valueSource.AllValues.Min();
            var max = valueSource.AllValues.Max();
            var totalSum = valueSource.AllValues.Sum() - (min + max);

            var plausibleNumbers =
                NumberPlausibilityHelper.GetCombinations(valueSource.AllValues.Except(new[] {min, max}));

            yield return new SandwichClue(SandwichSum, totalSum, min, max, positions, plausibleNumbers);
        }

        /// <param name="minPosition"></param>
        /// <param name="maxPosition"></param>
        /// <inheritdoc />
        public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
        {
            Position topLeftPosition = Parallel switch
            {
                Parallel.Row => new Position(0, ParallelIndex),
                Parallel.Column => new Position(ParallelIndex, 0),
                _ => throw new ArgumentOutOfRangeException()
            };

            yield return new TextCellOverlay(topLeftPosition, 1, 1, SandwichSum.ToString(), Color.Black, Color.Black);

        }

        public class SandwichClue : IRuleClue //TODO integration test with https://www.youtube.com/watch?v=qUZnq5nP0zI (only needs 2 of the givens
        {
            /// <inheritdoc />
            public string Name => "Sandwich";

            public SandwichClue(int sandwichSum, int totalSum, int bread1, int bread2, Position[] orderedPositions,
                ImmutableDictionary<int, ImmutableHashSet<int>> plausibleValues)
            {
                SandwichSum = sandwichSum;
                Bread1 = bread1;
                Bread2 = bread2;
                _orderedPositions = orderedPositions;
                PlausibleValues = plausibleValues;
                Positions = _orderedPositions.ToImmutableSortedSet();
                OutsideSum = totalSum - sandwichSum;
                BreadValues = new[] {Bread1, Bread2}.ToImmutableHashSet();
            }

            /// <inheritdoc />
            public ImmutableSortedSet<Position> Positions { get; }

            public ImmutableDictionary<int, ImmutableHashSet<int>> PlausibleValues { get; }

            private readonly Position[] _orderedPositions;

            public ImmutableHashSet<int> BreadValues { get; }

            public int Bread1 { get; }

            public int Bread2 { get; }

            public int SandwichSum { get; }

            public int OutsideSum { get; }

            /// <inheritdoc />
            public IEnumerable<ICellChangeResult> CalculateCellUpdates(Grid grid)
            {
                var cells = _orderedPositions.Select(grid.GetCellKVP).ToImmutableDictionary(x => x.Key);


                for (var i = 0; i < _orderedPositions.Length; i++)
                {
                    var cell = cells[_orderedPositions[i]];

                    foreach (var possibleValue in cell.Value)
                        if (!IsFakeValuePossible(cells, i, possibleValue))
                            yield return cell.CloneWithoutValue(possibleValue,
                                new SandwichReason(this));
                }

            }


            /// <summary>
            /// Essentially, Is there a solution to the sandwich that involves this value in this position.
            /// </summary>
            private bool IsFakeValuePossible(ImmutableDictionary<Position, KeyValuePair<Position, IntCell>> cells,
                int fakeIndex, int fakeValue)
            {
                if (fakeValue == Bread1 || fakeValue == Bread2)
                {
                    var otherBread = fakeValue == Bread1 ? Bread2 : Bread1;

                    for (var i = 0; i < _orderedPositions.Length; i++)
                    {
                        if (i == fakeIndex) continue;
                        var cell = cells[_orderedPositions[i]];
                        if (!cell.Value.Contains(otherBread)) continue;

                        var usedValues = new Stack<int>(BreadValues);

                        if (Math.Abs(i - fakeIndex) * 2 > cells.Count)
                        {
                            //Distance is big - use outside values
                            var positionsOutside = fakeIndex < i
                                ? new Stack<Position>(_orderedPositions[..fakeIndex]
                                    .Concat(_orderedPositions[(i + 1)..]))
                                : new Stack<Position>(_orderedPositions[..i]
                                    .Concat(_orderedPositions[(fakeIndex + 1)..]));

                            if (SumIsPossible(OutsideSum, cells, positionsOutside, usedValues))
                                return true;
                        }
                        else //Use inside values
                        {
                            var positionsInside = fakeIndex < i
                                ? new Stack<Position>(_orderedPositions[(fakeIndex + 1)..(i)])
                                : new Stack<Position>(_orderedPositions[(i + 1)..(fakeIndex)]);

                            if (SumIsPossible(SandwichSum, cells, positionsInside, usedValues))
                                return true;
                        }

                    }
                }
                else
                {
                    var usedValues = new Stack<int>(BreadValues.Append(fakeValue));

                    for (var i1 = 0; i1 < _orderedPositions.Length - 1; i1++)
                    {
                        if (i1 == fakeIndex) continue;
                        var cell1 = cells[_orderedPositions[i1]];

                        var i2PossibleValues = new HashSet<int>();
                        if (cell1.Value.Contains(Bread1)) i2PossibleValues.Add(Bread2);
                        if (cell1.Value.Contains(Bread2)) i2PossibleValues.Add(Bread1);

                        if (!i2PossibleValues.Any()) continue;

                        for (var i2 = i1 + 1; i2 < _orderedPositions.Length; i2++)
                        {
                            if (i2 == fakeIndex) continue;
                            var cell2 = cells[_orderedPositions[i2]];
                            if (!cell2.Value.Overlaps(i2PossibleValues)) continue;

                            if (i1 < fakeIndex && fakeIndex < i2)
                            {
                                //fake index is inside - use outside values

                                var positionsOutside = new Stack<Position>(_orderedPositions[..i1]
                                    .Concat(_orderedPositions[(i2 + 1)..]));

                                if (SumIsPossible(OutsideSum, cells, positionsOutside, usedValues))
                                    return true;
                            }
                            else
                            {
                                //fake index is outside - use inside values
                                var positionsInside = new Stack<Position>(_orderedPositions[(i1 + 1)..(i2)]);

                                if (SumIsPossible(SandwichSum, cells, positionsInside, usedValues))
                                    return true;
                            }
                        }
                    }
                }

                return false;
            }

            private bool SumIsPossible(int sum,
                ImmutableDictionary<Position, KeyValuePair<Position, IntCell>> cells,
                Stack<Position> remainingPositions, Stack<int> usedValues)
            {

                if (!PlausibleValues[remainingPositions.Count].Contains(sum)) return false;

                if (remainingPositions.Count == 0) return true;

                var positionToCheck = remainingPositions.Pop();

                var cellToCheck = cells[positionToCheck];

                foreach (var possibleValue in cellToCheck.Value.Except(usedValues))
                {
                    usedValues.Push(possibleValue);

                    var possible = SumIsPossible(sum - possibleValue, cells, remainingPositions, usedValues);

                    usedValues.Pop();

                    if (possible)
                        return true;
                }

                remainingPositions.Push(positionToCheck);
                return false;
            }
        }

    }

    public sealed record SandwichReason(SandwichClueBuilder.SandwichClue SandwichClue) : ISingleReason
    {
        /// <inheritdoc />
        public string Text => "No sandwich is possible with this value here";

        /// <inheritdoc />
        public IEnumerable<Position> GetContributingPositions(IGrid grid)
        {
            return SandwichClue.Positions;
        }

        /// <inheritdoc />
        public Maybe<IClue> Clue => SandwichClue;
    }


    public static class NumberPlausibilityHelper
    {
        private static readonly IDictionary<string, ImmutableDictionary<int, ImmutableHashSet<int>>> Cache = new Dictionary<string, ImmutableDictionary<int, ImmutableHashSet<int>>>();

        public static ImmutableDictionary<int, ImmutableHashSet<int>> GetCombinations(IEnumerable<int> allValues)
        {
            //TODO use regular dictionary
            var values = allValues.OrderBy(x => x).ToList();
            var key = string.Join(",", values);
            if (!Cache.TryGetValue(key, out var r))
            {
                r = values.Subsets()
                    .GroupBy(x => x.Count)
                    .ToImmutableDictionary(x => x.Key, x => x.Select(y => y.Sum()).ToImmutableHashSet());

                Cache.Add(key, r);
            }

            return r;
        }
    }
}
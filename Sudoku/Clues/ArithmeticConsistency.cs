//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.Linq;
//using CSharpFunctionalExtensions;
//using GridPuzzles;
//using GridPuzzles.Clues;
//using GridPuzzles.Overlays;
//using MoreLinq;
//using Sudoku.Variants;

//namespace Sudoku.Clues
//{
//    /// <summary>
//    /// Makes sure all sums add up
//    /// </summary>
//    public sealed class ArithmeticConsistency : NoArgumentVariantBuilder<int>
//    {
//        private ArithmeticConsistency()
//        {
//        }

//        public static ArithmeticConsistency Instance { get; } = new();
//        public override string Name => "Arithmetic Consistency";

//        /// <inheritdoc />
//        public override int Level => 11;

//        /// <inheritdoc />
//        public override bool OnByDefault => true;

//        /// <inheritdoc />
//        public override IEnumerable<IClue<int>> CreateClues(Position minPosition, Position maxPosition,
//            IValueSource<int> valueSource,
//            IReadOnlyCollection<IClue<int>> lowerLevelClues)
//        {
//            var fullGroupSize = valueSource.AllValues.Count;
//            var totalGroupValue = valueSource.AllValues.Sum();

//            var lookup = lowerLevelClues.OfType<ICompletenessClue<int>>()
//                .SelectMany(clue => clue.Positions.Select(p => (p, clue)))
//                .ToLookup(x => x.p, x => x.clue);
//            if (!lookup.Any())
//                yield break;
                
//            var originalSumClues = lowerLevelClues.OfType<SumClue>().Where(x => x.SimpleTotalSum.HasValue).ToList();
//            if (!originalSumClues.Any())
//                yield break;


//            var changed = true;
//            var newClues = new HashSet<SumClue>();

//            while (changed)
//            {
//                changed = false;
//                var groups =
//                    originalSumClues.Concat(newClues)
//                        .SelectMany(clue => GetContainingGroups(clue, lookup).Select(group => (group, clue)))
//                        .GroupBy(x => x.group, x => x.clue);

//                foreach (var group in groups)
//                {
//                    var completenessClue = group.Key;

//                    var bestCombination =
//                        group.Subsets()
//                            .OrderByDescending(x => x.Count)
//                            .Where(x =>
//                            {
//                                var fullCount = x.Sum(s => s.Positions.Count);

//                                if (fullCount > fullGroupSize) return false; //This group obviously contains duplicates
//                                if (fullCount * 2 < fullGroupSize)
//                                    return false; //This group is too small to worry about

//                                if (!x.SelectMany(g => g.Positions).Distinct().CountBetween(fullCount, fullCount))
//                                    return false; //All elements should be distinct

//                                return true;
//                            }).TryFirst();

//                    if (bestCombination.HasValue)
//                    {
//                        var newMultipliers = completenessClue.Positions.Except(
//                                bestCombination.Value.SelectMany(x => x.Positions)
//                            )
//                            .ToImmutableDictionary(x => x, _ => 1);

//                        if (newMultipliers.Any()) //make sure there is at least one position
//                        {
//                            var newSum = totalGroupValue - bestCombination.Value.Sum(x => x.SimpleTotalSum.Value);

//                            changed = true;
//                            var newSumClue = (SumClue)SumClue.Create(
//                                $"Derived Sum in {completenessClue.Name}",
//                                ImmutableSortedSet.Create(newSum),
//                                true,
//                                newMultipliers
//                            );

//                            newClues.Add(newSumClue);
//                        }
//                    }
//                }
//            }


//            foreach (var newSumClue in newClues)
//            {
//                var overlays =
//                    newSumClue.Positions.Select(p => 
//                        new CellColorOverlay(ClueColors.GetUniqueSumColor(newSumClue.SimpleTotalSum.Value), p)
//                        ).ToList();

//                yield return new VirtualRuleClue<int>(newSumClue, overlays);
//            }

//            static IEnumerable<ICompletenessClue<int>> GetContainingGroups(SumClue sumClue,
//                ILookup<Position, ICompletenessClue<int>> lookup)
//            {
//                return lookup[sumClue.Positions.First()]
//                    .Where(l => sumClue.Positions.All(l.Positions.Contains));
//            }
//        }
        
        
//    }
//}
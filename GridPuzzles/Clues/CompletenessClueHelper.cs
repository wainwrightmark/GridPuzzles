namespace GridPuzzles.Clues;
//public class ParallelClueHelper<T> : ClueTypeHelper<ICompletenessClue<T>, T>where T :notnull
//{
//    public ParallelClueHelper(IEnumerable<IClue<T>> clues)
//    {
//        ParallelClues = clues.OfType<IParallelClue<T>>().ToList();
//    }

//    public IReadOnlyCollection<IParallelClue<T>> ParallelClues { get; }

//    /// <inheritdoc />
//    public override IEnumerable<ICellChangeResult> GetUpdates(Grid<T> grid,
//        IEnumerable<Position> positionsToLookAt)
//    {
//        yield break; //TODO fix this

//        //var dict = ParallelClues.SelectMany(parallelClue =>
//        //    parallelClue.Positions.Select(grid.GetCellKVP)
//        //        .SelectMany(cell => cell.Value.PossibleValues
//        //            .Select(value =>
//        //                (parallelClue.Parallel,
//        //                    parallelClue.Index,
//        //                    parallelClue.Parallel.GetOrthogonal().GetIndex(cell.Key),
//        //                    value
//        //                ))
//        //        )
//        //).ToDictionary(x=> (x.value, x.Parallel, x.Index));

//        //foreach (var value in grid.ClueSource.ValueSource.AllValues)
//        //{

//        //    //foreach row in rows
//        //        //check number of values / get possible columns
//        //        //foreach row greater than row
//        //        //check if value columns is subset of possible columns
//        //        //If we get enough rows - go through columns and cancel values

//        //}
//    }
//}

public sealed class CompletenessClueHelper<T> : ClueHelper<ICompletenessClue<T>, T>, IClueUpdateHelper<T>
        
    where T :notnull
{
    public CompletenessClueHelper(IEnumerable<IClue<T>> clues) : base(clues)
    {
        CluesByPosition = Clues
            .SelectMany(clue =>
                clue.Positions.Select(position => (position, clue)))
            .ToLookup(x => x.position, x => x.clue);
    }

    public ILookup<Position, ICompletenessClue<T>> CluesByPosition { get; }
        
    public IEnumerable<ICellChangeResult> CalculateUpdates(Grid<T> grid,
        Maybe<IReadOnlyCollection<Position>> positionsToLookAt)
    {
        var clues =
            positionsToLookAt.HasValue?
                positionsToLookAt.Value.SelectMany(x => CluesByPosition[x]).Distinct() :
                Clues;

        return clues.SelectMany(x => UpdateCells(x, grid));
    }


    private static IEnumerable<ICellChangeResult> UpdateCells(ICompletenessClue<T> completenessClue, Grid<T> grid)
    {
        var positions = completenessClue.Positions;

        var myCells = completenessClue.Positions.Select(grid.GetCellKVP).ToList();

        //TODO look at efficiency


        var totalUnfixedCells = myCells.Count(x => x.Value.PossibleValues.Count != 1);
        if (totalUnfixedCells == 0) //nothing to do
            yield break;

        var cellsByValue =
            grid.ClueSource.ValueSource.AllValues.ToDictionary(v => v, v =>
                myCells.Where(x => x.Value.PossibleValues.Contains(v)).ToList() as
                    IReadOnlyCollection<KeyValuePair<Position, Cell<T>>>);

        foreach (var (v, cellsContainingV) in cellsByValue)
        {
            var fixedCells = cellsContainingV.Count(x => x.Value.PossibleValues.Count == 1);

            if (fixedCells > 1)
            {
                yield return new Contradiction(new AlreadyExistsReason<T>(v, completenessClue), cellsContainingV.Select(x=>x.Key).ToImmutableArray());
            }

            else if (fixedCells == 1)
            {
                if (cellsContainingV.Count <= 1) continue; //Nothing to do here

                foreach (var cp in cellsContainingV.Where(x => x.Value.PossibleValues.Count != 1))
                    yield return cp.CloneWithoutValue(v,  new AlreadyExistsReason<T>(v, completenessClue));
            }
            else
            {
                switch (cellsContainingV.Count)
                {
                    case 1:
                        yield return cellsContainingV.Single()
                            .CloneWithOnlyValue(v, new MustExistsReason<T>(v, completenessClue));
                        break;
                    case 0:
                        yield return new Contradiction(new MustExistsReason<T>(v, completenessClue), positions);
                        break;
                    default:
                    {
                        //Possibility Storm
                        if (cellsContainingV.Count <= 3)
                        {
                            var possiblePositions = cellsContainingV.Select(x => x.Key).ToImmutableSortedSet();

                            //Find positions which cannot have this value because at least one cell in this group must have the value
                            var excludedPositions = grid.ClueSource.UniquenessClueHelper
                                .GetCommunicatingPositions(possiblePositions)
                                .Where(x => !positions.Contains(x));

                            foreach (var cell in excludedPositions.Select(grid.GetCellKVP)
                                         .Where(x => x.Value.PossibleValues.Contains(v)))
                            {
                                yield return cell.CloneWithoutValue(v,
                                    new PossibilityStormReason<T>(v, completenessClue));
                            }
                        }

                        break;
                    }
                }
            }
        }

        //Hidden figures
        {
            var possibleHiddenGroups = cellsByValue.GroupBy(x => x.Value.Count, x => x.Key)
                .Where(x => x.Key > 1 && x.Key <= totalUnfixedCells / 2).OrderBy(x => x.Key);

            var restrictedValues = new HashSet<T>();

            foreach (var possibleHiddenGroup in possibleHiddenGroups)
            {
                var groupSize = possibleHiddenGroup.Key;

                restrictedValues.UnionWith(possibleHiddenGroup);

                if (groupSize >= restrictedValues.Count)
                {
                    foreach (var n in possibleHiddenGroup)
                    {
                        var containingCells = cellsByValue[n];
                        var matchingRestrictions = restrictedValues.Where(x => !n.Equals(x))
                            .Where(x =>
                                cellsByValue[x].All(y => containingCells.Any(z => y.Key == z.Key)))
                            .ToList();

                        matchingRestrictions.Add(n);

                        if (matchingRestrictions.Count > groupSize)
                        {
                            yield return new Contradiction(
                                new HiddenGroupReason<T>(matchingRestrictions, completenessClue),
                                containingCells.Select(x => x.Key).ToImmutableArray()
                            );
                        }
                        else if (matchingRestrictions.Count == groupSize)
                        {
                            //we've struck gold
                            restrictedValues.ExceptWith(
                                matchingRestrictions); //no need to check these again //TODO think about this?

                            foreach (var containingCell in containingCells)
                                yield return containingCell.CloneWithOnlyValues(matchingRestrictions,
                                    new HiddenGroupReason<T>(matchingRestrictions, completenessClue));
                        }
                    }
                }
            }
        }

        //Permutations
        {
            var cellsByNumberOfPossibilities = myCells
                .Where(x => x.Value.PossibleValues.Count != 1)
                .GroupBy(x => x.Value.PossibleValues.Count)
                .OrderBy(x => x.Key).ToList();


            var maxPermutationSizeToTry =
                Math.Min(totalUnfixedCells - 1, totalUnfixedCells / 2);

            for (var permutationSize = cellsByNumberOfPossibilities.First().Key;
                 permutationSize <= maxPermutationSizeToTry;
                 permutationSize++)
            {
                var groupsThisSizeOrLess =
                    cellsByNumberOfPossibilities.TakeWhile(x => x.Key <= permutationSize).SelectMany(x => x)
                        .ToList();

                if (groupsThisSizeOrLess.Count >= permutationSize)
                {
                    foreach (var (_, value) in groupsThisSizeOrLess
                                 .Where(x => x.Value.PossibleValues.Count == permutationSize)
                                 .GroupBy(x => x.Value.ToString()).Select(x => x.First())
                            )
                    {
                        var subGroups = groupsThisSizeOrLess
                            .Where(x => x.Value.PossibleValues.IsSupersetOf(value.PossibleValues)).ToList();

                        if (subGroups.Count > permutationSize)
                        {
                            yield return new Contradiction(
                                new PermutationReason<T>(value.PossibleValues, completenessClue),
                                subGroups.Select(x => x.Key).ToImmutableArray()
                            );
                        }
                        else if (subGroups.Count == permutationSize)
                        {
                            //we've found a subgroup
                            var subGroupPositions = subGroups.Select(x => x.Key).ToHashSet();

                            var cellsToChange = myCells
                                .Where(x =>
                                    !subGroupPositions.Contains(x.Key) &&
                                    x.Value.PossibleValues.Overlaps(value.PossibleValues)).ToList();

                            foreach (var r in cellsToChange
                                         .Select(cellToChange => cellToChange.CloneWithoutValues(value.PossibleValues,
                                             new PermutationReason<T>(value.PossibleValues, completenessClue)
                                         )))
                                yield return r;
                        }

                        if (groupsThisSizeOrLess.Count == permutationSize)
                            break; //This was the only possible group
                    }
                }
            }
        }
    }
}
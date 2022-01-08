using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using GridPuzzles.Clues;

namespace GridPuzzles;

public static class GridHelper
{
    /// <summary>
    /// Applies the update and then iterates repeatedly
    /// </summary>
    [Pure]
    public static (Grid<T> grid, UpdateResult<T> updateResult) IterateRepeatedly<T>(this Grid<T> grid,
        UpdateResultCombiner<T> combiner,
        int bifurcationDepth,
        UpdateResult<T>? latestUpdate = null) where T: notnull
    {
        var updateSoFar = latestUpdate ?? UpdateResult<T>.Empty;
        var gridSoFar = grid.CloneWithUpdates(updateSoFar, false);


        var initialPositions =
            updateSoFar.UpdatedPositions.Any() ? updateSoFar.UpdatedPositions : grid.AllPositions;

        var helpers = new List<(IClueUpdateHelper<T> clueHelper, HashSet<Position> remainingPositions)>()
        {
            // ReSharper disable PossibleMultipleEnumeration
            (grid.ClueSource.UniquenessClueHelper, initialPositions.ToHashSet()),
            (grid.ClueSource.CompletenessClueHelper, initialPositions.ToHashSet()),

            (grid.ClueSource.RelationshipClueHelper,initialPositions.ToHashSet()),
            (grid.ClueSource.RuleClueHelper, initialPositions.ToHashSet()),
            (grid.ClueSource.MetaRuleClueHelper, initialPositions.ToHashSet()),
            // ReSharper restore PossibleMultipleEnumeration
        };

        var index = 0;

        while (index < helpers.Count)
        {
            var (clueHelper, remainingPositions) = helpers[index];

            var positionsToCheck = remainingPositions.Any()
                ? remainingPositions
                : Maybe<IReadOnlySet<Position>>.None;
            var newUpdate = combiner.Combine(clueHelper.CalculateUpdates(gridSoFar, bifurcationDepth, positionsToCheck));

            updateSoFar = updateSoFar.Combine(newUpdate, out var hasChanges);
            remainingPositions.Clear();


            if (hasChanges)
            {
                gridSoFar = gridSoFar.CloneWithUpdates(newUpdate, false);
                if (newUpdate.HasContradictions)
                    return (gridSoFar, updateSoFar);
                foreach (var (_, hashSet) in helpers)
                    hashSet.UnionWith(newUpdate.UpdatedPositions);
                index = 0;
            }
            else
                index++;
        }

        return (gridSoFar, updateSoFar);
    }


    private static UpdateResult<T> CalculateUpdateResult<T>(this Grid<T> grid,
        UpdateResultCombiner<T> combiner,
        int bifurcationDepth,
        Maybe<IReadOnlySet<Position>> positions)  where T: notnull
    {
            
        var changes =
                grid.ClueSource.UniquenessClueHelper.CalculateUpdates(grid, bifurcationDepth, positions).Concat(
                    grid.ClueSource.CompletenessClueHelper.CalculateUpdates(grid,bifurcationDepth, positions)).Concat(
                    grid.ClueSource.RelationshipClueHelper.CalculateUpdates(grid,bifurcationDepth, positions)).Concat(
                    grid.ClueSource.RuleClueHelper.CalculateUpdates(grid,bifurcationDepth, positions)).Concat(
                    grid.ClueSource.MetaRuleClueHelper.CalculateUpdates(grid,bifurcationDepth, positions))
            ;

        return combiner.Combine(changes);
    }


    /// <summary>
    /// Go to the next iteration.
    /// Returns null if we can't iterate further.
    /// </summary>
    /// <returns></returns>
    [Pure]
    public static (Grid<T> grid, UpdateResult<T> updateResult) Iterate<T>(this Grid<T> grid,
        UpdateResultCombiner<T> combiner,
        int bifurcationDepth,
        Maybe<IReadOnlySet<Position>> positionsToUpdate) where T: notnull
    {
        var updateResult = CalculateUpdateResult(grid, combiner, bifurcationDepth, positionsToUpdate);

        var newGrid = grid.CloneWithUpdates(updateResult, false);

        return (newGrid, updateResult);
    }


    [Pure]
    public static async Task<(Grid<T> grid, UpdateResult<T> updateResult)> IterateAsync<T>(
        this Grid<T> grid,
        UpdateResultCombiner<T> combiner,
        int bifurcationDepth,
        Maybe<IReadOnlySet<Position>> positionsToUpdate) where T: notnull
    {
        var r = await Task.Run(() => grid.Iterate(combiner, bifurcationDepth, positionsToUpdate)).ConfigureAwait(false);

        return r;
    }

        
}
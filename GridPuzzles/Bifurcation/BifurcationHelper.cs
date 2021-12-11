using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GridPuzzles.Bifurcation;

public record BifurcationResult<T>(UpdateResult<T> UpdateResult, IReadOnlyCollection<Grid<T>>? CompletedGrids,
    int Depth)
    where T : notnull;

public static class BifurcationHelper
{
    public static BifurcationResult<T> Bifurcate<T>(this Grid<T> startGrid, int maxDepth,
        bool returnFirstUpdate, UpdateResultCombiner<T> updateResultCombiner) where T : notnull
    {
        var node = BifurcationNode<T>.CreateTopLevel(startGrid);

        while (node.LevelsDescended < maxDepth && !node.IsFinished)
        {
            node.Descend();

            if(returnFirstUpdate && node.UpdateResult.UpdatedCells.Any())
                break;
        }

        return updateResultCombiner.ToBifurcationResult(node);
    }

    public static async Task<BifurcationResult<T>> BifurcateAsync<T>(this Grid<T> startGrid, int maxDepth,
        bool returnFirstUpdate, UpdateResultCombiner<T> updateResultCombiner,
        CancellationToken cancellationToken
    ) where T : notnull =>
        await Task.Run(() => Bifurcate(startGrid, maxDepth, returnFirstUpdate, updateResultCombiner), cancellationToken)
            .ConfigureAwait(false);
}
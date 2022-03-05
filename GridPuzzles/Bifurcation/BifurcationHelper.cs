using System.Threading;
using System.Threading.Tasks;

namespace GridPuzzles.Bifurcation;

public record BifurcationResult<T, TCell>(UpdateResult<T, TCell> UpdateResult, IReadOnlyCollection<Grid<T, TCell>>? CompletedGrids,
    int Depth)
    where T :struct where TCell : ICell<T, TCell>, new();

public static class BifurcationHelper
{
    public static BifurcationResult<T, TCell> Bifurcate<T, TCell>(this Grid<T, TCell> startGrid, int maxDepth,
        bool returnFirstUpdate, UpdateResultCombiner<T, TCell> updateResultCombiner) where T :struct where TCell : ICell<T, TCell>, new()
    {
        var node = BifurcationNode<T, TCell>.CreateTopLevel(startGrid);

        while (node.LevelsDescended < maxDepth && !node.IsFinished)
        {
            node.Descend();

            if(returnFirstUpdate && node.UpdateResult.UpdatedCells.Any())
                break;
        }

        return updateResultCombiner.ToBifurcationResult(node);
    }

    public static async Task<BifurcationResult<T, TCell>> BifurcateAsync<T, TCell>(this Grid<T, TCell> startGrid, int maxDepth,
        bool returnFirstUpdate, UpdateResultCombiner<T, TCell> updateResultCombiner,
        CancellationToken cancellationToken
    ) where T :struct where TCell : ICell<T, TCell>, new() =>
        await Task.Run(() => Bifurcate(startGrid, maxDepth, returnFirstUpdate, updateResultCombiner), cancellationToken)
            .ConfigureAwait(false);
}
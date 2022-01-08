using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using GridPuzzles.Bifurcation;

namespace GridPuzzles.Session.Actions;

public class NextGridAction<T> : IGridViewAction<T> where T: notnull
{
    public static NextGridAction<T> Instance = new();

    private NextGridAction()
    {

    }

    /// <inheritdoc />
    public string Name => "Next";

    /// <inheritdoc />
    public async IAsyncEnumerable<ActionResult<T>> Execute(ImmutableStack<SolveState<T>> history,
        SessionSettings settings,[EnumeratorCancellation] CancellationToken cancellation)
    {
        if(cancellation.IsCancellationRequested)yield break;

        var currentState = history.Peek();
        var combiner = settings.SingleStep ? UpdateResultCombiner<T>.SingleStep : UpdateResultCombiner<T>.Default;
        var updatePositions = settings.SingleStep ?
            Maybe<IReadOnlySet<Position>>.None : 
            currentState.UpdateResult.UpdatedPositions.Any()?
                Maybe<IReadOnlySet<Position>>.From(currentState.UpdateResult.UpdatedPositions.ToHashSet()) : 
                Maybe<IReadOnlySet<Position>>.None;

        Stopwatch sw = Stopwatch.StartNew();

        var (newGrid, updateResult) = await currentState.Grid.IterateAsync(combiner,0, updatePositions);
        sw.Stop();
        if (updateResult.IsNotEmpty)
            yield return (ActionResult<T>)new SolveState<T>(newGrid,
                currentState.VariantBuilders,
                updateResult,
                ChangeType.LogicalMove,
                updateResult.Message,
                sw.Elapsed,
                currentState.FixedValues,
                currentState.Grid);
        else if (newGrid.IsComplete)
        {
            yield return (ActionResult<T>)new SolveState<T>(newGrid,
                currentState.VariantBuilders,
                updateResult,
                ChangeType.NoChange,
                "Grid is Complete", 
                sw.Elapsed, currentState.FixedValues, currentState.Grid);
        }
        else
        {
            if (settings.BifurcateDepth > 0)
            {
                if(cancellation.IsCancellationRequested)yield break;
                sw.Start();
                var br = await currentState.Grid.BifurcateAsync(
                    settings.BifurcateDepth, 
                    true,
                    combiner, cancellation);
                sw.Stop();
                if (br.UpdateResult.IsNotEmpty)
                {
                    var grid2 = currentState.Grid.CloneWithUpdates(br.UpdateResult, false);
                    yield return (ActionResult<T>)new SolveState<T>(grid2,
                        currentState.VariantBuilders, 
                        br.UpdateResult,
                        ChangeType.LogicalMove, 
                        br.UpdateResult.Message + $"(Depth {br.Depth})",
                        sw.Elapsed,
                        currentState.FixedValues,
                        currentState.Grid);
                    yield break;
                }
                else if (br.CompletedGrids is not null)
                {
                    var grid2 = currentState.Grid.CloneWithUpdates(br.UpdateResult, false);
                    yield return (ActionResult<T>)new SolveState<T>(grid2,currentState.VariantBuilders,
                        UpdateResult<T>.Empty, 
                        ChangeType.NoChange, 
                        $"At least {br.CompletedGrids.Count} possible solutions", 
                        sw.Elapsed, 
                        currentState.FixedValues,
                        null);
                    yield break;
                }

            }

            yield return new ActionResult<T>.NoChangeResult(sw.Elapsed);
        }
    }
}
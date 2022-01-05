using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GridPuzzles.Bifurcation;
using MoreLinq;

namespace GridPuzzles.Session.Actions;

public class RandomSolveAction<T> : IGridViewAction<T> where T :notnull
{
    private RandomSolveAction()
    {
    }

    public static IGridViewAction<T> Instance { get; } = new RandomSolveAction<T>();

    /// <inheritdoc />
    public string Name => "Random Solve";

    /// <inheritdoc />
    public IAsyncEnumerable<ActionResult<T>> Execute(ImmutableStack<SolveState<T>> history,
        SessionSettings settings, CancellationToken cancellation)
    {
        var originalState = history.Peek();
        var sw = Stopwatch.StartNew();


        var results =
            originalState.Grid
                .RandomSolveIncremental(null, cancellation)
                .Prepend((originalState.Grid, UpdateResult<T>.Empty))
                .Pairwise((previous, current) =>
                    CreateActionResult(
                        current.Grid, 
                        current.Item2,
                        originalState, 
                        previous.Grid, sw))
                .ToAsyncEnumerable();

                


        //await foreach (var incrementalGrid in currentState.Grid.RandomSolveIncremental(null).WithCancellation(cancellation))
        //{
        //    var solveState = new SolveState<T>(incrementalGrid,
        //        currentState.VariantBuilders,
        //        UpdateResult<T>.Empty,
        //        ChangeType.RandomMove,
        //        "Random Solve",
        //        sw.Elapsed,
        //        currentState.FixedValues,
        //        currentState.Grid);
        //    yield return (ActionResult<T>)solveState;
        //}
        //sw.Stop();


        if (settings.MaxFinalInterval == TimeSpan.Zero)
            return results;

        return results.Zip(MyTimer(settings), (x,_)=>x);
    }

    static async IAsyncEnumerable<int> MyTimer(SessionSettings settings)
    {
        var i = 0;
        while (true)
        {
            yield return i++;
            await Task.Delay(settings.MaxFinalInterval);
        }
    }

    private static ActionResult<T> CreateActionResult(Grid<T> latestGrid, UpdateResult<T> updateResult, SolveState<T> originalState, Grid<T> previousGrid, Stopwatch sw)
    {
        var solveState = new SolveState<T>(latestGrid,
            originalState.VariantBuilders,
            updateResult,
            ChangeType.RandomMove,
            updateResult.Message,
            sw.Elapsed,
            originalState.FixedValues,
            previousGrid);
        sw.Reset();
        return(ActionResult<T>) solveState;
    }
}
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GridPuzzles.Bifurcation;
using MoreLinq;

namespace GridPuzzles.Session.Actions;

public class RandomSolveAction<T, TCell> : IGridViewAction<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    private RandomSolveAction()
    {
    }

    public static IGridViewAction<T, TCell> Instance { get; } = new RandomSolveAction<T, TCell>();

    /// <inheritdoc />
    public string Name => "Random Solve";

    /// <inheritdoc />
    public IAsyncEnumerable<ActionResult<T, TCell>> Execute(ImmutableStack<SolveState<T, TCell>> history,
        SessionSettings settings, CancellationToken cancellation)
    {
        var originalState = history.Peek();
        var sw = Stopwatch.StartNew();


        var results =
            originalState.Grid
                .RandomSolveIncremental(null, cancellation)
                .Prepend((originalState.Grid, UpdateResult<T, TCell>.Empty))
                .Pairwise((previous, current) =>
                    CreateActionResult(
                        current.Grid, 
                        current.Item2,
                        originalState, 
                        previous.Grid, sw))
                .ToAsyncEnumerable();

                


        //await foreach (var incrementalGrid in currentState.Grid.RandomSolveIncremental(null).WithCancellation(cancellation))
        //{
        //    var solveState = new SolveState<T, TCell>(incrementalGrid,
        //        currentState.VariantBuilders,
        //        UpdateResult<T, TCell>.Empty,
        //        ChangeType.RandomMove,
        //        "Random Solve",
        //        sw.Elapsed,
        //        currentState.FixedValues,
        //        currentState.Grid);
        //    yield return (ActionResult<T, TCell>)solveState;
        //}
        //sw.Stop();


        if (settings.MaxFinalInterval == TimeSpan.Zero)
            return results;

        return results.Zip(MyTimer(settings), (x,_)=>x);
    }

    private static async IAsyncEnumerable<int> MyTimer(SessionSettings settings)
    {
        var i = 0;
        while (true)
        {
            yield return i++;
            await Task.Delay(settings.MaxFinalInterval);
        }
    }

    private static ActionResult<T, TCell> CreateActionResult(Grid<T, TCell> latestGrid, UpdateResult<T, TCell> updateResult, SolveState<T, TCell> originalState, Grid<T, TCell> previousGrid, Stopwatch sw)
    {
        var solveState = new SolveState<T, TCell>(latestGrid,
            originalState.VariantBuilders,
            updateResult,
            ChangeType.RandomMove,
            updateResult.Message,
            sw.Elapsed,
            originalState.FixedValues,
            previousGrid);
        sw.Reset();
        return(ActionResult<T, TCell>) solveState;
    }
}
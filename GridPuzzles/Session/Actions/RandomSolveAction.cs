using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using GridPuzzles.Bifurcation;

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
    public async IAsyncEnumerable<ActionResult<T>> Execute(ImmutableStack<SolveState<T>> history,
        SessionSettings settings, [EnumeratorCancellation] CancellationToken cancellation)
    {
        var currentState = history.Peek();
        var sw = Stopwatch.StartNew();
        var grid = await currentState.Grid.RandomSolveAsync();
        sw.Stop();
        if (grid.HasValue) //TODO give incremental results
        {
            var solveState = new SolveState<T>(grid.Value,
                currentState.VariantBuilders,
                UpdateResult<T>.Empty,
                ChangeType.RandomMove,
                "Random Solve",
                sw.Elapsed,
                currentState.FixedValues,
                currentState.Grid);
            yield return (ActionResult<T>)solveState;
        }
    }
}
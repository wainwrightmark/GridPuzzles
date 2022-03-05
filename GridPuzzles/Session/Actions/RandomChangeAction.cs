using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using GridPuzzles.Bifurcation;

namespace GridPuzzles.Session.Actions;

public class RandomChangeAction<T, TCell> : IGridViewAction<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    public static RandomChangeAction<T, TCell> Instance = new();

    private RandomChangeAction()
    {
    }

    /// <inheritdoc />
    public string Name => "Random Change";

    /// <inheritdoc />
    public async IAsyncEnumerable<ActionResult<T, TCell>> Execute(ImmutableStack<SolveState<T, TCell>> history,
        SessionSettings settings, [EnumeratorCancellation] CancellationToken cancellation)
    {
        var currentState = history.Peek();
        var sw = Stopwatch.StartNew();
        var (grid, updateResult) = await currentState.Grid.RandomIterateAsync();

        if (updateResult.IsNotEmpty)
            yield return (ActionResult<T, TCell>)new SolveState<T, TCell>(grid,
                currentState.VariantBuilders,
                updateResult, ChangeType.RandomMove, updateResult.Message, sw.Elapsed,
                currentState.FixedValues,
                currentState.Grid);
    }
}
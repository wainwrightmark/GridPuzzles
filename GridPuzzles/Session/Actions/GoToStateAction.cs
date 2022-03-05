using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace GridPuzzles.Session.Actions;

public record GoToStateAction<T, TCell>(SolveState<T, TCell> SolveState)
    : IGridViewAction<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    /// <inheritdoc />
    public string Name => $"Go To '{SolveState.Message}'";

    /// <inheritdoc />
    public async IAsyncEnumerable<ActionResult<T, TCell>> Execute(ImmutableStack<SolveState<T, TCell>> history,
        SessionSettings settings, [EnumeratorCancellation] CancellationToken cancellation)
    {
        await Task.CompletedTask;
        if (history.Peek().ChangeType == ChangeType.GoToState)
        {
            history = history.Pop();
        }
        history = history.Push(SolveState with{ChangeType = ChangeType.GoToState});

        yield return new ActionResult<T, TCell>.ChangeHistoryResult(history);
    }
}
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace GridPuzzles.Session.Actions;

public record GoToStateAction<T>(SolveState<T> SolveState) : IGridViewAction<T> where T :notnull
{
    /// <inheritdoc />
    public string Name => $"Go To '{SolveState.Message}'";

    /// <inheritdoc />
    public async IAsyncEnumerable<ActionResult<T>> Execute(ImmutableStack<SolveState<T>> history,
        SessionSettings settings, [EnumeratorCancellation] CancellationToken cancellation)
    {
        await Task.CompletedTask;
        if (history.Peek().ChangeType == ChangeType.GoToState)
        {
            history = history.Pop();
        }
        history = history.Push(SolveState with{ChangeType = ChangeType.GoToState});

        yield return new ActionResult<T>.ChangeHistoryResult(history);
    }
}
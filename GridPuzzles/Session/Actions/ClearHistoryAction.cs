using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace GridPuzzles.Session.Actions;

public record ClearHistoryAction<T> : IGridViewAction<T>  where T:notnull
{
    private ClearHistoryAction() { }

    public static ClearHistoryAction<T> Instance { get; } = new ();

    /// <inheritdoc />
    public string Name => "Clear History";

    /// <inheritdoc />
    public async IAsyncEnumerable<ActionResult<T>> Execute(ImmutableStack<SolveState<T>> history,
        SessionSettings settings,[EnumeratorCancellation] CancellationToken cancellation)
    {
        await Task.CompletedTask;
        var top = history.Peek();

        top = top with
        {
            UpdateResult = UpdateResult<T>.Empty, ChangeType = ChangeType.InitialState,
            Duration = TimeSpan.Zero, Message = "History Cleared"
        };

        yield return new ActionResult<T>.ChangeHistoryResult(ImmutableStack<SolveState<T>>.Empty.Push(top));
    }
}
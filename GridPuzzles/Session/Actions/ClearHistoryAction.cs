using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace GridPuzzles.Session.Actions;

public record ClearHistoryAction<T, TCell> : IGridViewAction<T, TCell>  where T :struct where TCell : ICell<T, TCell>, new()
{
    private ClearHistoryAction() { }

    public static ClearHistoryAction<T, TCell> Instance { get; } = new ();

    /// <inheritdoc />
    public string Name => "Clear History";

    /// <inheritdoc />
    public async IAsyncEnumerable<ActionResult<T, TCell>> Execute(ImmutableStack<SolveState<T, TCell>> history,
        SessionSettings settings,[EnumeratorCancellation] CancellationToken cancellation)
    {
        await Task.CompletedTask;
        var top = history.Peek();

        top = top with
        {
            UpdateResult = UpdateResult<T, TCell>.Empty, ChangeType = ChangeType.InitialState,
            Duration = TimeSpan.Zero, Message = "History Cleared"
        };

        yield return new ActionResult<T, TCell>.ChangeHistoryResult(ImmutableStack<SolveState<T, TCell>>.Empty.Push(top));
    }
}
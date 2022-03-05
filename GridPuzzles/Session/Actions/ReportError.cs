using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace GridPuzzles.Session.Actions;

public record ReportError<T, TCell>(string Message, TimeSpan Duration) : IGridViewAction<T, TCell>
    where T :struct where TCell : ICell<T, TCell>, new()
{
    /// <inheritdoc />
    public string Name => $"Report Error {Message}";

    /// <inheritdoc />
    public async IAsyncEnumerable<ActionResult<T, TCell>> Execute(ImmutableStack<SolveState<T, TCell>> history,
        SessionSettings settings, [EnumeratorCancellation] CancellationToken cancellation)
    {
        await Task.CompletedTask;
        yield return new ActionResult<T, TCell>.ErrorResult(Message, Duration);
    }
}
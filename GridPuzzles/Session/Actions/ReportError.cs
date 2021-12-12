using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace GridPuzzles.Session.Actions;

public record ReportError<T>(string Message, TimeSpan Duration) : IGridViewAction<T> where T : notnull
{
    /// <inheritdoc />
    public string Name => $"Report Error {Message}";

    /// <inheritdoc />
    public async IAsyncEnumerable<ActionResult<T>> Execute(ImmutableStack<SolveState<T>> history,
        SessionSettings settings, [EnumeratorCancellation] CancellationToken cancellation)
    {
        await Task.CompletedTask;
        yield return new ActionResult<T>.ErrorResult(Message, Duration);
    }
}
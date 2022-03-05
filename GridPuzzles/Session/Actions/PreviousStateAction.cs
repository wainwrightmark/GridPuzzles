using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace GridPuzzles.Session.Actions;

public record PreviousStateAction<T, TCell> : IGridViewAction<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    private PreviousStateAction() { }

    public static PreviousStateAction<T, TCell> Instance { get; } = new();

    /// <inheritdoc />
    public string Name => "Previous";

    /// <inheritdoc />
    public async IAsyncEnumerable<ActionResult<T, TCell>> Execute(ImmutableStack<SolveState<T, TCell>> history,
        SessionSettings settings, [EnumeratorCancellation] CancellationToken cancellation)
    {
        await Task.CompletedTask;
        while (!history.IsEmpty)
        {
            if(history.Peek().ChangeType is ChangeType.ManualChange or ChangeType.VariantChange or ChangeType.InitialState)
                yield break;

            history = history.Pop(out var ss);
            if(ss.ChangeType is ChangeType.LogicalMove or ChangeType.RandomMove)
            {
                yield return new ActionResult<T, TCell>.ChangeHistoryResult(history);
                yield break;
            }
        }
    }
}
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace GridPuzzles.Session.Actions;

public record PreviousStateAction<T> : IGridViewAction<T> where T: notnull
{
    private PreviousStateAction() { }

    public static PreviousStateAction<T> Instance { get; } = new();

    /// <inheritdoc />
    public string Name => "Previous";

    /// <inheritdoc />
    public async IAsyncEnumerable<ActionResult<T>> Execute(ImmutableStack<SolveState<T>> history,
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
                yield return new ActionResult<T>.ChangeHistoryResult(history);
                yield break;
            }
        }
    }
}
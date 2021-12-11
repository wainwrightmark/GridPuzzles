using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace GridPuzzles.Session.Actions;

public class UndoLastManualChangeAction<T> : IGridViewAction<T> where T :notnull
{
    private UndoLastManualChangeAction()
    {
    }

    public static IGridViewAction<T> Instance { get; } = new UndoLastManualChangeAction<T>();
    public string Name => "Undo";

    /// <inheritdoc />
    public async IAsyncEnumerable<ActionResult<T>> Execute(ImmutableStack<SolveState<T>> history,
        SessionSettings settings, [EnumeratorCancellation] CancellationToken cancellation)
    {
        await Task.CompletedTask;
        while (!history.IsEmpty)
        {
            history = history.Pop(out var state);
            if (state.ChangeType is ChangeType.ManualChange or ChangeType.VariantChange or ChangeType.RandomMove)
            {
                var newState = history.Peek() with
                {
                    ChangeType = ChangeType.Undo, Message = $"Undid {state.Message}", Duration = TimeSpan.Zero,
                    UpdateResult = UpdateResult<T>.Empty
                };
                history = history.Push(newState);

                yield return new ActionResult<T>.ChangeHistoryResult(history);

                yield break;
            }
        }
    }
}
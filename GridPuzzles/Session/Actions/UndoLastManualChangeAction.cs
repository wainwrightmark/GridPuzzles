using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace GridPuzzles.Session.Actions;

public class UndoLastManualChangeAction<T, TCell> : IGridViewAction<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    private UndoLastManualChangeAction()
    {
    }

    public static IGridViewAction<T, TCell> Instance { get; } = new UndoLastManualChangeAction<T, TCell>();
    public string Name => "Undo";

    /// <inheritdoc />
    public async IAsyncEnumerable<ActionResult<T, TCell>> Execute(ImmutableStack<SolveState<T, TCell>> history,
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
                    UpdateResult = UpdateResult<T, TCell>.Empty
                };
                history = history.Push(newState);

                yield return new ActionResult<T, TCell>.ChangeHistoryResult(history);

                yield break;
            }
        }
    }
}
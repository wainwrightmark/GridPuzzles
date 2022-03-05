using System.Threading;

namespace GridPuzzles.Session.Actions;

public interface IGridViewAction<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    string Name { get; }
    IAsyncEnumerable<ActionResult<T, TCell>> Execute(ImmutableStack<SolveState<T, TCell>> history, SessionSettings settings,
        CancellationToken cancellation);
}
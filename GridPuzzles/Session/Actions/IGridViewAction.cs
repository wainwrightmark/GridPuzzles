using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace GridPuzzles.Session.Actions;

public interface IGridViewAction<T> where T:notnull
{
    string Name { get; }
    IAsyncEnumerable<ActionResult<T>> Execute(ImmutableStack<SolveState<T>> history, SessionSettings settings,
        CancellationToken cancellation);
}
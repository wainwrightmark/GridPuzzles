using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GridPuzzles.Cells;

namespace GridPuzzles.Session.Actions;

public record ClearAction<T> : IGridViewAction<T> where T:notnull
{
    private ClearAction() { }

    public static ClearAction<T> Instance { get; } = new();

    /// <inheritdoc />
    public string Name => "Clear Cell Values";

    /// <inheritdoc />
    public async IAsyncEnumerable<ActionResult<T>> Execute(ImmutableStack<SolveState<T>> history,
        SessionSettings settings, [EnumeratorCancellation] CancellationToken cancellation)
    {
        await Task.CompletedTask;
        var state = history.Peek();

        var newGrid = Grid<T>.Create(
            state.FixedValues.Select(x=> new KeyValuePair<Position, Cell<T>>(x.Key, new Cell<T>(ImmutableSortedSet<T>.Empty.Add(x.Value)))),
            state.Grid.MaxPosition, state.Grid.ClueSource);

        yield return (ActionResult<T>)new SolveState<T>(newGrid,
            state.VariantBuilders,
            UpdateResult<T>.Empty,
            ChangeType.ManualChange,
            "Cell Values Cleared", TimeSpan.Zero, 
            state.FixedValues,
            null);
    }
}
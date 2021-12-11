using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using GridPuzzles.Clues;

namespace GridPuzzles.Session.Actions;

public record ClearAllAction<T> : IGridViewAction<T> where T : notnull
{
    private ClearAllAction()
    {
    }

    public static ClearAllAction<T> Instance { get; } = new ClearAllAction<T>();

    /// <inheritdoc />
    public string Name => "Clear All";

    /// <inheritdoc />
    public async IAsyncEnumerable<ActionResult<T>> Execute(ImmutableStack<SolveState<T>> history,
        SessionSettings settings, [EnumeratorCancellation] CancellationToken cancellation)
    {
        var state = history.Peek();
        var variants = state.VariantBuilders
            .Where(x => x.VariantBuilder.DefaultArguments is not null).ToImmutableHashSet();

        var sw = Stopwatch.StartNew();

        var newClueSource = await ClueSource<T>.TryCreateAsync(variants, state.Grid.MaxPosition,
            state.Grid.ClueSource.ValueSource, cancellation);

        if (newClueSource.IsFailure)
        {
            yield return new ActionResult<T>.ErrorResult(newClueSource.Error, sw.Elapsed);
            yield break;
        }


        var grid = Grid<T>.Create(null, state.Grid.MaxPosition, newClueSource.Value);
        var newState = new SolveState<T>(grid, variants, UpdateResult<T>.Empty, ChangeType.InitialState,
            "All Cleared", sw.Elapsed, ImmutableSortedDictionary<Position, T>.Empty, null);


        yield return new ActionResult<T>.ChangeHistoryResult(ImmutableStack<SolveState<T>>.Empty.Push(newState));
    }
}
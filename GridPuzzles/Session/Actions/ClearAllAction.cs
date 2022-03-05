using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using GridPuzzles.Clues;

namespace GridPuzzles.Session.Actions;

public record ClearAllAction<T, TCell> : IGridViewAction<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    private ClearAllAction()
    {
    }

    public static ClearAllAction<T, TCell> Instance { get; } = new ClearAllAction<T, TCell>();

    /// <inheritdoc />
    public string Name => "Clear All";

    /// <inheritdoc />
    public async IAsyncEnumerable<ActionResult<T, TCell>> Execute(ImmutableStack<SolveState<T, TCell>> history,
        SessionSettings settings, [EnumeratorCancellation] CancellationToken cancellation)
    {
        var state = history.Peek();
        var variants = state.VariantBuilders
            .Where(x => x.VariantBuilder.DefaultArguments is not null).ToImmutableHashSet();

        var sw = Stopwatch.StartNew();

        var newClueSource = await ClueSource<T, TCell>.TryCreateAsync(variants, state.Grid.MaxPosition,
            state.Grid.ClueSource.ValueSource, cancellation);

        if (newClueSource.IsFailure)
        {
            yield return new ActionResult<T, TCell>.ErrorResult(newClueSource.Error, sw.Elapsed);
            yield break;
        }


        var grid = Grid<T, TCell>.Create(null, state.Grid.MaxPosition, newClueSource.Value);
        var newState = new SolveState<T, TCell>(grid, variants, UpdateResult<T, TCell>.Empty, ChangeType.InitialState,
            "All Cleared", sw.Elapsed, ImmutableSortedDictionary<Position, T>.Empty, null);


        yield return new ActionResult<T, TCell>.ChangeHistoryResult(ImmutableStack<SolveState<T, TCell>>.Empty.Push(newState));
    }
}
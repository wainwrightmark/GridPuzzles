using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace GridPuzzles.Session.Actions;

public record ClearAction<T, TCell> : IGridViewAction<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    private ClearAction() { }

    public static ClearAction<T, TCell> Instance { get; } = new();

    /// <inheritdoc />
    public string Name => "Clear Cell Values";

    /// <inheritdoc />
    public async IAsyncEnumerable<ActionResult<T, TCell>> Execute(ImmutableStack<SolveState<T, TCell>> history,
        SessionSettings settings, [EnumeratorCancellation] CancellationToken cancellation)
    {
        await Task.CompletedTask;
        var state = history.Peek();

        

        var newGrid = Grid<T, TCell>.Create(
            state.FixedValues.Select(x=> 
                new KeyValuePair<Position, TCell>(x.Key, state.Grid.ClueSource.ValueSource.AnyValueCell)),
            state.Grid.MaxPosition, state.Grid.ClueSource);

        yield return (ActionResult<T, TCell>)new SolveState<T, TCell>(newGrid,
            state.VariantBuilders,
            UpdateResult<T, TCell>.Empty,
            ChangeType.ManualChange,
            "Cell Values Cleared", TimeSpan.Zero, 
            state.FixedValues,
            null);
    }
}
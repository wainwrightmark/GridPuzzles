using System.Runtime.CompilerServices;
using System.Threading;

namespace GridPuzzles.Session.Actions;

public class SetCellAction<T, TCell> : IGridViewAction<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    public SetCellAction(Position position, Maybe<T> newValue)
    {
        Position = position;
        NewValue = newValue;
    }

    public Position Position { get; }

    public Maybe<T> NewValue { get; }

    /// <inheritdoc />
    public string Name => "Set " + Position + " to " +
                          (NewValue.HasValue ? NewValue.Value.ToString() ?? "Empty" : "Empty");

    /// <inheritdoc />
    public async IAsyncEnumerable<ActionResult<T, TCell>> Execute(ImmutableStack<SolveState<T, TCell>> history,
        SessionSettings settings, [EnumeratorCancellation] CancellationToken cancellation)
    {
        var currentState = history.Peek();
        var builder = UpdateResult<T, TCell>.Empty;

        Grid<T, TCell> newGrid;

        ImmutableSortedDictionary<Position, T> newFixedValues;
        if (NewValue.HasValue)
        {
            var cell = CellHelper.Create<T, TCell>(NewValue.Value);
            builder = builder.CloneWithCellUpdate(new CellUpdate<T, TCell>(cell, Position, CellManuallySetReason.Instance));
            newGrid = currentState.Grid.CloneWithUpdates(builder, true);
            newFixedValues = currentState.FixedValues.SetItem(Position, NewValue.Value);
        }
        else
        {
            newGrid = currentState.Grid.CloneWithoutCells(new[] { Position });
            newFixedValues = currentState.FixedValues.Remove(Position);
        }

        var newState = new SolveState<T, TCell>(newGrid, currentState.VariantBuilders, builder, ChangeType.ManualChange,
            $"{Position} manually set to {NewValue}", TimeSpan.Zero, newFixedValues
            ,
                
                
            null);
        yield return (ActionResult<T, TCell>)newState;

        if (settings.GoToFinalStateOnKeyPress)
        {
            await foreach (var nextState in FinalGridAction<T, TCell>.Instance.Execute(history.Push(newState), settings, cancellation))
                yield return nextState;
        }
        else
        {
            var (_, contradictionCheck) =
                newState.Grid.Iterate(UpdateResultCombiner<T, TCell>.Default, 0, Maybe<IReadOnlySet<Position>>.None);

            if (contradictionCheck.HasContradictions)
            {
                var contradictionsOnly = new UpdateResult<T, TCell>(
                    ImmutableDictionary<Position, CellUpdate<T, TCell>>.Empty, contradictionCheck.Contradictions
                );

                var contradictionState = new SolveState<T, TCell>
                (
                    newGrid, currentState.VariantBuilders, contradictionsOnly, ChangeType.LogicalMove,
                    contradictionCheck.Message, TimeSpan.Zero, newFixedValues, null
                );
                yield return (ActionResult<T, TCell>)contradictionState;
            }
        }
    }
}
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using GridPuzzles.Clues;
using GridPuzzles.VariantBuilderArguments;

namespace GridPuzzles.Session.Actions;

public record AddVariantBuilderAction<T, TCell> 
    (VariantBuilderArgumentPair<T, TCell> VariantBuilderArgumentPair) : IGridViewAction<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    public string Name => $"Add {VariantBuilderArgumentPair.AsString(true)}";

    public async IAsyncEnumerable<ActionResult<T, TCell>> Execute(ImmutableStack<SolveState<T, TCell>> history,
        SessionSettings settings, [EnumeratorCancellation] CancellationToken cancellation)
    {
        var sw = Stopwatch.StartNew();
        string message = "Added " + VariantBuilderArgumentPair.AsString(true);

        var current = history.Peek();

        if (current.VariantBuilders.Contains(VariantBuilderArgumentPair))
            yield break;

        current = current with { VariantBuilders = current.VariantBuilders.Add(VariantBuilderArgumentPair) };

        var clueSourceResult  = await ClueSource<T, TCell>.TryCreateAsync(current.VariantBuilders, current.Grid.MaxPosition, current.Grid.ClueSource.ValueSource, CancellationToken.None);
        sw.Stop();
        if (clueSourceResult.IsFailure)
        {
            yield return new ActionResult<T, TCell>.ErrorResult(clueSourceResult.Error, sw.Elapsed);
            yield break;
        }

        var newGrid = current.Grid;
        newGrid = newGrid.CloneWithClueSource(clueSourceResult.Value);
        newGrid = newGrid.CloneWithUpdates(UpdateResult<T, TCell>.Empty, true);

        current = current with { Grid = newGrid, UpdateResult = UpdateResult<T, TCell>.Empty, ChangeType = ChangeType.VariantChange, Duration = sw.Elapsed, Message = message, };

        yield return (ActionResult<T, TCell>)current;
    }
}
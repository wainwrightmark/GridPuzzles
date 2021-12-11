using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using GridPuzzles.Clues;
using GridPuzzles.VariantBuilderArguments;

namespace GridPuzzles.Session.Actions;

public record RemoveVariantBuilderAction<T>(VariantBuilderArgumentPair<T> VariantBuilderArgumentPair) : IGridViewAction<T>
    where T : notnull
{
    /// <inheritdoc />
    public string Name => $"Remove {VariantBuilderArgumentPair.AsString(true)}";

    /// <inheritdoc />
    public async IAsyncEnumerable<ActionResult<T>> Execute(ImmutableStack<SolveState<T>> history,
        SessionSettings settings, [EnumeratorCancellation] CancellationToken cancellation)
    {
        var sw = Stopwatch.StartNew();
        string message = "Removed " + VariantBuilderArgumentPair.AsString(true);

        var current = history.Peek();

        if (!current.VariantBuilders.Contains(VariantBuilderArgumentPair))
            yield break;

        current = current with { VariantBuilders = current.VariantBuilders.Remove(VariantBuilderArgumentPair) };

        var clueSourceResult  = await ClueSource<T>.TryCreateAsync(
            current.VariantBuilders, 
            current.Grid.MaxPosition, 
            current.Grid.ClueSource.ValueSource,
            CancellationToken.None);

        sw.Stop();
        if (clueSourceResult.IsFailure)
        {
            yield return new ActionResult<T>.ErrorResult(clueSourceResult.Error, sw.Elapsed);
            yield break;
        }

        var newGrid = current.Grid;
        newGrid = newGrid.CloneWithClueSource(clueSourceResult.Value);
        newGrid = newGrid.CloneWithUpdates(UpdateResult<T>.Empty, true);

        current = current with { Grid = newGrid, UpdateResult = UpdateResult<T>.Empty, ChangeType = ChangeType.VariantChange, Duration = sw.Elapsed, Message = message, };

        yield return (ActionResult<T>)current;
    }
}
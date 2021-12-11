using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using GridPuzzles.Cells;
using GridPuzzles.Clues;
using MoreLinq.Extensions;

namespace GridPuzzles.Session.Actions;

public record TransformAction<T>(int QuarterTurns, bool FlipHorizontal, bool FlipVertical) : IGridViewAction<T> where T : notnull
{
    /// <inheritdoc />
    public string Name
    {
        get
        {
            var terms = new List<string>();
            if (QuarterTurns % 4 != 0)
            {
                terms.Add($"Rotate {(QuarterTurns % 4) * 90}°");
            }
            if(FlipHorizontal) terms.Add("Flip Horizontally");
            if(FlipVertical) terms.Add("Flip Vertically");

            if (!terms.Any()) return "Empty Transform";
            return terms.ToDelimitedString(" and ");
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ActionResult<T>> Execute
        (ImmutableStack<SolveState<T>> history, SessionSettings settings,  [EnumeratorCancellation] CancellationToken cancellation)
    {
        var current = history.Peek();

        var newVariantBuilders = current.VariantBuilders
            .Select(x => x.Transform(QuarterTurns, FlipHorizontal, FlipVertical, current.Grid.MaxPosition))
            .ToImmutableHashSet();

        var sw = Stopwatch.StartNew();

        var newClueSource = await ClueSource<T>.TryCreateAsync(newVariantBuilders, current.Grid.MaxPosition,
            current.Grid.ClueSource.ValueSource, cancellation);

        if (newClueSource.IsFailure)
        {
            yield return new ActionResult<T>.ErrorResult(newClueSource.Error, sw.Elapsed);
            yield break;
        }


        var newCells = current.Grid.Cells.Select(x =>
                new KeyValuePair<Position, Cell<T>>(x.Key.Transform(QuarterTurns, FlipHorizontal, FlipVertical,
                    current.Grid.MaxPosition), x.Value))
            .ToList();

        var newFixedValues = current.FixedValues.Select(x =>
            new KeyValuePair<Position, T>(x.Key.Transform(QuarterTurns, FlipHorizontal, FlipVertical,
                current.Grid.MaxPosition), x.Value)).ToImmutableSortedDictionary();

        var newGrid = Grid<T>.Create(newCells, current.Grid.MaxPosition, newClueSource.Value);
        var newSolveState = new SolveState<T>(
            newGrid,
            newVariantBuilders,
            UpdateResult<T>.Empty, ChangeType.ManualChange,
            Name,
            sw.Elapsed,
            newFixedValues,
            null);

        yield return (ActionResult<T>)newSolveState;

    }
}
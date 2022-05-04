using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using GridPuzzles.Clues;
using MoreLinq.Extensions;

namespace GridPuzzles.Session.Actions;

public record TransformAction<T, TCell>(int QuarterTurns, bool FlipHorizontal, bool FlipVertical) : IGridViewAction<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
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
    public async IAsyncEnumerable<ActionResult<T, TCell>> Execute
        (ImmutableStack<SolveState<T, TCell>> history, SessionSettings settings,  [EnumeratorCancellation] CancellationToken cancellation)
    {
        var current = history.Peek();

        var newVariantBuilders = current.VariantBuilders
            .Select(x => x.Transform(QuarterTurns, FlipHorizontal, FlipVertical, current.Grid.MaxPosition))
            .ToImmutableHashSet();

        var sw = Stopwatch.StartNew();

        var newClueSource = await ClueSource<T, TCell>.TryCreateAsync(newVariantBuilders, current.Grid.MaxPosition,
            current.Grid.ClueSource.ValueSource, cancellation);

        if (newClueSource.IsFailure)
        {
            yield return new ActionResult<T, TCell>.ErrorResult(newClueSource.Error, sw.Elapsed);
            yield break;
        }


        var newCells = current.Grid.AllCells.Select(x =>
                new KeyValuePair<Position, TCell>(x.Key.Transform(QuarterTurns, FlipHorizontal, FlipVertical,
                    current.Grid.MaxPosition), x.Value))
            .ToList();

        var newFixedValues = current.FixedValues.Select(x =>
            new KeyValuePair<Position, T>(x.Key.Transform(QuarterTurns, FlipHorizontal, FlipVertical,
                current.Grid.MaxPosition), x.Value)).ToImmutableSortedDictionary();

        var newGrid = Grid<T, TCell>.Create(newCells, current.Grid.MaxPosition, newClueSource.Value);
        var newSolveState = new SolveState<T, TCell>(
            newGrid,
            newVariantBuilders,
            UpdateResult<T, TCell>.Empty, ChangeType.ManualChange,
            Name,
            sw.Elapsed,
            newFixedValues,
            null);

        yield return (ActionResult<T, TCell>)newSolveState;

    }
}
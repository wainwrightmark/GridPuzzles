using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using GridPuzzles.Bifurcation;

namespace GridPuzzles.Session.Actions;

public class RandomChangeAction<T> : IGridViewAction<T> where T : notnull
{
    public static RandomChangeAction<T> Instance = new();

    private RandomChangeAction()
    {
    }

    /// <inheritdoc />
    public string Name => "Random Change";

    /// <inheritdoc />
    public async IAsyncEnumerable<ActionResult<T>> Execute(ImmutableStack<SolveState<T>> history,
        SessionSettings settings, [EnumeratorCancellation] CancellationToken cancellation)
    {
        var currentState = history.Peek();
        var sw = Stopwatch.StartNew();
        var (grid, updateResult) = await currentState.Grid.RandomIterateAsync();

        if (updateResult.IsNotEmpty)
            yield return (ActionResult<T>)new SolveState<T>(grid,
                currentState.VariantBuilders,
                updateResult, ChangeType.RandomMove, updateResult.Message, sw.Elapsed,
                currentState.FixedValues,
                currentState.Grid);
    }
}
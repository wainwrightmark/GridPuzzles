using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace GridPuzzles.Session.Actions;

public class FinalGridAction<T, TCell> : IGridViewAction<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    public static FinalGridAction<T, TCell> Instance = new();

    private FinalGridAction()
    {

    }

    /// <inheritdoc />
    public string Name => "Final";

    /// <inheritdoc />
    public IAsyncEnumerable<ActionResult<T, TCell>> Execute(ImmutableStack<SolveState<T, TCell>> history,
        SessionSettings settings, CancellationToken cancellation)
    {
        var results = GetResults(history, settings, cancellation);

        if (settings.MaxFinalInterval == TimeSpan.Zero)
            return results;

        return results.Zip(MyTimer(settings), (x,_)=>x);
    }

    static async IAsyncEnumerable<int> MyTimer(SessionSettings settings)
    {
        var i = 0;
        while (true)
        {
            yield return i++;
            await Task.Delay(settings.MaxFinalInterval);
        }
    }

    private static async  IAsyncEnumerable<ActionResult<T, TCell>> GetResults(ImmutableStack<SolveState<T, TCell>> history, SessionSettings settings, [EnumeratorCancellation] CancellationToken cancellation)
    {
        var @continue = true;
            

        while (@continue)
        {
            @continue = false;
            await foreach (var s in NextGridAction<T, TCell>.Instance.Execute(history, settings, cancellation))
            {
                yield return s;
                if (s is ActionResult<T, TCell>.NewStateResult newState)
                {
                    @continue = true;
                    history = history.Push(newState.State);

                    if(newState.State.UpdateResult.HasContradictions || !newState.State.UpdateResult.IsNotEmpty)
                        yield break;
                }
                else
                    yield break;
            }
        }
        yield break;
    }
        
}
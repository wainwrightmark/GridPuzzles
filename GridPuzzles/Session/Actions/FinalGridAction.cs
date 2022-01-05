using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace GridPuzzles.Session.Actions;

public class FinalGridAction<T> : IGridViewAction<T> where T: notnull
{
    public static FinalGridAction<T> Instance = new();

    private FinalGridAction()
    {

    }

    /// <inheritdoc />
    public string Name => "Final";

    /// <inheritdoc />
    public IAsyncEnumerable<ActionResult<T>> Execute(ImmutableStack<SolveState<T>> history,
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

    private static async  IAsyncEnumerable<ActionResult<T>> GetResults(ImmutableStack<SolveState<T>> history, SessionSettings settings, [EnumeratorCancellation] CancellationToken cancellation)
    {
        var @continue = true;
            

        while (@continue)
        {
            @continue = false;
            await foreach (var s in NextGridAction<T>.Instance.Execute(history, settings, cancellation))
            {
                yield return s;
                if (s is ActionResult<T>.NewStateResult newState)
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
namespace GridPuzzles.Session.Actions;

public abstract record ActionResult<T, TCell>  where T :struct where TCell : ICell<T, TCell>, new()
{
    public record NoChangeResult(TimeSpan Duration) : ActionResult<T, TCell>;

    public record ErrorResult(string Message,TimeSpan Duration) : ActionResult<T, TCell>;

    public record NewStateResult(SolveState<T, TCell> State) : ActionResult<T, TCell>;

    public record ChangeHistoryResult(ImmutableStack<SolveState<T, TCell>> NewHistory) : ActionResult<T, TCell>;

    public static explicit operator ActionResult<T, TCell>(SolveState<T, TCell> b) => new NewStateResult(b);

}
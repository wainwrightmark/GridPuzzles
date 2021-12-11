using System;
using System.Collections.Immutable;

namespace GridPuzzles.Session.Actions;

public abstract record ActionResult<T>  where T:notnull
{
    public record NoChangeResult(TimeSpan Duration) : ActionResult<T>;

    public record ErrorResult(string Message,TimeSpan Duration) : ActionResult<T>;

    public record NewStateResult(SolveState<T> State) : ActionResult<T>;

    public record ChangeHistoryResult(ImmutableStack<SolveState<T>> NewHistory) : ActionResult<T>;

    public static explicit operator ActionResult<T>(SolveState<T> b) => new NewStateResult(b);

}
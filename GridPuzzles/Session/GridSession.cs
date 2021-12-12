using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GridPuzzles.Clues;
using GridPuzzles.Session.Actions;
using GridPuzzles.VariantBuilderArguments;

namespace GridPuzzles.Session;

public interface IGridSession
{
    ISolveState SolveState { get; }
    IEnumerable<ISolveState> StateHistory { get; }
    ISolveState ChosenState { get; }
    SessionSettings SessionSettings { get; }
}

public class GridSession<T> : IGridSession where T:notnull
{
    /// <summary>
    /// Creates a new Session
    /// </summary>
    private GridSession(Grid<T> grid, IEnumerable<IVariantBuilder<T>> variantBuilders,
        ImmutableHashSet<VariantBuilderArgumentPair<T>> variantsInPlay)
    {
        PotentialVariantBuilders = variantBuilders.ToList();
        var state = new SolveState<T>(grid,
            variantsInPlay,
            UpdateResult<T>.Empty,
            ChangeType.InitialState,
            "Initial State",
            TimeSpan.Zero, 
            grid.Cells.Where(x=>x.Value.PossibleValues.Count == 1)
                .ToImmutableSortedDictionary(x=>x.Key, x=>x.Value.PossibleValues.Single()), 
            null);
        StateHistory = ImmutableStack<SolveState<T>>.Empty.Push(state);
    }

    public static async Task<GridSession<T>> CreateAsync(Grid<T> grid,
        IEnumerable<IVariantBuilder<T>> variantBuilders,
        IReadOnlyCollection<VariantBuilderArgumentPair<T>> variantsInPlay, CancellationToken cancellationToken)
    {
        var clueSourceResult = await ClueSource<T>.TryCreateAsync(
            variantsInPlay,
            grid.MaxPosition, grid.ClueSource.ValueSource, CancellationToken.None);

        //TODO handle errors here
        grid = grid.CloneWithClueSource(clueSourceResult.Value);

        var gv = new GridSession<T>(grid, variantBuilders, variantsInPlay.ToImmutableHashSet());
        return gv;
    }
        
    public IReadOnlyCollection<IVariantBuilder<T>> PotentialVariantBuilders { get; }

    /// <inheritdoc />
    ISolveState IGridSession.SolveState => SolveState;

    public SolveState<T> SolveState => StateHistory.Peek();

    public bool ButtonsDisabled { get; set; } = false;

    /// <inheritdoc />
    public SessionSettings SessionSettings { get; set; } = new ();

    /// <inheritdoc />
    IEnumerable<ISolveState> IGridSession.StateHistory => StateHistory;

    public ImmutableStack<SolveState<T>> StateHistory { get; private set; }

    public Action? StateHasChangedAction { get; set; }

    public SolveState<T>? SelectedState { get; set; }

    public ISolveState ChosenState => SelectedState ?? SolveState;

    private void UpdateState(ActionResult<T> actionResult, string actionName)
    {
        if (actionResult is ActionResult<T>.NoChangeResult ncr)
        {
            StateHistory = StateHistory.Push(SolveState with
            {
                UpdateResult = UpdateResult<T>.Empty,
                Message = $"No Change from {actionName}",
                Duration = ncr.Duration, ChangeType = ChangeType.NoChange
            });
        }
        else if (actionResult is ActionResult<T>.ErrorResult errorResult)
        {
            StateHistory = StateHistory.Push(SolveState with
            {
                UpdateResult = UpdateResult<T>.Empty,
                Message = errorResult.Message,
                Duration = errorResult.Duration, ChangeType = ChangeType.Error
            });
        }

        else if (actionResult is ActionResult<T>.NewStateResult newStateResult)
        {
            StateHistory = StateHistory.Push(newStateResult.State);
        }
        else if (actionResult is ActionResult<T>.ChangeHistoryResult changeHistory)
        {
            StateHistory = changeHistory.NewHistory;
        }

        SelectedState = null;
        StateHasChangedAction?.Invoke();
    }

    private CancellationTokenSource _cancellationTokenSource = new ();

    public void Cancel()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        ButtonsDisabled = false;
        StateHasChangedAction?.Invoke();
    }

        
    private async Task DoIfFree(IGridViewAction<T> action)
    {
        if (!ButtonsDisabled)
        {
            var sw = Stopwatch.StartNew();
            ButtonsDisabled = true;
            var changed = false;

            await foreach (var ss in action.Execute(StateHistory, SessionSettings, _cancellationTokenSource.Token))
            {
                changed = true;
                UpdateState(ss, action.Name);
            }

            sw.Stop();
            ButtonsDisabled = false;
            if (!changed)
            {
                UpdateState(new ActionResult<T>.NoChangeResult(sw.Elapsed), action.Name);
            }
            else
            {
                StateHasChangedAction?.Invoke();
            }

        }
    }

    public Task UndoLastManualChange() => DoIfFree(UndoLastManualChangeAction<T>.Instance);


    public Task RemoveVariantBuilder(VariantBuilderArgumentPair<T> variantBuilderArgumentPair) =>
        DoIfFree(new RemoveVariantBuilderAction<T>(variantBuilderArgumentPair));


    public Task AddVariantBuilder(VariantBuilderArgumentPair<T> variantBuilderArgumentPair) =>
        DoIfFree(new AddVariantBuilderAction<T>(variantBuilderArgumentPair));


    public Task PreviousGrid() => DoIfFree(PreviousStateAction<T>.Instance);


    public Task Clear() => DoIfFree(ClearAction<T>.Instance);
    public Task ClearAll() => DoIfFree(ClearAllAction<T>.Instance);
    public Task Transform(int quarterTurns, bool flipHorizontal, bool flipVertical) => DoIfFree(new TransformAction<T>(quarterTurns, flipHorizontal, flipVertical));
    public Task ClearHistory() => DoIfFree(ClearHistoryAction<T>.Instance);
    public Task ReportError(string message, TimeSpan duration) => DoIfFree(new ReportError<T>(message, duration));

    public Task RandomChange() => DoIfFree(RandomChangeAction<T>.Instance);

    public Task RandomSolve() => DoIfFree(RandomSolveAction<T>.Instance);

    public Task NextGrid() => DoIfFree(NextGridAction<T>.Instance);

    public Task FinalGrid() => DoIfFree(FinalGridAction<T>.Instance);

    public Task GoToState(SolveState<T> state) => DoIfFree(new GoToStateAction<T>(state));

    public async Task SetCell(Maybe<T> i, Position position)
    {
        var kpa = new SetCellAction<T>(position, i);
        await DoIfFree(kpa);
    }

        
}
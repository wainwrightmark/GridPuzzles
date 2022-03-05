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

public class GridSession<T, TCell> : IGridSession where T :struct where TCell : ICell<T, TCell>, new()
{
    /// <summary>
    /// Creates a new Session
    /// </summary>
    private GridSession(Grid<T, TCell> grid, IEnumerable<IVariantBuilder<T, TCell>> variantBuilders,
        ImmutableHashSet<VariantBuilderArgumentPair<T, TCell>> variantsInPlay)
    {
        PotentialVariantBuilders = variantBuilders.ToList();
        var state = new SolveState<T, TCell>(grid,
            variantsInPlay,
            UpdateResult<T, TCell>.Empty,
            ChangeType.InitialState,
            "Initial State",
            TimeSpan.Zero, 
            grid.Cells.Where(x=>x.Value.HasSingleValue())
                .ToImmutableSortedDictionary(x=>x.Key, x=>x.Value.Single()), 
            null);
        StateHistory = ImmutableStack<SolveState<T, TCell>>.Empty.Push(state);
    }

    public static async Task<GridSession<T, TCell>> CreateAsync(Grid<T, TCell> grid,
        IEnumerable<IVariantBuilder<T, TCell>> variantBuilders,
        IReadOnlyCollection<VariantBuilderArgumentPair<T, TCell>> variantsInPlay, CancellationToken cancellationToken)
    {
        var clueSourceResult = await ClueSource<T, TCell>.TryCreateAsync(
            variantsInPlay,
            grid.MaxPosition, grid.ClueSource.ValueSource, CancellationToken.None);

        //TODO handle errors here
        grid = grid.CloneWithClueSource(clueSourceResult.Value);

        var gv = new GridSession<T, TCell>(grid, variantBuilders, variantsInPlay.ToImmutableHashSet());
        return gv;
    }
        
    public IReadOnlyCollection<IVariantBuilder<T, TCell>> PotentialVariantBuilders { get; }

    /// <inheritdoc />
    ISolveState IGridSession.SolveState => SolveState;

    public SolveState<T, TCell> SolveState => StateHistory.Peek();

    public bool ButtonsDisabled { get; set; } = false;

    /// <inheritdoc />
    public SessionSettings SessionSettings { get; set; } = new ();

    /// <inheritdoc />
    IEnumerable<ISolveState> IGridSession.StateHistory => StateHistory;

    public ImmutableStack<SolveState<T, TCell>> StateHistory { get; private set; }

    public Action? StateHasChangedAction { get; set; }

    public SolveState<T, TCell>? SelectedState { get; set; }

    public ISolveState ChosenState => SelectedState ?? SolveState;

    private void UpdateState(ActionResult<T, TCell> actionResult, string actionName)
    {
        if (actionResult is ActionResult<T, TCell>.NoChangeResult ncr)
        {
            StateHistory = StateHistory.Push(SolveState with
            {
                UpdateResult = UpdateResult<T, TCell>.Empty,
                Message = $"No Change from {actionName}",
                Duration = ncr.Duration, ChangeType = ChangeType.NoChange
            });
        }
        else if (actionResult is ActionResult<T, TCell>.ErrorResult errorResult)
        {
            StateHistory = StateHistory.Push(SolveState with
            {
                UpdateResult = UpdateResult<T, TCell>.Empty,
                Message = errorResult.Message,
                Duration = errorResult.Duration, ChangeType = ChangeType.Error
            });
        }

        else if (actionResult is ActionResult<T, TCell>.NewStateResult newStateResult)
        {
            StateHistory = StateHistory.Push(newStateResult.State);
        }
        else if (actionResult is ActionResult<T, TCell>.ChangeHistoryResult changeHistory)
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

        
    private async Task DoIfFree(IGridViewAction<T, TCell> action)
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
                UpdateState(new ActionResult<T, TCell>.NoChangeResult(sw.Elapsed), action.Name);
            }
            else
            {
                StateHasChangedAction?.Invoke();
            }

        }
    }

    public Task UndoLastManualChange() => DoIfFree(UndoLastManualChangeAction<T, TCell>.Instance);


    public Task RemoveVariantBuilder(VariantBuilderArgumentPair<T, TCell> variantBuilderArgumentPair) =>
        DoIfFree(new RemoveVariantBuilderAction<T, TCell>(variantBuilderArgumentPair));


    public Task AddVariantBuilder(VariantBuilderArgumentPair<T, TCell> variantBuilderArgumentPair) =>
        DoIfFree(new AddVariantBuilderAction<T, TCell>(variantBuilderArgumentPair));


    public Task PreviousGrid() => DoIfFree(PreviousStateAction<T, TCell>.Instance);


    public Task Clear() => DoIfFree(ClearAction<T, TCell>.Instance);
    public Task ClearAll() => DoIfFree(ClearAllAction<T, TCell>.Instance);
    public Task Transform(int quarterTurns, bool flipHorizontal, bool flipVertical) => DoIfFree(new TransformAction<T, TCell>(quarterTurns, flipHorizontal, flipVertical));
    public Task ClearHistory() => DoIfFree(ClearHistoryAction<T, TCell>.Instance);
    public Task ReportError(string message, TimeSpan duration) => DoIfFree(new ReportError<T, TCell>(message, duration));

    public Task RandomChange() => DoIfFree(RandomChangeAction<T, TCell>.Instance);

    public Task RandomSolve() => DoIfFree(RandomSolveAction<T, TCell>.Instance);

    public Task NextGrid() => DoIfFree(NextGridAction<T, TCell>.Instance);

    public Task FinalGrid() => DoIfFree(FinalGridAction<T, TCell>.Instance);

    public Task GoToState(SolveState<T, TCell> state) => DoIfFree(new GoToStateAction<T, TCell>(state));

    public async Task SetCell(Maybe<T> i, Position position)
    {
        var kpa = new SetCellAction<T, TCell>(position, i);
        await DoIfFree(kpa);
    }

        
}
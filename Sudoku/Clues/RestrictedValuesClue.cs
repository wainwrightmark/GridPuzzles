namespace Sudoku.Clues;

public class RestrictedValuesClue<T, TCell> : IRuleClue<T, TCell>  where T :struct where TCell : ICell<T, TCell>, new()
{
    public RestrictedValuesClue(string reason, IEnumerable<Position> positions, IEnumerable<T> possibleValues)
    {
        Positions = positions.ToImmutableSortedSet();
        PossibleValues = new TCell().AddRange(possibleValues);
        Reason = reason;
    }

    public string Reason { get; }

    /// <inheritdoc />
    public string Name => Reason;

    /// <inheritdoc />
    public ImmutableSortedSet<Position> Positions { get; }

    public TCell PossibleValues { get; }

    /// <inheritdoc />
    public IEnumerable<ICellChangeResult> CalculateCellUpdates(Grid<T, TCell> grid)
    {
        foreach (var cell in Positions.Select(grid.GetCellKVP))
            yield return cell.CloneWithOnlyValues<T, TCell>(PossibleValues, new RestrictedValuesReason<T, TCell>(this));
    }
}

public sealed record RestrictedValuesReason<T, TCell>(RestrictedValuesClue<T, TCell> RestrictedValuesClue)
    : ISingleReason where T :struct where TCell : ICell<T, TCell>, new()
{
    /// <inheritdoc />
    public string Text => RestrictedValuesClue.Reason;

    /// <inheritdoc />
    public IEnumerable<Position> GetContributingPositions(IGrid grid)
    {
        yield break;
    }

    /// <inheritdoc />
    public Maybe<IClue> Clue => RestrictedValuesClue;
}
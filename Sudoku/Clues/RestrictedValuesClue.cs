namespace Sudoku.Clues;

public class RestrictedValuesClue<T> : IRuleClue<T>  where T :notnull
{
    public RestrictedValuesClue(string reason, IEnumerable<Position> positions, IEnumerable<T> possibleValues)
    {
        Positions = positions.ToImmutableSortedSet();
        PossibleValues = possibleValues.ToImmutableHashSet();
        Reason = reason;
    }

    public string Reason { get; }

    /// <inheritdoc />
    public string Name => Reason;

    /// <inheritdoc />
    public ImmutableSortedSet<Position> Positions { get; }

    public ImmutableHashSet<T> PossibleValues { get; }

    /// <inheritdoc />
    public IEnumerable<ICellChangeResult> GetCellUpdates(Grid<T> grid)
    {
        foreach (var cell in Positions.Select(grid.GetCellKVP))
            yield return cell.CloneWithOnlyValues(PossibleValues, new RestrictedValuesReason<T>(this));
    }
}

public sealed record RestrictedValuesReason<T>(RestrictedValuesClue<T> RestrictedValuesClue)
    : ISingleReason where T:notnull
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
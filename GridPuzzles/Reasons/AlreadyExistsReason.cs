using GridPuzzles.Clues;

namespace GridPuzzles.Reasons;

public sealed record AlreadyExistsReason<T, TCell>(T Value, IUniquenessClue<T, TCell> UniquenessClue)
    : ISingleReason
    where T :struct where TCell : ICell<T, TCell>, new()
{
    /// <inheritdoc />
    public string Text => $"{Value} already exists in {UniquenessClue.Domain}.";

    /// <inheritdoc />
    public IEnumerable<Position> GetContributingPositions(IGrid grid)
    {
        if (grid is Grid<T, TCell> gridT)
        {
            return 
                UniquenessClue.Positions.Select(gridT.GetCellKVP)
                    .Where(x => x.Value.HasSingleValue() && x.Value.Contains(Value))
                    .Select(x=>x.Key);
        }

        return UniquenessClue.Positions;
    }

    /// <inheritdoc />
    public Maybe<IClue> Clue => Maybe<IClue>.From(UniquenessClue);
}
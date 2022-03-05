using GridPuzzles.Clues;

namespace GridPuzzles.Reasons;

public sealed record PossibilityStormReason<T, TCell>(T Value, ICompletenessClue<T, TCell> CompletenessClue)
    : ISingleReason
    where T :struct where TCell : ICell<T, TCell>, new()
{
        
    /// <inheritdoc />
    public string Text => $"Every possible {Value} in {CompletenessClue.Domain} prevents this from being {Value}.";

    /// <inheritdoc />
    public IEnumerable<Position> GetContributingPositions(IGrid grid)
    {
        if (grid is Grid<T, TCell> gridT)
        {
            return 
                CompletenessClue.Positions.Select(gridT.GetCellKVP)
                    .Where(x => x.Value.Contains(Value))
                    .Select(x=>x.Key);
        }

        return CompletenessClue.Positions;
    }

    /// <inheritdoc />
    public Maybe<IClue> Clue => Maybe<IClue>.From(CompletenessClue);
}
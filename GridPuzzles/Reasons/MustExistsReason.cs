using GridPuzzles.Clues;

namespace GridPuzzles.Reasons;

public sealed record MustExistsReason<T, TCell>(T Value, ICompletenessClue<T, TCell> CompletenessClue)
    : ISingleReason
    where T :struct where TCell : ICell<T, TCell>, new()
{
    /// <inheritdoc />
    public string Text => $"{Value} must exist in {CompletenessClue.Domain}.";

    /// <inheritdoc />
    public IEnumerable<Position> GetContributingPositions(IGrid grid)
    {
        return CompletenessClue.Positions;
    }

    /// <inheritdoc />
    public Maybe<IClue> Clue => Maybe<IClue>.From(CompletenessClue);
}
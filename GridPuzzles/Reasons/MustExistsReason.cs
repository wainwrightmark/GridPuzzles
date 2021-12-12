using GridPuzzles.Clues;

namespace GridPuzzles.Reasons;

public sealed record MustExistsReason<T>(T Value, ICompletenessClue<T> CompletenessClue)
    : ISingleReason
    where T : notnull
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
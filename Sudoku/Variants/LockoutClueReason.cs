namespace Sudoku.Variants;

public sealed record LockoutClueReason(LockoutClue LockoutClue) : ISingleReason
{
    /// <inheritdoc />
    public string Text => LockoutClue.Name;

    /// <inheritdoc />
    public IEnumerable<Position> GetContributingPositions(IGrid grid)
    {
        return LockoutClue.Positions;
    }

    /// <inheritdoc />
    public Maybe<IClue> Clue => LockoutClue;
}
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using GridPuzzles;
using GridPuzzles.Clues;
using GridPuzzles.Reasons;

namespace Sudoku.Variants;

public sealed record BetweenClueReason(BetweenClue BetweenClue) : ISingleReason
{
    /// <inheritdoc />
    public string Text => BetweenClue.Name;

    /// <inheritdoc />
    public IEnumerable<Position> GetContributingPositions(IGrid grid)
    {
        return BetweenClue.Positions;
    }

    /// <inheritdoc />
    public Maybe<IClue> Clue => BetweenClue;
}
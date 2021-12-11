using System.Collections.Generic;
using CSharpFunctionalExtensions;
using GridPuzzles.Clues;

namespace GridPuzzles.Reasons;

public sealed record AllLedToContradiction(ISingleReason Reason) : ISingleReason
{
    /// <inheritdoc />
    public string Text => $"All {Reason.Text} led to contradiction";

    /// <inheritdoc />
    public IEnumerable<Position> GetContributingPositions(IGrid grid)
    {
        return Reason.GetContributingPositions(grid);
    }

    /// <inheritdoc />
    public Maybe<IClue> Clue => Reason.Clue;
}
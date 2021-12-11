using System.Collections.Generic;
using CSharpFunctionalExtensions;
using GridPuzzles.Clues;

namespace GridPuzzles.Reasons;

public sealed record ImpliedByAllReason(ISingleReason Reason) : ISingleReason
{
    /// <inheritdoc />
    public string Text => $"Implied by all {Reason.Text}";

    /// <inheritdoc />
    public IEnumerable<Position> GetContributingPositions(IGrid grid)
    {
        return Reason.GetContributingPositions(grid);
    }

    /// <inheritdoc />
    public Maybe<IClue> Clue => Reason.Clue;
}
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using GridPuzzles.Clues;

namespace GridPuzzles.Reasons;

public sealed record PossibleValuesReason(Position Position) : ISingleReason
{
    /// <inheritdoc />
    public string Text => $"Possible Values of {Position}";

    /// <inheritdoc />
    public IEnumerable<Position> GetContributingPositions(IGrid grid)
    {
        yield return Position;
    }

    /// <inheritdoc />
    public Maybe<IClue> Clue => Maybe<IClue>.None;
}
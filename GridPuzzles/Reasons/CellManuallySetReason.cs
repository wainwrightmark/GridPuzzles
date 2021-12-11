using System.Collections.Generic;
using CSharpFunctionalExtensions;
using GridPuzzles.Clues;

namespace GridPuzzles.Reasons;

public sealed record CellManuallySetReason : ISingleReason
{
    private CellManuallySetReason() { }

    public static CellManuallySetReason Instance { get; } = new();

    /// <inheritdoc />
    public string Text => "Manually Set";

    /// <inheritdoc />
    public IEnumerable<Position> GetContributingPositions(IGrid grid)
    {
        yield break;
    }

    /// <inheritdoc />
    public Maybe<IClue> Clue => Maybe<IClue>.None;
}
using GridPuzzles.Clues;

namespace GridPuzzles.Reasons;

public sealed record BifurcationAttemptReason : ISingleReason
{
    private BifurcationAttemptReason() { }

    public static BifurcationAttemptReason Instance { get; } = new ();

    /// <inheritdoc />
    public string Text => "Bifurcation Attempt";

    /// <inheritdoc />
    public IEnumerable<Position> GetContributingPositions(IGrid grid)
    {
        yield break;
    }

    /// <inheritdoc />
    public Maybe<IClue> Clue => Maybe<IClue>.None;
}
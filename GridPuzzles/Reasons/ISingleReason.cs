using GridPuzzles.Clues;

namespace GridPuzzles.Reasons;

public interface ISingleReason : IUpdateReason
{
    Maybe<IClue> Clue { get; }
}
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using GridPuzzles.Clues;

namespace GridPuzzles.Reasons;

public sealed record PossibilityStormReason<T>(T Value, ICompletenessClue<T> CompletenessClue)
    : ISingleReason
    where T : notnull
{
        
    /// <inheritdoc />
    public string Text => $"Every possible {Value} in {CompletenessClue.Domain} prevents this from being {Value}.";

    /// <inheritdoc />
    public IEnumerable<Position> GetContributingPositions(IGrid grid)
    {
        if (grid is Grid<T> gridT)
        {
            return 
                CompletenessClue.Positions.Select(gridT.GetCellKVP)
                    .Where(x => x.Value.PossibleValues.Contains(Value))
                    .Select(x=>x.Key);
        }

        return CompletenessClue.Positions;
    }

    /// <inheritdoc />
    public Maybe<IClue> Clue => Maybe<IClue>.From(CompletenessClue);
}
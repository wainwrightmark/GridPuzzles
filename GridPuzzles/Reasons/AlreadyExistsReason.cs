using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using GridPuzzles.Clues;

namespace GridPuzzles.Reasons;

public sealed record AlreadyExistsReason<T>(T Value, IUniquenessClue<T> UniquenessClue)
    : ISingleReason
    where T : notnull
{
    /// <inheritdoc />
    public string Text => $"{Value} already exists in {UniquenessClue.Domain}.";

    /// <inheritdoc />
    public IEnumerable<Position> GetContributingPositions(IGrid grid)
    {
        if (grid is Grid<T> gridT)
        {
            return 
                UniquenessClue.Positions.Select(gridT.GetCellKVP)
                    .Where(x => x.Value.PossibleValues.Count == 1 && x.Value.PossibleValues.Contains(Value))
                    .Select(x=>x.Key);
        }

        return UniquenessClue.Positions;
    }

    /// <inheritdoc />
    public Maybe<IClue> Clue => Maybe<IClue>.From(UniquenessClue);
}
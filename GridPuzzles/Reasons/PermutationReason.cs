using GridPuzzles.Clues;
using MoreLinq;

namespace GridPuzzles.Reasons;

public sealed record PermutationReason<T, TCell>(TCell Values, IUniquenessClue<T, TCell> UniquenessClue): ISingleReason
    where T :struct where TCell : ICell<T, TCell>, new()
{
        
    /// <inheritdoc />
    public string Text => $"The values [{Values.ToDelimitedString("")}] are a permutation in {UniquenessClue.Domain}.";

    /// <inheritdoc />
    public IEnumerable<Position> GetContributingPositions(IGrid grid)
    {
        if (grid is Grid<T, TCell> gridT)
        {
            return 
                UniquenessClue.Positions.Select(gridT.GetCellKVP)
                    .Where(x => Values.IsSupersetOf(x.Value))
                    .Select(x=>x.Key);
        }

        return UniquenessClue.Positions;
    }

    /// <inheritdoc />
    public Maybe<IClue> Clue => Maybe<IClue>.From(UniquenessClue);
}
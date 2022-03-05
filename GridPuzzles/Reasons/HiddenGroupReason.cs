using GridPuzzles.Clues;
using MoreLinq;

namespace GridPuzzles.Reasons;

public sealed record HiddenGroupReason<T, TCell>(TCell Values, ICompletenessClue<T, TCell> CompletenessClue)
    : ISingleReason
    where T :struct where TCell : ICell<T, TCell>, new()
{
    /// <inheritdoc />
    public string Text => $"Only {Values.Count()} cells can contain the values [{Values.OrderBy(x=>x).ToDelimitedString("")}] in {CompletenessClue.Domain}.";

    /// <inheritdoc />
    public IEnumerable<Position> GetContributingPositions(IGrid grid)
    {
        if (grid is Grid<T, TCell> gridT)
        {
            return 
                CompletenessClue.Positions.Select(gridT.GetCellKVP)
                    .Where(x => x.Value.Overlaps(Values))
                    .Select(x=>x.Key);
        }

        return CompletenessClue.Positions;
    }

    /// <inheritdoc />
    public Maybe<IClue> Clue => Maybe<IClue>.From(CompletenessClue);
}
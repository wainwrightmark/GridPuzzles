using GridPuzzles.Clues;
using MoreLinq;

namespace GridPuzzles.Reasons;

public sealed record HiddenGroupReason<T>(IReadOnlyList<T> Values, ICompletenessClue<T> CompletenessClue)
    : ISingleReason
    where T : notnull
{
    /// <inheritdoc />
    public string Text => $"Only {Values.Count} cells can contain the values [{Values.OrderBy(x=>x).ToDelimitedString("")}] in {CompletenessClue.Domain}.";

    /// <inheritdoc />
    public IEnumerable<Position> GetContributingPositions(IGrid grid)
    {
        if (grid is Grid<T> gridT)
        {
            return 
                CompletenessClue.Positions.Select(gridT.GetCellKVP)
                    .Where(x => x.Value.PossibleValues.Overlaps(Values))
                    .Select(x=>x.Key);
        }

        return CompletenessClue.Positions;
    }

    /// <inheritdoc />
    public Maybe<IClue> Clue => Maybe<IClue>.From(CompletenessClue);

    /// <inheritdoc />
    public bool Equals(HiddenGroupReason<T>? other)
    {
        return other is not null && other.CompletenessClue == CompletenessClue && Values.SequenceEqual(other.Values);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(CompletenessClue, Values.Count, Values.First(), Values.Last());
    }
}
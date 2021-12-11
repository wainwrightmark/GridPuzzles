using GridPuzzles.Reasons;

namespace GridPuzzles.Cells;

public sealed record CellUpdate<T>(
    Cell<T> NewCell, 
    Position Position, 
    IUpdateReason Reason) : ICellChangeResult
{
    public ICellChangeResult TryCombine(CellUpdate<T> otherCellUpdate)
    {
        var (otherCell, _, otherReason) = otherCellUpdate;

        var newSet = NewCell.PossibleValues.Intersect(otherCell.PossibleValues);

        if (newSet.Count == NewCell.PossibleValues.Count)
            return this; //reuse this, ignore the other result and its reason
        if (newSet.Count == otherCell.PossibleValues.Count)
            return otherCellUpdate; //Just use the other one, ignore this

        var ccr = CellHelper.TryCreate(newSet, Position, Reason.Combine(otherReason));
        return ccr;
    }

    /// <inheritdoc />
    public bool Equals(CellUpdate<T>? other)
    {
        return other is not null&& Position == other.Position && NewCell.Equals(other.NewCell);
    }

    public override int GetHashCode() => Position.GetHashCode() + 2 * NewCell.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => Position + ": " + NewCell;
}
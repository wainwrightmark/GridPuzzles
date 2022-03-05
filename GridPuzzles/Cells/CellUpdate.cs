namespace GridPuzzles.Cells;

public sealed record CellUpdate<T, TCell>(
    TCell NewCell, 
    Position Position, 
    IUpdateReason Reason) : ICellChangeResult where T: struct where TCell : ICell<T, TCell>, new()
{
    public ICellChangeResult TryCombine(CellUpdate<T, TCell> otherCellUpdate)
    {
        var (otherCell, _, otherReason) = otherCellUpdate;

        var combinedCell = NewCell.Intersect(otherCell);

        if (combinedCell.Equals(NewCell))
            return this; //reuse this, ignore the other result and its reason
        if (combinedCell.Equals(otherCell))
            return otherCellUpdate; //Just use the other one, ignore this

        var ccr = CellHelper.TryCreate<T, TCell>(combinedCell, Position, Reason.Combine(otherReason));
        return ccr;
    }

    /// <inheritdoc />
    public bool Equals(CellUpdate<T, TCell>? other)
    {
        return other is not null&& Position == other.Position && NewCell.Equals(other.NewCell);
    }

    public override int GetHashCode() => Position.GetHashCode() + 2 * NewCell.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => Position + ": " + NewCell;
}
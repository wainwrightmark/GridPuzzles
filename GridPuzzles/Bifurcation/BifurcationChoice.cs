namespace GridPuzzles.Bifurcation;

public interface IBifurcationChoice<T, TCell> : IComparable<IBifurcationChoice<T, TCell>>
    where T :struct where TCell : ICell<T, TCell>, new()
{
    UpdateResult<T, TCell> UpdateResult { get; }
}

public sealed class BifurcationCellChoice<T, TCell> : IBifurcationChoice<T, TCell>
    where T :struct where TCell : ICell<T, TCell>, new()
{
    public BifurcationCellChoice(CellUpdate<T, TCell> cellUpdate)
    {
        CellUpdate = cellUpdate;
    }

    public CellUpdate<T, TCell> CellUpdate { get; }

    public UpdateResult<T, TCell> UpdateResult => UpdateResult<T, TCell>.Empty.CloneWithCellUpdate(CellUpdate);

    /// <inheritdoc />
    public override string ToString() => CellUpdate.ToString();

    /// <inheritdoc />
    public override int GetHashCode() => CellUpdate.GetHashCode();
        

    /// <inheritdoc />
    public int CompareTo(IBifurcationChoice<T, TCell>? other)
    {
        if (other is BifurcationCellChoice<T, TCell> bcc)
            return StringComparer.OrdinalIgnoreCase.Compare(CellUpdate.ToString(), bcc.CellUpdate.ToString());

        return 0;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is BifurcationCellChoice<T, TCell> bcc && CellUpdate.Equals(bcc.CellUpdate);

    public static bool operator ==(BifurcationCellChoice<T, TCell> left, BifurcationCellChoice<T, TCell> right)
    {

        return left.Equals(right);
    }

    public static bool operator !=(BifurcationCellChoice<T, TCell> left, BifurcationCellChoice<T, TCell> right) => !(left == right);

    public static bool operator <(BifurcationCellChoice<T, TCell> left, BifurcationCellChoice<T, TCell> right) => left.CompareTo(right) < 0;

    public static bool operator <=(BifurcationCellChoice<T, TCell> left, BifurcationCellChoice<T, TCell> right) => left.CompareTo(right) <= 0;

    public static bool operator >(BifurcationCellChoice<T, TCell> left, BifurcationCellChoice<T, TCell> right) => left.CompareTo(right) > 0;

    public static bool operator >=(BifurcationCellChoice<T, TCell> left, BifurcationCellChoice<T, TCell> right) => left.CompareTo(right) >= 0;
}
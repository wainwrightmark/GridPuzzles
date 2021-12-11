using System;
using GridPuzzles.Cells;

namespace GridPuzzles.Bifurcation;

public interface IBifurcationChoice<T> : IComparable<IBifurcationChoice<T>>
{
    UpdateResult<T> UpdateResult { get; }
}

public sealed class BifurcationCellChoice<T> : IBifurcationChoice<T>
{
    public BifurcationCellChoice(CellUpdate<T> cellUpdate)
    {
        CellUpdate = cellUpdate;
    }

    public CellUpdate<T> CellUpdate { get; }

    public UpdateResult<T> UpdateResult => UpdateResult<T>.Empty.CloneWithCellUpdate(CellUpdate);

    /// <inheritdoc />
    public override string ToString() => CellUpdate.ToString();

    /// <inheritdoc />
    public override int GetHashCode() => CellUpdate.GetHashCode();
        

    /// <inheritdoc />
    public int CompareTo(IBifurcationChoice<T>? other)
    {
        if (other is BifurcationCellChoice<T> bcc)
            return StringComparer.OrdinalIgnoreCase.Compare(CellUpdate.ToString(), bcc.CellUpdate.ToString());

        return 0;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is BifurcationCellChoice<T> bcc && CellUpdate.Equals(bcc.CellUpdate);

    public static bool operator ==(BifurcationCellChoice<T> left, BifurcationCellChoice<T> right)
    {

        return left.Equals(right);
    }

    public static bool operator !=(BifurcationCellChoice<T> left, BifurcationCellChoice<T> right) => !(left == right);

    public static bool operator <(BifurcationCellChoice<T> left, BifurcationCellChoice<T> right) => left.CompareTo(right) < 0;

    public static bool operator <=(BifurcationCellChoice<T> left, BifurcationCellChoice<T> right) => left.CompareTo(right) <= 0;

    public static bool operator >(BifurcationCellChoice<T> left, BifurcationCellChoice<T> right) => left.CompareTo(right) > 0;

    public static bool operator >=(BifurcationCellChoice<T> left, BifurcationCellChoice<T> right) => left.CompareTo(right) >= 0;
}
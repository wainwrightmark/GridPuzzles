using System.Diagnostics.Contracts;
using GridPuzzles.Clues;

namespace GridPuzzles.Cells;

public static class CellHelper
{
    [Pure]
    public static TCell Create<T, TCell>(T value)
        where T: struct where TCell : ICell<T, TCell>, new()
        
        => new TCell().Add(value);

    [Pure]
    public static CellUpdate<T, TCell> Create<T, TCell>(T value, Position p, ISingleReason reason)
        where T: struct where TCell : ICell<T, TCell>, new()
        => new(Create<T, TCell>(value), p, reason );

    [Pure]
    public static string GetDisplayString<T, TCell>(this TCell cell, IValueSource<T, TCell> valueSource)
        where T: struct where TCell : ICell<T, TCell>, new()
    {
        if (cell.HasSingleValue())
            return cell.Single() + "";

        if (cell.CouldHaveAnyValue(valueSource))
            return "*";

        if( cell.Count() * 2 <= valueSource.AllValues.Count + 1)
            return string.Join("", cell) ;

        return $"^{string.Join("",valueSource.AllValues.Except(cell))}";
    }

    [Pure]
    public static bool ArePossibleValuesContiguous(this IntCell cell)
    {
        if (cell.IsEmpty()) return true;
        var last = cell.First();
        foreach (var v2 in cell.Skip(1))
        {
            if (v2 == last + 1) last = v2;
            else return false;  
        }

        return true;
    }

    [Pure]
    public static ICellChangeResult TryCreate<T, TCell>(TCell cell, Position p, IUpdateReason reason)
    where T: struct where TCell : ICell<T, TCell>, new()
    {

        return cell.Count() switch
        {
            0 => new Contradiction(reason, new []{p}),
            _ => new CellUpdate<T, TCell>(cell, p, reason)
        };
    }

    [Pure]
    public static ICellChangeResult CloneWithoutValue<T, TCell>
        (this KeyValuePair<Position, TCell> kvp, T valueToRemove, ISingleReason reason)
        where T: struct where TCell : ICell<T, TCell>, new()
    {

        var newSet = kvp.Value.Remove(valueToRemove);
        if(newSet.Equals(kvp.Value))
            return NoChange.Instance;

        return TryCreate<T, TCell>(newSet, kvp.Key, reason);
    }

    [Pure]
    public static ICellChangeResult CloneWithOnlyValue<T, TCell>
        (this KeyValuePair<Position, TCell> kvp, T allowedValue, ISingleReason reason)
        where T: struct where TCell : ICell<T, TCell>, new()
    {
        if (kvp.Value.Contains(allowedValue))
            return kvp.Value.HasSingleValue()
                ? NoChange.Instance
                : Create<T, TCell>(allowedValue, kvp.Key, reason);

        return new Contradiction(reason, new[]{kvp.Key});
    }

    [Pure]
    public static ICellChangeResult CloneWithOnlyValues<T, TCell>
        (this KeyValuePair<Position, TCell> kvp, TCell allowedValues, ISingleReason reason)
        where T: struct where TCell : ICell<T, TCell>, new()
    {
        var r = TryCreate<T, TCell>(kvp.Value.Intersect(allowedValues), kvp.Key, reason);

        if (r is CellUpdate<T, TCell> update && update.NewCell.Equals(kvp.Value))
        {
            return NoChange.Instance;
        }
            
        return r;
    }

    [Pure]
    public static ICellChangeResult CloneWithoutValues<T, TCell>
        (this KeyValuePair<Position, TCell> kvp, TCell valueToRemoves, ISingleReason reason)
        where T: struct where TCell : ICell<T, TCell>, new()
    {
        var r = TryCreate<T, TCell>(kvp.Value.Except(valueToRemoves), kvp.Key, reason);

        if (r is CellUpdate<T, TCell> update && update.NewCell.Equals(kvp.Value))
        {
            return NoChange.Instance;
        }

        return r;
    }
        
    [Pure]
    public static ICellChangeResult CloneWithoutValuesAbove(
        this KeyValuePair<Position, IntCell> kvp, int max, ISingleReason reason)
    {
        if (kvp.Value.IsEmpty() || kvp.Value.Max() <= max)
            return NoChange.Instance;

        return kvp.CloneWithoutValues<int, IntCell>(kvp.Value.Where(x => x > max).ToIntCell(),
            reason
        );
    }
        
    [Pure]
    public static ICellChangeResult CloneWithoutValuesBetween(this KeyValuePair<Position, IntCell> kvp, int minBoundary, int maxBoundary, ISingleReason reason)
    {
        return kvp.CloneWithoutValues<int, IntCell>(kvp.Value.Where(x => x < maxBoundary && x > minBoundary).ToIntCell(),
            reason
        );
    }
        
    [Pure]
    public static ICellChangeResult CloneWithoutValuesBelow(this KeyValuePair<Position, IntCell> kvp, int min, ISingleReason reason)
    {
        if (kvp.Value.IsEmpty() || kvp.Value.Min()>= min)
            return NoChange.Instance;

        return kvp.CloneWithoutValues<int, IntCell>(kvp.Value.Where(x => x < min).ToIntCell(),reason);
    }

}
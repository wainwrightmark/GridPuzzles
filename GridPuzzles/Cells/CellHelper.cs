using System.Diagnostics.Contracts;
using GridPuzzles.Clues;

namespace GridPuzzles.Cells;

public static class CellHelper
{
    [Pure]
    public static Cell<T> Create<T>(T value) => new(ImmutableSortedSet.Create(value));

    [Pure]
    public static CellUpdate<T> Create<T>(T value, Position p, ISingleReason reason) => new(Create(value), p, reason );

    [Pure]
    public static string GetDisplayString<T>(this Cell<T> cell, IValueSource<T> valueSource)
    {
        if (cell.PossibleValues.Count == 1)
            return cell.PossibleValues.Count + "";

        if (cell.PossibleValues.Count == valueSource.AllValues.Count)
            return "*";

        if( cell.PossibleValues.Count * 2 <= valueSource.AllValues.Count + 1)
            return string.Join("", cell.PossibleValues) ;

        return $"^{string.Join("",valueSource.AllValues.Except(cell.PossibleValues))}";
    }

    [Pure]
    public static bool ArePossibleValuesContiguous(this Cell<int> cell)
    {
        if (!cell.PossibleValues.Any()) return true;
        var last = cell.PossibleValues.Min;
        foreach (var v2 in cell.PossibleValues.Skip(1))
        {
            if (v2 == last + 1) last = v2;
            else return false;  
        }

        return true;
    }

    [Pure]
    public static ICellChangeResult TryCreate<T>(ImmutableSortedSet<T> set, Position p, IUpdateReason reason)
    {
        return set.Count switch
        {
            0 => new Contradiction(reason, new []{p}),
            _ => new CellUpdate<T>(new Cell<T>(set), p, reason)
        };
    }

    [Pure]
    public static ICellChangeResult CloneWithoutValue<T>(this KeyValuePair<Position, Cell<T>> kvp, T valueToRemove, ISingleReason reason)
    {

        var newSet = kvp.Value.PossibleValues.Remove(valueToRemove);
        if(newSet.Count == kvp.Value.PossibleValues.Count)
            return NoChange.Instance;

        return TryCreate(newSet, kvp.Key, reason);
    }

    [Pure]
    public static ICellChangeResult CloneWithOnlyValue<T>(this KeyValuePair<Position, Cell<T>> kvp, T allowedValue, ISingleReason reason)
    {
        if (kvp.Value.PossibleValues.Contains(allowedValue))
            return kvp.Value.PossibleValues.Count == 1
                ? NoChange.Instance
                : Create(allowedValue, kvp.Key, reason);

        return new Contradiction(reason, new[]{kvp.Key});
    }

    [Pure]
    public static ICellChangeResult CloneWithOnlyValues<T>(this KeyValuePair<Position, Cell<T>> kvp,IEnumerable<T> allowedValues, ISingleReason reason)
    {
        var r = TryCreate(kvp.Value.PossibleValues.Intersect(allowedValues), kvp.Key, reason);

        if (r is CellUpdate<T> update && update.NewCell.PossibleValues.Count == kvp.Value.PossibleValues.Count)
        {
            return NoChange.Instance;
        }
            
        return r;
    }

    [Pure]
    public static ICellChangeResult CloneWithoutValues<T>(this KeyValuePair<Position, Cell<T>> kvp, IEnumerable<T> valueToRemoves, ISingleReason reason)
    {
        var r = TryCreate(kvp.Value.PossibleValues.Except(valueToRemoves), kvp.Key, reason);

        if (r is CellUpdate<T> update && update.NewCell.PossibleValues.Count == kvp.Value.PossibleValues.Count)
        {
            return NoChange.Instance;
        }

        return r;
    }
        
    [Pure]
    public static ICellChangeResult CloneWithoutValuesAbove(this KeyValuePair<Position, Cell<int>> kvp, int max, ISingleReason reason)
    {
        if (kvp.Value.PossibleValues.Max <= max)
            return NoChange.Instance;

        return kvp.CloneWithoutValues(kvp.Value.PossibleValues.Where(x => x > max),
            reason
        );
    }
        
    [Pure]
    public static ICellChangeResult CloneWithoutValuesBetween(this KeyValuePair<Position, Cell<int>> kvp, int minBoundary, int maxBoundary, ISingleReason reason)
    {
        return kvp.CloneWithoutValues(kvp.Value.PossibleValues.Where(x => x < maxBoundary && x > minBoundary),
            reason
        );
    }
        
    [Pure]
    public static ICellChangeResult CloneWithoutValuesBelow(this KeyValuePair<Position, Cell<int>> kvp, int min, ISingleReason reason)
    {
        if (kvp.Value.PossibleValues.Min>= min)
            return NoChange.Instance;

        return kvp.CloneWithoutValues(kvp.Value.PossibleValues.Where(x => x < min),reason);
    }

}
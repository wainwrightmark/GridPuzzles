using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Numerics;
using GridPuzzles.Bifurcation;
using GridPuzzles.Clues;

namespace GridPuzzles.Cells;

public interface ICell
{
    [Pure]
    public bool IsEmpty();
    [Pure]
    public bool HasSingleValue();
    [Pure]
    public int Count();
}

public interface ICell<T> : ICell, IImmutableSet<T>
    where T : struct
{
    [Pure]
    bool Contains(T i);
}

public interface ICell<T, TCell> : ICell<T>, IEquatable<TCell>
    where T : struct
    where TCell : ICell<T, TCell>, new()
{
    [Pure]
    TCell Intersect(TCell other);
    [Pure]
    TCell Union(TCell other);
    [Pure]
    TCell Except(TCell other);
    [Pure]
    bool IsSupersetOf(TCell other);
    [Pure]
    bool IsSubsetOf(TCell other);

    [Pure]
    TCell Add(T value);
    [Pure]
    TCell AddRange(IEnumerable<T> values);
    [Pure]
    TCell Remove(T value);


    [Pure]
    public IEnumerable<IBifurcationOption<T, TCell>> EnumerateBifurcationOptions(Position p, int maxValues)
    {
        var pvCount = Count();

        if (pvCount < 2)
            yield break;

        if (pvCount <= maxValues)
        {
            var choices = this
                .Select(value => CellHelper.Create<T, TCell>(value, p, BifurcationAttemptReason.Instance))
                .Select(cellUpdate => new BifurcationCellChoice<T, TCell>(cellUpdate));


            var bo = new BifurcationOption<T, TCell>(0, new PossibleValuesReason(p), choices);
            yield return bo;
        }
    }
}

public static class CellExtensions
{
    [Pure]
    public static bool CouldHaveAnyValue<T, TCell>(this TCell cell, IValueSource<T, TCell> valueSource) where T :struct where TCell : ICell<T, TCell>, new()
    {
        return valueSource.AnyValueCell.Equals(cell);
    }

    [Pure]
    public static IntCell ToIntCell(this IEnumerable<int> values) => new (values);

    [Pure]
    public static CharCell ToCharCell(this IEnumerable<char> values) => new (values);
}

public readonly record struct IntCell(BitVector32 _bitVector) :  ICell<int, IntCell>
{
    private readonly BitVector32 _bitVector = _bitVector;

    public IntCell(IEnumerable<int> possibilities) : this(new BitVector32(possibilities.Aggregate(0, (a, b) => a | (1 << b))))
    {
    }

    public IntCell() : this(new BitVector32(0))
    {
    }

    /// <inheritdoc />
    public IEnumerator<int> GetEnumerator()
    {
        var max = 31 - BitOperations.LeadingZeroCount((uint) _bitVector.Data);
        for (var i = 0; i <= max; i++) //Just 
        {
            var b = 1 << i;
            if (_bitVector[b])
            {
                yield return i;
            }
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count()=> BitOperations.PopCount((uint) _bitVector.Data);

    /// <inheritdoc />
    int IReadOnlyCollection<int>.Count => Count();

    /// <inheritdoc />
    IImmutableSet<int> IImmutableSet<int>.Add(int value) => Add(value);
    
    public IntCell Add(int value)
    {
        var v = _bitVector.Data | (1 << value);

        return new IntCell(new BitVector32(v));
    }
    
    public IntCell AddRange(IEnumerable<int> values)
    {
        var v = values.Aggregate(_bitVector.Data, (current, value) => current | (1 << value));


        return new IntCell(new BitVector32(v));
    }

    
    /// <inheritdoc />
    public IImmutableSet<int> Clear() => new IntCell(new BitVector32(0));
    
    public bool Contains(int i)
    {
        var b = 1 << i;
        return _bitVector[b];
    }

    /// <inheritdoc />
    public IImmutableSet<int> Except(IEnumerable<int> other)
    {
        if (other is IntCell ss) return Except(ss);
        
        return other.Where(IsInCorrectRange).Aggregate(this, (current, i) => current.Remove(i));
    }

    public IntCell Except(IntCell other)
    {
        return new IntCell(new BitVector32(_bitVector.Data & ~other._bitVector.Data));
    }
    

    /// <inheritdoc />
    public bool IsProperSubsetOf(IEnumerable<int> other)
    {
        if(other is IntCell ss) return IsProperSubsetOf(ss);

        var se = SymmetricExcept(other);

        if(se.Count == 0) return false; //sets were the same - not a proper subset

        if (Overlaps(se)) return false; //this contained elements not in the other set

        return true;
    }

    public bool IsProperSubsetOf(IntCell other)
    {
        return this.Count() < other.Count() && Except(other).IsEmpty();
    }

    /// <inheritdoc />
    public bool IsProperSupersetOf(IEnumerable<int> other)
    {
        if (other is IntCell ss) return IsProperSupersetOf(ss);

        var count = 0;
        foreach (var o in other)
        {
            if (!Contains(o)) return false;
            count++;
        }

        if (count >= this.Count()) return false;
        return true;
    }

    /// <inheritdoc />
    public bool IsSubsetOf(IEnumerable<int> other)
    {
        if(other is IntCell ss) return IsSubsetOf(ss);

        return Except(other).Count == 0;
    }

    public bool IsSubsetOf(IntCell other)
    {
        return Except(other).IsEmpty();
    }

    /// <inheritdoc />
    public bool IsSupersetOf(IEnumerable<int> other)
    {
        if (other is IntCell ss) return IsSupersetOf(ss);

        return other.All(Contains);
    }

    public bool IsSupersetOf(IntCell other)
    {
        return other.Except(this).IsEmpty();
    }
    
    public bool IsProperSupersetOf(IntCell other)
    {
        return this.Count() > other.Count() && IsSupersetOf(other);
    }

    public bool IsEmpty() => _bitVector.Data == 0;
    public bool HasSingleValue() => this.Count() == 1;

    private static bool IsInCorrectRange(int i) => i is >= 0 and <= 31;

    /// <inheritdoc />
    IImmutableSet<int> IImmutableSet<int>.Remove(int value) => Remove(value);

    public IntCell Remove(int value)
    {
        var bit = 1 << value;

        var newData = _bitVector.Data & ~bit;
        return new IntCell(new BitVector32(newData));
    }

    /// <inheritdoc />
    public bool SetEquals(IEnumerable<int> other)
    {
        return Equals(other.ToIntCell());
    }
    
    /// <inheritdoc />
    public bool TryGetValue(int equalValue, out int actualValue)
    {
        actualValue = equalValue;
        return Contains(equalValue);
    }

    /// <inheritdoc />
    public bool Overlaps(IEnumerable<int> other)
    {
        if (other is IntCell ss) return Overlaps(ss);
        return other.Any(Contains);
    }

    public bool Overlaps(IntCell other)
    {
        return (_bitVector.Data & other._bitVector.Data) != 0;
    }

    /// <inheritdoc />
    public IImmutableSet<int> SymmetricExcept(IEnumerable<int> other)
    {
        return SymmetricExcept(new IntCell(other));
    }

    public IntCell SymmetricExcept(IntCell other)
    {
        return new IntCell(new BitVector32(_bitVector.Data ^ other._bitVector.Data));
    }
    
    /// <inheritdoc />
    public IImmutableSet<int> Intersect(IEnumerable<int> other)
    {
        if (other is IntCell ss) return Intersect(ss);

        return Intersect(other.Where(IsInCorrectRange).ToIntCell());
    }

    public IntCell Intersect(IntCell other)
    {
        return new IntCell(new BitVector32(_bitVector.Data & other._bitVector.Data));
    }
    
    /// <inheritdoc />
    public IImmutableSet<int> Union(IEnumerable<int> other) => Union(other.ToIntCell());

    public IntCell Union(IntCell other)
    {
        return new IntCell(new BitVector32(_bitVector.Data | other._bitVector.Data));
    }

    public override string ToString() => "[" + string.Join("", this) + "]";
}
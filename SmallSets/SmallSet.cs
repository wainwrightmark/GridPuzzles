using System.Collections;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Numerics;

namespace SmallSets;

/// <summary>
/// A set that can only contain values 0 to 31 inclusive.
/// Very fast and memory efficient
/// </summary>
public readonly record struct SmallSet(BitVector32 _bitVector) : IImmutableSet<int>
{
    private readonly BitVector32 _bitVector = _bitVector;

    public SmallSet(IEnumerable<int> possibilities) : this(new BitVector32(possibilities.Aggregate(0, (a, b) => a | (1 << b))))
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

    /// <inheritdoc />
    int IReadOnlyCollection<int>.Count => BitOperations.PopCount((uint) _bitVector.Data);

    /// <inheritdoc />
    IImmutableSet<int> IImmutableSet<int>.Add(int value) => Add(value);
    
    public SmallSet Add(int value)
    {
        var v = _bitVector.Data | (1 << value);

        return new SmallSet(new BitVector32(v));
    }

    /// <inheritdoc />
    public IImmutableSet<int> Clear() => new SmallSet(new BitVector32(0));

    /// <inheritdoc />
    public bool Contains(int i)
    {
        var b = 1 << i;
        return _bitVector[b];
    }

    /// <inheritdoc />
    public IImmutableSet<int> Except(IEnumerable<int> other)
    {
        if (other is SmallSet ss) return Except(ss);
        
        return other.Where(IsInCorrectRange).Aggregate(this, (current, i) => current.Remove(i));
    }

    public SmallSet Except(SmallSet other)
    {
        return new SmallSet(new BitVector32(_bitVector.Data & ~other._bitVector.Data));
    }
    

    /// <inheritdoc />
    public bool IsProperSubsetOf(IEnumerable<int> other)
    {
        if(other is SmallSet ss) return IsProperSubsetOf(ss);

        var se = SymmetricExcept(other);

        if(se.Count == 0) return false; //sets were the same - not a proper subset

        if (Overlaps(se)) return false; //this contained elements not in the other set

        return true;
    }

    public bool IsProperSubsetOf(SmallSet other)
    {
        return this.Count() < other.Count() && Except(other).IsEmpty();
    }

    /// <inheritdoc />
    public bool IsProperSupersetOf(IEnumerable<int> other)
    {
        if (other is SmallSet ss) return IsProperSupersetOf(ss);

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
        if(other is SmallSet ss) return IsSubsetOf(ss);

        return Except(other).Count == 0;
    }

    public bool IsSubsetOf(SmallSet other)
    {
        return Except(other).IsEmpty();
    }

    /// <inheritdoc />
    public bool IsSupersetOf(IEnumerable<int> other)
    {
        if (other is SmallSet ss) return IsSupersetOf(ss);

        return other.All(Contains);
    }

    public bool IsSupersetOf(SmallSet other)
    {
        return other.Except(this).IsEmpty();
    }
    
    public bool IsProperSupersetOf(SmallSet other)
    {
        return this.Count() > other.Count() && IsSupersetOf(other);
    }

    public bool IsEmpty() => _bitVector.Data == 0;

    private static bool IsInCorrectRange(int i) => i is >= 0 and <= 31;

    /// <inheritdoc />
    IImmutableSet<int> IImmutableSet<int>.Remove(int value) => Remove(value);

    public SmallSet Remove(int value)
    {
        var bit = 1 << value;

        var newData = _bitVector.Data & ~bit;
        return new SmallSet(new BitVector32(newData));
    }

    /// <inheritdoc />
    public bool SetEquals(IEnumerable<int> other)
    {
        return Equals(other.ToSmallSet());
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
        if (other is SmallSet ss) return Overlaps(ss);
        return other.Any(Contains);
    }

    public bool Overlaps(SmallSet other)
    {
        return (_bitVector.Data & other._bitVector.Data) != 0;
    }

    /// <inheritdoc />
    public IImmutableSet<int> SymmetricExcept(IEnumerable<int> other)
    {
        return SymmetricExcept(new SmallSet(other));
    }

    public SmallSet SymmetricExcept(SmallSet other)
    {
        return new SmallSet(new BitVector32(_bitVector.Data ^ other._bitVector.Data));
    }
    
    /// <inheritdoc />
    public IImmutableSet<int> Intersect(IEnumerable<int> other)
    {
        if (other is SmallSet ss) return Intersect(ss);

        return Intersect(other.Where(IsInCorrectRange).ToSmallSet());
    }

    public SmallSet Intersect(SmallSet other)
    {
        return new SmallSet(new BitVector32(_bitVector.Data & other._bitVector.Data));
    }
    
    /// <inheritdoc />
    public IImmutableSet<int> Union(IEnumerable<int> other) => Union(other.ToSmallSet());

    public SmallSet Union(SmallSet other)
    {
        return new SmallSet(new BitVector32(_bitVector.Data | other._bitVector.Data));
    }
}
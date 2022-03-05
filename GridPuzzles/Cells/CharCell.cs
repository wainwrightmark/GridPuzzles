using System.Collections;
using System.Collections.Specialized;
using System.Numerics;

namespace GridPuzzles.Cells;


public readonly record struct CharCell(BitVector32 _bitVector) :  ICell<char, CharCell>
{
    private readonly BitVector32 _bitVector = _bitVector;

    public CharCell(IEnumerable<char> possibilities) : this(new BitVector32(possibilities.Aggregate(0, (a, b) => a | (1 << MapCharToInt(b)))))
    {
    }
    public CharCell() : this(new BitVector32(0))
    {
    }

    /// <inheritdoc />
    public IEnumerator<char> GetEnumerator()
    {
        var max = 31 - BitOperations.LeadingZeroCount((uint) _bitVector.Data);
        for (var i = 0; i <= max; i++) //Just 
        {
            var b = 1 << i;
            if (_bitVector[b])
            {
                yield return MapIntToChar(i);
            }
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count()=> BitOperations.PopCount((uint) _bitVector.Data);

    /// <inheritdoc />
    int IReadOnlyCollection<char>.Count => Count();

    /// <inheritdoc />
    IImmutableSet<char> IImmutableSet<char>.Add(char value) => Add(value);
    
    public CharCell Add(char value)
    {
        var v = _bitVector.Data | (1 << MapCharToInt(value));

        return new CharCell(new BitVector32(v));
    }

    public CharCell AddRange(IEnumerable<char> values)
    {
        var v = values.Aggregate(_bitVector.Data, (current, value) => current | (1 << MapCharToInt(value)));


        return new CharCell(new BitVector32(v));
    }

    /// <inheritdoc />
    public IImmutableSet<char> Clear() => new CharCell(new BitVector32(0));
    
    public bool Contains(char i)
    {
        var b = 1 << MapCharToInt(i);
        return _bitVector[b];
    }

    /// <inheritdoc />
    public IImmutableSet<char> Except(IEnumerable<char> other)
    {
        if (other is CharCell ss) return Except(ss);
        
        return other.Where(IsInCorrectRange).Aggregate(this, (current, i) => current.Remove(i));
    }

    public CharCell Except(CharCell other)
    {
        return new CharCell(new BitVector32(_bitVector.Data & ~other._bitVector.Data));
    }
    

    /// <inheritdoc />
    public bool IsProperSubsetOf(IEnumerable<char> other)
    {
        if(other is CharCell ss) return IsProperSubsetOf(ss);

        var se = SymmetricExcept(other);

        if(se.Count == 0) return false; //sets were the same - not a proper subset

        if (Overlaps(se)) return false; //this contained elements not in the other set

        return true;
    }

    public bool IsProperSubsetOf(CharCell other)
    {
        return this.Count() < other.Count() && Except(other).IsEmpty();
    }

    /// <inheritdoc />
    public bool IsProperSupersetOf(IEnumerable<char> other)
    {
        if (other is CharCell ss) return IsProperSupersetOf(ss);

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
    public bool IsSubsetOf(IEnumerable<char> other)
    {
        if(other is CharCell ss) return IsSubsetOf(ss);

        return Except(other).Count == 0;
    }

    public bool IsSubsetOf(CharCell other)
    {
        return Except(other).IsEmpty();
    }

    /// <inheritdoc />
    public bool IsSupersetOf(IEnumerable<char> other)
    {
        if (other is CharCell ss) return IsSupersetOf(ss);

        return other.All(Contains);
    }

    public bool IsSupersetOf(CharCell other)
    {
        return other.Except(this).IsEmpty();
    }
    
    public bool IsProperSupersetOf(CharCell other)
    {
        return this.Count() > other.Count() && IsSupersetOf(other);
    }

    public bool IsEmpty() => _bitVector.Data == 0;
    public bool HasSingleValue() => this.Count() == 1;

    private static bool IsInCorrectRange(char i) => i == '.' || char.IsLetter(i);

    /// <inheritdoc />
    IImmutableSet<char> IImmutableSet<char>.Remove(char value) => Remove(value);

    public CharCell Remove(char value)
    {
        var bit = 1 << MapCharToInt(value);

        var newData = _bitVector.Data & ~bit;
        return new CharCell(new BitVector32(newData));
    }

    /// <inheritdoc />
    public bool SetEquals(IEnumerable<char> other)
    {
        return Equals(other.ToCharCell());
    }
    
    /// <inheritdoc />
    public bool TryGetValue(char equalValue, out char actualValue)
    {
        actualValue = equalValue;
        return Contains(equalValue);
    }

    /// <inheritdoc />
    public bool Overlaps(IEnumerable<char> other)
    {
        if (other is CharCell ss) return Overlaps(ss);
        return other.Any(Contains);
    }

    public bool Overlaps(CharCell other)
    {
        return (_bitVector.Data & other._bitVector.Data) != 0;
    }

    /// <inheritdoc />
    public IImmutableSet<char> SymmetricExcept(IEnumerable<char> other)
    {
        return SymmetricExcept(new CharCell(other));
    }

    public CharCell SymmetricExcept(CharCell other)
    {
        return new CharCell(new BitVector32(_bitVector.Data ^ other._bitVector.Data));
    }
    
    /// <inheritdoc />
    public IImmutableSet<char> Intersect(IEnumerable<char> other)
    {
        if (other is CharCell ss) return Intersect(ss);

        return Intersect(other.Where(IsInCorrectRange).ToCharCell());
    }

    public CharCell Intersect(CharCell other)
    {
        return new CharCell(new BitVector32(_bitVector.Data & other._bitVector.Data));
    }
    
    /// <inheritdoc />
    public IImmutableSet<char> Union(IEnumerable<char> other) => Union(other.ToCharCell());

    public CharCell Union(CharCell other)
    {
        return new CharCell(new BitVector32(_bitVector.Data | other._bitVector.Data));
    }

    private static int MapCharToInt(char c)
    {
        switch (c)
        {
            case '.':
                return 0;
            default:
            {
                if (c >= 65 && c <= 90) return c - 64;//Capital letters
                if (c >= 97 && c <= 122) return c - 96; //Capital letters
                throw new Exception($"Cannot put '{c}' in a FastCell");
            }
        }
    }

    private static char MapIntToChar(int i)
    {
        if (i == 0) return '.';
        return (char)(i + 64);
    }

    public override string ToString() => "[" + string.Join("", this) + "]";

    
}

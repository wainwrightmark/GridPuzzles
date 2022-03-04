using System.Collections.Specialized;
using System.Numerics;
using GridPuzzles.Clues;

namespace GridPuzzles.Cells;

public interface ICell
{
    public bool HasSingleValue();
    public int PopCount();
}

public interface ICell<T> : ICell
    where T : struct
{
    bool HasValue(int i);

    IEnumerable<T> GetValues();
}

public interface ICell<T, TCell> : ICell<T>, IEquatable<TCell>
    where T : struct
    where TCell : ICell<T, TCell>
{
    TCell Intersect(TCell other);
    TCell Union(TCell other);
}

public static class CellExtensions
{
    public static bool CouldHaveAnyValue<T>(this T cell, IValueSource<T> valueSource) where  T : ICell
    {
        return valueSource.AnyValueCell.Equals(cell);
    }
}

public readonly record struct IntCell : ICell<int, IntCell>, ICell<char, IntCell>
{
    private readonly BitVector32 _bitVector;

    private IntCell(BitVector32 bitVector)=> _bitVector = bitVector;

    public IntCell(IEnumerable<int> possibilities)
    {
        var val = possibilities.Aggregate(0, (a, b) => 1 << a | 1 << b);
        _bitVector = new BitVector32(val);
    }
    
    public IntCell(IEnumerable<char> possibilities)
    {
        var val = possibilities.Aggregate(0, (a, b) => a | MapCharToInt(b));
        _bitVector = new BitVector32(val);
    }

    public IntCell Intersect(IntCell other)
    {
        return new IntCell(new BitVector32(_bitVector.Data & other._bitVector.Data));
    }
    
    public IntCell Union(IntCell other)
    {
        return new IntCell(new BitVector32(_bitVector.Data | other._bitVector.Data));
    }

    public bool HasValue(int i)
    {
        var b = 1 << i;
        return _bitVector[b];
    }

    /// <inheritdoc />
    IEnumerable<char> ICell<char>.GetValues()
    {
        return GetCharValues();
    }

    /// <inheritdoc />
    IEnumerable<int> ICell<int>.GetValues()
    {
        //31 - BitOperations.LeadingZeroCount((uint) _bitVector.Data);
        return GetValues(31);
    }

    public bool HasValue(char c)
    {
        var b = 1 << MapCharToInt(c);
        return _bitVector[b];
    }

    public int PopCount()
    {
        return System.Numerics.BitOperations.PopCount((uint) _bitVector.Data);
    }
    
    public bool HasSingleValue() => PopCount() == 1;

    public IEnumerable<int> GetValues(int max)
    {
        for (var i = 0; i <= max; i++) //Just 
        {
            var b = 1 << i;
            if (_bitVector[b])
            {
                yield return i;
            }
        }
    }

    public IEnumerable<char> GetCharValues()
    {
        for (var i = 0; i <= 26; i++) //Just 
        {
            var b = 1 << i;
            if (_bitVector[b])
            {
                yield return MapIntToChar(i);
            }
        }
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
    
}
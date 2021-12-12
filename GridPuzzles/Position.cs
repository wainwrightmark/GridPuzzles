using System.Text.RegularExpressions;

namespace GridPuzzles;

public readonly struct Position : IComparable<Position>
{
    public Position(ushort c, ushort r)
    {
        Column = c;
        Row = r;
    }
    public Position(int c, int r)
    {
        Column = Convert.ToUInt16(c);
        Row = Convert.ToUInt16(r);
    }

    /// <summary>
    /// 1, 1
    /// </summary>
    public static readonly Position Origin = new(1,1);

    /// <summary>
    /// 9, 9
    /// </summary>
    public static readonly Position NineNine = new(9,9);


    public ushort Column { get; }
    public ushort Row { get; }
    public override bool Equals(object? obj) => obj is Position p && Row == p.Row && Column == p.Column;

    public override int GetHashCode() => (Row << 16) | Column;

    public static bool operator ==(Position left, Position right) => left.Equals(right);

    public static bool operator !=(Position left, Position right) => !(left == right);

    /// <inheritdoc />
    public override string ToString() => Serialize();

    /// <inheritdoc />
    public int CompareTo(Position other)
    {
        var rowComparison = Row.CompareTo(other.Row);
        if (rowComparison != 0) return rowComparison;
        return Column.CompareTo(other.Column);
    }

    public static bool operator <(Position left, Position right) => left.CompareTo(right) < 0;

    public static bool operator <=(Position left, Position right) => left.CompareTo(right) <= 0;

    public static bool operator >(Position left, Position right) => left.CompareTo(right) > 0;

    public static bool operator >=(Position left, Position right) => left.CompareTo(right) >= 0;

    public string Serialize() => (char)('A' + Column - 1) + Row.ToString();

    /// <summary>
    /// Tries to deserialize strings like A1
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static Result<Position> Deserialize(string s)
    {
        var m = PositionRegex.Match(s);
        if (m.Success)
        {
            var row = int.Parse(m.Groups["row"].Value);
            var colChar = m.Groups["col"].Value.Single();

            var col = colChar - 'A' + 1;
            return new Position(col, row );
        }


        return Result.Failure<Position>("Could not deserialize");
    }

    private static readonly Regex PositionRegex = new(@"(?<col>[A-Z])(?<row>\d+)", RegexOptions.Compiled);
}
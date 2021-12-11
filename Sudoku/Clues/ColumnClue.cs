using System.Collections.Immutable;
using System.Linq;
using GridPuzzles;

namespace Sudoku.Clues;

public class ColumnClue<T> : ParallelClue<T> where T :notnull
{
    public ushort ColumnNumber { get; }

    public ColumnClue(ushort columnNumber, ushort rowStart, ushort columnLength) : 
        base( $"Column {(char) ('A' + columnNumber - 1)}")
    {
        ColumnNumber = columnNumber;
        Positions = Enumerable.Range(rowStart,  columnLength - rowStart + 1)
            .Select(x => new Position(columnNumber,
                x))
            .ToImmutableSortedSet();
    }

    /// <inheritdoc />
    public override Position GetParallelPosition(Position p)
    {
        return new Position(Index, p.Row);
    }

    /// <inheritdoc />
    public override Parallel Parallel => Parallel.Column;

    /// <inheritdoc />
    public override ushort Index => ColumnNumber;

    /// <inheritdoc />
    public override ImmutableSortedSet<Position> Positions { get; }
}
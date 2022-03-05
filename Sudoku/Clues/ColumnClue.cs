namespace Sudoku.Clues;

public class ColumnClue<T, TCell> : ParallelClue<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
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
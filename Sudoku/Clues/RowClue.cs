﻿namespace Sudoku.Clues;

public class RowClue<T, TCell> : ParallelClue<T, TCell>  where T :struct where TCell : ICell<T, TCell>, new()
{
    public ushort RowNumber { get; }

    public RowClue(ushort rowNumber, ushort rowStart, ushort rowEnd, ushort? level) :
        base(level.HasValue? $"Row {rowNumber} Level {level.Value}" : $"Row {rowNumber}"
        )
    {
        RowNumber = rowNumber;
        Positions = Enumerable.Range(rowStart, rowEnd - rowStart + 1)
            .Select(x => new Position(x,
                rowNumber))
            .ToImmutableSortedSet();
    }


    /// <inheritdoc />
    public override Position GetParallelPosition(Position p)
    {
        return new Position(p.Column, RowNumber);
    }

    /// <inheritdoc />
    public override Parallel Parallel => Parallel.Row;

    /// <inheritdoc />
    public override ushort Index => RowNumber;

    /// <inheritdoc />
    public override ImmutableSortedSet<Position> Positions { get; }
}
namespace Sudoku.Clues;

public class BoxClue<T> : BasicClue<T>  where T: notnull
{
    public BoxClue(Position topLeft, Position bottomRight, int index) : base($"Box {index}")
    {
        Positions = topLeft.GetPositionsBetween(bottomRight, true).SelectMany(x => x).ToImmutableSortedSet();
    }

    /// <inheritdoc />
    public override ImmutableSortedSet<Position> Positions { get; }
}
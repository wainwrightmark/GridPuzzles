namespace Sudoku.Clues;

public class BoxClue<T, TCell> : BasicClue<T, TCell>  where T :struct where TCell : ICell<T, TCell>, new()
{
    public BoxClue(Position topLeft, Position bottomRight, int index) : base($"Box {index}")
    {
        Positions = topLeft.GetPositionsBetween(bottomRight, true).SelectMany(x => x).ToImmutableSortedSet();
    }

    /// <inheritdoc />
    public override ImmutableSortedSet<Position> Positions { get; }
}
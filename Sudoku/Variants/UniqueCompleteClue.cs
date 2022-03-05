using Sudoku.Clues;

namespace Sudoku.Variants;

public class UniqueCompleteClue<T, TCell> : BasicClue<T, TCell>where T :struct where TCell : ICell<T, TCell>, new()
{
    /// <inheritdoc />
    public UniqueCompleteClue(string domain, IEnumerable<Position> positions) : base(domain)
    {
        Positions = positions.ToImmutableSortedSet();
    }

    /// <inheritdoc />
    public override ImmutableSortedSet<Position> Positions { get; }
}
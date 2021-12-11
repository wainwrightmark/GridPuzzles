using System.Collections.Generic;
using System.Collections.Immutable;
using GridPuzzles;
using Sudoku.Clues;

namespace Sudoku.Variants;

public class UniqueCompleteClue<T> : BasicClue<T>where T : notnull
{
    /// <inheritdoc />
    public UniqueCompleteClue(string domain, IEnumerable<Position> positions) : base(domain)
    {
        Positions = positions.ToImmutableSortedSet();
    }

    /// <inheritdoc />
    public override ImmutableSortedSet<Position> Positions { get; }
}
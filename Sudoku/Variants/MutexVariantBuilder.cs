using System.Collections.Generic;
using System.Linq;
using GridPuzzles;
using GridPuzzles.Clues;
using Sudoku.Clues;

namespace Sudoku.Variants;

public abstract class MutexVariantBuilder<T> : NoArgumentVariantBuilder<T>where T : notnull
{

    /// <inheritdoc />
    public override IEnumerable<IClue<T>> CreateClues(Position minPosition, Position maxPosition, IValueSource<T> valueSource,
        IReadOnlyCollection<IClue<T>> lowerLevelClues)
    {
        var biggerUniquenessClues =
            lowerLevelClues.OfType<IUniquenessClue<T>>()
                .Where(x => x.Positions.Count >= 2).ToList();

        return GetClues(minPosition, maxPosition).Where(x => !biggerUniquenessClues.Any(b => b.Positions.IsSupersetOf(x.Positions)));
    }

    protected abstract IEnumerable<UniquenessClue<T>> GetClues(Position minPosition, Position maxPosition);
        
    /// <inheritdoc />
    public override bool OnByDefault => false;
}
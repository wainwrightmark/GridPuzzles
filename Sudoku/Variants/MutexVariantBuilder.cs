namespace Sudoku.Variants;

public abstract class MutexVariantBuilder<T, TCell> : NoArgumentVariantBuilder<T, TCell>where T :struct where TCell : ICell<T, TCell>, new()
{

    /// <inheritdoc />
    public override IEnumerable<IClue<T, TCell>> CreateClues(Position minPosition, Position maxPosition, IValueSource<T, TCell> valueSource,
        IReadOnlyCollection<IClue<T, TCell>> lowerLevelClues)
    {
        var biggerUniquenessClues =
            lowerLevelClues.OfType<IUniquenessClue<T, TCell>>()
                .Where(x => x.Positions.Count >= 2).ToList();

        return GetClues(minPosition, maxPosition).Where(x => !biggerUniquenessClues.Any(b => b.Positions.IsSupersetOf(x.Positions)));
    }

    protected abstract IEnumerable<UniquenessClue<T, TCell>> GetClues(Position minPosition, Position maxPosition);
        
    /// <inheritdoc />
    public override bool OnByDefault => false;
}
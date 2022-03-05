namespace GridPuzzles.Clues;

public sealed class UniquenessClue<T, TCell> : IUniquenessClue<T, TCell>
    where T :struct where TCell : ICell<T, TCell>, new()
{

    /// <inheritdoc />
    public string Name => Domain;

    public string Domain { get; }

    /// <inheritdoc />
    public override string ToString() => Domain + "-" + string.Join(",", Positions);

    public UniquenessClue(Position position1, Position position2, string domain) 
        : this (ImmutableSortedSet.Create(position1, position2), domain) {}

    public UniquenessClue(ImmutableSortedSet<Position> positions, string domain)
    {
        Domain = domain;
        Positions = positions;
    }

    /// <inheritdoc />
    public ImmutableSortedSet<Position> Positions { get; }
}
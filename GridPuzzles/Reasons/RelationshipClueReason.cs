using GridPuzzles.Clues;

namespace GridPuzzles.Reasons;

public sealed record RelationshipClueReason<T, TCell>(IRelationshipClue<T, TCell> RelationshipClue): ISingleReason
    where T :struct where TCell : ICell<T, TCell>, new()
{
    /// <inheritdoc />
    public string Text => RelationshipClue.Name; //TODO look at this

    /// <inheritdoc />
    public IEnumerable<Position> GetContributingPositions(IGrid grid)
    {
        yield return RelationshipClue.Position1;
        yield return RelationshipClue.Position2;
    }

    /// <inheritdoc />
    public Maybe<IClue> Clue => Maybe<IClue>.From(RelationshipClue);
}
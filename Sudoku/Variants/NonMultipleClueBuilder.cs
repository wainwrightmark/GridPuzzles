namespace Sudoku.Variants;

public class NonMultipleClueBuilder : NoArgumentVariantBuilder
{
    public static readonly NonMultipleClueBuilder Instance = new();

    private NonMultipleClueBuilder()
    {
    }

    /// <inheritdoc />
    public override string Name => "Non Multiple";

    /// <inheritdoc />
    public override  int Level => 4;

    /// <inheritdoc />
    public override IEnumerable<IClue<int, IntCell>> CreateClues(Position minPosition, Position maxPosition, IValueSource valueSource,
        IReadOnlyCollection<IClue<int, IntCell>> lowerLevelClues)
    {
        var positionsChecked = new HashSet<Position>();

        foreach (var position in minPosition.GetPositionsBetween(maxPosition, true).SelectMany(x=>x))
        {
            positionsChecked.Add(position);

            var adjacentPositions = position.GetAdjacentPositions(minPosition, maxPosition)
                .Where(x => !positionsChecked.Contains(x));

            foreach (var adjacent in adjacentPositions)
            {
                yield return RelationshipClue.Create(position, adjacent, NonMultipleConstraint.Instance);
            }
        }
    }

    /// <inheritdoc />
    public override bool OnByDefault => false;

    /// <inheritdoc />
    public override bool IsValid(Position maxPosition) => true;


        

}
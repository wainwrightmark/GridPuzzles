namespace Sudoku.Variants;

public class NonConsecutiveVariantBuilder : NoArgumentVariantBuilder
{

    public static readonly NonConsecutiveVariantBuilder Instance = new();

    private NonConsecutiveVariantBuilder()
    {
    }

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
                yield return RelationshipClue.Create(position, adjacent, NonConsecutiveConstraint.Instance);
            }
        }
    }

    /// <inheritdoc />
    public override string Name => "Non Consecutive";

    /// <inheritdoc />
    public override int Level => 2;

    /// <inheritdoc />
    public override bool IsValid(Position maxPosition)
    {
        return true;
    }

    /// <inheritdoc />
    public override bool OnByDefault => false;
}
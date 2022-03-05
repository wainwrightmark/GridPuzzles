namespace Sudoku.Variants;

public class KingVariantBuilder <T, TCell> : MutexVariantBuilder<T, TCell>where T :struct where TCell : ICell<T, TCell>, new()
{
    public static KingVariantBuilder<T, TCell> Instance = new();

    private KingVariantBuilder()
    {
    }

    public override string Name => "King";
        

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments { get; } = new List<VariantBuilderArgument>();
        

    /// <inheritdoc />
    public override int Level => 3;

    /// <inheritdoc />
    protected override IEnumerable<UniquenessClue<T, TCell>> GetClues(Position minPosition, Position maxPosition)
    {
        var positionsChecked = new HashSet<Position>();

        foreach (var position in minPosition.GetPositionsBetween(maxPosition, true).SelectMany(x=>x))
        {
            positionsChecked.Add(position);

            var newKingsPositions =
                GetDiagonallyAdjacentPositions(position, minPosition, maxPosition)
                    .Where(x => !positionsChecked.Contains(x));

            foreach (var newKingsPosition in newKingsPositions)
                yield return new UniquenessClue<T, TCell>(position, newKingsPosition, "king's move");
        }
    }

    public static IEnumerable<Position> GetDiagonallyAdjacentPositions(Position position, Position min, Position max)
    {
        if (position.Column > min.Column)
        {
            if(position.Row > min.Row)
                yield return new Position(position.Column - 1, position.Row - 1);
            if(position.Row < max.Row)
                yield return new Position(position.Column - 1, position.Row + 1);
        }

        if (position.Column < max.Column)
        {
            if(position.Row > min.Row)
                yield return new Position(position.Column + 1, position.Row - 1);
            if(position.Row < max.Row)
                yield return new Position(position.Column + 1, position.Row + 1);

        }
    }
}
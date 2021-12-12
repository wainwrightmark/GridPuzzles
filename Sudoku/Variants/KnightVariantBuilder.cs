namespace Sudoku.Variants;

public class KnightVariantBuilder <T> : MutexVariantBuilder<T>where T : notnull
{

    public static KnightVariantBuilder<T> Instance = new();

    private KnightVariantBuilder()
    {
    }

    public override string Name => "Knight";
        

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments { get; } = new List<VariantBuilderArgument>();
        
    /// <inheritdoc />
    public override int Level => 3;

    /// <inheritdoc />
    protected override IEnumerable<UniquenessClue<T>> GetClues(Position minPosition, Position maxPosition)
    {
        var positionsChecked = new HashSet<Position>();

        foreach (var position in minPosition.GetPositionsBetween(maxPosition, true).SelectMany(x=>x))
        {
            positionsChecked.Add(position);

            var newKnightsPositions =
                GetKnightAdjacentPositions(position, minPosition, maxPosition)
                    .Where(x => !positionsChecked.Contains(x));

            foreach (var newKnightsPosition in newKnightsPositions)
                yield return new UniquenessClue<T>(position, newKnightsPosition, "knight's move");
        }
    }

    public static IEnumerable<Position> GetKnightAdjacentPositions(Position position, Position min , Position max)
    {
        var positions = KnightModifiers.Select(m =>
                (c: position.Column - m.col, r: position.Row - m.row))
            .Where(x => x.c >= min.Column && x.c <= max.Column)
            .Where(x => x.r >= min.Row && x.r <= max.Row)
            .Select(x => new Position(x.c, x.r));

        return positions;
    }

    // ReSharper disable once StaticMemberInGenericType
    private static readonly IReadOnlyCollection<(int col, int row)> KnightModifiers =
        new List<(int col, int row)>
        {
            (-2,1), (-2,-1), (-1,-2), (-1,2), (1,-2), (1,2), (2,1), (2,-1)
        };
}
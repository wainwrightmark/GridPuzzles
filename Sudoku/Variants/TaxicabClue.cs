namespace Sudoku.Variants;
///// <summary>
///// All the numbers in this group are consecutive (in some order) and unique
///// </summary>
//public class ConsecutiveGroup : IUniquenessClue, IRuleClue
//{
//    public ConsecutiveGroup(ImmutableSortedSet<Position> positions)
//    {
//        Positions = positions;
//    }

//    /// <inheritdoc />
//    public void GetCellUpdates(Grid grid, IClueResultBuilder<int> clueResultBuilder)
//    {
//        var cells = Positions.Select(grid.GetCellKVP).ToList();
//        HashSet<int>? allAllowedValues;

//        var maxDistance = Positions.Count - 1;
//        var totalValues = grid.ClueSource.ValueSource.AllValues.Count;
//        var minValue = grid.ClueSource.ValueSource.AllValues.Min;
//        var maxValue = grid.ClueSource.ValueSource.AllValues.Min;

//        foreach (var (_, cell) in cells)
//        {
//            if(cell.PossibleValues.Count + maxDistance >= totalValues)
//                continue; //cannot gain any information
//            HashSet<int>? allowedValues = null;
//            foreach (var cellPossibleValue in cell.PossibleValues)
//            {
//                var range = Enumerable.Range(cellPossibleValue - maxDistance, 1 + (maxDistance * 2)).ToHashSet();
//                if (allowedValues is null)
//                    allowedValues = range.ToHashSet();
//                else allowedValues.UnionWith(range);
//            }

//            if(allAllowedValues is null)

//        }

//    }

//    /// <inheritdoc />
//    public string Name => "Consecutive Group";

//    /// <inheritdoc />
//    public ImmutableSortedSet<Position> Positions { get; }
//}

public class TaxicabClue : IRuleClue
{
    public TaxicabClue(Position position, Position minPosition, Position maxPosition) //TODO change to some sort of option clue so we can use uniqueness
    {
        Position = position;
        MaxPosition = maxPosition;
        MinPosition = minPosition;

        Positions = new[] {Position}.ToImmutableSortedSet();
    }

    /// <inheritdoc />
    public string Name => "Taxicab";

    public Position Position { get; }

    public Position MinPosition { get; }

    public Position MaxPosition { get; }

    /// <inheritdoc />
    public ImmutableSortedSet<Position> Positions { get; }

    /// <inheritdoc />
    public IEnumerable<ICellChangeResult> CalculateCellUpdates(Grid grid)
    {
        var cell = grid.GetCell(Position);

        if (cell.HasSingleValue())
        {
            var cellValue = cell.Single();

            var taxiCabPositions = GetTaxicabPositions(Position, cellValue);

            foreach (var cell1 in taxiCabPositions.Select( grid.GetCellKVP))
            {
                yield return cell1.CloneWithoutValue(cellValue, 
                    new TaxicabReason(this)
                    //$"{Position} has value {cellValue} and is {cellValue} orthogonal cells away"
                );
            }
        }
    }


    private IEnumerable<Position> GetTaxicabPositions(Position startPosition, int distance)
    {
        for (var i = 0; i < distance; i++)
        {
            var horizontal = i;
            var  vertical = distance - i;

            var topRight = OffSet(horizontal, vertical);
            var bottomRight = OffSet(horizontal, -vertical);
            var bottomLeft = OffSet(-horizontal, -vertical);
            var topLeft = OffSet(-horizontal, vertical);

            foreach (var p in new []{topRight, bottomRight, bottomLeft, topLeft})
                if (p != null)
                    yield return p.Value;

        }

        Position? OffSet( int h, int v)
        {
            return IfValid(startPosition.Column + h, startPosition.Row + v);
        }

        Position? IfValid(int column, int row)
        {
            if (column >= MinPosition.Column && row >= MinPosition.Row && column <= MaxPosition.Column && row <= MaxPosition.Row)
                return new Position(column, row);

            return null;
        }
    }
}

public sealed record TaxicabReason(TaxicabClue TaxicabClue) : ISingleReason
{
    /// <inheritdoc />
    public string Text => "Taxicab";

    /// <inheritdoc />
    public IEnumerable<Position> GetContributingPositions(IGrid grid)
    {
        return TaxicabClue.Positions;
    }

    /// <inheritdoc />
    public Maybe<IClue> Clue => TaxicabClue;
}
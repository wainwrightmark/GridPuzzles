using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GridPuzzles;
using GridPuzzles.Cells;
using GridPuzzles.Clues;

namespace Crossword;

public abstract class CrosswordSymmetryClue : IRuleClue<char>
{

    /// <inheritdoc />
    public string Name => "Crossword Symmetry";

    public abstract IEnumerable<(bool horizontal, ushort index)> SymmetricalParallels { get; }

    /// <inheritdoc />
    public abstract ImmutableSortedSet<Position> Positions { get; }

    /// <inheritdoc />
    public abstract IEnumerable<ICellChangeResult> CalculateCellUpdates(Grid<char> grid);
}

public class CrosswordSymmetryClueRotational4 : CrosswordSymmetryClue
{

    public static CrosswordSymmetryClueRotational4 TryMake(Position position1, Position minPosition, Position maxPosition)
    {
        return new CrosswordSymmetryClueRotational4(position1, minPosition, maxPosition);
    }

    private CrosswordSymmetryClueRotational4(Position position, Position minPosition, Position maxPosition)
    {
        MinPosition = minPosition;
        MaxPosition = maxPosition;

        var positions = GetRotatedPositions(position, minPosition, maxPosition).ToImmutableSortedSet();
        Positions = positions;

        if(positions.Count == 1)
            SymmetricalParallels = new List<(bool horizontal, ushort index)>
            {
                (false, positions.Single().Column),
                (true, positions.Single().Row)
            };
        else SymmetricalParallels = new List<(bool horizontal, ushort index)>();


        SmallestPosition = positions.OrderBy(x => x.Column).ThenBy(x => x.Row).First();
    }

    public Position MinPosition { get; }
    public Position MaxPosition { get; }

    public Position SmallestPosition { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return "Rotational Symmetry 4 - " + SmallestPosition;
    }


    /// <inheritdoc />
    public override IEnumerable<(bool horizontal, ushort index)> SymmetricalParallels { get; }

    /// <inheritdoc />
    public override ImmutableSortedSet<Position> Positions { get; }

    /// <inheritdoc />
    public override IEnumerable<ICellChangeResult> CalculateCellUpdates(Grid<char> grid)
    {
        var cells = Positions.Select(grid.GetCellKVP).ToList();

        if (cells.Any(x => x.MustBeABlock()))
        {
            foreach (var cell in cells)
            {
                var update = cell.CloneWithOnlyValue(CrosswordValueSource.BlockChar, new CrosswordReason("Must be a block due to symmetry"));
                yield return (update);
            }
        }
        else if (cells.Any(x => x.MustNotBeABlock()))
        {
            foreach (var cell in cells)
            {
                var update = cell.CloneWithoutValue(CrosswordValueSource.BlockChar, new CrosswordReason("Must not be a block due to symmetry"));
                yield return (update);
            }
        }
    }


    private static IEnumerable<Position> GetRotatedPositions(Position p, Position minPosition, Position maxPosition)
    {
        var colOffset = p.Column - minPosition.Column;
        var rowOffset = p.Row - minPosition.Row;

        var oppositeColumn = maxPosition.Column - colOffset;
        var oppositeRow = maxPosition.Row - rowOffset;

        yield return p;
        yield return new Position(oppositeRow, p.Column);
        yield return new Position(oppositeColumn, oppositeRow);
        yield return new Position( p.Row, oppositeColumn);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is CrosswordSymmetryClueRotational4 r && SmallestPosition == r.SmallestPosition;
    }

    /// <inheritdoc />
    public override int GetHashCode() => SmallestPosition.GetHashCode();
}

public class CrosswordSymmetryClueRotational2 : CrosswordSymmetryClue
{
    public Position Position1 { get; }
    public Position Position2 { get; }
    public Position MinPosition { get; }
    public Position MaxPosition { get; }

    /// <inheritdoc />
    public override IEnumerable<(bool horizontal, ushort index)> SymmetricalParallels { get; }

    public static CrosswordSymmetryClueRotational2? TryMake(Position position1, Position minPosition, Position maxPosition)
    {
        var opp = GetOppositePosition(position1, minPosition, maxPosition);
        if (position1 == opp) return null;

        return new CrosswordSymmetryClueRotational2(position1, opp, minPosition, maxPosition);
    }

    private CrosswordSymmetryClueRotational2(Position position, Position oppositePosition, Position minPosition, Position maxPosition)
    {
        Position1 = position;
        Position2 = oppositePosition;
        MinPosition = minPosition;
        MaxPosition = maxPosition;
        Positions = new[] {Position1, Position2}.ToImmutableSortedSet();

        if (Position1.Column == Position2.Column) SymmetricalParallels = new []{(false, Position1.Column)} ;
        else if (Position1.Row == Position2.Row) SymmetricalParallels = new []{(true, Position1.Row)} ;
        else SymmetricalParallels = new List<(bool horizontal, ushort index)>();
    }

    /// <inheritdoc />
    public override ImmutableSortedSet<Position> Positions { get; }

    /// <inheritdoc />
    public override IEnumerable<ICellChangeResult> CalculateCellUpdates(Grid<char> grid)
    {
        var cell1 = grid.GetCellKVP(Position1);
        var cell2 = grid.GetCellKVP(Position2);

        if (cell1.CouldBeBlock())
        {
            if (cell1.Value.PossibleValues.Count == 1)
            {
                var update = cell2.CloneWithOnlyValue(CrosswordValueSource.BlockChar,new CrosswordReason( "Must be box due to symmetry"));
                yield return (update);
            }
            else if (!cell2.Value.CouldBeBlock()) //Cell 2 is not a box, therefore this should not be a box
            {
                var update = cell1.CloneWithoutValue(CrosswordValueSource.BlockChar, new CrosswordReason("Must not be a box due to symmetry"));
                yield return (update);
            }
            else if(cell2.Value.PossibleValues.Count == 1)//Cell 2 is a box therefore this should be a box
            {
                var update = cell1.CloneWithOnlyValue(CrosswordValueSource.BlockChar, new CrosswordReason("Must be box due to symmetry"));
                yield return (update);
            }
        }
        else
        {
            var update = cell2.CloneWithoutValue(CrosswordValueSource.BlockChar, new CrosswordReason("Must not be a box due to symmetry"));
            yield return (update);
        }
    }

    private static Position GetOppositePosition(Position p, Position minPosition, Position maxPosition)
    {
        var colOffset = p.Column - minPosition.Column;
        var rowOffset = p.Row - minPosition.Row;

        return new Position(maxPosition.Column - colOffset, maxPosition.Row - rowOffset);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "Box Symmetry " + Position1;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Position1.GetHashCode() + Position2.GetHashCode(); //order independent
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is CrosswordSymmetryClue csc && Positions.SetEquals(csc.Positions);
    }
}
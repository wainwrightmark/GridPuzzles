using System;
using GridPuzzles.Enums;

namespace Sudoku.Variants;

public class PivotArrowClue : IRuleClue<int>
{
    /// <inheritdoc />
    public string Name => "Pivot Arrow";

    public PivotArrowClue(Position centrePosition, int maxDistance,
        IReadOnlyCollection<CompassDirection> compassDirections)
    {
        CentrePosition = centrePosition;
        MaxDistance = maxDistance;
        CompassDirections = compassDirections;

        Positions = CompassDirections.SelectMany(x => x.GetAdjacentPositions(centrePosition, MaxDistance))
            .Prepend(centrePosition)
            .ToImmutableSortedSet();
    }


    public Position CentrePosition { get; }

    public int MaxDistance { get; }

    public IReadOnlyCollection<CompassDirection> CompassDirections { get; }

    /// <inheritdoc />
    public ImmutableSortedSet<Position> Positions { get; }

    /// <inheritdoc />
    public IEnumerable<ICellChangeResult> GetCellUpdates(Grid<int> grid)
    {

        throw new NotImplementedException("Pivot arrows are not yet supported");
        //if (CompassDirections.Count < 2)
        //{
        //    yield return new Contradiction("Pivot should have at least 2 CompassDirections",
        //        new[] { CentrePosition }
        //    );
        //    yield break;
        //}


        //var centreCell = grid.GetCellKVP(CentrePosition);

        //if (centreCell.Value.PossibleValues.Any(x => x > MaxDistance))
        //    yield return (centreCell.CloneWithOnlyValues(
        //        centreCell.Value.PossibleValues.Where(x => x <= MaxDistance), "Maximum pivot arrow length"));

        //var newMax = Math.Min(MaxDistance, centreCell.Value.PossibleValues.Max());


        //var groups = CompassDirections.Select(d =>
        //    d.GetAdjacentPositions(CentrePosition, newMax).Select(grid.GetCellKVP).ToList());

        //throw new NotImplementedException("Pivot arrows are not yet supported");

        //if (centreCell.Value.PossibleValues.Count == 1)
        //{

        //}
        //else
        //{

        //}
    }
}
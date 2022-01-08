using System;

namespace Sudoku.Variants;

public sealed class BetweenClue : IUniquenessClue<int>, IRuleClue<int>
{
    public BetweenClue(Position pAlpha, Position pOmega, ImmutableSortedSet<Position> middlePositions)
    {
        PAlpha = pAlpha;
        POmega = pOmega;
        MiddlePositions = middlePositions;
        Positions = middlePositions.Add(PAlpha).Add(POmega);
    }

    /// <inheritdoc />
    public string Name => $"Between {PAlpha}-{POmega}";

    /// <inheritdoc />
    public string Domain => Name;

    /// <inheritdoc />
    public ImmutableSortedSet<Position> Positions { get; }

    public Position PAlpha { get; }
    public Position POmega { get; }

    public ImmutableSortedSet<Position> MiddlePositions { get; }

    /// <inheritdoc />
    public IEnumerable<ICellChangeResult> CalculateCellUpdates(Grid<int> grid)
    {
        var cellAlpha = grid.GetCellKVP(PAlpha);
        var cellOmega = grid.GetCellKVP(POmega);

        var minDifference = MiddlePositions.Count;
        var canGoAZ = cellAlpha.Value.PossibleValues.Min + MiddlePositions.Count <
                      cellOmega.Value.PossibleValues.Max;
        var canGoZA = cellOmega.Value.PossibleValues.Min + MiddlePositions.Count <
                      cellAlpha.Value.PossibleValues.Max;

        var reason = new BetweenClueReason(this);

        if (!canGoAZ && !canGoZA)
        {
            yield return new Contradiction(
                reason,
                //$"Impossible to place {minDifference} values between {PAlpha} and {POmega}", 
                new[] { PAlpha, POmega }
            );
            yield break;
        }

        var otherCells = MiddlePositions.Select(grid.GetCellKVP).ToList();

        int minEdge;
        int maxEdge;

        if (canGoAZ && canGoZA)
        {
            minEdge = Math.Min(cellAlpha.Value.PossibleValues.Min, cellOmega.Value.PossibleValues.Min);
            maxEdge = Math.Max(cellAlpha.Value.PossibleValues.Max, cellOmega.Value.PossibleValues.Max);


            for (var i = cellAlpha.Value.PossibleValues.Max - minDifference;
                 i <= cellAlpha.Value.PossibleValues.Min + minDifference;
                 i++)
                yield return cellOmega.CloneWithoutValue(i,
                    reason
                    //$"Too close to the possible values of {cellAlpha}"
                );

            for (var i = cellOmega.Value.PossibleValues.Max - minDifference;
                 i <= cellOmega.Value.PossibleValues.Min + minDifference;
                 i++)
                yield return cellAlpha.CloneWithoutValue(i,
                    reason
                    //$"Too close to the possible values of {cellOmega}"
                );
        }
        else if (canGoAZ)
        {
            minEdge = cellAlpha.Value.PossibleValues.Min;
            maxEdge = cellOmega.Value.PossibleValues.Max;

            yield return cellAlpha.CloneWithoutValuesAbove(cellOmega.Value.PossibleValues.Max - minDifference - 1,
                reason);
            yield return cellOmega.CloneWithoutValuesBelow(cellAlpha.Value.PossibleValues.Min + minDifference + 1,
                reason);
        }
        else
        {
            minEdge = cellOmega.Value.PossibleValues.Min;
            maxEdge = cellAlpha.Value.PossibleValues.Max;

            yield return cellOmega.CloneWithoutValuesAbove(cellAlpha.Value.PossibleValues.Max - minDifference - 1,
                reason);
            yield return cellAlpha.CloneWithoutValuesBelow(cellOmega.Value.PossibleValues.Min + minDifference + 1,
                reason);
        }

        foreach (var oc in otherCells)
        {
            yield return oc.CloneWithoutValuesAbove(maxEdge - 1, reason);
            yield return oc.CloneWithoutValuesBelow(minEdge + 1, reason);
        }
    }
}
using System;

namespace Sudoku.Variants;

public sealed class BetweenClue : IUniquenessClue, IRuleClue
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
    public IEnumerable<ICellChangeResult> CalculateCellUpdates(Grid grid)
    {
        var cellAlpha = grid.GetCellKVP(PAlpha);
        var cellOmega = grid.GetCellKVP(POmega);

        var minDifference = MiddlePositions.Count;
        var canGoAZ = cellAlpha.Value.Min() + MiddlePositions.Count <
                      cellOmega.Value.Max();
        var canGoZA = cellOmega.Value.Min() + MiddlePositions.Count <
                      cellAlpha.Value.Max();

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
            minEdge = Math.Min(cellAlpha.Value.Min(), cellOmega.Value.Min());
            maxEdge = Math.Max(cellAlpha.Value.Max(), cellOmega.Value.Max());


            for (var i = cellAlpha.Value.Max() - minDifference;
                 i <= cellAlpha.Value.Min() + minDifference;
                 i++)
                yield return cellOmega.CloneWithoutValue(i,
                    reason
                    //$"Too close to the possible values of {cellAlpha}"
                );

            for (var i = cellOmega.Value.Max() - minDifference;
                 i <= cellOmega.Value.Min() + minDifference;
                 i++)
                yield return cellAlpha.CloneWithoutValue(i,
                    reason
                    //$"Too close to the possible values of {cellOmega}"
                );
        }
        else if (canGoAZ)
        {
            minEdge = cellAlpha.Value.Min();
            maxEdge = cellOmega.Value.Max();

            yield return cellAlpha.CloneWithoutValuesAbove(cellOmega.Value.Max() - minDifference - 1,
                reason);
            yield return cellOmega.CloneWithoutValuesBelow(cellAlpha.Value.Min() + minDifference + 1,
                reason);
        }
        else
        {
            minEdge = cellOmega.Value.Min();
            maxEdge = cellAlpha.Value.Max();

            yield return cellOmega.CloneWithoutValuesAbove(cellAlpha.Value.Max() - minDifference - 1,
                reason);
            yield return cellAlpha.CloneWithoutValuesBelow(cellOmega.Value.Min() + minDifference + 1,
                reason);
        }

        foreach (var oc in otherCells)
        {
            yield return oc.CloneWithoutValuesAbove(maxEdge - 1, reason);
            yield return oc.CloneWithoutValuesBelow(minEdge + 1, reason);
        }
    }
}
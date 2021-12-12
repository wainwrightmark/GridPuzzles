using System;

namespace Sudoku.Variants;

public sealed class LockoutClue : IRuleClue<int>
{
    public LockoutClue(Position pAlpha,
        Position pOmega,
        ImmutableSortedSet<Position> middlePositions,
        int minimumDifference)
    {
        PAlpha = pAlpha;
        POmega = pOmega;
        MiddlePositions = middlePositions;
        MinimumDifference = minimumDifference;
        Positions = middlePositions.Add(PAlpha).Add(POmega);
    }

    /// <inheritdoc />
    public string Name => $"NotBetween {PAlpha}-{POmega}";

    /// <inheritdoc />
    public ImmutableSortedSet<Position> Positions { get; }

    public Position PAlpha { get; }
    public Position POmega { get; }

    public ImmutableSortedSet<Position> MiddlePositions { get; }
    public int MinimumDifference { get; }

    /// <inheritdoc />
    public IEnumerable<ICellChangeResult> GetCellUpdates(Grid<int> grid)
    {
        var cellAlpha = grid.GetCellKVP(PAlpha);
        var cellOmega = grid.GetCellKVP(POmega);


        var canGoAZ = cellAlpha.Value.PossibleValues.Min + MinimumDifference <= cellOmega.Value.PossibleValues.Max;
        var canGoZA = cellOmega.Value.PossibleValues.Min + MinimumDifference <= cellAlpha.Value.PossibleValues.Max;

        var reason = new LockoutClueReason(this);

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

        int maxBottomEdge;
        int minTopEdge;

        if (canGoAZ && canGoZA)
        {
            maxBottomEdge = Math.Max(
                cellAlpha.Value.PossibleValues.Where(a =>
                    a + MinimumDifference <= cellOmega.Value.PossibleValues.Max).DefaultIfEmpty(0).Max(),
                cellOmega.Value.PossibleValues
                    .Where(a => a + MinimumDifference <= cellAlpha.Value.PossibleValues.Max)
                    .DefaultIfEmpty(0)
                    .Max()
            );
                
            minTopEdge = Math.Min(
                cellAlpha.Value.PossibleValues.Where(a =>
                        a  >= cellOmega.Value.PossibleValues.Min + MinimumDifference)
                    .DefaultIfEmpty(grid.ClueSource.ValueSource.AllValues.Max + 1).Min(),
                cellOmega.Value.PossibleValues
                    .Where(a => a  >= cellAlpha.Value.PossibleValues.Min + MinimumDifference)
                    .DefaultIfEmpty(grid.ClueSource.ValueSource.AllValues.Max + 1)
                    .Min()
            );
        }
        else if (canGoAZ)
        {
            maxBottomEdge = cellAlpha.Value.PossibleValues.Where(a =>
                a + MinimumDifference <= cellOmega.Value.PossibleValues.Max).DefaultIfEmpty(0).Max();
            minTopEdge = cellOmega.Value.PossibleValues
                .Where(a => a >= cellAlpha.Value.PossibleValues.Min + MinimumDifference)
                .DefaultIfEmpty(grid.ClueSource.ValueSource.AllValues.Max + 1)
                .Min();

            yield return cellAlpha.CloneWithoutValuesAbove(maxBottomEdge, reason);
            yield return cellOmega.CloneWithoutValuesBelow(minTopEdge, reason);
        }
        else
        {
            maxBottomEdge = cellOmega.Value.PossibleValues.Where(a =>
                a + MinimumDifference <= cellAlpha.Value.PossibleValues.Max).DefaultIfEmpty(0).Max();
            minTopEdge = cellAlpha.Value.PossibleValues
                .Where(a => a >= cellOmega.Value.PossibleValues.Min + MinimumDifference)
                .DefaultIfEmpty(grid.ClueSource.ValueSource.AllValues.Max + 1)
                .Min();

            yield return cellOmega.CloneWithoutValuesAbove(maxBottomEdge, reason);
            yield return cellAlpha.CloneWithoutValuesBelow(minTopEdge, reason);
        }

        foreach (var oc in otherCells)
        {
            yield return oc.CloneWithoutValuesBetween(maxBottomEdge - 1, minTopEdge + 1, reason);
        }
    }
}
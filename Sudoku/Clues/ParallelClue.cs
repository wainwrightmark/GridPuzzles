using System.Collections.Generic;
using System.Linq;
using GridPuzzles;

namespace Sudoku.Clues;

public abstract class ParallelClue<T> :BasicClue<T>, IParallelClue<T>where T : notnull
{
    /// <inheritdoc />
    protected ParallelClue(string domain) : base(domain)
    {
    }

    /// <summary>
    /// Gets the parallel cell to this in this line
    /// </summary>
    public abstract Position GetParallelPosition(Position p);

    public abstract Parallel Parallel { get; }


    public abstract ushort Index { get; }



    //public IEnumerable<ICellChangeResult> GetSwordfishUpdates
    //    (Grid<T> grid, Dictionary<T, IReadOnlyCollection<KeyValuePair<Position, Cell<T>>>> cellsByValue)
    //{
    //    foreach (var (element,myCellPositions) in cellsByValue.Where(x=>x.Value.Count == 2))
    //    {
    //        var (startPosition, finishPosition) = myCellPositions.Select(x => x.Key).OrderBy(x => x).GetFirstTwo();

    //        var parallels = grid.ClueSource.GetParallelClues(Parallel)
    //            .Where(x => x.Parallel == Parallel && x.Index > Index ).ToList();

    //        var swordfishPositions =
    //            FindSwordfishPositions(element, startPosition, finishPosition, parallels, Parallel, grid);

    //        if (swordfishPositions == null) continue;
    //        var orthogonalIndices = swordfishPositions.Select(x => x.GetOtherIndex(Parallel)).ToHashSet();

    //        var orthogonals =
    //            grid.ClueSource.GetParallelClues(Parallel.GetOrthogonal())
    //                .Where(x => orthogonalIndices.Contains(x.Index))
    //                .ToList();

    //        foreach (var cellToChange in orthogonals
    //            .SelectMany(x => x.Positions)
    //            .Except(new[] {startPosition, finishPosition}.Concat(swordfishPositions))
    //            .Select(grid.GetCellKVP)
    //            .Where(x => x.Value.PossibleValues.Contains(element)))
    //        {
    //            yield return (cellToChange.CloneWithoutValue(element, $"Swordfish across {element} in {Domain}."));
    //        }

    //    }
    //}

    private static IReadOnlyCollection<Position>? FindSwordfishPositions(T element,
        Position pStart,
        Position pFinish,
        IReadOnlyCollection<ParallelClue<T>> parallels,
        Parallel parallel,
        Grid<T> grid)
    {
        foreach (var parallelClue in parallels)
        {
            var position = parallelClue.GetParallelPosition(pStart);
            var cell = grid.GetCell(position);

            if (cell.PossibleValues.Count == 1 || !cell.PossibleValues.Contains(element)) continue;

            var parallelMatchingCells = parallelClue.Positions
                .Where(x=> x != position)
                .Where(x=> grid.GetCell(x).PossibleValues.Contains(element)).ToList();
            if (parallelMatchingCells.Count != 1) continue;

            var otherPosition = parallelMatchingCells.Single();

            if (otherPosition.GetOtherIndex(parallel) == pFinish.GetOtherIndex(parallel))
            {
                //we're done - we've matched the finish
                return new[] {position, otherPosition};
            }

            var newParallels = parallels.Where(x => x != parallelClue).ToList();

            var positions =
                FindSwordfishPositions(element, otherPosition, pFinish, newParallels, parallel, grid);

            if (positions != null)
                return new[] {position, otherPosition}.Concat(positions).ToList();
        }

        return null;
    }


}
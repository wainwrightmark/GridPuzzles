using System.Threading;
using System.Threading.Tasks;
using MoreLinq;

namespace GridPuzzles.Bifurcation;

public static class RandomSolver
{
    private sealed class SolveNode<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
    {
        private SolveNode(Grid<T, TCell> grid, IEnumerable<UpdateResult<T, TCell>> remainingUpdates, Random random)
        {
            Grid = grid;
            RemainingUpdates = new Stack<UpdateResult<T, TCell>>(remainingUpdates.Shuffle(random));
            OriginalUpdates = RemainingUpdates.Count;
        }

        public static Maybe<SolveNode<T, TCell>> TryCreate(Grid<T, TCell> grid, Random random)
        {
            const int max = 10000;
            var bifurcationOptions =
                grid.ClueSource.BifurcationClueHelper.CalculateBifurcationOptions(grid, grid.AllPositions, max).ToList();

            foreach (var (position, cell) in grid.Cells)
                bifurcationOptions.AddRange(cell.EnumerateBifurcationOptions(position, 10000));

            var option = bifurcationOptions
                .GroupBy(x=> (x.ChoiceCount, -x.Priority))
                .OrderBy(x=>x.Key)
                .Take(1)
                .SelectMany(x=>x.RandomSubset(1, random))
                .FirstOrDefault();

            if (option != null)
                return new SolveNode<T, TCell>(grid, option.Choices.Select(x => x.UpdateResult), random);

            //special case for first cell
            if (grid.Cells.All(x =>
                    x.Value.HasSingleValue() && x.Value.All(c => c.ToString() == ".")))
            {
                var cellToChange = grid
                    .AllCells
                    .Where(x => x.Value.Count() > 1)
                    .Shuffle(random)
                    .First();


                var updateResults = cellToChange.Value.Select(x =>
                        UpdateResult<T, TCell>.Empty.CloneWithCellChangeResult(
                            cellToChange.CloneWithOnlyValue(x, new PossibleValuesReason(cellToChange.Key))));

                return new SolveNode<T, TCell>(grid, updateResults, random);
            }

            return Maybe<SolveNode<T, TCell>>.None;
        }


        public Grid<T, TCell> Grid { get; }

        public int OriginalUpdates { get; }

        public Stack<UpdateResult<T, TCell>> RemainingUpdates { get; }

        public int Hits { get; set; } = 0;

        /// <inheritdoc />
        public override string ToString() => $"{Grid} ({RemainingUpdates.Count}/{OriginalUpdates} Options Remain)";
    }

    public static IEnumerable<(Grid<T, TCell> Grid, UpdateResult<T, TCell> UpdateResult)> RandomSolveIncremental<T, TCell>(this Grid<T, TCell> initialGrid, Random? random, CancellationToken cancellation)
        where T :struct where TCell : ICell<T, TCell>, new()
    {
        UpdateResult<T, TCell> initialUpdateResult;

        (initialGrid, initialUpdateResult) = initialGrid.IterateRepeatedly(UpdateResultCombiner<T, TCell>.Fast, 0);

        if (initialUpdateResult.HasContradictions)
            yield break;

        var rand = random ?? new Random();

        var sn1 = SolveNode<T, TCell>.TryCreate(initialGrid, rand);

        if (sn1.HasNoValue)
            yield break;

        var triedGrids = new Dictionary<Grid<T, TCell>, SolveNode<T, TCell>>()
        {
            { initialGrid, sn1.Value }
        };
        var nodesToTry = new Stack<SolveNode<T, TCell>>();

        var backlogNodes = new Stack<SolveNode<T, TCell>>();

        nodesToTry.Push(sn1.Value);

        const uint triesBeforeBackTrack = 3;

        while (nodesToTry.TryPop(out var currentNode))
        {
            if(cancellation.IsCancellationRequested)
                yield break;

            if (!nodesToTry.Any() && backlogNodes.Any())
            {
                nodesToTry = new Stack<SolveNode<T, TCell>>(backlogNodes);
                backlogNodes = new Stack<SolveNode<T, TCell>>();
            }

            currentNode.Hits++;
            if (currentNode.Hits > 0 && (currentNode.Hits % triesBeforeBackTrack == 0) && nodesToTry.Any())
            {
                backlogNodes.Push(currentNode);
                continue;
            }

            if (currentNode.RemainingUpdates.TryPop(out var updateResult))
            {
                if (currentNode.RemainingUpdates.Any())
                    nodesToTry.Push(currentNode);

                var (newGrid, newUpdateResult) = currentNode.Grid.IterateRepeatedly(UpdateResultCombiner<T, TCell>.Fast, 0,updateResult);

                if (newUpdateResult.HasContradictions)
                {
                    continue;
                }

                if (newGrid.IsComplete)
                {
                    yield return (newGrid, newUpdateResult);
                    yield break;
                }

                if (triedGrids.ContainsKey(newGrid))
                    continue;

                var newSolveNode = SolveNode<T, TCell>.TryCreate(newGrid, rand);

                if (newSolveNode.HasValue)
                {
                    triedGrids.Add(newGrid, newSolveNode.Value);
                    nodesToTry.Push(newSolveNode.Value);
                    yield return (newGrid, newUpdateResult);
                }
            }
        }
    }

    public static Maybe<Grid<T, TCell>> RandomSolve<T, TCell>(this Grid<T, TCell> initialGrid, Random? random) where T :struct where TCell : ICell<T, TCell>, new()
    {
        UpdateResult<T, TCell> initialUpdateResult;

        (initialGrid, initialUpdateResult) = initialGrid.IterateRepeatedly(UpdateResultCombiner<T, TCell>.Fast, 0);

        if (initialUpdateResult.HasContradictions)
            return Maybe<Grid<T, TCell>>.None;

        var rand = random ?? new Random();

        var sn1 = SolveNode<T, TCell>.TryCreate(initialGrid, rand);

        if (sn1.HasNoValue)
            return Maybe<Grid<T, TCell>>.None;

        var triedGrids = new Dictionary<Grid<T, TCell>, SolveNode<T, TCell>>()
        {
            { initialGrid, sn1.Value }
        };
        var nodesToTry = new Stack<SolveNode<T, TCell>>();

        var backlogNodes = new Stack<SolveNode<T, TCell>>();

        nodesToTry.Push(sn1.Value);

        const uint triesBeforeBackTrack = 3;

        while (nodesToTry.TryPop(out var currentNode))
        {
            if (!nodesToTry.Any() && backlogNodes.Any())
            {
                nodesToTry = new Stack<SolveNode<T, TCell>>(backlogNodes);
                backlogNodes = new Stack<SolveNode<T, TCell>>();
            }

            currentNode.Hits++;
            if (currentNode.Hits > 0 && (currentNode.Hits % triesBeforeBackTrack == 0) && nodesToTry.Any())
            {
                backlogNodes.Push(currentNode);
                continue;
            }

            if (currentNode.RemainingUpdates.TryPop(out var updateResult))
            {
                if (currentNode.RemainingUpdates.Any())
                    nodesToTry.Push(currentNode);

                var (newGrid, newUpdateResult) = currentNode.Grid.IterateRepeatedly(UpdateResultCombiner<T, TCell>.Fast,0,updateResult);
                    
                if (newUpdateResult.HasContradictions)
                    continue;
                if (newGrid.IsComplete)
                    return newGrid;

                if (triedGrids.ContainsKey(newGrid))
                    continue;

                var newSolveNode = SolveNode<T, TCell>.TryCreate(newGrid, rand);

                if (newSolveNode.HasValue)
                {
                    triedGrids.Add(newGrid, newSolveNode.Value);
                    nodesToTry.Push(newSolveNode.Value);
                }
            }
        }

        return Maybe<Grid<T, TCell>>.None;
    }


    /// <summary>
    /// Apply a random change
    /// </summary>
    public static (Grid<T, TCell> grid, UpdateResult<T, TCell> updateResult) RandomIterate<T, TCell>(
        this Grid<T, TCell> grid,
        Random? random) where T :struct where TCell : ICell<T, TCell>, new()
    {
        random ??= new Random();

        var node = SolveNode<T, TCell>.TryCreate(grid, random);

        if (node.HasNoValue)
            return (grid, UpdateResult<T, TCell>.Empty);

        while (node.Value.RemainingUpdates.TryPop(out var updateResult))
        {
            var (newGrid, updateResult2) = grid.IterateRepeatedly(UpdateResultCombiner<T, TCell>.Fast,0,updateResult);

            if (!updateResult2.HasContradictions) //otherwise this update is no good, try another one
                return (newGrid, updateResult2);
        }

        return (grid, UpdateResult<T, TCell>.Empty);
    }

    public static async Task<(Grid<T, TCell> grid, UpdateResult<T, TCell> updateResult)> RandomIterateAsync<T, TCell>(
        this Grid<T, TCell> startGrid)where T :struct where TCell : ICell<T, TCell>, new()
    {
        var r = await Task.Run(() => RandomIterate(startGrid, null)).ConfigureAwait(false);

        return r;
    }

    public static async Task<Maybe<Grid<T, TCell>>> RandomSolveAsync<T, TCell>(this Grid<T, TCell> startGrid)where T :struct where TCell : ICell<T, TCell>, new()
    {
        var r = await Task.Run(() => RandomSolve(startGrid, null)).ConfigureAwait(false);

        return r;
    }
}
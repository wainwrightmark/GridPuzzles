using System.Threading;
using System.Threading.Tasks;
using MoreLinq;

namespace GridPuzzles.Bifurcation;

public static class RandomSolver
{
    private sealed class SolveNode<T> where T: notnull
    {
        private SolveNode(Grid<T> grid, IEnumerable<UpdateResult<T>> remainingUpdates, Random random)
        {
            Grid = grid;
            RemainingUpdates = new Stack<UpdateResult<T>>(remainingUpdates.Shuffle(random));
            OriginalUpdates = RemainingUpdates.Count;
        }

        public static Maybe<SolveNode<T>> TryCreate(Grid<T> grid, Random random)
        {
            const int max = 10000;
            var bifurcationOptions =
                grid.ClueSource.BifurcationClueHelper.CalculateBifurcationOptions(grid, grid.AllPositions, max).ToList();

            foreach (var (position, cell) in grid.Cells)
                bifurcationOptions.AddRange(cell.GetBifurcationOptions(position, 10000));

            var option = bifurcationOptions
                .GroupBy(x=> (x.ChoiceCount, -x.Priority))
                .OrderBy(x=>x.Key)
                .Take(1)
                .SelectMany(x=>x.RandomSubset(1, random))
                .FirstOrDefault();

            if (option != null)
                return new SolveNode<T>(grid, option.Choices.Select(x => x.UpdateResult), random);

            //special case for first cell
            if (grid.Cells.All(x =>
                    x.Value.PossibleValues.Count == 1 && x.Value.PossibleValues.All(c => c.ToString() == ".")))
            {
                var cellToChange = grid
                    .AllCells
                    .Where(x => x.Value.PossibleValues.Count > 1)
                    .Shuffle(random)
                    .First();


                var updateResults = cellToChange.Value.PossibleValues
                    .Select(x =>
                        UpdateResult<T>.Empty.CloneWithCellChangeResult(
                            cellToChange.CloneWithOnlyValue(x, new PossibleValuesReason(cellToChange.Key))));

                return new SolveNode<T>(grid, updateResults, random);
            }

            return Maybe<SolveNode<T>>.None;
        }


        public Grid<T> Grid { get; }

        public int OriginalUpdates { get; }

        public Stack<UpdateResult<T>> RemainingUpdates { get; }

        public int Hits { get; set; } = 0;

        /// <inheritdoc />
        public override string ToString() => $"{Grid} ({RemainingUpdates.Count}/{OriginalUpdates} Options Remain)";
    }

    public static IEnumerable<(Grid<T> Grid, UpdateResult<T> UpdateResult)> RandomSolveIncremental<T>(this Grid<T> initialGrid, Random? random, CancellationToken cancellation)
        where T : notnull
    {
        UpdateResult<T> initialUpdateResult;

        (initialGrid, initialUpdateResult) = initialGrid.IterateRepeatedly(UpdateResultCombiner<T>.Fast, 0);

        if (initialUpdateResult.HasContradictions)
            yield break;

        var rand = random ?? new Random();

        var sn1 = SolveNode<T>.TryCreate(initialGrid, rand);

        if (sn1.HasNoValue)
            yield break;

        var triedGrids = new Dictionary<Grid<T>, SolveNode<T>>()
        {
            { initialGrid, sn1.Value }
        };
        var nodesToTry = new Stack<SolveNode<T>>();

        var backlogNodes = new Stack<SolveNode<T>>();

        nodesToTry.Push(sn1.Value);

        const uint triesBeforeBackTrack = 3;

        while (nodesToTry.TryPop(out var currentNode))
        {
            if(cancellation.IsCancellationRequested)
                yield break;

            if (!nodesToTry.Any() && backlogNodes.Any())
            {
                nodesToTry = new Stack<SolveNode<T>>(backlogNodes);
                backlogNodes = new Stack<SolveNode<T>>();
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

                var (newGrid, newUpdateResult) = currentNode.Grid.IterateRepeatedly(UpdateResultCombiner<T>.Fast, 0,updateResult);

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

                var newSolveNode = SolveNode<T>.TryCreate(newGrid, rand);

                if (newSolveNode.HasValue)
                {
                    triedGrids.Add(newGrid, newSolveNode.Value);
                    nodesToTry.Push(newSolveNode.Value);
                    yield return (newGrid, newUpdateResult);
                }
            }
        }
    }

    public static Maybe<Grid<T>> RandomSolve<T>(this Grid<T> initialGrid, Random? random) where T:notnull
    {
        UpdateResult<T> initialUpdateResult;

        (initialGrid, initialUpdateResult) = initialGrid.IterateRepeatedly(UpdateResultCombiner<T>.Fast, 0);

        if (initialUpdateResult.HasContradictions)
            return Maybe<Grid<T>>.None;

        var rand = random ?? new Random();

        var sn1 = SolveNode<T>.TryCreate(initialGrid, rand);

        if (sn1.HasNoValue)
            return Maybe<Grid<T>>.None;

        var triedGrids = new Dictionary<Grid<T>, SolveNode<T>>()
        {
            { initialGrid, sn1.Value }
        };
        var nodesToTry = new Stack<SolveNode<T>>();

        var backlogNodes = new Stack<SolveNode<T>>();

        nodesToTry.Push(sn1.Value);

        const uint triesBeforeBackTrack = 3;

        while (nodesToTry.TryPop(out var currentNode))
        {
            if (!nodesToTry.Any() && backlogNodes.Any())
            {
                nodesToTry = new Stack<SolveNode<T>>(backlogNodes);
                backlogNodes = new Stack<SolveNode<T>>();
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

                var (newGrid, newUpdateResult) = currentNode.Grid.IterateRepeatedly(UpdateResultCombiner<T>.Fast,0,updateResult);
                    
                if (newUpdateResult.HasContradictions)
                    continue;
                if (newGrid.IsComplete)
                    return newGrid;

                if (triedGrids.ContainsKey(newGrid))
                    continue;

                var newSolveNode = SolveNode<T>.TryCreate(newGrid, rand);

                if (newSolveNode.HasValue)
                {
                    triedGrids.Add(newGrid, newSolveNode.Value);
                    nodesToTry.Push(newSolveNode.Value);
                }
            }
        }

        return Maybe<Grid<T>>.None;
    }


    /// <summary>
    /// Apply a random change
    /// </summary>
    public static (Grid<T> grid, UpdateResult<T> updateResult) RandomIterate<T>(
        this Grid<T> grid,
        Random? random) where T: notnull
    {
        random ??= new Random();

        var node = SolveNode<T>.TryCreate(grid, random);

        if (node.HasNoValue)
            return (grid, UpdateResult<T>.Empty);

        while (node.Value.RemainingUpdates.TryPop(out var updateResult))
        {
            var (newGrid, updateResult2) = grid.IterateRepeatedly(UpdateResultCombiner<T>.Fast,0,updateResult);

            if (!updateResult2.HasContradictions) //otherwise this update is no good, try another one
                return (newGrid, updateResult2);
        }

        return (grid, UpdateResult<T>.Empty);
    }

    public static async Task<(Grid<T> grid, UpdateResult<T> updateResult)> RandomIterateAsync<T>(
        this Grid<T> startGrid)where T :notnull
    {
        var r = await Task.Run(() => RandomIterate(startGrid, null)).ConfigureAwait(false);

        return r;
    }

    public static async Task<Maybe<Grid<T>>> RandomSolveAsync<T>(this Grid<T> startGrid)where T :notnull
    {
        var r = await Task.Run(() => RandomSolve(startGrid, null)).ConfigureAwait(false);

        return r;
    }
}
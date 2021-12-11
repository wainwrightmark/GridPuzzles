using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;
using GridPuzzles.Cells;
using GridPuzzles.Reasons;
using MoreLinq;

namespace GridPuzzles.Bifurcation;

public class BifurcationNode<T> where T : notnull
{
    /// <summary>
    /// Create a new bifurcation node.
    /// </summary>
    protected BifurcationNode(Grid<T> grid, UpdateResult<T> updateResult,
        ImmutableHashSet<IBifurcationChoice<T>> choicesNotToTake)
    {
        Grid = grid;
        UpdateResult = updateResult;
        ChoicesNotToTake = choicesNotToTake;
        IsFinished = grid.IsComplete;
        UsedChoices = ChoicesNotToTake.ToBuilder();
        if (!updateResult.HasContradictions && grid.IsComplete)
        {
            //var (_, contradictionCheck) =
            //    grid.Iterate(UpdateResultCombiner<T>.Fast,
            //        Maybe<IReadOnlyCollection<Position>>.None);

            //if (contradictionCheck.HasContradictions)
            //{
            //    UpdateResult = updateResult.Combine(contradictionCheck, out _);
            //}
            //else
            //{
            //    CompleteGrids = new[] { grid };
            //}
            CompleteGrids = new[] { grid };
        }
    }

    public static BifurcationNode<T> CreateTopLevel(Grid<T> grid) => new(grid, UpdateResult<T>.Empty,
        ImmutableHashSet<IBifurcationChoice<T>>.Empty);


    public Grid<T> Grid { get; private set; }

    public UpdateResult<T> UpdateResult { get; private set; }

    private Maybe<UpdateResult<T>> UpdateResultToPropagate { get; set; }

    public int LevelsDescended { get; private set; }

    /// <summary>
    /// Choices precluded by a different step
    /// </summary>
    public ImmutableHashSet<IBifurcationChoice<T>> ChoicesNotToTake { get; }

    private ImmutableHashSet<IBifurcationChoice<T>>.Builder UsedChoices { get; }

    public virtual int InitialHeight => 0;

    private Dictionary<int, List<PossibleChoice>>? NextLevelChoices { get; set; }

    private HashSet<ChoiceNode<T>> NextLevelNodes { get; } = new();

    public bool IsFinished { get; private set; }

    public string StateString
    {
        get
        {
            if (CompleteGrids != null)
                return "Complete";
            if (UpdateResult.HasContradictions)
                return "Contradiction";
            if (IsFinished)
                return "Finished";
            return $"Level {LevelsDescended}";
        }
    }

    /// <summary>
    /// Possible complete grid that the Grid could lead to.
    /// </summary>
    public IReadOnlyCollection<Grid<T>>? CompleteGrids { get; private set; } = null;

    private void ApplyUpdate(UpdateResult<T> updateResult)
    {
        if (!updateResult.IsNotEmpty) return;

        if (UpdateResultToPropagate.HasValue)
        {
            updateResult = updateResult.Combine(UpdateResultToPropagate.Value, out _);
            UpdateResultToPropagate = Maybe<UpdateResult<T>>.None;
        }

        UpdateResult<T> newUpdateResult;

        (Grid, newUpdateResult) = Grid.IterateRepeatedly(UpdateResultCombiner<T>.Fast, updateResult);

        foreach (var choiceNode in NextLevelNodes)
            choiceNode.ApplyUpdate(newUpdateResult);

        UpdateResult = UpdateResult.Combine(newUpdateResult, out _);
    }


    public void Descend()
    {
        if (UpdateResult.HasContradictions)
            return;

        if (Grid.IsComplete)
        {
            IsFinished = true;
            CompleteGrids = new[] { Grid };
        }

        if (IsFinished)
            return;

        if (NextLevelChoices == null)
        {
            var maxChoices = (int)Math.Pow(2, 3); //cap max choices at 8

            var positions = UpdateResult.UpdatedPositions.Any()
                ? Maybe<IEnumerable<Position>>.From(UpdateResult.UpdatedPositions)
                : Maybe<IEnumerable<Position>>.None;

            IEnumerable<IBifurcationOption<T>> cellBifurcationOptions;

            if (positions.HasValue)
            {
                cellBifurcationOptions = positions.Value.Select(Grid.GetCellKVP)
                    .SelectMany(x => x.Value.GetBifurcationOptions(x.Key, maxChoices));
            }
            else
            {
                cellBifurcationOptions = Grid.AllCells
                    .SelectMany(x => x.Value.GetBifurcationOptions(x.Key, maxChoices));
            }

            var bifurcationOptions =
                cellBifurcationOptions.Concat(
                    Grid.ClueSource.BifurcationClueHelper.GetBifurcationOptions(Grid,
                        positions,
                        maxChoices));

            var groupedBifurcations = bifurcationOptions
                .Where(x => !ChoicesNotToTake.Overlaps(x.Choices))
                .OrderByDescending(x => x.Priority)
                .ThenBy(x => x.ChoiceCount)
                .SelectMany(option => option.Choices.Select(choice => (option, choice)))
                .GroupBy(x => x.choice, x => x.option);

            var nextLevelChoiceList = new List<PossibleChoice>();

            foreach (var grouping in groupedBifurcations)
            {
                var options = grouping.ToList();
                var possible = new PossibleChoice(grouping.Key, options, InitialHeight);
                nextLevelChoiceList.Add(possible);
            }

            NextLevelChoices = nextLevelChoiceList
                .GroupBy(x => x.Height)
                .ToDictionary(x => x.Key, x => x.ToList());
        }
        else if (UpdateResultToPropagate.HasValue)
        {
            NextLevelNodes.ForEach(x => x.ApplyUpdate(UpdateResultToPropagate.Value));
            UpdateResultToPropagate = Maybe<UpdateResult<T>>.None;
        }

        var newHeight = InitialHeight + LevelsDescended + 1;

        var choicesToPromote = NextLevelChoices.Where(x => x.Key <= newHeight)
            .OrderBy(x => x.Key) //lower heights first
            .ToList();

        foreach (var choiceList in choicesToPromote)
        {
            NextLevelChoices.Remove(choiceList.Key);
            foreach (var choice in choiceList.Value)
            {
                UsedChoices.Add(choice.Choice);

                NextLevelNodes.Add(choice.ToChoiceNode(Grid, UsedChoices.ToImmutable()));
            }
        }

        ChoiceNode<T>.CombineAll(NextLevelNodes);

        if (NextLevelNodes.Count == 0 && NextLevelChoices.Count == 0)
        {
            IsFinished = true;
            return;
        }

        foreach (var choiceNode in NextLevelNodes)
        {
            if (newHeight > choiceNode.InitialHeight + choiceNode.LevelsDescended)
                choiceNode.Descend();
        }

        var optionGroups = NextLevelNodes
            .SelectMany(choiceNode => choiceNode.Options.Select(option => (option, choiceNode)))
            .GroupBy(x => x.option, x => x.choiceNode);

        var newUpdateResultToPropagate = UpdateResult<T>.Empty;

        foreach (var group in optionGroups)
        {
            if (group.Count() != group.Key.ChoiceCount)
                continue; //Not all of these options have been checked yet

            if (group.All(x => x.IsFinished && x.CompleteGrids != null))
            {
                CompleteGrids = group.SelectMany(x => x.CompleteGrids!).ToHashSet();
                IsFinished = true;
                return;
            }

            var ur = BifurcationCombine(group.Key.Reason, group, Grid);

            var changedUpdateResult = newUpdateResultToPropagate.Combine(ur, out var changed);
            if (changed)
                newUpdateResultToPropagate = changedUpdateResult;
        }

        if (!newUpdateResultToPropagate.IsEmpty)
        {
            (Grid, UpdateResult) = Grid.IterateRepeatedly(UpdateResultCombiner<T>.Fast, newUpdateResultToPropagate);

            if (UpdateResult.HasContradictions)
                IsFinished = true;
            UpdateResultToPropagate = newUpdateResultToPropagate;
        }

        LevelsDescended++;
    }

    private class PossibleChoice
    {
        public PossibleChoice(IBifurcationChoice<T> choice, IReadOnlyCollection<IBifurcationOption<T>> options,
            int parentHeight)
        {
            Choice = choice;
            Options = options;
            var minChoices = options.Min(x => x.ChoiceCount);

            Height = parentHeight + GetDepth(minChoices);
        }

        /// <summary>
        /// The choice
        /// </summary>
        public IBifurcationChoice<T> Choice { get; }

        /// <summary>
        /// Options whose choices led to this node.
        /// </summary>
        public IReadOnlyCollection<IBifurcationOption<T>> Options { get; }

        public int Height { get; }

        public ChoiceNode<T> ToChoiceNode(Grid<T> grid, ImmutableHashSet<IBifurcationChoice<T>> choicesNotToTake)
        {
            var (newGrid, newUpdateResult) =
                grid.IterateRepeatedly(UpdateResultCombiner<T>.Fast, Choice.UpdateResult);

            return new ChoiceNode<T>(newGrid, newUpdateResult, Height, choicesNotToTake, Options, new[] { Choice });
        }


        /// <inheritdoc />
        public override string ToString() => Choice.ToString()!;

        private static int GetDepth(int optionChoices)
        {
            var depth = 1;
            while (optionChoices > 2)
            {
                optionChoices /= 2;
                depth++;
            }

            return depth;
        }
    }


    static ICellChangeResult CombineCellUpdates(IGrouping<Position, CellUpdate<T>> cells,
        ISingleReason reason,
        Func<Position, Cell<T>> getCellValue)
    {
        if (cells.Count() == 1)
            return cells.Single();

        var r = CellHelper.TryCreate(
            cells.Select(x => x.NewCell.PossibleValues).UnionAllSortedSets(), cells.Key,
            new ImpliedByAllReason(reason));

        if (r is CellUpdate<T> cellUpdate)
        {
            var currentValue = getCellValue(cells.Key);

            if (currentValue.PossibleValues.Count == cellUpdate.NewCell.PossibleValues.Count)
                return NoChange.Instance;
        }


        return r;
    }

    private static UpdateResult<T> BifurcationCombine(
        ISingleReason reason,
        IEnumerable<ChoiceNode<T>> results,
        Grid<T> startGrid)
    {
        var (withContradictions, withoutContradictions) =
            results.Partition(x => x.UpdateResult.Contradictions.Any());
        var resultsWithContradictions = withContradictions.ToList();
        var resultsWithoutContradictions = withoutContradictions.ToList();

        if (!resultsWithoutContradictions.Any())
        {
            var commonContradictions = resultsWithContradictions
                .Select(x => x.UpdateResult.Contradictions)
                .IntersectAllSets().ToImmutableHashSet();

            if (commonContradictions
                .Any()) //This is the best case - the contradiction happens whatever choice was chosen
                return new UpdateResult<T>(ImmutableDictionary<Position, CellUpdate<T>>.Empty,
                    commonContradictions);

            var positions = resultsWithContradictions
                .SelectMany(x => x.Choices)
                .SelectMany(x => x.UpdateResult.UpdatedPositions)
                .ToImmutableSortedSet();

            return new UpdateResult<T>(ImmutableDictionary<Position, CellUpdate<T>>.Empty,
                ImmutableHashSet<Contradiction>.Empty.Add(
                    new Contradiction(new AllLedToContradiction(reason), positions)));
        }

        if (resultsWithoutContradictions.Count == 1)
            return resultsWithoutContradictions.Single().UpdateResult;

        var cellUpdates = resultsWithoutContradictions
            .SelectMany(x => x.UpdateResult.UpdatedCells.Values)
            .GroupBy(x => x.Position, x => x)
            .Where(x => x.Count() == resultsWithoutContradictions.Count)
            .Select(x => CombineCellUpdates(x, reason, startGrid.GetCell));

        return UpdateResultCombiner<T>.Fast.Combine(cellUpdates);
    }
}
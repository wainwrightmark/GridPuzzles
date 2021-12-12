using System;

namespace Sudoku.Variants;

public class SumClue : IRuleClue<int>
{
    private SumClue(string name, ISumChecker sumChecker, ImmutableSortedSet<Position> positions)
    {
        Name = name;
        SumChecker = sumChecker;
        Positions = positions;
    }

    public static IClue<int> Create(string name, ImmutableSortedSet<int> sums, bool sumsGivenAreValid,
        ImmutableDictionary<Position, int> multipliers)
    {
        var positions = multipliers
            .Where(x => x.Value != 0)
            .Select(x => x.Key)
            .ToImmutableSortedSet();

        var allOnes = multipliers.All(x => x.Value == 1);

        ISumChecker sumChecker;

        if (allOnes)
        {
            sumChecker = new AllOnesSumChecker(sums, sumsGivenAreValid);

            if (multipliers.Count == 2)
            {
                var p1 = multipliers.Keys.First();
                var p2 = multipliers.Keys.Skip(1).First();
                if (sums.Count == 1 && sumsGivenAreValid)
                    return RelationshipClue<int>.Create(p1, p2, new SumConstraint(sums.Single()));
                if (!sumsGivenAreValid)
                    return RelationshipClue<int>.Create(p1, p2, new NonSumConstraint(sums));
            }
        }

        else
            sumChecker = new MultipliersSumChecker(sums, sumsGivenAreValid, multipliers);

        return new SumClue(name, sumChecker, positions);
    }

    public string Name { get; }

    public ISumChecker SumChecker { get; }

    public Maybe<int> SimpleTotalSum => SumChecker is AllOnesSumChecker { CorrectSumsAreLegal: true } checker
        ? checker.Sums.Single()
        : Maybe<int>.None;

    /// <inheritdoc />
    public ImmutableSortedSet<Position> Positions { get; }


    /// <inheritdoc />
    public IEnumerable<ICellChangeResult> GetCellUpdates(Grid<int> grid)
    {
        var cells = Positions.Select(grid.GetCellKVP).ToList();

        if (SumChecker.SumIsAlwaysPossible(cells))
            yield break;

        var allUnique = grid.ClueSource.UniquenessClueHelper.ArePositionsMutuallyUnique(Positions);
        var valueChecker = new SimpleValueChecker(allUnique, grid.ClueSource.RelationshipClueHelper,
            grid.ClueSource.UniquenessClueHelper);


        var pvsToCheck = new HashSet<PositionValue>();
        var unassignedValuesBuilder =
            ImmutableSortedDictionary<Position, ImmutableSortedSet<int>>.Empty.ToBuilder();
        var possiblePositionValues = new HashSet<PositionValue>();
        var assignedValues = ImmutableList<PositionValue>.Empty;
        var usedValues = allUnique ? new HashSet<int>() : Maybe<HashSet<int>>.None;


        //TODO maybe get rid of all this - should be handled by earlier clues
        foreach (var grouping in cells.GroupBy(x => x.Value, x => x.Key))
        {
            var count = grouping.Count();
            var cell = grouping.Key;

            var newPossibleValues = usedValues.HasValue
                ? cell.PossibleValues.Except(usedValues.Value)
                : cell.PossibleValues;

            if (newPossibleValues.Count == 1)
            {
                if (usedValues.HasValue)
                    usedValues.Value.Add(newPossibleValues.Single());

                foreach (var position in grouping)
                {
                    var positionValue = new PositionValue(position, newPossibleValues.Single());
                    if (!assignedValues.All(av => valueChecker.AreValuesPossible(av, positionValue)))
                    {
                        yield return new Contradiction(
                            new SumReason(this),
                            //$"{Name} is impossible",
                            Positions);
                        yield break;
                    }

                    possiblePositionValues.Add(positionValue);
                    assignedValues = assignedValues.Add(positionValue);
                }
            }
            else
            {
                if (allUnique && newPossibleValues.Count <= count)
                    usedValues.Value.UnionWith(newPossibleValues);

                foreach (var position in grouping)
                {
                    var newNewPossibleValues = newPossibleValues;
                    foreach (var possibleValue in newPossibleValues)
                    {
                        var positionValue = new PositionValue(position, possibleValue);
                        if (assignedValues.All(av => valueChecker.AreValuesPossible(av, positionValue)))
                            pvsToCheck.Add(positionValue);
                        else
                            newNewPossibleValues = newNewPossibleValues.Remove(possibleValue);
                    }

                    if (!newNewPossibleValues.Any())
                    {
                        yield return (new Contradiction(
                            new SumReason(this),
                            //$"{Name} is impossible",
                            Positions));
                        yield break;
                    }

                    unassignedValuesBuilder.Add(position, newNewPossibleValues);
                }
            }
        }

        //TODO assign cells which can be assigned here
        //TODO check min and max values

        //TODO group remainingValues in some way

        var unassignedValues = unassignedValuesBuilder.ToImmutable();

        while (pvsToCheck.Any()) //TODO give up immediately if the remaining values for a cell are exhausted
        {
            var v = pvsToCheck.First();

            var newAssignedValues = assignedValues.Add(v);
            var newRemainingCells = unassignedValues.Remove(v.Position);

            var assignment = FindValidAssignment(newAssignedValues, newRemainingCells, SumChecker, valueChecker);
            if (assignment.HasValue)
            {
                possiblePositionValues.UnionWith(assignment.Value);
                pvsToCheck.ExceptWith(assignment.Value);
            }
            else
                pvsToCheck.Remove(v);
        }

        var cellPossibleValues = possiblePositionValues.ToLookup(x => x.Position, x => x.Value);

        foreach (var cell in cells)
        {
            var possibleValues = cellPossibleValues[cell.Key];
            // ReSharper disable PossibleMultipleEnumeration
            var count = possibleValues.Count();

            if (count <= 0)
            {
                yield return (new Contradiction(
                    new SumReason(this),
//                        $"{Name} is impossible",
                    Positions));
                yield break;
            }

            if (count < cell.Value.PossibleValues.Count)
                yield return (cell.CloneWithOnlyValues(possibleValues, 
                    new SumReason(this)
                    //$"{Name} restricts values"
                ));
            // ReSharper restore PossibleMultipleEnumeration
        }

        if (cellPossibleValues.All(x => x.Count() == 1))
        {
            if (!SumChecker.IsLegalSum(possiblePositionValues))
            {
                yield return (new Contradiction(
                    new SumReason(this),
//                        $"{Name} is impossible",
                    Positions));
                yield break;
            }
        }
    }

    private static Maybe<ImmutableList<PositionValue>> FindValidAssignment(
        ImmutableList<PositionValue> assignedValues,
        ImmutableSortedDictionary<Position, ImmutableSortedSet<int>> unassignedCells, ISumChecker sumChecker,
        IValueChecker valueChecker)
    {
        if (!unassignedCells.Any())
            return sumChecker.IsLegalSum(assignedValues)
                ? assignedValues
                : Maybe<ImmutableList<PositionValue>>.None;

        var cellToTry = unassignedCells.First();

        var newUnassignedCells = unassignedCells.Remove(cellToTry.Key);

        foreach (var value in cellToTry.Value)
        {
            var pv = new PositionValue(cellToTry.Key, value);

            if (valueChecker.AreValuesPossible(pv, assignedValues))
            {
                var newAssignedValues = assignedValues.Add(pv);
                var nextResult =
                    FindValidAssignment(newAssignedValues, newUnassignedCells, sumChecker, valueChecker);
                if (nextResult.HasValue)
                    return nextResult;
            }
        }

        return Maybe<ImmutableList<PositionValue>>.None;
    }


    public readonly struct PositionValue : IEquatable<PositionValue>
    {
        public PositionValue(Position position, int value)
        {
            Position = position;
            Value = value;
        }

        public Position Position { get; }
        public int Value { get; }

        /// <inheritdoc />
        public override string ToString() => (Position, Value).ToString();

        /// <inheritdoc />
        public bool Equals(PositionValue other) => Position.Equals(other.Position) && Value == other.Value;

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is PositionValue other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Position, Value);
    }

    private interface IValueChecker
    {
        bool AreValuesPossible(PositionValue positionValue, ImmutableList<PositionValue> assignedValues)
        {
            foreach (var pv2 in assignedValues)
            {
                if (!AreValuesPossible(positionValue, pv2))
                    return false;
            }

            return true;
        }

        bool AreValuesPossible(PositionValue pv1, PositionValue pv2);
    }

    private class SimpleValueChecker : IValueChecker
    {
        public SimpleValueChecker(bool allUnique, RelationshipClueHelper<int> relationshipClueHelper,
            UniquenessClueHelper<int> uniquenessClueHelper)
        {
            AllUnique = allUnique;
            RelationshipClueHelper = relationshipClueHelper;
            UniquenessClueHelper = uniquenessClueHelper;
        }

        public bool AllUnique { get; }
        public RelationshipClueHelper<int> RelationshipClueHelper { get; }
        public UniquenessClueHelper<int> UniquenessClueHelper { get; }

        public bool AreValuesPossible(PositionValue pv1, PositionValue pv2)
        {
            if (pv1.Value == pv2.Value)
            {
                if (AllUnique || UniquenessClueHelper.ArePositionsMutuallyUnique(pv1.Position, pv2.Position))
                    return false;
            }

            var r = RelationshipClueHelper.AreValuesValid(pv1.Position, pv2.Position, pv1.Value, pv2.Value);
            return r;
        }
    }

    public interface ISumChecker
    {
        bool IsLegalSum(IEnumerable<PositionValue> pairs);

        ImmutableSortedSet<int> Sums { get; }

        /// <summary>
        /// True is any total outside the allowed sums leads to a contradiction
        /// </summary>
        bool CorrectSumsAreLegal { get; }

        bool SumIsAlwaysPossible(IEnumerable<KeyValuePair<Position, Cell<int>>> cells)
        {
            return !CorrectSumsAreLegal && cells.All(x => x.Value.PossibleValues.Count > Sums.Count + 1);
        }
    }

    public class AllOnesSumChecker : ISumChecker
    {
        public AllOnesSumChecker(ImmutableSortedSet<int> sums, bool sumsAreValid)
        {
            Sums = sums;
            CorrectSumsAreLegal = sumsAreValid;
        }

        public ImmutableSortedSet<int> Sums { get; }
        public bool CorrectSumsAreLegal { get; }

        /// <inheritdoc />
        public bool IsLegalSum(IEnumerable<PositionValue> pairs)
        {
            var sum = pairs.Sum(x => x.Value);
            return CorrectSumsAreLegal == Sums.Contains(sum);
        }
    }


    public class MultipliersSumChecker : ISumChecker
    {
        public ImmutableDictionary<Position, int> Multipliers { get; }

        public ImmutableSortedSet<int> Sums { get; }
        public bool CorrectSumsAreLegal { get; }

        /// <inheritdoc />
        public bool IsLegalSum(IEnumerable<PositionValue> pairs)
        {
            var s = 0;

            foreach (var positionValue in pairs)
            {
                var m = Multipliers[positionValue.Position];
                s += m * positionValue.Value;
            }

            return CorrectSumsAreLegal == Sums.Contains(s);
        }

        public MultipliersSumChecker(ImmutableSortedSet<int> sums, bool sumsAreValid,
            ImmutableDictionary<Position, int> multipliers)
        {
            Sums = sums;
            CorrectSumsAreLegal = sumsAreValid;
            Multipliers = multipliers;
        }
    }
}

public sealed record SumReason(SumClue SumClue) : ISingleReason
{
    /// <inheritdoc />
    public string Text => SumClue.Name;

    /// <inheritdoc />
    public IEnumerable<Position> GetContributingPositions(IGrid grid)
    {
        return SumClue.Positions;
    }

    /// <inheritdoc />
    public Maybe<IClue> Clue => SumClue;
}
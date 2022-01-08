using System;

namespace Sudoku.Variants;

public class SumClue : IRuleClue<int>
{
    private SumClue(string name, ISumChecker sumChecker, ImmutableSortedSet<Position> positions, bool alwaysAllUnique)
    {
        Name = name;
        SumChecker = sumChecker;
        Positions = positions;
        AlwaysAllUnique = alwaysAllUnique;
    }

    public static IClue<int> Create(string name, ImmutableSortedSet<int> sums, bool sumsGivenAreValid,
        ImmutableDictionary<Position, int> multipliers, bool alwaysAllUnique)
    {
        var positions = multipliers
            .Where(x => x.Value != 0)
            .Select(x => x.Key)
            .ToImmutableSortedSet();

        var allOnes = multipliers.All(x => x.Value == 1);

        ISumChecker sumChecker;

        if (allOnes)
        {
            sumChecker = new KillerSumChecker(sums, sumsGivenAreValid);

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

        return new SumClue(name, sumChecker, positions, alwaysAllUnique);
    }

    public string Name { get; }

    /// <inheritdoc />
    public override string ToString() => Name;

    public ISumChecker SumChecker { get; }

    public bool AlwaysAllUnique { get; }

    public Maybe<int> SimpleTotalSum => SumChecker is KillerSumChecker { CorrectSumsAreLegal: true, Sums.Count: 1} checker
        ? checker.Sums.Single()
        : Maybe<int>.None;

    /// <inheritdoc />
    public ImmutableSortedSet<Position> Positions { get; }


    /// <inheritdoc />
    public IEnumerable<ICellChangeResult> CalculateCellUpdates(Grid<int> grid)
    {
        var updates = GetCellUpdates1(grid);
        return updates;
    }


    private IEnumerable<ICellChangeResult> GetCellUpdates1(Grid<int> grid)
    {
        var cells = Positions.Select(grid.GetCellKVP).ToList();


        var allUnique = AlwaysAllUnique || grid.ClueSource.UniquenessClueHelper.ArePositionsMutuallyUnique(Positions);
        var anyRelations = grid.ClueSource.RelationshipClueHelper.DoAnyRelationshipsExist(Positions);
        var relationshipChecker = new SimpleRelationshipChecker(
            !anyRelations ? Maybe<RelationshipClueHelper<int>>.None : grid.ClueSource.RelationshipClueHelper,
            allUnique ? Maybe<UniquenessClueHelper<int>>.None : grid.ClueSource.UniquenessClueHelper);

        var pvsToCheck = new HashSet<PositionValue>();
        var unassignedValuesBuilder =
            ImmutableSortedDictionary<Position, ImmutableSortedSet<int>>.Empty.ToBuilder();
        var possiblePositionValues = new HashSet<PositionValue>();
        var assignedValues = ImmutableList<PositionValue>.Empty;

        
        //Group cells with the same set of possible values together
        foreach (var grouping in cells.GroupBy(x => x.Value, x => x.Key))
        {
            var usedValues = allUnique ? new HashSet<int>() : Maybe<HashSet<int>>.None;
            var count = grouping.Count();
            var cell = grouping.Key;

            var newPossibleValues = usedValues.HasValue
                ? cell.PossibleValues.Except(usedValues.Value)
                : cell.PossibleValues;

            //If cells of this type only have one possible value, assign that value
            if (newPossibleValues.Count == 1)
            {
                if (usedValues.HasValue)
                    usedValues.Value.Add(newPossibleValues.Single());

                foreach (var position in grouping)
                {
                    var positionValue = new PositionValue(position, newPossibleValues.Single());
                    if (!assignedValues.All(av => relationshipChecker.AreValuesPossible(av, positionValue)))
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
                        if (assignedValues.All(av => relationshipChecker.AreValuesPossible(av, positionValue)))
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

        var unassignedValues = unassignedValuesBuilder.ToImmutable();

        IReadOnlyList<IReadOnlySet<Position>> positionGroups =SumChecker.GroupPositions(relationshipChecker, unassignedValues).ToList();

        while (pvsToCheck.Any())
        {
            var positionValue = pvsToCheck.First();

            var newAssignedValues = assignedValues.Add(positionValue);
            var newRemainingCells = unassignedValues.Remove(positionValue.Position);

            var assignment = FindValidAssignment(newAssignedValues, newRemainingCells, SumChecker, relationshipChecker);
            if (assignment.HasValue)
            {
                var multipliedAssignments = MultiplyOut1(assignment.Value, positionGroups);
                possiblePositionValues.UnionWith(multipliedAssignments);
                pvsToCheck.ExceptWith(multipliedAssignments);
            }
            else
                pvsToCheck.ExceptWith(MultiplyOut2(positionValue, positionGroups));


            static IReadOnlyList<PositionValue> MultiplyOut1(ImmutableList<PositionValue> assignments,
                IReadOnlyList<IReadOnlySet<Position>> positionGroups)
            {
                if (!positionGroups.Any()) return assignments;

                return assignments.SelectMany(x => MultiplyOut2(x, positionGroups)).ToList();
            }

            static IEnumerable<PositionValue> MultiplyOut2(PositionValue pv,
                IReadOnlyList<IReadOnlySet<Position>> positionGroups)
            {
                if (positionGroups.Any())
                {
                    var group = positionGroups.FirstOrDefault(x => x.Contains(pv.Position));
                    if (group is not null)
                    {
                        foreach (var position in group)
                            yield return new PositionValue(position, pv.Value);

                        yield break; //Don't return the pv again
                    }
                }

                yield return pv;
            }
        }

        var cellPossibleValues = possiblePositionValues.ToLookup(x => x.Position, x => x.Value);

        //If there is only one set of possible values, check the sum of those values
        if (cellPossibleValues.All(x => x.Count() == 1))
        {
            if (!SumChecker.IsLegalSum(possiblePositionValues))
            {
                yield return (new Contradiction(
                    new SumReason(this),
                    Positions));
                yield break;
            }
        }

        foreach (var cell in cells)
        {
            var possibleValues = cellPossibleValues[cell.Key];
            // ReSharper disable PossibleMultipleEnumeration
            var count = possibleValues.Count();

            if (count <= 0)
            {
                yield return (new Contradiction(
                    new SumReason(this),
                    Positions));
                yield break;
            }

            if (count < cell.Value.PossibleValues.Count)
                yield return cell.CloneWithOnlyValues(possibleValues,
                    new SumReason(this)
                );
            // ReSharper restore PossibleMultipleEnumeration
        }
    }

    private static Maybe<ImmutableList<PositionValue>> FindValidAssignment(
        ImmutableList<PositionValue> assignedValues,
        ImmutableSortedDictionary<Position, ImmutableSortedSet<int>> unassignedCells, ISumChecker sumChecker,
        IRelationshipChecker relationshipChecker)
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

            if (relationshipChecker.AreValuesPossible(pv, assignedValues))
            {
                var newAssignedValues = assignedValues.Add(pv);
                var nextResult =
                    FindValidAssignment(newAssignedValues, newUnassignedCells, sumChecker, relationshipChecker);
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

    public interface IRelationshipChecker
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

        bool HaveSameRestrictions(Position p1, Position p2, IEnumerable<Position> allPositions);

        bool AllValuesUnique { get; }
        bool NoRelationships { get; }
    }


    private class SimpleRelationshipChecker : IRelationshipChecker
    {
        public SimpleRelationshipChecker(
            Maybe<RelationshipClueHelper<int>> relationshipClueHelper,
            Maybe<UniquenessClueHelper<int>> uniquenessClueHelper)
        {
            RelationshipClueHelper = relationshipClueHelper;
            UniquenessClueHelper = uniquenessClueHelper;
        }

        private Maybe<RelationshipClueHelper<int>> RelationshipClueHelper { get; }
        private Maybe<UniquenessClueHelper<int>> UniquenessClueHelper { get; }

        public bool AreValuesPossible(PositionValue pv1, PositionValue pv2)
        {
            if (pv1.Value == pv2.Value)
            {
                if (UniquenessClueHelper.HasNoValue ||
                    UniquenessClueHelper.Value.ArePositionsMutuallyUnique(pv1.Position, pv2.Position))
                    return false;
            }

            if (RelationshipClueHelper.HasNoValue) return true;

            var r = RelationshipClueHelper.Value.AreValuesValid(pv1.Position, pv2.Position, pv1.Value, pv2.Value);
            return r;
        }

        /// <inheritdoc />
        public bool HaveSameRestrictions(Position p1, Position p2, IEnumerable<Position> allPositions)
        {
            if (UniquenessClueHelper.HasNoValue && RelationshipClueHelper.HasNoValue) return true;

            //Check that all constraints between the two positions are commutative
            if (RelationshipClueHelper.HasValue && !RelationshipClueHelper.Value
                    .CheckRelationship(p1, p2, clue => clue.Constraint.IsCommutative).All(x => x))
                return false;

            var otherPositions = allPositions.Except(new[] { p1, p2 }).ToHashSet();

            if (UniquenessClueHelper.HasValue)
            {
                var l1 =
                    otherPositions.Intersect(UniquenessClueHelper.Value.Lookup[p1]
                        .SelectMany(x => x.Positions)).ToHashSet();
                var l2 = otherPositions.Intersect(UniquenessClueHelper.Value.Lookup[p2]
                    .SelectMany(x => x.Positions));

                if (!l1.SetEquals(l2)) return false;
            }

            if (RelationshipClueHelper.HasNoValue) return true;

            var r1 = RelationshipClueHelper.Value.Lookup[p1]
                .Where(x => otherPositions.Contains(x.Position2));
            var r2 = RelationshipClueHelper.Value.Lookup[p2]
                .Where(x => otherPositions.Contains(x.Position2));

            if (!r1.ToHashSet(RelationshipCluePosition1AgnosticComparer<int>.Instance).SetEquals(r2))
                return false;


            return true;
        }

        /// <inheritdoc />
        public bool AllValuesUnique => UniquenessClueHelper.HasNoValue;

        /// <inheritdoc />
        public bool NoRelationships => RelationshipClueHelper.HasNoValue;
    }

    public interface ISumChecker
    {
        bool IsLegalSum(IEnumerable<PositionValue> pairs);

        ImmutableSortedSet<int> Sums { get; }

        public bool IsLegalSumPlausible(IEnumerable<PositionValue> assignedPositionValues,
            IEnumerable<PositionValue> possiblePositionValues);

        /// <summary>
        /// True if the desired total is any amount inside the Sums collection
        /// False if the desired total is any amount outside the Sums collection
        /// </summary>
        bool CorrectSumsAreLegal { get; }

        IEnumerable<IReadOnlySet<Position>> GroupPositions(IRelationshipChecker relationshipChecker,
            ImmutableSortedDictionary<Position, ImmutableSortedSet<int>> cells);
    }

    public class KillerSumChecker : ISumChecker
    {
        public KillerSumChecker(ImmutableSortedSet<int> sums, bool sumsAreValid)
        {
            Sums = sums;
            CorrectSumsAreLegal = sumsAreValid;
        }

        public ImmutableSortedSet<int> Sums { get; }
        public bool CorrectSumsAreLegal { get; }

        /// <inheritdoc />
        public IEnumerable<IReadOnlySet<Position>> GroupPositions(IRelationshipChecker relationshipChecker,
            ImmutableSortedDictionary<Position, ImmutableSortedSet<int>> cells)
        {
            var allPositions = cells.Select(x => x.Key).ToList();
            foreach (var grouping in
                     cells.GroupBy(x => x.Value, x => x.Key)
                         .Select(x => x.ToList())
                         .Where(x => x.Count > 1))
            {
                var setsSoFar = grouping.Take(1).Select(x => new HashSet<Position>() { x }).ToList();

                foreach (var position in grouping.Skip(1))
                {
                    var foundASet = false;
                    foreach (var set in setsSoFar)
                    {
                        if (relationshipChecker.HaveSameRestrictions(set.First(), position, allPositions))
                        {
                            set.Add(position);
                            foundASet = true;
                            break;
                        }
                    }

                    if (!foundASet) setsSoFar.Add(new HashSet<Position> { position });
                }

                foreach (var set in setsSoFar.Where(x => x.Count > 1))
                    yield return set;
            }
        }
        

        /// <inheritdoc />
        public bool IsLegalSum(IEnumerable<PositionValue> pairs)
        {
            var sum = pairs.Sum(x => x.Value);
            return CorrectSumsAreLegal == Sums.Contains(sum);
        }

        /// <inheritdoc />
        public bool IsLegalSumPlausible(IEnumerable<PositionValue> assignedPositionValues,
            IEnumerable<PositionValue> possiblePositionValues)
        {
            if (!CorrectSumsAreLegal) return true; //Always plausible if we're just trying to avoid particular numbers
            var assignedAmount = assignedPositionValues.Select(x => x.Value).DefaultIfEmpty(0).Sum();

            var maxSoFar = assignedAmount;
            var minSoFar = assignedAmount;
            if (minSoFar > Sums.Max) return false;

            foreach (var group in possiblePositionValues.GroupBy(x => x.Position, x => x.Value))
            {
                minSoFar += group.Min();
                if (minSoFar > Sums.Max) return false; //The minimum amount is greater than the highest allowed sum
                maxSoFar += group.Max();
            }

            if (maxSoFar < Sums.Min) return false; //The maximum amount is less than the highest allowed sum
            return true;
        }
    }


    public class MultipliersSumChecker : ISumChecker
    {
        public MultipliersSumChecker(ImmutableSortedSet<int> sums, bool sumsAreValid,
            ImmutableDictionary<Position, int> multipliers)
        {
            Sums = sums;
            CorrectSumsAreLegal = sumsAreValid;
            Multipliers = multipliers;
        }

        public ImmutableDictionary<Position, int> Multipliers { get; }

        public ImmutableSortedSet<int> Sums { get; }

        /// <inheritdoc />
        public bool IsLegalSumPlausible(IEnumerable<PositionValue> assignedPositionValues,
            IEnumerable<PositionValue> possiblePositionValues)
        {
            if (!CorrectSumsAreLegal) return true; //Always plausible if we're just trying to avoid particular numbers

            var assignedAmount = assignedPositionValues.Select(x =>
                Multipliers[x.Position] * x.Value).DefaultIfEmpty(0).Sum();

            var maxSoFar = assignedAmount;
            var minSoFar = assignedAmount;

            foreach (var group in possiblePositionValues.GroupBy(x => x.Position, x => x.Value))
            {
                var multiplier = Multipliers[group.Key];
                if (multiplier > 0)
                {
                    minSoFar += multiplier * group.Min();
                    maxSoFar += multiplier * group.Max();
                }
                else
                {
                    minSoFar += multiplier * group.Max();
                    maxSoFar += multiplier * group.Min();
                }
            }

            if (minSoFar > Sums.Max) return false; //The minimum amount is greater than the highest allowed sum
            if (maxSoFar < Sums.Min) return false; //The maximum amount is less than the highest allowed sum
            return true;
        }

        public bool CorrectSumsAreLegal { get; }

        /// <inheritdoc />
        public bool SumIsAlwaysPossible(IRelationshipChecker relationshipChecker,
            IReadOnlyList<KeyValuePair<Position, Cell<int>>> cells, Grid<int> grid)
        {
            return false;
            //TODO if all cells have the same number of choices DOF then return true maybe?
        }

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

        /// <inheritdoc />
        public IEnumerable<IReadOnlySet<Position>> GroupPositions(IRelationshipChecker relationshipChecker,
            ImmutableSortedDictionary<Position, ImmutableSortedSet<int>> cells)
        {
            var allPositions = cells.Select(x => x.Key).ToList();
            foreach (var grouping in
                     cells.GroupBy(x => (Multipliers[x.Key], x.Value), x => x.Key)
                         .Select(x => x.ToList())
                         .Where(x => x.Count > 1))
            {
                var setsSoFar = grouping.Take(1).Select(x => new HashSet<Position>() { x }).ToList();

                foreach (var position in grouping.Skip(1))
                {
                    var foundASet = false;
                    foreach (var set in setsSoFar)
                    {
                        if (set.All(setPosition =>
                                relationshipChecker.HaveSameRestrictions(setPosition, position, allPositions)))
                        {
                            set.Add(position);
                            foundASet = true;
                            break;
                        }
                    }

                    if (!foundASet) setsSoFar.Add(new HashSet<Position> { position });
                }

                foreach (var set in setsSoFar.Where(x => x.Count > 1))
                    yield return set;
            }
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
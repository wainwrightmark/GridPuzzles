using System;
using System.Diagnostics.Contracts;
using System.Text;
using CSharpFunctionalExtensions;
using GridPuzzles.Clues.Constraints;
using Sudoku.Variants;

namespace Sudoku.Clues
{
    public sealed class ArithmeticConsistency : NoArgumentVariantBuilder<int>
    {
        private ArithmeticConsistency()
        {
        }

        public static NoArgumentVariantBuilder<int> Instance { get; } = new ArithmeticConsistency();

        /// <inheritdoc />
        public override string Name => "Arithmetic Consistency";

        /// <inheritdoc />
        public override int Level => 11;

        /// <inheritdoc />
        public override IEnumerable<IClue<int>> CreateClues(
            Position minPosition,
            Position maxPosition, IValueSource<int> valueSource,
            IReadOnlyCollection<IClue<int>> lowerLevelClues)
        {
            if (true || !lowerLevelClues.OfType<SumClue>().Any()) //Shortcut
                return ArraySegment<IClue<int>>.Empty;

            var completenessClueValue = valueSource.AllValues.Sum();

            var basicEquations = ImmutableArray.CreateRange(
                lowerLevelClues.Select(
                        clue => Equation.TryCreateFromClue(clue, completenessClueValue)
                    ).Where(x => x.HasValue)
                    .Select(x => x.Value.NormalizePolarity())
                    .Distinct().OrderBy(x => x.TotalCells));

            //var basicClues = lowerLevelClues.OfType<BasicClue<int>>().ToList();


            //var startingEquation = //this should essentially be that all cells add up to the total of the grid
            //    Equation.Create(basicClues.SelectMany(x => x.Positions), completenessClueValue * basicClues.Count, true)
            //        .SimplifyMultiples();

            //startingEquation = basicEquations.Where(x => !x.IsBoring)
            //    .Aggregate(startingEquation, (a, b) => a.Subtract(b));

            //TODO try doing this in parallel
            var knownEquations = basicEquations.ToHashSet();
            knownEquations.Add(Equation.Empty);

            var newEquations = new List<Equation>();

            foreach (var basicEquation in basicEquations)
            {
                TryFindSimplifications(basicEquation,
                    basicEquations, 0,
                    knownEquations,
                    valueSource.AllValues.Count * 2, newEquations);
            }

            

            return newEquations.Select(x => VirtualClue.Create(x.ToClue(), ClueColors.GetUniqueSumColor(x.Sum)));
        }

        private static bool TryFindSimplifications(Equation startingPoint,
            ImmutableArray<Equation> basicEquations,
            int basicEquationsIndex,
            HashSet<Equation> knownEquations, int maxSize, List<Equation> newEquations)
        {
            var any = false;
            for (var index = basicEquationsIndex; index < basicEquations.Length; index++)
            {
                var equation = basicEquations[index];
                var simplification = startingPoint.TrySimplifyWith(equation).Map(x=>x.NormalizePolarity());
                if (simplification.HasValue && knownEquations.Add(simplification.Value))
                {
                    var furtherSimplifications =
                        TryFindSimplifications(simplification.Value,
                            basicEquations, index + 1, knownEquations, maxSize, newEquations);

                    if (!furtherSimplifications && simplification.Value.TotalCells <= maxSize)
                    {
                        //This if fully simplified
                        newEquations.Add(simplification.Value);
                        any = true;
                        return true;
                    }
                }
            }

            return any;
        }

        /// <inheritdoc />
        public override bool OnByDefault => true;
    }
}


public record Equation(ImmutableSortedDictionary<Position, int> CellMultiples, int TotalCells, int Sum, bool IsBoring)
{
    private int? lazyHashCode;

    public static Equation Empty { get; } = new(ImmutableSortedDictionary<Position, int>.Empty, 0, 0, true);

    /// <inheritdoc />
    public override string ToString()
    {
        if (CellMultiples.Count > 4)
        {
            return $"{TotalCells} Cells = {Sum}";
        }

        var sb = new StringBuilder();
        var isFirst = true;
        foreach (var (position, value) in CellMultiples)
        {
            if (value > 0)
            {
                if (!isFirst) sb.Append(" + ");
            }
            else
            {
                if (isFirst) sb.Append('-');
                else sb.Append(" - ");
            }


            if (Math.Abs(value) != 1)
            {
                sb.Append(Math.Abs(value));
                sb.Append('*');
            }

            sb.Append(position);

            isFirst = false;
        }

        sb.Append($" = {Sum}");
        return sb.ToString();
    }

    public IClue<int> ToClue()
    {
        if (CellMultiples.Count == 2)
        {
            if (CellMultiples.All(x => x.Value == 1))
                return new RelationshipClue<int>(CellMultiples.Keys.First(), CellMultiples.Keys.Last(),
                    new SumConstraint(Sum));

            if (Sum == 0 && CellMultiples.Values.Sum() == 0)
            {
                return new RelationshipClue<int>(CellMultiples.Keys.First(), CellMultiples.Keys.Last(),
                    AreEqualConstraint<int>.Instance);
            }
        }

        return SumClue.Create($"Virtual Sum to {Sum}", ImmutableSortedSet<int>.Empty.Add(Sum),
            true, CellMultiples.ToImmutableDictionary(x => x.Key, x => x.Value)
        );
    }

    public Equation NormalizePolarity()
    {
        if (CellMultiples.Any() && CellMultiples.First().Value < 0)
        {
            return new Equation(
                CellMultiples.ToImmutableSortedDictionary(x => x.Key, x => -x.Value),
                TotalCells,
                -Sum,
                IsBoring

            );
        }

        return this;
    }

    public Equation SimplifyMultiples()
    {
        var min = CellMultiples.Values.Select(Math.Abs).DefaultIfEmpty(1).Min();
        if (min == 1)
            return this;
        //TODO check all factors of min
        if (Sum % min == 0 && TotalCells % min == 0 && CellMultiples.Values.All(v => v % min == 0))
        {
            var newPositions = CellMultiples.ToImmutableSortedDictionary(x => x.Key, x => x.Value / min);

            return new(newPositions, TotalCells / min, Sum / min, IsBoring);
        }

        return this;
    }

    private int CalculateHashCode()
    {
        if (CellMultiples.Count == 0)
            return HashCode.Combine(TotalCells, Sum);

        return HashCode.Combine(Sum,
            CellMultiples.Count,
            TotalCells,
            CellMultiples.First().Key,
            CellMultiples.First().Value,
            CellMultiples.Last().Key,
            CellMultiples.Last().Value);
    }

    /// <inheritdoc />
    public virtual bool Equals(Equation? other)
    {
        if (other is null) return false;
        if (Sum != other.Sum) return false;
        if (GetHashCode() != other.GetHashCode()) return false;
        if (CellMultiples.SequenceEqual(other.CellMultiples))
            return true;

        return false;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // ReSharper disable NonReadonlyMemberInGetHashCode
        if (lazyHashCode is null)
            lazyHashCode = CalculateHashCode();
        return lazyHashCode.Value;
        // ReSharper restore NonReadonlyMemberInGetHashCode
    }

    [Pure]
    public static Equation Create(IEnumerable<Position> allPositions, int sum, bool boring)
    {
        var ps = allPositions.ToList();

        return new Equation(ps.GroupBy(x => x)
            .ToImmutableSortedDictionary(
                x => x.Key,
                x => x.Count()), ps.Count, sum, boring);
    }

    [Pure]
    public static Maybe<Equation> TryCreateFromClue(IClue<int> clue, int completenessClueValue)
    {
        if (clue is IRelationshipClue<int> relationship)
        {
            switch (relationship.Constraint)
            {
                case AreEqualConstraint<int>:
                    return new Equation(ImmutableSortedDictionary<Position, int>.Empty
                            .Add(relationship.Position1, 1)
                            .Add(relationship.Position2, -1)
                        ,
                        2, 0, false
                    );
                case SumConstraint sumConstraint:
                    return Create(relationship.Positions, sumConstraint.Sum, false);
                default:
                    return Maybe<Equation>.None;
            }
        }

        if (clue is ICompletenessClue<int> completenessClue)
        {
            return Create(completenessClue.Positions, completenessClueValue, true);
        }

        if (clue is SumClue sumClue && sumClue.SumChecker.CorrectSumsAreLegal && sumClue.SumChecker.Sums.Count == 1)
        {
            switch (sumClue.SumChecker)
            {
                case SumClue.AllOnesSumChecker:
                    return Create(sumClue.Positions, sumClue.SumChecker.Sums.Single(), false);
                case SumClue.MultipliersSumChecker multipliersSumChecker:
                    return new Equation(
                        multipliersSumChecker.Multipliers.ToImmutableSortedDictionary(x => x.Key, x => x.Value),
                        multipliersSumChecker.Multipliers.Values.Sum(Math.Abs),
                        multipliersSumChecker.Sums.Single(), false
                    );
            }
        }

        return Maybe<Equation>.None;
    }

    public Equation Add(Equation side2) => Combine(side2, 1);
    public Equation Subtract(Equation side2) => Combine(side2, -1);

    [Pure]
    public Maybe<Equation> TrySimplifyWith(Equation other)
    {
        if (IsBoring && other.IsBoring)
            return Maybe<Equation>.None;

        if (CellMultiples.Count < other.CellMultiples.Count)
        {
            var flip = other.TrySimplifyWith(this); //efficiency
            if (flip.HasValue && flip.Value.TotalCells < TotalCells)
                return flip.Value;
            return Maybe<Equation>.None;
        }

        //Assume this has more positions than the other

        //Check if the two sets overlap
        if (!other.CellMultiples.Keys.Any(CellMultiples.ContainsKey))
            return Maybe<Equation>.None;

        var minusResult = Subtract(other);
        if (minusResult.TotalCells < TotalCells)
        {
            var furtherSimplification = minusResult.TrySimplifyWith(other);
            if (furtherSimplification.HasValue) return furtherSimplification.Value;
            return minusResult;
        }

        var plusResult = Add(other);
        if (plusResult.TotalCells < TotalCells)
        {
            var furtherSimplification = plusResult.TrySimplifyWith(other);
            if (furtherSimplification.HasValue) return furtherSimplification.Value;
            return plusResult;
        }

        return Maybe<Equation>.None;
    }

    [Pure]
    private Equation Combine(Equation side2, int multiplier)
    {
        if (side2.Sum == 0 && !side2.CellMultiples.Any())
            return this;

        var newPositions = CellMultiples.ToBuilder();
        var newTotalCells = TotalCells;

        foreach (var (position, value) in side2.CellMultiples)
        {
            if (value != 0)
            {
                if (newPositions.TryGetValue(position, out var oldValue))
                {
                    var newValue = oldValue + (multiplier * value);
                    if (newValue == 0)
                        newPositions.Remove(position);
                    else
                    {
                        newPositions[position] = newValue;
                    }

                    newTotalCells += (Math.Abs(newValue) - Math.Abs(oldValue));
                }
                else
                {
                    newPositions[position] = multiplier * value;
                    newTotalCells += Math.Abs(multiplier * value);
                }
            }
        }

        var newSum = Sum + multiplier * side2.Sum;

        return new(newPositions.ToImmutable(), newTotalCells, newSum, IsBoring && side2.IsBoring);
    }
}
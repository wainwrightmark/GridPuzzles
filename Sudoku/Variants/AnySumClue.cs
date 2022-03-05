using Sudoku.Clues;

namespace Sudoku.Variants;

/// <summary>
/// One of the numbers must be the sum of the other numbers
/// </summary>
public partial class AnySumVariantBuilder : VariantBuilder
{
    private AnySumVariantBuilder()
    {
    }

    public static VariantBuilder Instance { get; } = new AnySumVariantBuilder();

    /// <inheritdoc />
    public override string Name => "Any Sum";

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder>> TryGetClueBuilders1(IReadOnlyDictionary<string, string> arguments)
    {
        var pr = PositionArgument.TryGetFromDictionary(arguments);
        if (pr.IsFailure) return pr.ConvertFailure<IReadOnlyCollection<IClueBuilder>>();

        var l = new List<IClueBuilder>
        {
            new AnySumClueBuilder(pr.Value.ToImmutableSortedSet())
        };

        return l;
    }

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments => new VariantBuilderArgument[]
    {
        PositionArgument
    };

    private static readonly ListPositionArgument PositionArgument = new("Positions",
        4,
        4);
    [Equatable]
    public partial record AnySumClueBuilder([property:SetEquality] ImmutableSortedSet<Position> Positions) : IClueBuilder
    {
        /// <inheritdoc />
        public string Name => "Cells Add up to";

        /// <inheritdoc />
        public int Level => 3;

        /// <inheritdoc />
        public IEnumerable<IClue<int, IntCell>> CreateClues(Position minPosition, Position maxPosition,
            IValueSource valueSource,
            IReadOnlyCollection<IClue<int, IntCell>> lowerLevelClues)
        {
            var unique = true;
            var uniquenessClues = lowerLevelClues.OfType<IUniquenessClue>().ToList();
            var size2Subsets = Positions.Subsets().Where(x => x.Count == 2);
                 
            foreach (var subset in size2Subsets)
            {
                if (uniquenessClues.Any(uc => uc.Positions.Contains(subset[0]) && uc.Positions.Contains(subset[1])))
                    continue;

                unique = false;
                break;
            }

            yield return AnySumClue.Create("Any Sum", Positions, unique);
        }


        /// <param name="minPosition"></param>
        /// <param name="maxPosition"></param>
        /// <inheritdoc />
        public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
        {
            if (CellOverlays.TryCreateSquareText(Positions, "o").TryExtract(out var co))
                yield return co;
            else
            {
                foreach (var position in Positions)
                {
                    yield return new CellColorOverlay(ClueColors.GetUniqueSumColor(0), position);
                }
            }
        }
    }
}

public sealed record AnySumReason(AnySumClue AnySumClue) : ISingleReason
{
    /// <inheritdoc />
    public string Text => AnySumClue.Name;

    /// <inheritdoc />
    public IEnumerable<Position> GetContributingPositions(IGrid grid)
    {
        return AnySumClue.Positions;
    }

    /// <inheritdoc />
    public Maybe<IClue> Clue => AnySumClue;
}

/// <summary>
/// One of the numbers must be the sum of the other numbers
/// </summary>
public class AnySumClue : IRuleClue
{
    public static IClue<int, IntCell> Create(string name, ImmutableSortedSet<Position> sums, bool unique)
    {
        return new AnySumClue(name, sums, unique);
    }

    private AnySumClue(string name, ImmutableSortedSet<Position> positions, bool unique)
    {
        Name = name;
        Positions = positions;
        Unique = unique;
    }

    public string Name { get; }

    public ImmutableSortedSet<Position> Positions { get; }
    public bool Unique { get; }

    public IEnumerable<ICellChangeResult> CalculateCellUpdates(Grid grid)
    {
        var cells = Positions.Select(grid.GetCellKVP).OrderBy(x => x.Value.Count()).ToArray();
        var solution = Solve(cells,
            0,
            Unique
                ? Maybe<ImmutableHashSet<int>>.From(ImmutableHashSet<int>.Empty)
                : Maybe<ImmutableHashSet<int>>.None);

        if (solution.HasNoValue)
            yield return new Contradiction(new AnySumReason(this), Positions);
        else
        {
            foreach (var (position, possibleValues) in solution.Value)
            {
                var currentCell = grid.GetCellKVP(position);
                if (!Equals(possibleValues, currentCell.Value))
                    yield return currentCell.CloneWithOnlyValues<int, IntCell>(possibleValues, new AnySumReason(this));
            }
        }
    }


    private static Maybe<IReadOnlyCollection<(Position position, IntCell possibleValues)>> Solve(
        KeyValuePair<Position, IntCell>[] cells, int extraAmount, Maybe<ImmutableHashSet<int>> proscribedValues)
    {
        if (cells.Length == 0)
            return Maybe<IReadOnlyCollection<(Position position, IntCell possibleValues)>>.None;
        if (cells.Length == 1)
        {
            if (cells.Single().Value.Contains(extraAmount))
            {
                return new (Position position, IntCell possibleValues)[]
                    { (cells.Single().Key, new IntCell().Add(extraAmount)) };
            }
            return Maybe<IReadOnlyCollection<(Position position, IntCell possibleValues)>>.None;
        }

        var allProscribedValues = proscribedValues.GetValueOrDefault(ImmutableHashSet<int>.Empty);

        //Assume first cell is sum
        var otherCells = cells[1..];

        var isSumOptions = Combine(
            cells.First().Value
                .Except(allProscribedValues)
                .Where(v => v >= extraAmount + otherCells.Length)
                .Select(v =>
                    SolveSum(otherCells, v - extraAmount, proscribedValues.Map(x => x.Add(v)))
                        .Map(x=>(x,v))
                        
                ).ToList(),
            otherCells.Length,
            cells.First().Key
        );

        //Assume focus is not sum

        var isNotSumOptions = Combine(
            cells.First().Value
                .Except(allProscribedValues)
                .Select(v =>
                    Solve(otherCells, v + extraAmount, proscribedValues.Map(x => x.Add(v)))
                        .Map(x=>(x,v))
                        
                ).ToList(),
            otherCells.Length,
            cells.First().Key
        );
        if (isSumOptions.HasNoValue)
            return isNotSumOptions;
        if (isNotSumOptions.HasNoValue)
            return isSumOptions;

        return 
            isSumOptions.Value.Concat(isNotSumOptions.Value).GroupBy(x => x.position)
                .Select(x => (x.Key, x.SelectMany(o => o.possibleValues).ToIntCell())).ToList();
            
    }

    private static Maybe<IReadOnlyCollection<(Position position, IntCell possibleValues)>>
        SolveSum(
            KeyValuePair<Position, IntCell>[] cells, int expectedSum,
            Maybe<ImmutableHashSet<int>> proscribedValues)
    {
        if (expectedSum <= 0)
            return Maybe<IReadOnlyCollection<(Position position, IntCell possibleValues)>>.None;

        if (cells.Length == 0)
            return Maybe<IReadOnlyCollection<(Position position, IntCell possibleValues)>>.None;

        if (cells.Length == 1)
        {
            if (cells.Single().Value.Contains(expectedSum))
                return new (Position position, IntCell possibleValues)[]
                    { (cells.Single().Key, new IntCell().Add(expectedSum)) };
            return Maybe<IReadOnlyCollection<(Position position, IntCell possibleValues)>>.None;
        }


        var otherCells = cells[1..];
        var allProscribedValues = proscribedValues.GetValueOrDefault(ImmutableHashSet<int>.Empty);

        return Combine(
            cells.First().Value
                .Except(allProscribedValues)
                .Where(v => v + otherCells.Length <= expectedSum)
                .Select(v => SolveSum(otherCells, expectedSum - v, proscribedValues.Map(x => x.Add(v))
                    ).Map(x=>(x, v))
                ).ToList() ,
            otherCells.Length,
            cells.First().Key
        );
    }

    private static Maybe<IReadOnlyCollection<(Position position, IntCell possibleValues)>> Combine(
        IReadOnlyCollection<Maybe<(IReadOnlyCollection<(Position position, IntCell possibleValues)>
            , int positionValue)>> options,
        int expectedNumberOfCells, Position positionToAdd)
    {
        var result = options.Where(x => x.HasValue)
            .SelectMany(x => x.Value.Item1)
            .GroupBy(x => x.position)
            .Select(x =>(x.Key, x.SelectMany(y => y.possibleValues).ToIntCell()))
            .ToList();

        if (result.Count() == expectedNumberOfCells)
        {
            result.Add((positionToAdd, options.Where(x => x.HasValue)
                .Select(x => x.Value.positionValue).ToIntCell()));
            return result;
        }

        return Maybe<IReadOnlyCollection<(Position position, IntCell possibleValues)>>.None;
    }
}
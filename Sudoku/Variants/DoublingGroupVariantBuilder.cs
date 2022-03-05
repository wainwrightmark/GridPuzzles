using MoreLinq.Experimental;

namespace Sudoku.Variants;

public partial class DoublingGroupVariantBuilder<T, TCell> : VariantBuilder<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    private DoublingGroupVariantBuilder()
    {
    }

    public static IVariantBuilder<T, TCell> Instance { get; } = new DoublingGroupVariantBuilder<T, TCell>();

    /// <inheritdoc />
    public override string Name => "Doubling Group";


    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder<T, TCell>>> TryGetClueBuilders1(
        IReadOnlyDictionary<string, string> arguments)
    {
        var pr = PositionArgument.TryGetFromDictionary(arguments);
        if (pr.IsFailure) return pr.ConvertFailure<IReadOnlyCollection<IClueBuilder<T, TCell>>>();

        if (pr.Value.Count % 2 != 0)
        {
            return Result.Failure<IReadOnlyCollection<IClueBuilder<T, TCell>>>(
                "Doubling Group must contain an even number of positions");
        }

        var l = new List<IClueBuilder<T, TCell>>
        {
            new DoublingGroupClueBuilder(pr.Value.ToImmutableArray())
        };

        return l;
    }

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments => new VariantBuilderArgument[]
    {
        PositionArgument
    };

    private static readonly ListPositionArgument PositionArgument = new("Positions",
        2, 18);

    [Equatable]
    private partial record DoublingGroupClueBuilder([property:OrderedEquality] ImmutableArray<Position> Positions) : IClueBuilder<T, TCell>
    {
        /// <inheritdoc />
        public string Name => "Doubling Group";

        /// <inheritdoc />
        public int Level => 2;

        /// <inheritdoc />
        public IEnumerable<IClue<T, TCell>> CreateClues(Position minPosition, Position maxPosition,
            IValueSource<T, TCell> valueSource,
            IReadOnlyCollection<IClue<T, TCell>> lowerLevelClues)
        {
            if (Positions.Length <= 0)
                yield break;

            var uniquenessClues = lowerLevelClues.OfType<IUniquenessClue<T, TCell>>().Memoize();
            var remainingPositions = Positions.ToList();

            var @continue = true;
            while (@continue)
            {
                @continue = false;
                if (remainingPositions.Count == 2)
                {
                    var (t1, t2) = remainingPositions.GetFirstTwo();
                    yield return new RelationshipClue<T, TCell>(t1, t2, AreEqualConstraint<T>.Instance);
                    yield break;
                }

                for (var i = 0; i < remainingPositions.Count; i++)
                {
                    var position = remainingPositions[i];
                    var myUniquenessClues = uniquenessClues
                        .Where(x => x.Positions.Contains(position))
                        .Memoize();

                    var matchingPositions = remainingPositions
                        .Where((x, j) => i != j && !myUniquenessClues.Any(clue => clue.Positions.Contains(x)))
                        .ToList();

                    if (matchingPositions.Count == 1)
                    {
                        remainingPositions.RemoveAt(i);
                        remainingPositions.Remove(matchingPositions.Single());
                        yield return new RelationshipClue<T, TCell>(position, matchingPositions.Single(),
                            AreEqualConstraint<T>.Instance);
                    }
                }

            }

            yield return new DoublingGroupClue(remainingPositions.ToImmutableSortedSet());
        }
            
        /// <inheritdoc />
        public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
        {
            var path = Positions.BuildContiguousPath(true);

            if (path.HasValue)
            {
                yield return new LineCellOverlay(path.Value.ToList(), Color.LightSlateGray);
            }
            else
            {
                foreach (var position in Positions)
                {
                    yield return new CellColorOverlay(Color.LightSlateGray, position);
                }
            }
        }
    }

    public class DoublingGroupClue : IRuleClue<T, TCell>
    {
        public DoublingGroupClue(ImmutableSortedSet<Position> positions)
        {
            Positions = positions;
        }

        /// <inheritdoc />
        public IEnumerable<ICellChangeResult> CalculateCellUpdates(Grid<T, TCell> grid)
        {
            var cells = Positions.Select(grid.GetCellKVP).ToList();

            //restrict to just values which appear at least twice
            var valuesWhichAppearOnlyOnce = cells
                .SelectMany(cell => cell.Value.Select(pv => (cell, pv)))
                .GroupBy(x => x.pv)
                .Where(x => x.Count() == 1)
                .ToList();

            foreach (var val in valuesWhichAppearOnlyOnce)
            {
                var (kvp, pv) = val.Single();
                yield return kvp.CloneWithoutValue(pv,
                    new DoublingGroupReason<T, TCell>(pv, this));
            }

            var fixedValues = cells.Where(x => x.Value.HasSingleValue())
                .Select(cell => (cell, value: cell.Value.Single()))
                .GroupBy(x => x.value);

            foreach (var group in fixedValues)
            {
                switch (group.Count())
                {
                    case 1:
                    {
                        var otherCells = cells.Where(x =>
                            x.Value.Count() > 1 &&
                            x.Value.Contains(group.Key)).TrySingle(0, 1, 2);

                        if (otherCells.Cardinality == 1)
                        {
                            yield return (otherCells.Value.CloneWithOnlyValue(group.Key, new DoublingGroupReason<T, TCell>(group.Key, this)));
                        }

                        break;
                    }
                    case 2:
                    {
                        foreach (var kvp in cells.Where(x =>
                                     x.Value.Count() > 1 && x.Value.Contains(group.Key)))
                        {
                            yield return (kvp.CloneWithoutValue(group.Key, new DoublingGroupReason<T, TCell>(group.Key, this) ));
                        }

                        break;
                    }
                    default:
                    {
                        yield return new Contradiction(
                            new DoublingGroupReason<T, TCell>(group.Key, this),
                            @group.Select(x => x.cell.Key).ToImmutableArray());
                        break;
                    }
                }
            }
        }

        /// <inheritdoc />
        public string Name => "Doubling Group";

        /// <inheritdoc />
        public ImmutableSortedSet<Position> Positions { get; }
    }
}

public sealed record DoublingGroupReason<T, TCell>(T Value, DoublingGroupVariantBuilder<T, TCell>.DoublingGroupClue DoublingGroupClue)
    : ISingleReason where T :struct where TCell : ICell<T, TCell>, new()
{
    /// <inheritdoc />
    public string Text => $"{Value} must appear exactly twice in Doubling Group";

    /// <inheritdoc />
    public IEnumerable<Position> GetContributingPositions(IGrid grid)
    {
        return DoublingGroupClue.Positions;
    }

    /// <inheritdoc />
    public Maybe<IClue> Clue => DoublingGroupClue;
}
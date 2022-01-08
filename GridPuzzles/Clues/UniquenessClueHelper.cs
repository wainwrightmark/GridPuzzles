


namespace GridPuzzles.Clues;

public sealed class UniquenessClueHelper<T> : ClueHelper<IUniquenessClue<T>, T>, IClueUpdateHelper<T>
    where T : notnull
{
    /// <inheritdoc />
    public UniquenessClueHelper(IEnumerable<IClue<T>> allClues) : base(
        MergeClues(allClues.OfType<IUniquenessClue<T>>()))
    {
        Lookup = Clues
            .SelectMany(clue => clue.Positions.Select(p => (p, clue)))
            .ToLookup(x => x.p, x => x.clue);

        LazyUniquePairs =
            new Lazy<IReadOnlyDictionary<Position, IReadOnlySet<Position>>>(() => CreateUniquePairs(Lookup));
    }

    public readonly ILookup<Position, IUniquenessClue<T>> Lookup;

    private Lazy<IReadOnlyDictionary<Position, IReadOnlySet<Position>>> LazyUniquePairs { get; }

    private readonly ConcurrentDictionary<ImmutableSortedSet<Position>, bool> _mutuallyUniquePositionsCache =
        new(SortedSetElementsComparer<Position>.Instance);

    private readonly ConcurrentDictionary<ImmutableSortedSet<Position>, IReadOnlySet<Position>>
        _mutuallyCommunicationPositionsCache = new(SortedSetElementsComparer<Position>.Instance);


    /// <summary>
    /// Gets all positions which communicate with every member of the set and which are not members of the set
    /// </summary>
    public IReadOnlySet<Position> GetCommunicatingPositions(ImmutableSortedSet<Position> positions)
    {
        return _mutuallyCommunicationPositionsCache.GetOrAdd(positions, GetCommunicatingPositions2);

        IReadOnlySet<Position> GetCommunicatingPositions2(ImmutableSortedSet<Position> set)
        {
            var pairs = set.Select(x =>
                    LazyUniquePairs.Value.TryGetValue(x, out var s1)
                        ? s1
                        : ImmutableSortedSet<Position>.Empty)
                .OrderBy(x => x.Count)
                .IntersectAllSets().Except(set);

            return new HashSet<Position>(pairs);
        }
    }


    private static IReadOnlyDictionary<Position, IReadOnlySet<Position>> CreateUniquePairs(
        ILookup<Position, IUniquenessClue<T>> lookup)
    {
        var pairs =
            lookup.SelectMany(x => x).Distinct().SelectMany(clue =>
                    clue.Positions.SelectMany(p =>
                        clue.Positions.Remove(p).Select(p2 => new KeyValuePair<Position, Position>(p, p2))))
                .GroupBy(x => x.Key, x => x.Value)
                .ToDictionary(x => x.Key,
                    x => new HashSet<Position>(x) as IReadOnlySet<Position>);

        return pairs;
    }

    public bool ArePositionsMutuallyUnique(Position p1, Position p2) =>
        LazyUniquePairs.Value.TryGetValue(p1, out var s) && s.Contains(p2);

    public bool ArePositionsMutuallyUnique(ImmutableSortedSet<Position> ps)
    {
        return _mutuallyUniquePositionsCache.GetOrAdd(ps, ArePositionsMutuallyUniqueHelper);

        bool ArePositionsMutuallyUniqueHelper(IImmutableSet<Position> positions)
        {
            var remaining = positions;

            while (remaining.Any())
            {
                var position = remaining.First();
                remaining = remaining.Remove(position);

                if (!LazyUniquePairs.Value.TryGetValue(position, out var s) || !s.IsSupersetOf(remaining))
                {
                    return false;
                }
            }

            return true;
        }
    }


    private static IEnumerable<IUniquenessClue<T>> MergeClues(IEnumerable<IUniquenessClue<T>> sourceClues)
    {
        var sets = new HashSet<ImmutableSortedSet<Position>>(SortedSetElementsComparer<Position>.Instance);

        foreach (var uniquenessClue in sourceClues.Distinct().OrderByDescending(x => x.Positions.Count))
        {
            if (sets.Contains(uniquenessClue.Positions)) continue;
            if (sets.Any(s => s.IsSupersetOf(uniquenessClue.Positions))) continue;
            sets.Add(uniquenessClue.Positions);
            yield return uniquenessClue;
        }
    }

    public IEnumerable<ICellChangeResult> CalculateUpdates(Grid<T> grid,
        int bifurcationLevel,
        Maybe<IReadOnlyCollection<Position>> positions)
    {
        var clues =
            positions.HasValue ? positions.Value.SelectMany(p => Lookup[p]).Distinct() : Clues;
        return clues
            .SelectMany(clue => GetCellUpdates(clue, grid));
    }

    private static IEnumerable<ICellChangeResult> GetCellUpdates(IUniquenessClue<T> clue, Grid<T> grid)
    {
        var cellPositionsGroups = clue.Positions
            .Select(grid.GetCellKVP)
            .GroupBy(x => x.Value, x => x.Key);

        foreach (var cellPositions in cellPositionsGroups)
        {
            var positionsCount = cellPositions.Count();
            var reason = GetReason(cellPositions.Key);

            if (cellPositions.Key.PossibleValues.Count < positionsCount)
            {
                yield return (new Contradiction(
                    reason,
                    cellPositions.ToImmutableArray()));
            }
            else if (cellPositions.Key.PossibleValues.Count == positionsCount)
            {
                IEnumerable<Position> positionsToCheck;

                if (positionsCount is <= 1 or >= 4)
                    positionsToCheck = clue.Positions.Except(cellPositions);
                else
                {
                    positionsToCheck = grid.ClueSource
                        .UniquenessClueHelper
                        .GetCommunicatingPositions(cellPositions.ToImmutableSortedSet());
                }

                foreach (var p in positionsToCheck)
                {
                    var cell = grid.GetCellKVP(p);
                    if (cell.Value.PossibleValues.Overlaps(cellPositions.Key.PossibleValues))
                    {
                        yield return cell.CloneWithoutValues(cellPositions.Key.PossibleValues,
                            reason);
                    }
                }
            }
        }

        ISingleReason GetReason(Cell<T> cell)
        {
            if (cell.PossibleValues.Count == 1)
                return new AlreadyExistsReason<T>(cell.PossibleValues.Single(), clue);
            else return new PermutationReason<T>(cell.PossibleValues, clue);
        }
    }
}
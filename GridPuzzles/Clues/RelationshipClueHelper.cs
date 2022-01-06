using System.Diagnostics.Contracts;
using GridPuzzles.Bifurcation;

namespace GridPuzzles.Clues;

public interface IClueUpdateHelper<T> where T : notnull
{
    /// <summary>
    /// Calculate Updates related to this clue
    /// </summary>
    [Pure]
    IEnumerable<ICellChangeResult> CalculateUpdates(Grid<T> grid, Maybe<IReadOnlyCollection<Position>> positionsToLookAt);
}

public abstract class ClueHelper<TClue,T> where TClue : IClue<T>where T: notnull
{
    protected ClueHelper(IEnumerable<IClue<T>> clues)
    {
        Clues = clues.OfType<TClue>().ToImmutableArray();
    }

    public ImmutableArray<TClue> Clues { get; }
}

public sealed class BifurcationClueHelper<T> : ClueHelper<IBifurcationClue<T>, T>
    where T: notnull
{
    public BifurcationClueHelper(IEnumerable<IClue<T>> allClues) : base(allClues)
    {
        Lookup = Clues
            .SelectMany(clue => clue.Positions.Select(p => (p, clue)))
            .ToLookup(x => x.p, x => x.clue);
    }
        
    public ILookup<Position, IBifurcationClue<T>> Lookup;
        
    /// <summary>
    /// Find Bifurcation Options relating to this clue
    /// </summary>
    [Pure]
    public IEnumerable<IBifurcationOption<T>> CalculateBifurcationOptions(Grid<T> grid, 
        Maybe<IEnumerable<Position>>
            positionsToLookAt,
            
        int maxChoices)
    {
        var clues = positionsToLookAt.HasValue?
            positionsToLookAt.Value.SelectMany(x => Lookup[x]).Distinct() : 
            Clues;

        foreach (var ruleClue in clues)
        foreach (var bifurcationOption in ruleClue.GetBifurcationOptions(grid, maxChoices))
            yield return bifurcationOption;
    }
}

public sealed class DynamicOverlayClueHelper<T> : ClueHelper<IDynamicOverlayClue<T>, T>
    where T: notnull
{
    public DynamicOverlayClueHelper(IEnumerable<IClue<T>> allClues) : base(allClues) { }

    /// <summary>
    /// Create the Cell Overlays relating to this clue
    /// </summary>
    [Pure]
    public IEnumerable<CellOverlayWrapper> CreateCellOverlays(Grid<T> grid)
    {
        return Clues.SelectMany(x => x.GetCellOverlays(grid))
            .Select(x=> new CellOverlayWrapper(x, CellOverlayMetadata.Empty));
    }
}

public sealed class RuleClueHelper<T> : ClueHelper<IRuleClue<T>, T>, IClueUpdateHelper<T>
    where T: notnull
{
    /// <inheritdoc />
    public RuleClueHelper(IEnumerable<IClue<T>> allClues) : base(allClues)
    {
        Lookup = Clues
            .SelectMany(clue => clue.Positions.Select(p => (p, clue)))
            .ToLookup(x => x.p, x => x.clue);
    }

    public ILookup<Position, IRuleClue<T>> Lookup;

    /// <inheritdoc />
    public IEnumerable<ICellChangeResult> CalculateUpdates(Grid<T> grid,
        Maybe<IReadOnlyCollection<Position>> positionsToLookAt)
    {
        var clues =
            positionsToLookAt.HasValue?
                positionsToLookAt.Value.SelectMany(x => Lookup[x]).Distinct() :
                Clues;

        return clues.SelectMany(x => x.GetCellUpdates(grid));
    }
}

public sealed class RelationshipClueHelper<T> : ClueHelper<IRelationshipClue<T>, T>, IClueUpdateHelper<T>//TODO special helper for if lookup is empty
    where T: notnull
{
    /// <inheritdoc />
    public RelationshipClueHelper(UniquenessClueHelper<T> uniquenessClueHelper, IEnumerable<IClue<T>> allClues) : base(allClues)
    {
        //TODO combine clues
        //Note this does not include uniqueness clues
        Lookup = Clues
            .Select(c=> uniquenessClueHelper.ArePositionsMutuallyUnique(c.Position1, c.Position2)? c.UniqueVersion : c)
            .SelectMany(x => new[] {x, x.Flipped})
            .ToLookup(x=>x.Position1);
    }

    /// <summary>
    /// Lookup from positions to clues where Position1 is that position
    /// </summary>
    public ILookup<Position, IRelationshipClue<T>> Lookup { get; }

    /// <inheritdoc />
    public IEnumerable<ICellChangeResult> CalculateUpdates(Grid<T> grid,
        Maybe<IReadOnlyCollection<Position>> positionsToLookAt)
    {
        if (Lookup.Any())
        {
            foreach (var position in positionsToLookAt.GetValueOrDefault(grid.AllPositions))
            {
                var clueCellPairs = Lookup[position]
                    .Select(clue => (clue,cell: grid.GetCellKVP(clue.Position2))).ToList();

                if (!clueCellPairs.Any())
                    continue;

                var cell = grid.GetCellKVP(position);

                foreach (var (clue, otherCell) in clueCellPairs)
                {
                    var (changed, newSet1, newSet2) = clue.GetValidValues(cell.Value.PossibleValues, otherCell.Value.PossibleValues);
                    if (changed)
                    {
                        yield return cell.CloneWithOnlyValues(newSet1, clue.Reason);
                        yield return otherCell.CloneWithOnlyValues(newSet2, clue.Reason);
                    }
                }

            }
        }
    }

    public bool DoAnyRelationshipsExist(IReadOnlyList<Position> positions)
    {
        if (!Lookup.Any()) return false;
        for (var index = 0; index < positions.Count; index++)
        {
            var position1 = positions[index];
            for (var i = index + 1; i < positions.Count; i++)
            {
                var position2 = positions[i];
                var clues = GetClues(position1, position2);
                if (clues.Any()) return true;
            }
        }

        return false;
    }

    public bool AreValuesValid(Position p1, Position p2, T v1, T v2)
    {
        var clues = GetClues(p1, p2);

        return clues.All(c => c.IsValidCombination(v1, v2));
    }

    public IEnumerable<bool> CheckRelationship(Position p1, Position p2, Func<IRelationshipClue<T>, bool> func)
    {
        var clues = GetClues(p1, p2);

        return clues.Select(func);
    }

    private IReadOnlyCollection<IRelationshipClue<T>> GetClues(Position p1, Position p2)
    {
        if (!Lookup.Any()) return ArraySegment<IRelationshipClue<T>>.Empty;
        var clues = _pairWiseCluesConcurrentDictionary.GetOrAdd((p1, p2), FindPairwiseClues);
        return clues;
        IReadOnlyList<IRelationshipClue<T>> FindPairwiseClues((Position p1, Position p2) pair) =>
            Lookup[pair.p1].Where(x => x.Position2 == pair.p2).ToList();
    }

    

    private readonly ConcurrentDictionary<(Position p1, Position p2), IReadOnlyCollection<IRelationshipClue<T>>>
        _pairWiseCluesConcurrentDictionary = new();


}

public class SortedSetElementsComparer<T> : IEqualityComparer<ImmutableSortedSet<T>> where T : notnull
{
    private SortedSetElementsComparer() {}

    public static IEqualityComparer<ImmutableSortedSet<T>> Instance { get; } = new SortedSetElementsComparer<T>();

    /// <inheritdoc />
    public bool Equals(ImmutableSortedSet<T>? x, ImmutableSortedSet<T>? y)
    {
        if (x is null || y is null)
            return false;

        if (ReferenceEquals(x, y)) return true;

        return x.SetEquals(y);
    }

    /// <inheritdoc />
    public int GetHashCode(ImmutableSortedSet<T> obj) => HashCode.Combine(obj.Count, obj.Min, obj.Max);
}
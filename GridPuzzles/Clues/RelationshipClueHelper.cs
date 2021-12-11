using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using CSharpFunctionalExtensions;
using GridPuzzles.Bifurcation;
using GridPuzzles.Cells;
using GridPuzzles.Overlays;

namespace GridPuzzles.Clues;

public interface IClueUpdateHelper<T> where T : notnull
{
    [Pure]
    IEnumerable<ICellChangeResult> GetUpdates(Grid<T> grid, Maybe<IReadOnlyCollection<Position>> positionsToLookAt);
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
        
    [Pure]
    public IEnumerable<IBifurcationOption<T>> GetBifurcationOptions(Grid<T> grid, 
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

    [Pure]
    public IEnumerable<ICellOverlay> GetCellOverlays(Grid<T> grid)
    {
        return Clues.SelectMany(x => x.GetCellOverlays(grid));
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
    public IEnumerable<ICellChangeResult> GetUpdates(Grid<T> grid,
        Maybe<IReadOnlyCollection<Position>> positionsToLookAt)
    {
        var clues =
            positionsToLookAt.HasValue?
                positionsToLookAt.Value.SelectMany(x => Lookup[x]).Distinct() :
                Clues;

        return clues.SelectMany(x => x.GetCellUpdates(grid));
    }
}

public sealed class RelationshipClueHelper<T> : ClueHelper<IRelationshipClue<T>, T>, IClueUpdateHelper<T>
    where T: notnull
{
    /// <inheritdoc />
    public RelationshipClueHelper(UniquenessClueHelper<T> uniquenessClueHelper, IEnumerable<IClue<T>> allClues) : base(allClues)
    {
        //TODO combine clues
        Lookup = Clues
            .Select(c=> uniquenessClueHelper.ArePositionsMutuallyUnique(c.Position1, c.Position2)? c.UniqueVersion : c)
            .SelectMany(x => new[] {x, x.Flipped})
            .ToLookup(x=>x.Position1);
    }

    public ILookup<Position, IRelationshipClue<T>> Lookup { get; }

    /// <inheritdoc />
    public IEnumerable<ICellChangeResult> GetUpdates(Grid<T> grid,
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


    public bool AreValuesValid(Position p1, Position p2, T v1, T v2)
    {
        var clues = _pairWiseCluesConcurrentDictionary.GetOrAdd((p1, p2), GetPairwiseClues);

        return clues.All(c => c.IsValidCombination(v1, v2));

        IReadOnlyList<IRelationshipClue<T>> GetPairwiseClues((Position p1, Position p2) pair) =>
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
using System.Diagnostics.Contracts;
using GridPuzzles.Bifurcation;
using GridPuzzles.Clues.Constraints;
using GridPuzzles.Overlays;

namespace GridPuzzles.Clues;

public interface IClue
{
    string Name { get; }

    /// <summary>
    /// List of affected positions
    /// </summary>
    ImmutableSortedSet<Position> Positions { get; }
}

// ReSharper disable once UnusedTypeParameter
public interface IClue<T> : IClue where T: notnull {}

/// <summary>
/// A clue that says cell values in affected positions must be unique
/// </summary>
public interface IUniquenessClue<T> : IClue<T> where T : notnull
{
    string Domain { get; }
}


public interface IRelationshipClue<T> :
    IClue<T> where T: notnull
{
    Position Position1 { get; }
    Position Position2 { get; }

    /// <summary>
    /// Get a version of this clue with the positions flipped
    /// </summary>
    IRelationshipClue<T> Flipped { get; }

    /// <summary>
    /// Gets a unique version of this clue
    /// </summary>
    IRelationshipClue<T> UniqueVersion { get; }

    /// <summary>
    /// Get valid values for this relationship
    /// </summary>
    (bool changed, ImmutableSortedSet<T> newSet1, ImmutableSortedSet<T> newSet2) FindValidValues(
        ImmutableSortedSet<T> set1, ImmutableSortedSet<T> set2);

    bool IsValidCombination(T p1Value, T p2Value);

    ISingleReason Reason { get; }

    Constraint<T> Constraint { get; }

}

/// <summary>
/// A clue that also give bifurcation options
/// </summary>
public interface IBifurcationClue<T> : IClue<T>where T: notnull
{
    IEnumerable<IBifurcationOption<T>> FindBifurcationOptions(Grid<T> grid, int maxChoices);
}

/// <summary>
/// A clue that gives cell updates
/// </summary>
public interface IRuleClue<T> : IClue<T> where T: notnull
{
    /// <summary>
    /// Calculate cell updates based on this rule
    /// </summary>
    [Pure]
    IEnumerable<ICellChangeResult> CalculateCellUpdates(Grid<T> grid);
}

/// <summary>
/// A clue which gives cell updates but affects the entire grid
/// </summary>
public interface IMetaClue<T> : IClue<T> where T : notnull
{
    /// <summary>
    /// Creates clues based on the current grid state
    /// </summary>
    IEnumerable<IClue<T>> CreateClues(Grid<T> grid, Maybe<IReadOnlySet<Position>> positions);

    /// <inheritdoc />
    ImmutableSortedSet<Position> IClue.Positions => ImmutableSortedSet<Position>.Empty;
}

/// <summary>
/// A clue that displays a dynamic overlay
/// </summary>
public interface IDynamicOverlayClue<T> : IClue<T> where T: notnull
{
    /// <summary>
    /// Create Cell overlays from this clue
    /// </summary>
    [Pure]
    IEnumerable<ICellOverlay> CreateCellOverlays(Grid<T> grid);
}
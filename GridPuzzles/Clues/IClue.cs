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
public interface IClue<T, TCell> : IClue where T :struct where TCell : ICell<T, TCell>, new() {}

/// <summary>
/// A clue that says cell values in affected positions must be unique
/// </summary>
public interface IUniquenessClue<T, TCell> : IClue<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    string Domain { get; }
}


public interface IRelationshipClue<T, TCell> :
    IClue<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    Position Position1 { get; }
    Position Position2 { get; }

    /// <summary>
    /// Get a version of this clue with the positions flipped
    /// </summary>
    IRelationshipClue<T, TCell> Flipped { get; }

    /// <summary>
    /// Gets a unique version of this clue
    /// </summary>
    IRelationshipClue<T, TCell> UniqueVersion { get; }

    /// <summary>
    /// Get valid values for this relationship
    /// </summary>
    (bool changed, TCell newSet1, TCell newSet2) FindValidValues(
        TCell set1, TCell set2);

    bool IsValidCombination(T p1Value, T p2Value);

    ISingleReason Reason { get; }

    Constraint<T> Constraint { get; }

}

/// <summary>
/// A clue that also give bifurcation options
/// </summary>
public interface IBifurcationClue<T, TCell> : IClue<T, TCell>where T :struct where TCell : ICell<T, TCell>, new()
{
    IEnumerable<IBifurcationOption<T, TCell>> FindBifurcationOptions(Grid<T, TCell> grid, int maxChoices);
}

/// <summary>
/// A clue that gives cell updates
/// </summary>
public interface IRuleClue<T, TCell> : IClue<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    /// <summary>
    /// Calculate cell updates based on this rule
    /// </summary>
    [Pure]
    IEnumerable<ICellChangeResult> CalculateCellUpdates(Grid<T, TCell> grid);
}

/// <summary>
/// A clue which gives cell updates but affects the entire grid
/// </summary>
public interface IMetaClue<T, TCell> : IClue<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    /// <summary>
    /// Creates clues based on the current grid state
    /// </summary>
    IEnumerable<IClue<T, TCell>> CreateClues(Grid<T, TCell> grid, Maybe<IReadOnlySet<Position>> positions);

    /// <inheritdoc />
    ImmutableSortedSet<Position> IClue.Positions => ImmutableSortedSet<Position>.Empty;
}

/// <summary>
/// A clue that displays a dynamic overlay
/// </summary>
public interface IDynamicOverlayClue<T, TCell> : IClue<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    /// <summary>
    /// Create Cell overlays from this clue
    /// </summary>
    [Pure]
    IEnumerable<ICellOverlay> CreateCellOverlays(Grid<T, TCell> grid);
}
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

public interface IClue<T> : IClue where T: notnull {}

/// <summary>
/// A clue that says cell values in affected positions must be unique
/// </summary>
/// <typeparam name="T"></typeparam>
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


    (bool changed, ImmutableSortedSet<T> newSet1, ImmutableSortedSet<T> newSet2) GetValidValues(
        ImmutableSortedSet<T> set1, ImmutableSortedSet<T> set2);

    bool IsValidCombination(T p1Value, T p2Value);

    ISingleReason Reason { get; }

    Constraint<T> Constraint { get; }
}

/// <summary>
/// A clue that also give bifurcation options
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IBifurcationClue<T> : IClue<T>where T: notnull
{
    IEnumerable<IBifurcationOption<T>> GetBifurcationOptions(Grid<T> grid, int maxChoices);
}

public interface IRuleClue<T> : IClue<T>where T: notnull
{
    [Pure]
    IEnumerable<ICellChangeResult> GetCellUpdates(Grid<T> grid);
}

public interface IDynamicOverlayClue<T> : IClue<T> where T: notnull
{
    [Pure]
    IEnumerable<ICellOverlay> GetCellOverlays(Grid<T> grid);
}
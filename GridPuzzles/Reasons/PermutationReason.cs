using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;
using GridPuzzles.Clues;
using MoreLinq;

namespace GridPuzzles.Reasons;

public sealed record PermutationReason<T>(ImmutableSortedSet<T> Values, IUniquenessClue<T> UniquenessClue): ISingleReason
    where T : notnull
{
        
    /// <inheritdoc />
    public string Text => $"The values [{Values.ToDelimitedString("")}] are a permutation in {UniquenessClue.Domain}.";

    /// <inheritdoc />
    public IEnumerable<Position> GetContributingPositions(IGrid grid)
    {
        if (grid is Grid<T> gridT)
        {
            return 
                UniquenessClue.Positions.Select(gridT.GetCellKVP)
                    .Where(x => Values.IsSupersetOf(x.Value.PossibleValues))
                    .Select(x=>x.Key);
        }

        return UniquenessClue.Positions;
    }

    /// <inheritdoc />
    public bool Equals(PermutationReason<T>? other)
    {
        return other is not null && other.UniquenessClue == UniquenessClue && Values.SequenceEqual(other.Values);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(UniquenessClue, Values.Count, Values.First(), Values.Last());
    }

    /// <inheritdoc />
    public Maybe<IClue> Clue => Maybe<IClue>.From(UniquenessClue);
}
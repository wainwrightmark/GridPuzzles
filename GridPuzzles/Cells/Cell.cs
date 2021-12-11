using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using GridPuzzles.Bifurcation;
using GridPuzzles.Clues;
using GridPuzzles.Reasons;

namespace GridPuzzles.Cells;

public sealed class Cell<T> : IEquatable<Cell<T>>
{
    public Cell(ImmutableSortedSet<T> possibilities)
    {
        PossibleValues = possibilities;
        HashCode = new Lazy<int>(() => GetSetHashCode(PossibleValues));
    }

    private static int GetSetHashCode(ImmutableSortedSet<T> set)
    {
        return set.Count switch
        {
            0 => -1,
            1 => set.Single()!.GetHashCode(),
            2 => System.HashCode.Combine(set[0], set[1]),
            3 => System.HashCode.Combine(set[0], set[1], set[2]),
            4 => System.HashCode.Combine(set[0], set[1], set[2], set[3]),
            _ => System.HashCode.Combine(set[0], set[1], set[2], set[3], set[4])
        };
    }

    /// <inheritdoc />
    public override string ToString() => String;
    public ImmutableSortedSet<T> PossibleValues { get; } //TODO Make private
    private string String => "[" + string.Join("", PossibleValues) + "]";


    public bool CouldHaveAnyValue(IValueSource<T> valueSource) => PossibleValues.SequenceEqual(valueSource.AllValues);
    public bool HasFixedValue => PossibleValues.Count == 1;

    [Pure]
    public IEnumerable<IBifurcationOption<T>> GetBifurcationOptions(Position p, int maxValues)
    {
        if(PossibleValues.Count < 2)
            yield break;

        if (PossibleValues.Count <= maxValues)
        {
            var choices = PossibleValues
                .Select(value => CellHelper. Create(value, p, BifurcationAttemptReason.Instance))
                .Select(cellUpdate => new BifurcationCellChoice<T>(cellUpdate));
                

            var bo = new BifurcationOption<T>(0, new PossibleValuesReason(p), choices);
            yield return bo;
        }
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is Cell<T> other && Equals(other);

    private Lazy<int> HashCode { get; }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Value;

    /// <inheritdoc />
    public bool Equals(Cell<T>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ReferenceEquals(PossibleValues, other.PossibleValues) || PossibleValues.SequenceEqual(other.PossibleValues);
    }
}
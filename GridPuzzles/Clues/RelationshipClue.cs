using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using GridPuzzles.Clues.Constraints;
using GridPuzzles.Reasons;

namespace GridPuzzles.Clues;

public class RelationshipClue<T> : IRelationshipClue<T> where T : notnull
{
    public RelationshipClue(Position position1, Position position2, Constraint<T> constraint) :
        this(position1, position2, constraint, true, false) {}

    public static RelationshipClue<T> Create(Position position1, Position position2, Constraint<T> constraint) => new(position1, position2, constraint);

    private RelationshipClue(Position position1, Position position2, Constraint<T> constraint, bool setFlipped, bool unique)
    {
        Unique = unique;
        Position1 = position1;
        Position2 = position2;
        Constraint = constraint;
        Positions = ImmutableSortedSet.Create(position1, position2);
        if (setFlipped)
            Flipped = new RelationshipClue<T>(position2, position1, constraint.FlippedConstraint, false, unique)
            {
                Flipped = this
            };
        else
            Flipped = null!;

        Reason = new RelationshipClueReason<T>(this);
    }

    public Position Position1 { get; }
    public Position Position2 { get; }
    public ISingleReason Reason { get; } 

    public bool Unique { get; }

    /// <inheritdoc />
    public IRelationshipClue<T> Flipped { get; private set; }

    /// <inheritdoc />
    public IRelationshipClue<T> UniqueVersion
    {
        get
        {
            if (Unique) return this;

            return new RelationshipClue<T>(Position1, Position2, Constraint, true, true);
        }
    }

    /// <inheritdoc />
    public (bool changed, ImmutableSortedSet<T> newSet1, ImmutableSortedSet<T> newSet2) GetValidValues(ImmutableSortedSet<T> set1, ImmutableSortedSet<T> set2)
    {
        var key = new Key(Constraint, set1, set2, Unique);

        var r = Cache.GetOrAdd(key, k => k.Calculate());

        return r;
    }

    /// <inheritdoc />
    public bool IsValidCombination(T p1Value, T p2Value) => Constraint.IsValid(p1Value, p2Value);


    public Constraint<T> Constraint { get; }

    /// <inheritdoc />
    public string Name => Constraint.Name;

    /// <inheritdoc />
    public override string ToString() => $"{Name} ({Position1},{Position2})";

    /// <inheritdoc />
    public ImmutableSortedSet<Position> Positions { get; }

    private static readonly ConcurrentDictionary<Key, (bool changed, ImmutableSortedSet<T> set1, ImmutableSortedSet<T> set2)> Cache
        = new();

    private class Key : IEquatable<Key>
    {
        public Key(Constraint<T> constraint, ImmutableSortedSet<T> set1, ImmutableSortedSet<T> set2, bool isUnique)
        {
            Constraint = constraint;
            Set1 = set1;
            Set2 = set2;
            IsUnique = isUnique;
        }

        public (bool changed, ImmutableSortedSet<T> set1, ImmutableSortedSet<T> set2) Calculate()
        {
            var changed = false;
            var newSet1 = Set1;
            var newSet2 = Set2;

            foreach (var val1 in Set1)
            {
                if (Set2.Any(val2 => (!IsUnique || !val1.Equals(val2)) && Constraint.IsValid(val1, val2))) continue;
                newSet1 = newSet1.Remove(val1);
                changed = true;
            }


            foreach (var val2 in Set2)
            {
                if (newSet1.Any(val1 => (!IsUnique || !val1.Equals(val2)) && Constraint.IsValid(val1, val2))) continue;
                newSet2 = newSet2.Remove(val2);
                changed = true;
            }


            return (changed, newSet1, newSet2);
        }

        /// <inheritdoc />
        public override string ToString() => Constraint.Name;

        public Constraint<T> Constraint { get; }
        public ImmutableSortedSet<T> Set1 { get; }
        public ImmutableSortedSet<T> Set2 { get; }

        public bool IsUnique { get; }

        /// <inheritdoc />
        public bool Equals(Key? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return
                IsUnique == other.IsUnique
                && Constraint.Equals(other.Constraint)
                && SortedSetElementsComparer<T>.Instance.Equals(Set1, other.Set1)
                && SortedSetElementsComparer<T>.Instance.Equals(Set2, other.Set2);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Key) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Constraint, SortedSetElementsComparer<T>.Instance.GetHashCode(Set1) , SortedSetElementsComparer<T>.Instance.GetHashCode(Set2), IsUnique);
    }

}
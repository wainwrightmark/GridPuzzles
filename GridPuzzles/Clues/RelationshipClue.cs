using GridPuzzles.Clues.Constraints;

namespace GridPuzzles.Clues;

public class RelationshipCluePosition1AgnosticComparer<T, TCell> : IEqualityComparer<IRelationshipClue<T, TCell>>
    where T : struct where TCell : ICell<T, TCell>, new()
{
    private RelationshipCluePosition1AgnosticComparer()
    {
    }

    public static IEqualityComparer<IRelationshipClue<T, TCell>> Instance { get; } =
        new RelationshipCluePosition1AgnosticComparer<T, TCell>();

    public bool Equals(IRelationshipClue<T, TCell>? x, IRelationshipClue<T, TCell>? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.Position2.Equals(y.Position2) && x.Constraint.Equals(y.Constraint);
    }

    public int GetHashCode(IRelationshipClue<T, TCell> obj) => HashCode.Combine(obj.Position2, obj.Constraint);
}

public class RelationshipClue<T, TCell> : IRelationshipClue<T, TCell>
    where T : struct where TCell : ICell<T, TCell>, new()
{
    public RelationshipClue(Position position1, Position position2, Constraint<T> constraint) :
        this(position1, position2, constraint, true, false)
    {
    }

    public static RelationshipClue<T, TCell> Create(Position position1, Position position2, Constraint<T> constraint) =>
        new(position1, position2, constraint);

    private RelationshipClue(Position position1, Position position2, Constraint<T> constraint, bool setFlipped,
        bool unique)
    {
        Unique = unique;
        Position1 = position1;
        Position2 = position2;
        Constraint = constraint;
        Positions = ImmutableSortedSet.Create(position1, position2);
        if (setFlipped)
            Flipped = new RelationshipClue<T, TCell>(position2, position1, constraint.FlippedConstraint, false, unique)
            {
                Flipped = this
            };
        else
            Flipped = null!;

        Reason = new RelationshipClueReason<T, TCell>(this);
    }

    public Position Position1 { get; }
    public Position Position2 { get; }
    public ISingleReason Reason { get; }

    public bool Unique { get; }

    /// <inheritdoc />
    public IRelationshipClue<T, TCell> Flipped { get; private set; }

    /// <inheritdoc />
    public IRelationshipClue<T, TCell> UniqueVersion
    {
        get
        {
            if (Unique) return this;

            return new RelationshipClue<T, TCell>(Position1, Position2, Constraint, true, true);
        }
    }

    /// <inheritdoc />
    public (bool changed, TCell newSet1, TCell newSet2) FindValidValues(TCell set1, TCell set2)
    {
        var key = new Key(Constraint, set1, set2, Unique);

        var r = Cache.GetOrAdd(key, k => k.Calculate());

        return r;
    }

    /// <inheritdoc />
    public bool IsValidCombination(T p1Value, T p2Value) => Constraint.IsMet(p1Value, p2Value);


    public Constraint<T> Constraint { get; }

    /// <inheritdoc />
    public string Name => Constraint.Name;

    /// <inheritdoc />
    public override string ToString() => $"{Name} ({Position1},{Position2})";

    /// <inheritdoc />
    public ImmutableSortedSet<Position> Positions { get; }

    private static readonly
        ConcurrentDictionary<Key, (bool changed, TCell set1, TCell set2)> Cache
            = new();

    private readonly record struct Key(Constraint<T> Constraint, TCell Set1, TCell Set2, bool IsUnique)
    {
        public (bool changed, TCell set1, TCell set2) Calculate()
        {
            var changed = false;
            var newSet1 = Set1;
            var newSet2 = Set2;

            var u = IsUnique;
            var c = Constraint;

            foreach (var val1 in Set1)
            {
                if (Set2.Any(val2 => (!u || !val1.Equals(val2)) && c.IsMet(val1, val2))) continue;
                newSet1 = newSet1.Remove(val1);
                changed = true;
            }


            foreach (var val2 in Set2)
            {
                if (newSet1.Any(val1 => (!u || !val1.Equals(val2)) && c.IsMet(val1, val2))) continue;
                newSet2 = newSet2.Remove(val2);
                changed = true;
            }


            return (changed, newSet1, newSet2);
        }

        /// <inheritdoc />
        public override string ToString() => Constraint.Name;
    }
}
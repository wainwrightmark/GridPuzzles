namespace GridPuzzles.Clues.Constraints;

public class MultipleConstraint<T> : Constraint<T>
{
    public static Constraint<T> Combine(IReadOnlyCollection<Constraint<T>> constraints)
    {
        var topConstraints = new HashSet<Constraint<T>>();

        foreach (var constraint in constraints)
        {
            if(topConstraints.Any(x=> x.IsSuperConstraint(constraint)))
                continue;

            topConstraints.RemoveWhere(x => constraint.IsSuperConstraint(x));
            topConstraints.Add(constraint);
        }

        if (topConstraints.Count == 1)
            return topConstraints.Single();

        return Create(topConstraints.ToImmutableSortedSet());

    }


    public static MultipleConstraint<T> Create(ImmutableSortedSet<Constraint<T>> constraints)
    {

        var mc = new MultipleConstraint<T>(constraints);
        var flipped = new MultipleConstraint<T>(constraints.Select(x=>x.FlippedConstraint).ToImmutableSortedSet())
        {
            FlippedConstraintPrivate = mc
        };
        mc.FlippedConstraintPrivate = flipped;

        return mc;
    }

    /// <inheritdoc />
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    private MultipleConstraint(ImmutableSortedSet<Constraint<T>> constraints)
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    {
        Constraints = constraints;
    }

    public ImmutableSortedSet<Constraint<T>> Constraints { get; }

    private Constraint<T> FlippedConstraintPrivate { get; set; }

    public override Constraint<T> FlippedConstraint => FlippedConstraintPrivate;

    /// <inheritdoc />
    public override string Name => string.Join(", ", Constraints.Select(x=>x.Name));


    /// <inheritdoc />
    public override bool IsValid(T t1, T t2) => Constraints.All(c => c.IsValid(t1, t2));

    /// <inheritdoc />
    public override bool IsSuperConstraint(Constraint<T> other)
    {
        if (other is MultipleConstraint<T> otherMc)
        {
            return otherMc.Constraints.All(IsSuperConstraint);
        }

        return Constraints.Any(c => c.IsSuperConstraint(other));
    }

    /// <inheritdoc />
    public override bool Equals(Constraint<T>? other)
    {
        if (ReferenceEquals(this, other)) return true;

        return other is MultipleConstraint<T> mc && SortedSetElementsComparer<Constraint<T>>.Instance.Equals(Constraints, mc.Constraints);
    }

    /// <inheritdoc />
    protected override int GetHashCode1() => SortedSetElementsComparer<Constraint<T>>.Instance.GetHashCode(Constraints);
}
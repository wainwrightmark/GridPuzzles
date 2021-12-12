namespace GridPuzzles.Clues.Constraints;

public abstract class Constraint<T> : IEquatable<Constraint<T>> //TODO make constraints records
{
    public abstract string Name { get; }

    public abstract Constraint<T> FlippedConstraint { get; }

    public abstract bool IsValid(T t1, T t2);

    public abstract bool IsSuperConstraint(Constraint<T> other);

    /// <inheritdoc />
    public abstract bool Equals(Constraint<T>? other);

    /// <inheritdoc />
    public override int GetHashCode() => GetHashCode1();


    protected abstract int GetHashCode1();
}
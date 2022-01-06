namespace GridPuzzles.Clues.Constraints;

public sealed class AreEqualConstraint<T> : CommutativeConstraint<T> where T : notnull
{
    private AreEqualConstraint() {}

    public static CommutativeConstraint<T> Instance { get; } = new AreEqualConstraint<T>();

    /// <inheritdoc />
    public override string Name => "Are Equal";

    /// <inheritdoc />
    public override bool IsMet(T t1, T t2)
    {
        return t1.Equals(t2);
    }

    /// <inheritdoc />
    public override bool IsSuperConstraint(Constraint<T> other)
    {
        return other is AreEqualConstraint<T>;
    }

    /// <inheritdoc />
    public override bool Equals(Constraint<T>? other)
    {
        return other is AreEqualConstraint<T>;
    }

    /// <inheritdoc />
    protected override int GetHashCode1()
    {
        return 7777;
    }
}
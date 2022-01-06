namespace GridPuzzles.Clues.Constraints;

public class NonConsecutiveConstraint : CommutativeConstraint<int>
{
    private NonConsecutiveConstraint() { }

    public static CommutativeConstraint<int> Instance { get; } = new NonConsecutiveConstraint();

    /// <inheritdoc />
    public override string Name => "Non Consecutive";

    /// <inheritdoc />
    public override bool IsMet(int t1, int t2) => Math.Abs(t1 - t2) > 1;

    /// <inheritdoc />
    public override bool IsSuperConstraint(Constraint<int> other) => other is NonSumConstraint;

    /// <inheritdoc />
    public override bool Equals(Constraint<int>? other) => other is NonSumConstraint;

    /// <inheritdoc />
    protected override int GetHashCode1() => 765;
}
namespace GridPuzzles.Clues.Constraints;

public class SumConstraint : CommutativeConstraint<int>
{
    public SumConstraint(int sum) => Sum = sum;

    /// <inheritdoc />
    public override string Name => $"Sum to {Sum}";

    public int Sum { get; }

    /// <inheritdoc />
    public override bool IsValid(int t1, int t2) => t1 + t2 == Sum;

    /// <inheritdoc />
    public override bool IsSuperConstraint(Constraint<int> other) => other is SumConstraint s && Sum == s.Sum;

    /// <inheritdoc />
    public override bool Equals(Constraint<int>? other) => other is SumConstraint s && Sum == s.Sum;

    /// <inheritdoc />
    protected override int GetHashCode1() => Sum;
}
namespace GridPuzzles.Clues.Constraints;

public class DontDifferByConstraint : CommutativeConstraint<int>
{
    public DontDifferByConstraint(int amount) => Amount = amount;

    public int Amount { get; }

    /// <inheritdoc />
    public override string Name => $"Don't differ by {Amount}";

    /// <inheritdoc />
    public override bool IsValid(int t1, int t2) => Math.Abs(t1 - t2) != Amount;

    /// <inheritdoc />
    public override bool IsSuperConstraint(Constraint<int> other) => other is DontDifferByConstraint dbc && dbc.Amount == Amount;

    /// <inheritdoc />
    public override bool Equals(Constraint<int>? other) => other is DontDifferByConstraint dbc && dbc.Amount == Amount;

    /// <inheritdoc />
    protected override int GetHashCode1() => Amount + 927;
}
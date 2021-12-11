namespace GridPuzzles.Clues.Constraints;

public class XMoreThanConstraint : Constraint<int>
{
    public XMoreThanConstraint(int amount) => Amount = amount;

    /// <inheritdoc />
    public override string Name => $"{Amount} more than";

    /// <inheritdoc />
    public override Constraint<int> FlippedConstraint => new XLessThanConstraint(Amount);

    public int Amount { get; }

    /// <inheritdoc />
    public override bool IsValid(int t1, int t2) => t1 + Amount >= t2;

    /// <inheritdoc />
    public override bool IsSuperConstraint(Constraint<int> other)
    {
        return other is XMoreThanConstraint x && Amount >= x.Amount;
    }

    /// <inheritdoc />
    public override bool Equals(Constraint<int>? other) => other is XMoreThanConstraint x && Amount == x.Amount;

    /// <inheritdoc />
    protected override int GetHashCode1() => 543 + Amount;
}
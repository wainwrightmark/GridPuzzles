namespace GridPuzzles.Clues.Constraints;

public class XLessThanConstraint : Constraint<int>
{
    public XLessThanConstraint(int amount) => Amount = amount;

    /// <inheritdoc />
    public override string Name => $"{Amount} less than";

    /// <inheritdoc />
    public override Constraint<int> FlippedConstraint => new XMoreThanConstraint(Amount);

    public int Amount { get; }

    /// <inheritdoc />
    public override bool IsValid(int t1, int t2) => t1 + Amount <= t2;

    /// <inheritdoc />
    public override bool IsSuperConstraint(Constraint<int> other) => other is XLessThanConstraint xl && Amount >= xl.Amount;

    /// <inheritdoc />
    public override bool Equals(Constraint<int>? other) => other is XLessThanConstraint xl && Amount == xl.Amount;

    /// <inheritdoc />
    protected override int GetHashCode1()
    {
        return 345 + Amount;
    }
}
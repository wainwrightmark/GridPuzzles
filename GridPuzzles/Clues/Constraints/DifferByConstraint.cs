namespace GridPuzzles.Clues.Constraints;

public class DifferByConstraint : CommutativeConstraint<int>
{
    public DifferByConstraint(int amount) => Amount = amount;

    public int Amount { get; }


    /// <inheritdoc />
    public override string Name => $"Differ by exactly {Amount}";

    /// <inheritdoc />
    public override bool IsValid(int t1, int t2) => Math.Abs(t1 - t2) == Amount;

    /// <inheritdoc />
    public override bool IsSuperConstraint(Constraint<int> other) => other is DifferByConstraint dbc && dbc.Amount == Amount;

    /// <inheritdoc />
    public override bool Equals(Constraint<int>? other)
    {
        return other is DifferByConstraint dbc && dbc.Amount == Amount;
    }

    /// <inheritdoc />
    protected override int GetHashCode1() => Amount + 729;
}

public class DifferByAtMostConstraint : CommutativeConstraint<int>
{
    public DifferByAtMostConstraint(int amount)
    {
        Amount = amount;
    }

    public int Amount { get; }
    /// <inheritdoc />
    public override string Name => $"Differ By At Most {Amount}";

    /// <inheritdoc />
    public override bool IsValid(int t1, int t2)
    {
        return Math.Abs(t1 - t2) <= Amount;
    }

    /// <inheritdoc />
    public override bool IsSuperConstraint(Constraint<int> other)
    {
        return other is DifferByAtMostConstraint dmc && dmc.Amount >= Amount;
    }

    /// <inheritdoc />
    public override bool Equals(Constraint<int>? other)
    {
        return other is DifferByAtMostConstraint dmc && dmc.Amount == Amount;
    }

    /// <inheritdoc />
    protected override int GetHashCode1() => Amount + 1492;
        
}
    
public class DifferByAtLeastConstraint : CommutativeConstraint<int>
{
    public DifferByAtLeastConstraint(int amount)
    {
        Amount = amount;
    }

    public int Amount { get; }
    /// <inheritdoc />
    public override string Name => $"Differ By At Least {Amount}";

    /// <inheritdoc />
    public override bool IsValid(int t1, int t2)
    {
        return Math.Abs(t1 - t2) >= Amount;
    }

    /// <inheritdoc />
    public override bool IsSuperConstraint(Constraint<int> other)
    {
        return other is DifferByAtMostConstraint dmc && dmc.Amount <= Amount;
    }

    /// <inheritdoc />
    public override bool Equals(Constraint<int>? other)
    {
        return other is DifferByAtMostConstraint dmc && dmc.Amount == Amount;
    }

    /// <inheritdoc />
    protected override int GetHashCode1() => Amount + 1493;
        
}
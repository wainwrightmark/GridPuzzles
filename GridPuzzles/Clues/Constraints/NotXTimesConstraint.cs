namespace GridPuzzles.Clues.Constraints;

public class NotXTimesConstraint : CommutativeConstraint<int>
{
    public NotXTimesConstraint(int multiple) => Multiple = multiple;

    public int Multiple { get; }

    /// <inheritdoc />
    public override string Name => "Not " + Multiple + " Times";

    /// <inheritdoc />
    public override bool IsValid(int t1, int t2) => t1 * Multiple != t2 && t2 * Multiple != t1;

    /// <inheritdoc />
    public override bool IsSuperConstraint(Constraint<int> other) => other is NotXTimesConstraint fc && fc.Multiple == Multiple;

    /// <inheritdoc />
    public override bool Equals(Constraint<int>? other) => other is NotXTimesConstraint fc && fc.Multiple == Multiple;

    /// <inheritdoc />
    protected override int GetHashCode1() => Multiple + 931;
}
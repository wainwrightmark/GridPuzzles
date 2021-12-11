namespace GridPuzzles.Clues.Constraints;

public class XTimesConstraint : CommutativeConstraint<int>
{
    public XTimesConstraint(int multiple) => Multiple = multiple;

    public int Multiple { get; }

    /// <inheritdoc />
    public override string Name => Multiple + " Times";

    /// <inheritdoc />
    public override bool IsValid(int t1, int t2) => t1 * Multiple == t2 || t2 * Multiple == t1;

    /// <inheritdoc />
    public override bool IsSuperConstraint(Constraint<int> other) => other is XTimesConstraint fc && fc.Multiple == Multiple;

    /// <inheritdoc />
    public override bool Equals(Constraint<int>? other) => other is XTimesConstraint fc && fc.Multiple == Multiple;

    /// <inheritdoc />
    protected override int GetHashCode1() => Multiple + 139;
}
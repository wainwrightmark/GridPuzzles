namespace GridPuzzles.Clues.Constraints;

public class NonSumConstraint : CommutativeConstraint<int>
{
    /// <inheritdoc />
    public NonSumConstraint(ImmutableSortedSet<int> disallowedSums) => DisallowedSums = disallowedSums;

    public ImmutableSortedSet<int> DisallowedSums { get; }

    /// <inheritdoc />
    public override string Name => $"Do not sum to {string.Join(", ", DisallowedSums)}";

    /// <inheritdoc />
    public override bool IsValid(int t1, int t2)
    {
        var s = t1 + t2;
        return !DisallowedSums.Contains(s);
    }

    /// <inheritdoc />
    public override bool IsSuperConstraint(Constraint<int> other) => other is NonSumConstraint nsc && DisallowedSums.IsSupersetOf(nsc.DisallowedSums);

    /// <inheritdoc />
    public override bool Equals(Constraint<int>? other) => other is NonSumConstraint nsc && SortedSetElementsComparer<int>.Instance.Equals(DisallowedSums, nsc.DisallowedSums);

    /// <inheritdoc />
    protected override int GetHashCode1() => SortedSetElementsComparer<int>.Instance.GetHashCode(DisallowedSums);
}
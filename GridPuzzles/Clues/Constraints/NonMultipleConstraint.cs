namespace GridPuzzles.Clues.Constraints;

public class NonMultipleConstraint : CommutativeConstraint<int>
{
    private NonMultipleConstraint() {}

    public static CommutativeConstraint<int> Instance { get; } = new NonMultipleConstraint();

    /// <inheritdoc />
    public override string Name => "Non Multiple";

    /// <inheritdoc />
    public override bool IsMet(int t1, int t2) => !MultiplesDictionary[t1].Contains(t2);

    /// <inheritdoc />
    public override bool IsSuperConstraint(Constraint<int> other) => other is NonMultipleConstraint;

    /// <inheritdoc />
    public override bool Equals(Constraint<int>? other) => other is NonMultipleConstraint;

    /// <inheritdoc />
    protected override int GetHashCode1() => 567;

    /// <summary>
    /// TODO allow other sets of numbers
    /// </summary>
    public static readonly IReadOnlyDictionary<int, ImmutableSortedSet<int>> MultiplesDictionary =
        new Dictionary<int, ImmutableSortedSet<int>>
        {
            {2, ImmutableSortedSet.Create(4,6,8,1) },
            {3, ImmutableSortedSet.Create(6,9) },
            {4, ImmutableSortedSet.Create(2,8) },
            {5, ImmutableSortedSet.Create(1) },
            {6, ImmutableSortedSet.Create(2,3) },
            {7, ImmutableSortedSet<int>.Empty },
            {8, ImmutableSortedSet.Create(2,4) },
            {9, ImmutableSortedSet.Create(3) },
            {1, ImmutableSortedSet.Create(2,5) } //actually 10
        };
}
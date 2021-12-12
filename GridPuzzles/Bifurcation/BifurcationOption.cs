namespace GridPuzzles.Bifurcation;

public interface IBifurcationOption<T>
{
    int Priority { get; }

    ISingleReason Reason { get; }

    public IEnumerable<IBifurcationChoice<T>> Choices { get; }

    public int ChoiceCount { get; }

    public IBifurcationChoice<T> this [int index] { get; }
}

public sealed class BifurcationOption<T> : IBifurcationOption<T>
{
    /// <summary>
    /// Higher is higher priority.
    /// </summary>
    public int Priority { get; }

    /// <inheritdoc />
    public IEnumerable<IBifurcationChoice<T>> Choices => ChoicesSet;

    /// <inheritdoc />
    public int ChoiceCount => Choices.Count();

    /// <inheritdoc />
    public IBifurcationChoice<T> this[int index] => ChoicesSet[index];

    public ISingleReason Reason { get; }

    public BifurcationOption(int priority, ISingleReason reason, IEnumerable<IBifurcationChoice<T>> choices)
    {
        Reason = reason;
        Priority = priority;
        ChoicesSet = choices.ToImmutableSortedSet();

        var (t1, t2) = ChoicesSet.OrderBy(x=>x.ToString()).GetFirstTwo();

        _hashCode = HashCode.Combine(t1, t2);

        if (ChoicesSet.Count < 2)
            throw new Exception("Must be at least two choices");
    }
    public BifurcationOption(int priority, ISingleReason reason, params IBifurcationChoice<T>[] choices)
        :this(priority, reason, choices as IEnumerable<IBifurcationChoice<T>>) { }

    private readonly int _hashCode;

    public ImmutableSortedSet<IBifurcationChoice<T>> ChoicesSet { get; }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is BifurcationOption<T> bo && bo.ChoicesSet.SetEquals(ChoicesSet);

    /// <inheritdoc />
    public override int GetHashCode() => _hashCode;

    /// <inheritdoc />
    public override string ToString() => $"{Reason}: {string.Join(" or ", Choices)}";
}
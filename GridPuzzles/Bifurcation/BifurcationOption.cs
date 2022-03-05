namespace GridPuzzles.Bifurcation;

public interface IBifurcationOption<T, TCell>
    where T :struct where TCell : ICell<T, TCell>, new()
{
    int Priority { get; }

    ISingleReason Reason { get; }

    public IEnumerable<IBifurcationChoice<T, TCell>> Choices { get; }

    public int ChoiceCount { get; }

    public IBifurcationChoice<T, TCell> this [int index] { get; }
}

public sealed class BifurcationOption<T, TCell> : IBifurcationOption<T, TCell>
    where T :struct where TCell : ICell<T, TCell>, new()
{
    /// <summary>
    /// Higher is higher priority.
    /// </summary>
    public int Priority { get; }

    /// <inheritdoc />
    public IEnumerable<IBifurcationChoice<T, TCell>> Choices => ChoicesSet;

    /// <inheritdoc />
    public int ChoiceCount => Choices.Count();

    /// <inheritdoc />
    public IBifurcationChoice<T, TCell> this[int index] => ChoicesSet[index];

    public ISingleReason Reason { get; }

    public BifurcationOption(int priority, ISingleReason reason, IEnumerable<IBifurcationChoice<T, TCell>> choices)
    {
        Reason = reason;
        Priority = priority;
        ChoicesSet = choices.ToImmutableSortedSet();

        var (t1, t2) = ChoicesSet.OrderBy(x=>x.ToString()).GetFirstTwo();

        _hashCode = HashCode.Combine(t1, t2);

        if (ChoicesSet.Count < 2)
            throw new Exception("Must be at least two choices");
    }
    public BifurcationOption(int priority, ISingleReason reason, params IBifurcationChoice<T, TCell>[] choices)
        :this(priority, reason, choices as IEnumerable<IBifurcationChoice<T, TCell>>) { }

    private readonly int _hashCode;

    public ImmutableSortedSet<IBifurcationChoice<T, TCell>> ChoicesSet { get; }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is BifurcationOption<T, TCell> bo && bo.ChoicesSet.SetEquals(ChoicesSet);

    /// <inheritdoc />
    public override int GetHashCode() => _hashCode;

    /// <inheritdoc />
    public override string ToString() => $"{Reason}: {string.Join(" or ", Choices)}";
}
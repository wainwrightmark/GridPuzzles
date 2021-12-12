namespace GridPuzzles.VariantBuilderArguments;

public class ListPositionArgument : VariantBuilderArgument<IReadOnlyList<Position>>
{
    public ListPositionArgument(string name, int minimumPositions, int maximumPositions) : base(name,
        Maybe<IReadOnlyList<Position>>.None)
    {
        MinimumPositions = minimumPositions;
        MaximumPositions = maximumPositions;
    }

    public const char Delimiter = ';';

    public int MinimumPositions { get; }

    public int MaximumPositions { get; }


    /// <inheritdoc />
    public override string Display(string value)
    {
        return value.Replace(Delimiter, ',');
    }

    /// <inheritdoc />
    public override Result<IReadOnlyList<Position>> TryParseTyped(string s)
    {
        var r = ParsePositions(s)
            .Combine()
            .Map(x => x.Distinct().ToList() as IReadOnlyList<Position>)
            .Ensure(x => x.Count >= MinimumPositions, $"Must be at least {MinimumPositions} Positions")
            .Ensure(x => x.Count <= MaximumPositions, $"Must be at most {MaximumPositions} Positions");

        return r;
    }

    public static IEnumerable<Result<Position>> ParsePositions(string s)
    {
        return s.Split(Delimiter)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(s))
            .Select(Position.Deserialize);
    }

    /// <inheritdoc />
    public override IReadOnlyList<Position> GetCheckedPositions(string text)
    {
        var r = ParsePositions(text)
            .Combine()
            .Map(x => x.ToList() as IReadOnlyList<Position>);
        if (r.IsFailure)
            return ImmutableList<Position>.Empty;

        return r.Value;
    }

    /// <inheritdoc />
    public override VariantBuilderArgument CloneWithValue(string newValue) => this;

    /// <inheritdoc />
    public override bool ClearOnAdded => true;
}
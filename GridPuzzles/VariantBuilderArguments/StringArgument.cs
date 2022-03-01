namespace GridPuzzles.VariantBuilderArguments;

public class MultilineStringArgument : VariantBuilderArgument<string> //TODO list string, filepath etc..
{
    public MultilineStringArgument(string name, Maybe<string> defaultValue) : base(name, defaultValue)
    {
    }

    /// <inheritdoc />
    public override Result<string> TryParseTyped(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return Result.Failure<string>("String was empty");

        return s;
    }

    /// <inheritdoc />
    public override IReadOnlyList<Position> GetCheckedPositions(string text)
    {
        return ImmutableArray<Position>.Empty;
    }

    /// <inheritdoc />
    public override VariantBuilderArgument CloneWithValue(string newValue) => new MultilineStringArgument(Name, newValue);
}



public class StringArgument : VariantBuilderArgument<string> //TODO list string, filepath etc..
{
    public StringArgument(string name, Maybe<string> defaultValue) : base(name, defaultValue)
    {
    }

    /// <inheritdoc />
    public override Result<string> TryParseTyped(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return Result.Failure<string>("String was empty");

        return s;
    }

    /// <inheritdoc />
    public override IReadOnlyList<Position> GetCheckedPositions(string text)
    {
        return ImmutableArray<Position>.Empty;
    }

    /// <inheritdoc />
    public override VariantBuilderArgument CloneWithValue(string newValue) => new StringArgument(Name, newValue);
}
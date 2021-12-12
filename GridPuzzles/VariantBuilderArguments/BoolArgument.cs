namespace GridPuzzles.VariantBuilderArguments;

public class BoolArgument : VariantBuilderArgument<bool>
{
    /// <inheritdoc />
    public BoolArgument(string name, Maybe<bool> defaultValue) : base(name, defaultValue)
    {
    }

    /// <inheritdoc />
    public override Result<bool> TryParseTyped(string s)
    {
        if (bool.TryParse(s, out var r)) return r;

        return Result.Failure<bool>("Object was not a boolean");
    }

    /// <inheritdoc />
    public override IReadOnlyList<Position> GetCheckedPositions(string text)
    {
        return ImmutableList<Position>.Empty;
    }

    /// <inheritdoc />
    public override VariantBuilderArgument CloneWithValue(string newValue) => new BoolArgument(Name, bool.TryParse(newValue, out var b)? b : DefaultValue);

    /// <inheritdoc />
    public override string Display(string value)
    {
        if (bool.TryParse(value, out var b) && b)
            return Name;
        return $"Not {Name}";
    }
}
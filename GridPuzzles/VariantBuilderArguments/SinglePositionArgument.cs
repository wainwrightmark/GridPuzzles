namespace GridPuzzles.VariantBuilderArguments;

public class SinglePositionArgument : VariantBuilderArgument<Position>
{
    public SinglePositionArgument(string name) : base(name, Maybe<Position>.None)
    {
    }

    /// <inheritdoc />
    public override Result<Position> TryParseTyped(string s)
    {
        return Position.Deserialize(s);
    }

    /// <inheritdoc />
    public override VariantBuilderArgument CloneWithValue(string newValue) => this;

    /// <inheritdoc />
    public override IReadOnlyList<Position> GetCheckedPositions(string text)
    {
        var r = TryParseTyped(text);
        if(r.IsFailure)
            return ImmutableArray<Position>.Empty;

        return ImmutableArray<Position>.Empty.Add(r.Value);
    }

    /// <inheritdoc />
    public override bool ClearOnAdded => true;
}
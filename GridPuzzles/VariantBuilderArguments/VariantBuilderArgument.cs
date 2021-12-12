namespace GridPuzzles.VariantBuilderArguments;

public abstract class VariantBuilderArgument
{
    protected VariantBuilderArgument(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public abstract Result<object> TryParse(string s);

    public abstract IReadOnlyList<Position> GetCheckedPositions(string text);

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is VariantBuilderArgument vba && Name == vba.Name;
    }

    /// <inheritdoc />
    public override int GetHashCode() => Name.GetHashCode();

    public abstract string DefaultString { get; }

    public abstract VariantBuilderArgument CloneWithValue(string newValue);

    public virtual string Display(string value)
    {
        return Name + " : " + value;
    }

    public virtual bool ClearOnAdded => false;
}

public abstract class VariantBuilderArgument<TArg> : VariantBuilderArgument
{
    public abstract Result<TArg> TryParseTyped(string s);

    public Result<TArg> TryGetFromDictionary(IReadOnlyDictionary<string, string> dictionary)
    {
        if (dictionary.TryGetValue(Name, out var s))
            return TryParseTyped(s);
        return Result.Failure<TArg>($"{Name} was not present in dictionary");
    }


    public override Result<object> TryParse(string s) => TryParseTyped(s);

    /// <inheritdoc />
    protected VariantBuilderArgument(string name, Maybe<TArg> defaultValue) : base(name) => DefaultValue = defaultValue;

    public Maybe<TArg> DefaultValue { get; }

    /// <inheritdoc />
    public override string DefaultString => DefaultValue.Match(x => x!.ToString()!, () => "");
}
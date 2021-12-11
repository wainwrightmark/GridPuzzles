using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;

namespace GridPuzzles.VariantBuilderArguments;

public interface IEnumArgument
{
    string DefaultString { get; }

    IEnumerable<string> Options { get; }
}


public class EnumArgument<TArg> : VariantBuilderArgument<TArg>, IEnumArgument
    where TArg : struct, Enum
{

    public Type EnumType => typeof(TArg);

    /// <inheritdoc />
    public EnumArgument(string name, Maybe<TArg> d) : base(name, d)
    {
    }

    /// <inheritdoc />
    public override Result<TArg> TryParseTyped(string s)
    {
        if (Enum.TryParse(EnumType, s, true, out var r) && r is TArg rEnum)
            return rEnum;
        return Result.Failure<TArg>($"Could not parse {s} as {EnumType.Name})");
    }
    /// <inheritdoc />
    public override IReadOnlyList<Position> GetCheckedPositions(string text)
    {
        return ImmutableList<Position>.Empty;
    }

    /// <inheritdoc />
    public IEnumerable<string> Options => Enum.GetValues(typeof(TArg)).Cast<TArg>().Select(x => x.ToString());


    public override VariantBuilderArgument CloneWithValue(string newValue) => new EnumArgument<TArg>(Name, Enum.TryParse(newValue, true, out TArg b)? b : DefaultValue);

        
}
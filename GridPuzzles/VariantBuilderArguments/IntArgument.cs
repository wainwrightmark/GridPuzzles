using System.Collections.Generic;
using System.Collections.Immutable;
using CSharpFunctionalExtensions;

namespace GridPuzzles.VariantBuilderArguments;

public class IntArgument : VariantBuilderArgument<int>
{
    public IntArgument(string name, int minValue, int maxValue, Maybe<int> defaultValue) : base(name, defaultValue)
    {
        MinValue = minValue;
        MaxValue = maxValue;
    }
    public int MinValue { get; }
    public int MaxValue { get; }

    /// <inheritdoc />
    public override Result<int> TryParseTyped(string s)
    {
        if (int.TryParse(s, out var i))
        {
            if(i < MinValue)
                return Result.Failure<int>($"Value was less than {i}");
            if(i > MaxValue)
                return Result.Failure<int>($"Value was greater than {i}");

            return i;
        }
        return Result.Failure<int>("Object was not an integer");
    }

    /// <inheritdoc />
    public override IReadOnlyList<Position> GetCheckedPositions(string text)
    {
        return ImmutableList<Position>.Empty;
    }
    public override VariantBuilderArgument CloneWithValue(string newValue) => new IntArgument(Name, MinValue, MaxValue, int.TryParse(newValue, out var b)? b : DefaultValue);
}
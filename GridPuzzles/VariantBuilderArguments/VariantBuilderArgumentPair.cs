using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using MoreLinq;

namespace GridPuzzles.VariantBuilderArguments;

public sealed record VariantBuilderArgumentPair<T>(IVariantBuilder<T> VariantBuilder, IReadOnlyDictionary<string, string> Pairs)
    where T :notnull
{

    /// <inheritdoc />
    public override int GetHashCode() => AsString(true).GetHashCode();

    /// <inheritdoc />
    public bool Equals(VariantBuilderArgumentPair<T>? ap)
    {
        return ap is not null && VariantBuilder.Equals(ap.VariantBuilder) &&
               Pairs.ToHashSet().SetEquals(ap.Pairs);
    }
        

    public string AsString(bool includeName)
    {
        var data =  string.Join(' ', Pairs.Select(DisplayPair));
        if (includeName)
            return (VariantBuilder.Name + " " + data).Trim();
        else return data;
    }

    private string? DisplayPair(KeyValuePair<string, string> pair)
    {
        var arg = VariantBuilder.Arguments.TryFirst(x => x.Name.Equals(pair.Key, StringComparison.OrdinalIgnoreCase));

        if (arg.HasNoValue) return null;

        return arg.Value.Display(pair.Value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return AsString(true);
    }


    public VariantBuilderArgumentPair<T> Transform(int quarterTurns, bool flipHorizontal, bool flipVertical,
        Position maxPosition)
    {
        var newPairs = Pairs.Select(Flip).ToDictionary();

        return this with { Pairs = newPairs };

        KeyValuePair<string, string> Flip(KeyValuePair<string, string> kvp)
        {
            var arg = VariantBuilder.Arguments.FirstOrDefault(x => x.Name == kvp.Key);

            if (arg is ListPositionArgument lpa)
            {
                var positions = lpa.GetCheckedPositions(kvp.Value);
                var newPositions = positions
                    .Select(x => x.Transform(quarterTurns, flipHorizontal, flipVertical, maxPosition))
                    .ToDelimitedString(ListPositionArgument.Delimiter.ToString());

                return new KeyValuePair<string, string>(kvp.Key, newPositions);
            }

            if (arg is SinglePositionArgument spa)
            {
                var positions = spa.GetCheckedPositions(kvp.Value);
                var newPositions = positions
                    .Select(x => x.Transform(quarterTurns, flipHorizontal, flipVertical, maxPosition))
                    .ToDelimitedString(ListPositionArgument.Delimiter.ToString());

                return new KeyValuePair<string, string>(kvp.Key, newPositions);
            }

            return kvp;
        }
    }
}
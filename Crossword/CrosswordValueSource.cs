using System.Drawing;
using CSharpFunctionalExtensions;

namespace Crossword;

public class CrosswordValueSource : IValueSource<char, CharCell>
{
    public static CrosswordValueSource Instance => new();

    private CrosswordValueSource() { }

    /// <inheritdoc />
    public CharCell AnyValueCell { get; } = new(AllValuesSet);

    public CharCell BlockCell { get; } = new(ImmutableSortedSet<char>.Empty.Add(BlockChar));

    public const char BlockChar = '.';

    /// <inheritdoc />
    public ImmutableSortedSet<char> AllValues => AllValuesSet;

    public static ImmutableSortedSet<char> AllValuesSet =
        Enumerable.Range('A', 26).Select(x=>(char)x)
            .Concat(new []{BlockChar})
            .ToImmutableSortedSet();

    /// <inheritdoc />
    public Result<Maybe<char>> TryParse(char c)
    {
        c= char.ToUpperInvariant(c);

        if(c == '-' || c == '_' || c == '0')
            return Result.Success(Maybe<char>.None);

        if (AllValues.Contains(c))
            return Maybe<char>.From(c);

        return Result.Failure<Maybe<char>>($"Could not parse '{c}' as an letter or a '{BlockChar}'");
    }

    /// <inheritdoc />
    public Color? GetColor(char val)
    {
        return val == BlockChar ? Color.Black : null;
    }

}
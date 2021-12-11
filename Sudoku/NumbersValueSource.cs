using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using CSharpFunctionalExtensions;
using GridPuzzles.Cells;
using GridPuzzles.Clues;

namespace Sudoku;

public class NumbersValueSource : IValueSource<int>
{
    public static IDictionary<int, NumbersValueSource> Sources = //TODO make more efficient
        Enumerable.Range(1, 16).ToDictionary(x => x, x => new NumbersValueSource(x));

    private NumbersValueSource(int max)
    {
        Numbers  = Enumerable.Range(1, max).ToImmutableSortedSet();
        AnyValueCell = new Cell<int>(Numbers);
    }

    public readonly ImmutableSortedSet<int> Numbers;

    /// <inheritdoc />
    public Cell<int> AnyValueCell { get; }

    /// <inheritdoc />
    public ImmutableSortedSet<int> AllValues => Numbers;

    /// <inheritdoc />
    public Result<Maybe<int>> TryParse(char c)
    {
        if(c == '-' || c == '_')
            return Result.Success(Maybe<int>.None);

        if (int.TryParse(c.ToString(), out var i) && i > 0 && i <= 9)
            return Result.Success(Maybe<int>.From(i));

        return Result.Failure<Maybe<int>>($"Could not parse '{c}' as an integer between 1 and 9");
    }

    public Color? GetColor(int val)
    {
        return null;
    }
}
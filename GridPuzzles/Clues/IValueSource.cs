using System.Drawing;

namespace GridPuzzles.Clues;

public sealed class EmptyValueSource<T> : IValueSource<T>//, Cell<T> //, ICell<T>
{
    private EmptyValueSource() { }

    public static EmptyValueSource<T> Instance { get; } = new();

    /// <inheritdoc />
    public Cell<T> AnyValueCell { get; } = new(ImmutableSortedSet<T>.Empty);

    /// <inheritdoc />
    public ImmutableSortedSet<T> AllValues { get; } = ImmutableSortedSet<T>.Empty;

    /// <inheritdoc />
    public Result<Maybe<T>> TryParse(char c) => Result.Failure<Maybe<T>>("No value is possible");

    /// <inheritdoc />
    public Color? GetColor(T val) => null;
}
public interface IValueSource<T>
{
    Cell<T> AnyValueCell { get; }
    ImmutableSortedSet<T> AllValues { get; }
    Result<Maybe<T>> TryParse(char c);
    public Color? GetColor(T val);
}
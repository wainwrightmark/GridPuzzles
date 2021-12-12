using GridPuzzles.Overlays;
using GridPuzzles.VariantBuilderArguments;

namespace GridPuzzles.Clues;

public abstract class NoArgumentVariantBuilder<T> : VariantBuilder<T>, IClueBuilder<T> where T : notnull
{

    /// <inheritdoc />
    public abstract int Level { get; }

    /// <inheritdoc />
    public abstract IEnumerable<IClue<T>> CreateClues(Position minPosition, Position maxPosition,
        IValueSource<T> valueSource,
        IReadOnlyCollection<IClue<T>> lowerLevelClues);

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder<T>>> TryGetClueBuilders1(IReadOnlyDictionary<string, string> arguments)
    {
        return new[] {this};
    }

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments { get; } = new List<VariantBuilderArgument>();

    public abstract bool OnByDefault { get; }


    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string>? DefaultArguments =>
        OnByDefault ? new Dictionary<string, string>() : null;
        

    /// <param name="minPosition"></param>
    /// <param name="maxPosition"></param>
    /// <inheritdoc />
    public virtual IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
    {
        yield break;
    }
}
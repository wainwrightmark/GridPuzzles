using System.Threading;
using System.Threading.Tasks;
using GridPuzzles.Clues;
using GridPuzzles.VariantBuilderArguments;

namespace GridPuzzles;

public interface IVariantBuilder
{
    string Name { get; }
    IReadOnlyList<VariantBuilderArgument> Arguments { get; }

    IReadOnlyDictionary<string, string>? DefaultArguments { get; }

    /// <summary>
    /// Is this variant builder valid with this type and grid size
    /// </summary>
    public bool IsValid(Position maxPosition);

    Result<IReadOnlyCollection<IClueBuilder>> TryGetClueBuilders(
        IReadOnlyDictionary<string, string> arguments);
}

public interface IVariantBuilder<T, TCell> : IVariantBuilder where T :struct where TCell : ICell<T, TCell>, new()
{
    Task<Result<IReadOnlyCollection<IClueBuilder<T, TCell>>>> TryGetClueBuildersAsync(
        IReadOnlyDictionary<string, string> arguments, CancellationToken cancellationToken);

}
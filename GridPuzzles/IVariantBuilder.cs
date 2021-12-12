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

public interface IVariantBuilder<T> : IVariantBuilder where T : notnull
{
    Task<Result<IReadOnlyCollection<IClueBuilder<T>>>> TryGetClueBuildersAsync(
        IReadOnlyDictionary<string, string> arguments, CancellationToken cancellationToken);

}
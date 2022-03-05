using System.Threading;
using System.Threading.Tasks;
using GridPuzzles.VariantBuilderArguments;

namespace GridPuzzles.Clues;

public abstract class VariantBuilder<T, TCell> : IVariantBuilder<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyCollection<IClueBuilder<T, TCell>>>> TryGetClueBuildersAsync(IReadOnlyDictionary<string, string> arguments, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return TryGetClueBuilders1(arguments);
    }
        
    public abstract Result<IReadOnlyCollection<IClueBuilder<T, TCell>>> TryGetClueBuilders1(
        IReadOnlyDictionary<string, string> arguments);


    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public abstract IReadOnlyList<VariantBuilderArgument> Arguments { get; }

    /// <inheritdoc />
    public  virtual IReadOnlyDictionary<string, string>? DefaultArguments => null;

    /// <inheritdoc />
    public virtual bool IsValid(Position maxPosition)
    {
        return true;
    }

    /// <inheritdoc />
    public Result<IReadOnlyCollection<IClueBuilder>> TryGetClueBuilders(IReadOnlyDictionary<string, string> arguments)
    {
        return TryGetClueBuilders1(arguments).Map(x=> x.ToList<IClueBuilder>() as IReadOnlyCollection<IClueBuilder>);
    }
}
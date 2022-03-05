using System.Threading;
using System.Threading.Tasks;
using GridPuzzles.VariantBuilderArguments;

namespace GridPuzzles.Session;

public abstract class GridCreator<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    public abstract
        Task<Result<(Grid<T, TCell> Grid, IReadOnlyList<IVariantBuilder<T, TCell>> VariantBuilders,
            IReadOnlyList<VariantBuilderArgumentPair<T, TCell>> VariantsInPlay)>> TryCreate(int columns, int rows,
            string? gridText,
            CancellationToken cancellationToken);

    public abstract int MinSize { get; }
    public abstract int MaxSize { get; }
    public abstract bool WidthMustMatchHeight { get; }

    public abstract Task<(Grid<T, TCell> Grid, IReadOnlyList<IVariantBuilder<T, TCell>> VariantBuilders,
        IReadOnlyList<VariantBuilderArgumentPair<T, TCell>> VariantsInPlay)>  GetDefault();

    public abstract IReadOnlyList<IVariantBuilder<T, TCell>> VariantBuilderList { get; }

}
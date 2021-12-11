using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using GridPuzzles.VariantBuilderArguments;

namespace GridPuzzles.Session;

public abstract class GridCreator<T> where T :notnull
{
    public abstract
        Task<Result<(Grid<T> Grid, IReadOnlyList<IVariantBuilder<T>> VariantBuilders,
            IReadOnlyList<VariantBuilderArgumentPair<T>> VariantsInPlay)>> TryCreate(int columns, int rows,
            string? gridText,
            CancellationToken cancellationToken);

    public abstract int MinSize { get; }
    public abstract int MaxSize { get; }
    public abstract bool WidthMustMatchHeight { get; }

    public abstract Task<(Grid<T> Grid, IReadOnlyList<IVariantBuilder<T>> VariantBuilders,
        IReadOnlyList<VariantBuilderArgumentPair<T>> VariantsInPlay)>  GetDefault();

    public abstract IReadOnlyList<IVariantBuilder<T>> VariantBuilderList { get; }

}
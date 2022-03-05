using GridPuzzles.VariantBuilderArguments;

namespace GridPuzzles;

public static class VariantBuilderHelper
{
    public static IEnumerable<VariantBuilderArgumentPair<T, TCell>> GetVariantBuilderArgumentPairs<T, TCell>(
        this IReadOnlyCollection<IVariantBuilder<T, TCell>> variantBuilders, Position maxPosition) 
        where T :struct where TCell : ICell<T, TCell>, new()
        =>
            variantBuilders
                .Where(x => x.DefaultArguments != null)
                .Where(x => x.IsValid(maxPosition))
                .Select(x => new VariantBuilderArgumentPair<T, TCell>(x, x.DefaultArguments!))
                .ToList();
}
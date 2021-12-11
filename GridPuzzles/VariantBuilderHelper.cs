using System.Collections.Generic;
using System.Linq;
using GridPuzzles.Clues;
using GridPuzzles.VariantBuilderArguments;

namespace GridPuzzles;

public static class VariantBuilderHelper
{
    public static IEnumerable<VariantBuilderArgumentPair<T>> GetVariantBuilderArgumentPairs<T>(
        this IReadOnlyCollection<IVariantBuilder<T>> variantBuilders, Position maxPosition) where T :notnull
        =>
            variantBuilders
                .Where(x => x.DefaultArguments != null)
                .Where(x => x.IsValid(maxPosition))
                .Select(x => new VariantBuilderArgumentPair<T>(x, x.DefaultArguments!))
                .ToList();
}
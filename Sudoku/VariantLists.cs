using Sudoku.Clues;
using Sudoku.Variants;

namespace Sudoku;

public static class SudokuVariant
{
    public static readonly IReadOnlyList<IVariantBuilder> SudokuVariantBuilders = new List<IVariantBuilder>
    {
        CompleteColumnsVariantBuilder<int, IntCell>.Instance,
        CompleteRowsVariantBuilder<int, IntCell>.Instance,
        CompleteSquareBoxesVariantBuilder<int, IntCell>.Instance,
        CompleteRectangularBoxVariantBuilder<int, IntCell>.Instance,
        ArithmeticConsistency.Instance,

        KingVariantBuilder<int, IntCell>.Instance,
        KnightVariantBuilder<int, IntCell>.Instance,
        TallKnightVariantBuilder<int, IntCell>.Instance,

        MultipleOfVariantBuilder.Instance,
        DifferByVariantBuilder.Instance,

        NonConsecutiveVariantBuilder.Instance,
        NonMultipleClueBuilder.Instance,
        CompleteDiagonalVariantBuilder<int, IntCell>.Instance,
        CompleteOffsetDiagonalVariantBuilder<int, IntCell>.Instance,
        TaxicabClueBuilder.Instance,
        SumVariantBuilder.Instance,
        AnySumVariantBuilder.Instance,
        UnconstrainedPairsSumVariantBuilder.Instance,
        UnconstrainedPairsMultipleVariantBuilder.Instance,
        UnconstrainedPairsDifferVariantBuilder.Instance,
        ArrowVariantBuilder.Instance,
        MagicSquareVariantBuilder.Instance,
        ThermometerVariantBuilder.Instance,
        SandwichVariantBuilder.Instance,
        SlingshotVariantBuilder.Instance,
        RestrictValuesVariantBuilder<int, IntCell>.Instance,
        ConsecutiveGroupVariantBuilder.Instance,
        PalindromeVariantBuilder<int, IntCell>.Instance,
        UniqueGroupVariantBuilder<int, IntCell>.Instance,
        DoublingGroupVariantBuilder<int, IntCell>.Instance,
        WhispersVariantBuilder.Instance,
        DisjointBoxesVariantBuilder<int, IntCell>.Instance,
        NexusVariantBuilder.Instance,
        LockoutVariantBuilder.Instance,
        IndexVariantBuilder.Instance
    };

    public static readonly IReadOnlyDictionary<string, IVariantBuilder> SudokuVariantBuildersDictionary =
        SudokuVariantBuilders.ToDictionary(x => x.Name);
}
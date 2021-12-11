using System.Collections.Generic;
using System.Linq;
using GridPuzzles;
using Sudoku.Clues;
using Sudoku.Variants;

namespace Sudoku;

public static class SudokuVariant
{
    public static readonly IReadOnlyList<IVariantBuilder<int>> SudokuVariantBuilders = new List<IVariantBuilder<int>>
    {
        CompleteColumnsVariantBuilder<int>.Instance,
        CompleteRowsVariantBuilder<int>.Instance,
        CompleteSquareBoxesVariantBuilder<int>.Instance,
        CompleteRectangularBoxVariantBuilder<int>.Instance,
        ArithmeticConsistency.Instance,

        KingVariantBuilder<int>.Instance,
        KnightVariantBuilder<int>.Instance,
        TallKnightVariantBuilder<int>.Instance,

        MultipleOfVariantBuilder.Instance,
        DifferByVariantBuilder.Instance,

        NonConsecutiveVariantBuilder.Instance,
        NonMultipleClueBuilder.Instance,
        CompleteDiagonalVariantBuilder<int>.Instance,
        CompleteOffsetDiagonalVariantBuilder<int>.Instance,
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
        RestrictValuesVariantBuilder<int>.Instance,
        ConsecutiveGroupVariantBuilder.Instance,
        PalindromeVariantBuilder<int>.Instance,
        UniqueGroupVariantBuilder<int>.Instance,
        DoublingGroupVariantBuilder<int>.Instance,
        WhispersVariantBuilder.Instance,
        DisjointBoxesVariantBuilder<int>.Instance,
        NexusVariantBuilder.Instance,
        LockoutVariantBuilder.Instance,
        IndexVariantBuilder.Instance
    };

    public static readonly IReadOnlyDictionary<string, IVariantBuilder<int>> SudokuVariantBuildersDictionary =
        SudokuVariantBuilders.ToDictionary(x => x.Name);
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using GridPuzzles;
using GridPuzzles.Clues;
using GridPuzzles.Session;
using GridPuzzles.VariantBuilderArguments;

namespace Sudoku;

public class SudokuGridCreator : GridCreator<int>
{
    private SudokuGridCreator() { }

    public static GridCreator<int> Instance { get; } = new SudokuGridCreator();

    /// <inheritdoc />
    public override async Task<Result<(Grid<int> Grid, IReadOnlyList<IVariantBuilder<int>> VariantBuilders, IReadOnlyList<VariantBuilderArgumentPair<int>> VariantsInPlay)>> TryCreate(int columns, int rows, string? gridText, CancellationToken cancellationToken)
    {
        if(columns != rows)
            return Result.Failure<(Grid<int> Grid, IReadOnlyList<IVariantBuilder<int>> VariantBuilders, IReadOnlyList<VariantBuilderArgumentPair<int>> VariantsInPlay)>("Width must match height");
        var size = columns;

        if (!NumbersValueSource.Sources.TryGetValue(size, out var valueSource))
            return Result.Failure<(Grid<int> Grid, IReadOnlyList<IVariantBuilder<int>> VariantBuilders, IReadOnlyList<VariantBuilderArgumentPair<int>> VariantsInPlay)>($"Could not create sudoku size {size}");
        var maxPosition = new Position(size, size);

        var variantBuilders = SudokuVariant.SudokuVariantBuilders
            .Where(x => x.IsValid(maxPosition)).ToList();

        var variantsInPlay = variantBuilders.Where(x => x.DefaultArguments != null)
            .Select(x => new VariantBuilderArgumentPair<int>(x, x.DefaultArguments!)).ToList();

        var clueSource =
            await ClueSource<int>.TryCreateAsync(variantsInPlay, maxPosition, valueSource, cancellationToken);
        if (clueSource.IsFailure)
            return clueSource
                .ConvertFailure<(Grid<int> Grid, IReadOnlyList<IVariantBuilder<int>> VariantBuilders,
                    IReadOnlyList<VariantBuilderArgumentPair<int>> VariantsInPlay)>();

        var createResult = Grid<int>.CreateFromString((gridText ?? "").PadRight(size * size, '-'), clueSource.Value, maxPosition);

        if (createResult.IsFailure) return createResult.ConvertFailure<(Grid<int> Grid, IReadOnlyList<IVariantBuilder<int>> VariantBuilders, IReadOnlyList<VariantBuilderArgumentPair<int>> VariantsInPlay)>();

        return (createResult.Value, variantBuilders, variantsInPlay);
    }

    /// <inheritdoc />
    public override int MinSize => 4;

    /// <inheritdoc />
    public override int MaxSize => 9;

    /// <inheritdoc />
    public override bool WidthMustMatchHeight => true;

    /// <inheritdoc />
    public override async Task<(Grid<int> Grid, IReadOnlyList<IVariantBuilder<int>> VariantBuilders, IReadOnlyList<VariantBuilderArgumentPair<int>> VariantsInPlay)> GetDefault()
    {

        var variantBuilders = SudokuVariant.SudokuVariantBuilders
            .Where(x => x.IsValid(Position.NineNine)).ToList();

        var variantsInPlay = variantBuilders.Where(x => x.DefaultArguments != null)
            .Select(x => new VariantBuilderArgumentPair<int>(x, x.DefaultArguments!)).ToList();

        var clueSource =
            await ClueSource<int>.TryCreateAsync(variantsInPlay, Position.NineNine, NumbersValueSource.Sources[9], CancellationToken.None);

        var grid = Grid<int>.Create(null, Position.NineNine, clueSource.Value);

        return (grid, variantBuilders, variantsInPlay);
    }

    /// <inheritdoc />
    public override IReadOnlyList<IVariantBuilder<int>> VariantBuilderList => SudokuVariant.SudokuVariantBuilders;
}
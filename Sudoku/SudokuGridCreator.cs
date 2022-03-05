using System.Threading;
using System.Threading.Tasks;
using GridPuzzles.Session;

namespace Sudoku;

public class SudokuGridCreator : GridCreator<int, IntCell>
{
    private SudokuGridCreator() { }

    public static GridCreator<int, IntCell> Instance { get; } = new SudokuGridCreator();

    /// <inheritdoc />
    public override async Task<Result<(Grid Grid, IReadOnlyList<IVariantBuilder> VariantBuilders, IReadOnlyList<VariantBuilderArgumentPair> VariantsInPlay)>> 
        TryCreate(int columns, int rows, string? gridText, CancellationToken cancellationToken)
    {
        if(columns != rows)
            return Result.Failure<(Grid Grid, IReadOnlyList<IVariantBuilder> VariantBuilders, IReadOnlyList<VariantBuilderArgumentPair> VariantsInPlay)>("Width must match height");
        var size = columns;

        if (!NumbersValueSource.Sources.TryGetValue(size, out var valueSource))
            return Result.Failure<(Grid Grid, IReadOnlyList<IVariantBuilder> VariantBuilders, IReadOnlyList<VariantBuilderArgumentPair> VariantsInPlay)>($"Could not create sudoku size {size}");
        var maxPosition = new Position(size, size);

        var variantBuilders = SudokuVariant.SudokuVariantBuilders
            .Where(x => x.IsValid(maxPosition)).ToList();

        var variantsInPlay = variantBuilders.Where(x => x.DefaultArguments != null)
            .Select(x => new VariantBuilderArgumentPair(x, x.DefaultArguments!)).ToList();

        var clueSource =
            await ClueSource.TryCreateAsync(variantsInPlay, maxPosition, valueSource, cancellationToken);
        if (clueSource.IsFailure)
            return clueSource
                .ConvertFailure<(Grid Grid, IReadOnlyList<IVariantBuilder> VariantBuilders,
                    IReadOnlyList<VariantBuilderArgumentPair> VariantsInPlay)>();

        var createResult = Grid.CreateFromString((gridText ?? "").PadRight(size * size, '-'), clueSource.Value, maxPosition);

        if (createResult.IsFailure) return createResult.ConvertFailure<(Grid Grid, IReadOnlyList<IVariantBuilder> VariantBuilders, IReadOnlyList<VariantBuilderArgumentPair> VariantsInPlay)>();

        return (createResult.Value, variantBuilders, variantsInPlay);
    }

    /// <inheritdoc />
    public override int MinSize => 4;

    /// <inheritdoc />
    public override int MaxSize => 9;

    /// <inheritdoc />
    public override bool WidthMustMatchHeight => true;

    /// <inheritdoc />
    public override async Task<(Grid Grid, IReadOnlyList<IVariantBuilder> VariantBuilders, IReadOnlyList<VariantBuilderArgumentPair> VariantsInPlay)> GetDefault()
    {

        var variantBuilders = SudokuVariant.SudokuVariantBuilders
            .Where(x => x.IsValid(Position.NineNine)).ToList();

        var variantsInPlay = variantBuilders.Where(x => x.DefaultArguments != null)
            .Select(x => new VariantBuilderArgumentPair(x, x.DefaultArguments!)).ToList();

        var clueSource =
            await ClueSource.TryCreateAsync(variantsInPlay, Position.NineNine, NumbersValueSource.Sources[9], CancellationToken.None);

        var grid = Grid.Create(null, Position.NineNine, clueSource.Value);

        return (grid, variantBuilders, variantsInPlay);
    }

    /// <inheritdoc />
    public override IReadOnlyList<IVariantBuilder> VariantBuilderList => SudokuVariant.SudokuVariantBuilders;
}
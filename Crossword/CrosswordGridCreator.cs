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

namespace Crossword;

public class CrosswordGridCreator : GridCreator<char>
{
    private CrosswordGridCreator() {}

    public static GridCreator<char> Instance { get; } = new CrosswordGridCreator();

    /// <inheritdoc />
    public override async
        Task<Result<(Grid<char> Grid, IReadOnlyList<IVariantBuilder<char>> VariantBuilders,
            IReadOnlyList<VariantBuilderArgumentPair<char>> VariantsInPlay)>> TryCreate(int columns, int rows,
            string? gridText, CancellationToken cancellationToken)
    {
        var maxPosition = new Position(columns, rows);
        var variantsInPlay = CrosswordVariant.CrosswordVariantBuilders.Where(x => x.DefaultArguments != null)
            .Select(x => new VariantBuilderArgumentPair<char>(x, x.DefaultArguments!))
            .Where(x => x.VariantBuilder.IsValid(maxPosition))
            .ToList();

            

        var clueSource = await ClueSource<char>.TryCreateAsync(variantsInPlay, maxPosition,
            CrosswordValueSource.Instance, cancellationToken);
        if (clueSource.IsFailure) return clueSource.ConvertFailure<(Grid<char> Grid, IReadOnlyList<IVariantBuilder<char>> VariantBuilders,
            IReadOnlyList<VariantBuilderArgumentPair<char>> VariantsInPlay)>();


        var createResult =
            Grid<char>.CreateFromString((gridText ?? "").PadRight(columns * rows, '-'), clueSource.Value, maxPosition);

        if (createResult.IsFailure) return createResult.ConvertFailure<(Grid<char> Grid, IReadOnlyList<IVariantBuilder<char>> VariantBuilders,
            IReadOnlyList<VariantBuilderArgumentPair<char>> VariantsInPlay)>();

        var (newGrid, _) = createResult.Value.IterateRepeatedly(UpdateResultCombiner<char>.Default, UpdateResult<char>.Empty);

        return (newGrid, CrosswordVariant.CrosswordVariantBuilders, variantsInPlay);
    }

    /// <inheritdoc />
    public override int MinSize => 5;

    /// <inheritdoc />
    public override int MaxSize => 25;

    /// <inheritdoc />
    public override bool WidthMustMatchHeight => false;

    /// <inheritdoc />
    public override async Task<(Grid<char> Grid, IReadOnlyList<IVariantBuilder<char>> VariantBuilders, IReadOnlyList<VariantBuilderArgumentPair<char>> VariantsInPlay)> GetDefault()
    {
        var maxPosition = new Position(15, 15);
        var variantsInPlay = CrosswordVariant.CrosswordVariantBuilders.Where(x => x.DefaultArguments != null)
            .Select(x => new VariantBuilderArgumentPair<char>(x, x.DefaultArguments!))
            .Where(x => x.VariantBuilder.IsValid(maxPosition))
            .ToList();

        var clueSource = await ClueSource<char>.TryCreateAsync(variantsInPlay, maxPosition,
            CrosswordValueSource.Instance, CancellationToken.None);

        var grid = Grid<char>.Create(null, maxPosition, clueSource.Value);

        var (newGrid, _) = grid.IterateRepeatedly(UpdateResultCombiner<char>.Default, UpdateResult<char>.Empty);

        return (newGrid, CrosswordVariant.CrosswordVariantBuilders, variantsInPlay);
    }

    /// <inheritdoc />
    public override IReadOnlyList<IVariantBuilder<char>> VariantBuilderList => CrosswordVariant.CrosswordVariantBuilders;
}
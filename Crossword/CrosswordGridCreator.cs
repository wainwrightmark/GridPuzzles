using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using GridPuzzles.Session;
using GridPuzzles.VariantBuilderArguments;

namespace Crossword;

public class CrosswordGridCreator : GridCreator<char, CharCell>
{
    private CrosswordGridCreator() {}

    public static GridCreator<char, CharCell> Instance { get; } = new CrosswordGridCreator();

    /// <inheritdoc />
    public override async
        Task<Result<(Grid Grid, IReadOnlyList<IVariantBuilder<char, CharCell>> VariantBuilders,
            IReadOnlyList<VariantBuilderArgumentPair> VariantsInPlay)>> TryCreate(int columns, int rows,
            string? gridText, CancellationToken cancellationToken)
    {
        var maxPosition = new Position(columns, rows);
        var variantsInPlay = CrosswordVariant.CrosswordVariantBuilders.Where(x => x.DefaultArguments != null)
            .Select(x => new VariantBuilderArgumentPair(x, x.DefaultArguments!))
            .Where(x => x.VariantBuilder.IsValid(maxPosition))
            .ToList();

            

        var clueSource = await ClueSource.TryCreateAsync(variantsInPlay, maxPosition,
            CrosswordValueSource.Instance, cancellationToken);
        if (clueSource.IsFailure) return clueSource.ConvertFailure<(Grid Grid, IReadOnlyList<IVariantBuilder<char, CharCell>> VariantBuilders,
            IReadOnlyList<VariantBuilderArgumentPair> VariantsInPlay)>();


        var createResult =
            Grid.CreateFromString((gridText ?? "").PadRight(columns * rows, '-'), clueSource.Value, maxPosition);

        if (createResult.IsFailure) return createResult.ConvertFailure<(Grid Grid, IReadOnlyList<IVariantBuilder<char, CharCell>> VariantBuilders,
            IReadOnlyList<VariantBuilderArgumentPair> VariantsInPlay)>();

        var (newGrid, _) = createResult.Value.IterateRepeatedly(UpdateResultCombiner.Default,0, UpdateResult.Empty);

        return (newGrid, CrosswordVariant.CrosswordVariantBuilders, variantsInPlay);
    }

    /// <inheritdoc />
    public override int MinSize => 5;

    /// <inheritdoc />
    public override int MaxSize => 25;

    /// <inheritdoc />
    public override bool WidthMustMatchHeight => false;

    /// <inheritdoc />
    public override async Task<(Grid Grid, IReadOnlyList<IVariantBuilder<char, CharCell>> VariantBuilders, IReadOnlyList<VariantBuilderArgumentPair> VariantsInPlay)> GetDefault()
    {
        var maxPosition = new Position(15, 15);
        var variantsInPlay = CrosswordVariant.CrosswordVariantBuilders.Where(x => x.DefaultArguments != null)
            .Select(x => new VariantBuilderArgumentPair(x, x.DefaultArguments!))
            .Where(x => x.VariantBuilder.IsValid(maxPosition))
            .ToList();

        var clueSource = await ClueSource.TryCreateAsync(variantsInPlay, maxPosition,
            CrosswordValueSource.Instance, CancellationToken.None);

        var grid = Grid.Create(null, maxPosition, clueSource.Value);

        var (newGrid, _) = grid.IterateRepeatedly(UpdateResultCombiner.Default,0, UpdateResult.Empty);

        return (newGrid, CrosswordVariant.CrosswordVariantBuilders, variantsInPlay);
    }

    /// <inheritdoc />
    public override IReadOnlyList<IVariantBuilder<char, CharCell>> VariantBuilderList => CrosswordVariant.CrosswordVariantBuilders;
}
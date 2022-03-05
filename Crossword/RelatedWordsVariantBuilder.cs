using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using GridPuzzles.VariantBuilderArguments;

namespace Crossword;

public class RelatedWordsVariantBuilder : IVariantBuilder<char, CharCell>
{
    private RelatedWordsVariantBuilder() {}

    public static IVariantBuilder<char, CharCell> Instance { get; } = new RelatedWordsVariantBuilder();

    /// <inheritdoc />
    public string Name => "Words Related To";

    /// <inheritdoc />
    public async Task<Result<IReadOnlyCollection<IClueBuilder>>> TryGetClueBuildersAsync(
        IReadOnlyDictionary<string, string> arguments, CancellationToken cancellation)
    {
        var wr = WordsArgument.TryGetFromDictionary(arguments);
        if (wr.IsFailure) return wr.ConvertFailure<IReadOnlyCollection<IClueBuilder>>();

        var words = await GetRelatedWordsAsync(wr.Value, cancellation);

        var clueSource = new WordsClueBuilder(wr.Value, 5, words);

        return new List<IClueBuilder> { clueSource };

    }

    private static async Task<IReadOnlyCollection<string>>  GetRelatedWordsAsync(string s, CancellationToken cancellation)
    {
        var config = Configuration.Default.WithDefaultLoader();
        var address = $"https://relatedwords.io/{s}";
        var context = BrowsingContext.New(config);
        var document = await context.OpenAsync(address, cancellation);
        var cellSelector = "span.term";
        var cells = document.QuerySelectorAll(cellSelector);
        var titles = cells.Select(m => m.TextContent).ToList();

        return titles;
    }

    /// <inheritdoc />
    public IReadOnlyList<VariantBuilderArgument> Arguments => new[] { WordsArgument };

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string>? DefaultArguments => null;

    /// <inheritdoc />
    public bool IsValid(Position maxPosition)
    {
        return true;
    }

    /// <inheritdoc />
    public Result<IReadOnlyCollection<GridPuzzles.Clues.IClueBuilder>> TryGetClueBuilders(IReadOnlyDictionary<string, string> arguments)
    {
        return Result.Failure<IReadOnlyCollection<GridPuzzles.Clues.IClueBuilder>>("Must Create Clues Async");
    }

    public readonly StringArgument WordsArgument = new("Word", Maybe<string>.None);//TODO replace with file path???
}
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using GridPuzzles.VariantBuilderArguments;

namespace Crossword;

public class WordListFileVariantBuilder: IVariantBuilder<char, CharCell>
{
    public static WordListFileVariantBuilder Instance = new();

    private WordListFileVariantBuilder()
    {
    }

    /// <inheritdoc />
    public string Name => "Word List File";

    /// <inheritdoc />
    public async Task<Result<IReadOnlyCollection<IClueBuilder<char, CharCell>>>> TryGetClueBuildersAsync(
        IReadOnlyDictionary<string, string> arguments, CancellationToken cancellation)
    {
        var wr = WordsArgument.TryGetFromDictionary(arguments);
        if (wr.IsFailure) return wr.ConvertFailure<IReadOnlyCollection<IClueBuilder<char, CharCell>>>();


        IReadOnlyCollection<string> words = await File.ReadAllLinesAsync(wr.Value, cancellation);

        var clueSource = new WordsClueBuilder(wr.Value, 5, words);

        return new List<IClueBuilder<char, CharCell>>{clueSource};
    }

    public readonly StringArgument WordsArgument = new("Words List File", Maybe<string>.None);//TODO replace with file path???

    /// <inheritdoc />
    public IReadOnlyList<VariantBuilderArgument> Arguments => new[] {WordsArgument};

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
}
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using GridPuzzles;
using GridPuzzles.Clues;
using GridPuzzles.VariantBuilderArguments;

namespace Crossword;

public class WordListFileVariantBuilder: IVariantBuilder<char>
{
    public static WordListFileVariantBuilder Instance = new();

    private WordListFileVariantBuilder()
    {
    }

    /// <inheritdoc />
    public string Name => "Word List File";

    /// <inheritdoc />
    public async Task<Result<IReadOnlyCollection<IClueBuilder<char>>>> TryGetClueBuildersAsync(
        IReadOnlyDictionary<string, string> arguments, CancellationToken cancellation)
    {
        var wr = WordsArgument.TryGetFromDictionary(arguments);
        if (wr.IsFailure) return wr.ConvertFailure<IReadOnlyCollection<IClueBuilder<char>>>();


        IReadOnlyCollection<string> words = await File.ReadAllLinesAsync(wr.Value, cancellation);

        var clueSource = new WordsClueBuilder(wr.Value, 5, words);

        return new List<IClueBuilder<char>>{clueSource};
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
    public Result<IReadOnlyCollection<IClueBuilder>> TryGetClueBuilders(IReadOnlyDictionary<string, string> arguments)
    {
        return Result.Failure<IReadOnlyCollection<IClueBuilder>>("Must Create Clues Async");
    }
}
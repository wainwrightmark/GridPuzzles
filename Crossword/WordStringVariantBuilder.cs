﻿using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using GridPuzzles.VariantBuilderArguments;

namespace Crossword;

public class WordStringVariantBuilder : IVariantBuilder<char, CharCell>
{
    public static WordStringVariantBuilder Instance = new();

    private WordStringVariantBuilder()
    {
    }

    /// <inheritdoc />
    public string Name => "Word String";

    /// <inheritdoc />
    public async Task<Result<IReadOnlyCollection<IClueBuilder>>> TryGetClueBuildersAsync(
        IReadOnlyDictionary<string, string> arguments, CancellationToken cancellation)
    {
        await Task.CompletedTask;

        var wr = WordsArgument.TryGetFromDictionary(arguments);
        if (wr.IsFailure) return wr.ConvertFailure<IReadOnlyCollection<IClueBuilder>>();


        var words = wr.Value.Split(';');

        var clueSource = new WordsClueBuilder(wr.Value, 5, words);

        return new List<IClueBuilder>{clueSource};
    }

    public readonly StringArgument WordsArgument = new("Words", Maybe<string>.None); //TODO replace with list of string

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
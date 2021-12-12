using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using GridPuzzles;
using GridPuzzles.Clues;
using GridPuzzles.Overlays;
using GridPuzzles.VariantBuilderArguments;
using Words;

namespace Crossword;

public class DictionaryWordsVariantBuilder : IVariantBuilder<char>
{
    public DictionaryHelper DictionaryHelper { get; }

    public DictionaryWordsVariantBuilder(DictionaryHelper dictionaryHelper)
    {
        DictionaryHelper = dictionaryHelper;
    }

    /// <inheritdoc />
    public string Name => "Dictionary Words";

    /// <inheritdoc />
    public async Task<Result<IReadOnlyCollection<IClueBuilder<char>>>> TryGetClueBuildersAsync(
        IReadOnlyDictionary<string, string> arguments, CancellationToken cancellation)
    {
        await Task.CompletedTask;

        var wordTypeResult = WordTypeArgument.TryGetFromDictionary(arguments);

        if (wordTypeResult.IsFailure)
            return wordTypeResult.ConvertFailure<IReadOnlyCollection<IClueBuilder<char>>>();

        return wordTypeResult.Value switch
        {
            DictionaryWordType.CommonWords => new[] {WordsClueBuilder.CommonWords(DictionaryHelper)},
            DictionaryWordType.RareWords => new[] {WordsClueBuilder.RareWords(DictionaryHelper)},
            DictionaryWordType.Acronyms => new[] {WordsClueBuilder.Acronyms(DictionaryHelper)},
            DictionaryWordType.ProperNouns => new[] {WordsClueBuilder.ProperNouns(DictionaryHelper)},
            _ => throw new ArgumentOutOfRangeException()
        };
    }



    /// <inheritdoc />
    public IReadOnlyList<VariantBuilderArgument> Arguments => new[] {WordTypeArgument};

    public readonly EnumArgument<DictionaryWordType> WordTypeArgument = new("Word Type",Maybe<DictionaryWordType>.From(DictionaryWordType.CommonWords));

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string>? DefaultArguments => new Dictionary<string, string>()
    {
        {WordTypeArgument.Name, DictionaryWordType.CommonWords.ToString()}
    };

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

public enum DictionaryWordType
{
    CommonWords,
    RareWords,
    Acronyms,
    ProperNouns
}

public record WordsClueBuilder(string Name, ushort Priority, IReadOnlyCollection<string> Words) : IClueBuilder<char>
{
    public static WordsClueBuilder RareWords(DictionaryHelper dh)=> new("Rare Words", 2, dh.AllWords.Value);
    public static WordsClueBuilder ProperNouns(DictionaryHelper dh)=> new("Proper Nouns",2, dh.ProperNouns.Value);
    public static WordsClueBuilder Acronyms(DictionaryHelper dh)=> new("Acronyms",2, dh.Acronyms.Value);
    public static WordsClueBuilder CommonWords(DictionaryHelper dh)=> new("Common Words",3, dh.MostCommonWords.Value);

    public static WordsClueBuilder FromFile(string path) => new("From File",4, File.ReadAllLines(path).ToList());

    public static WordsClueBuilder FromText(string separator, string text) => new("From Text",5, text.Split(separator));
    
    /// <inheritdoc />
    public int Level => 1;

    /// <inheritdoc />
    public IEnumerable<IClue<char>> CreateClues(Position minPosition, Position maxPosition, IValueSource<char> valueSource,
        IReadOnlyCollection<IClue<char>> lowerLevelClues)
    {
        yield return new WordsClue(new []{(Priority, Words)});
    }

    /// <param name="minPosition"></param>
    /// <param name="maxPosition"></param>
    /// <inheritdoc />
    public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
    {
        yield break;
    }
}
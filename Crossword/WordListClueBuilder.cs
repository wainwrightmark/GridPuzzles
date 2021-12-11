using System;
using System.Collections.Generic;
using System.Linq;
using GridPuzzles;
using GridPuzzles.Clues;

namespace Crossword;

public class WordListClueBuilder : NoArgumentVariantBuilder<char> //TODO should be invisible
{

    public static WordListClueBuilder Instance = new();

    private WordListClueBuilder()
    {
    }

    /// <inheritdoc />
    public override string Name => "Words must come from Word Lists";

    /// <inheritdoc />
    public override int Level => 3;

    /// <inheritdoc />
    public override IEnumerable<IClue<char>> CreateClues(Position minPosition, Position maxPosition, IValueSource<char> valueSource,
        IReadOnlyCollection<IClue<char>> lowerLevelClues)
    {
        var words = lowerLevelClues.OfType<WordsClue>()
            .SelectMany(x=>x.Words)
            .ToList();

        var wordList = new PossibleWordList(Math.Max(maxPosition.Column, maxPosition.Row), new List<string>(), words);

        yield return  new WordListClue(wordList);
    }

    /// <inheritdoc />
    public override bool OnByDefault { get; } = true;

    /// <inheritdoc />
    public override bool IsValid(Position maxPosition)
    {
        return true;
    }
}
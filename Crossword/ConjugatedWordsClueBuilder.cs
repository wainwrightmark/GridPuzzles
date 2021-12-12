using System;
using System.Collections.Generic;
using System.Linq;
using GridPuzzles;
using GridPuzzles.Clues;
using Words;

namespace Crossword;

public class ConjugatedWordsClueBuilder : NoArgumentVariantBuilder<char>
{
    public ConjugatedWordsClueBuilder(DictionaryHelper dictionaryHelper)
    {
        DictionaryHelper = dictionaryHelper;
    }

    public DictionaryHelper DictionaryHelper { get; }


    /// <inheritdoc />
    public override string Name => "Conjugated Words";

    /// <inheritdoc />
    public override int Level => 2;

    /// <inheritdoc />
    public override IEnumerable<IClue<char>> CreateClues(Position minPosition, Position maxPosition, IValueSource<char> valueSource,
        IReadOnlyCollection<IClue<char>> lowerLevelClues)
    {
        var words = lowerLevelClues.OfType<WordsClue>()
            .SelectMany(x=>x.Words)
            .ToList();

        var newWords = words.GroupBy(x => x.priority, x => x.Item2)
            .Select(grouping => (Convert.ToUInt16(grouping.Key - 1) ,
                grouping.SelectMany(l => l.SelectMany(w => DictionaryHelper.GetAllVariations(w))).ToList() as IReadOnlyCollection<string>))
            .ToList();

        yield return new WordsClue(newWords);
    }

    /// <inheritdoc />
    public override bool OnByDefault => false;

    /// <inheritdoc />
    public override bool IsValid(Position maxPosition)
    {
        return true;
    }
}
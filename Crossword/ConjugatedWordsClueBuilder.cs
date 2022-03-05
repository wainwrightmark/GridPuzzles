using System;
using Words;

namespace Crossword;

public class ConjugatedWordsClueBuilder : NoArgumentVariantBuilder
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
    public override IEnumerable<IClue<char, CharCell>> CreateClues(Position minPosition, Position maxPosition, IValueSource<char, CharCell> valueSource,
        IReadOnlyCollection<IClue<char, CharCell>> lowerLevelClues)
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
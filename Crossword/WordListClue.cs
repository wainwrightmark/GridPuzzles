using System.Collections.Immutable;
using GridPuzzles;
using GridPuzzles.Clues;

namespace Crossword;

public class WordListClue : IClue<char>
{
    public WordListClue(PossibleWordList possibleWordList)
    {
        PossibleWordList = possibleWordList;
        Positions = ImmutableSortedSet<Position>.Empty;
    }

    public PossibleWordList PossibleWordList { get; }

    /// <inheritdoc />
    public ImmutableSortedSet<Position> Positions { get; }
        

    /// <inheritdoc />
    public string Name => "Word List";
}
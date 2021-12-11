using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GridPuzzles;
using GridPuzzles.Clues;

namespace Crossword;

public class WordsClue : IClue<char>
{
    public WordsClue(IEnumerable<(ushort priority, IReadOnlyCollection<string>)> words)
    {
        Words = words.ToList();
        Positions = ImmutableSortedSet<Position>.Empty;
    }

    public IEnumerable<(ushort priority, IReadOnlyCollection<string>)>  Words { get; }

    /// <inheritdoc />
    public ImmutableSortedSet<Position> Positions { get; }

    /// <inheritdoc />
    public string Name => "Crossword Words";
}
using System;
using System.Drawing;
using GridPuzzles.Overlays;

namespace Crossword;

public class WordListClue : IClue<char, CharCell>, IDynamicOverlayClue<char, CharCell>
{
    public WordListClue(PossibleWordList possibleWordList)
    {
        PossibleWordList = possibleWordList;
        Positions = ImmutableSortedSet<Position>.Empty;
    }

    public PossibleWordList PossibleWordList { get; }

    /// <inheritdoc />
    public ImmutableSortedSet<Position> Positions { get; }

    
    public IEnumerable<ICellOverlay> CreateCellOverlays(Grid grid)
    { 
        var words = NoDuplicateWordClue.TryGetAllWords(grid, null);

        if (words.IsSuccess)
        {
            foreach (var (word, positions) in words.Value)
            {
                var w = PossibleWordList.Words[new WordSearch((ushort)word.Length, (word[0], 0))].FirstOrDefault(x=>x.NormalizedText.Equals(word, StringComparison.OrdinalIgnoreCase));

                if (w is not null && w.Priority > 3)
                {
                    yield return new LineCellOverlay(positions, Color.Blue);
                }


            }
        }
    }
        

    /// <inheritdoc />
    public string Name => "Word List";
}
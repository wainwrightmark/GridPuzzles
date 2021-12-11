using System.Collections.Generic;
using System.Linq;
using System.Text;
using GridPuzzles;
using GridPuzzles.Cells;

namespace Crossword;

public class PossibleWordList
{
    /// <summary>
    /// Higher priority words are better
    /// </summary>
    public PossibleWordList(ushort maxLength, IEnumerable<string> bWords,  IReadOnlyCollection<(ushort priority, IReadOnlyCollection<string> words)> lists)
    {
        var wordsSoFar = bWords.Select(x=>Word.TryMake(x, 0, maxLength)).WhereNotNull().Select(x=>x.NormalizedText).ToHashSet();

        var finalWords = new List<Word>();

        Priorities = lists.Select(x => x.priority).OrderByDescending(x => x).ToList();


        foreach (var (priority, words) in lists.OrderByDescending(x=>x.priority))
            finalWords.AddRange(words.Select(x => Word.TryMake(x, priority, maxLength)).WhereNotNull()
                .Where(newWord => wordsSoFar.Add(newWord.NormalizedText)));

        Words = finalWords.SelectMany(w => GetWordSearches(w.NormalizedText).Select(ws => (w, ws)))
            .ToLookup(x => x.ws, x => x.w);

        TotalWords = finalWords.Count;
    }

    private static IEnumerable<WordSearch> GetWordSearches(string s)
    {
        for (ushort index = 0; index < s.Length; index++)
        {
            var c = s[index];
            yield return new WordSearch((ushort) s.Length,  (c, index));
        }

        yield return new WordSearch((ushort) s.Length, null);
    }

    public readonly int TotalWords;
    public readonly ILookup<WordSearch, Word> Words;
    public readonly IReadOnlyList<ushort> Priorities;

    private readonly IDictionary<string, IReadOnlyDictionary<ushort, IReadOnlyList<Word>>> _cache = new Dictionary<string, IReadOnlyDictionary<ushort, IReadOnlyList<Word>>>();
    public IReadOnlyDictionary<ushort, IReadOnlyList<Word>> Search(ExpressionWord w)
    {
        if (_cache.TryGetValue(w.Expression, out var r))
            return r;

        var search = w.WordSearch;

        var possibilities = Words[search];

        var truePossibilities = possibilities
            .Where(x => CouldBeWord(x.NormalizedText, w.Expression))
            .GroupBy(x=>x.Priority)
            .ToDictionary(x=>x.Key, x=>x.ToList() as IReadOnlyList<Word>);

        _cache.Add(w.Expression, truePossibilities);

        return truePossibilities;
    }

    public static bool CouldBeWord(string word, string expression)
    {
        if (word.Length != expression.Length) return false;

        if (expression.All(x => x.Equals('?'))) return true;

        var pairs = expression.Zip(word, (t, c) => (t, c));

        foreach (var (t,c) in pairs)
            switch (t)
            {
                case '?': break;
                default:
                    if (t != c) return false;
                    break;
            }
        return true;

    }
}

public class Word
{
    public Word(string normalizedText, string originalText, ushort priority)
    {
        NormalizedText = normalizedText;
        OriginalText = originalText;
        Priority = priority;
    }

    public static Word? TryMake(string original, ushort priority, ushort maxLength)
    {
        var normalized = new string(original.Where(char.IsLetter).ToArray()).ToUpperInvariant();

        if (normalized.Length > 2 && normalized.Length <= maxLength)
            return new Word(normalized, original, priority);

        return null;
    }

    public string NormalizedText { get; }

    public string OriginalText { get; }

    public ushort Priority { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return OriginalText;
    }

    /// <inheritdoc />
    public override int GetHashCode() => NormalizedText.GetHashCode();

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Word w && NormalizedText.Equals(w.NormalizedText);
    }
}

public readonly struct ExpressionWord
{
    public ExpressionWord(string expression, WordSearch wordSearch)
    {
        Expression = expression;
        WordSearch = wordSearch;
    }

    public string Expression { get; }
    public WordSearch WordSearch { get; }


    public static ExpressionWord? TryCreate(IEnumerable<KeyValuePair<Position, Cell<char>>> characters)
    {
        var pair = TryParse(characters);

        if (pair == null) return null;
        return new ExpressionWord(pair.Value.expression, pair.Value.wordSearch);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Expression;
    }

    private static (string expression, WordSearch wordSearch)? TryParse(IEnumerable<KeyValuePair<Position, Cell<char>>> characters)
    {
        var expression = new StringBuilder();
        (char c, ushort i)? wsTerm = null;

        ushort i = 0;

        foreach (var (_, cell) in characters)
        {
            if (cell.PossibleValues.Count == 1)
            {
                var val = cell.PossibleValues.Single();
                if (val == CrosswordValueSource.BlockChar) return null;
                expression.Append(val);
                if(wsTerm==null) wsTerm = (val, i);
            }
            else
            {
                expression.Append('?');
            }
            i++;
        }

        if (i < 3) return null;
        //if (wsTerm == null) return null;
        var wordSearch = new WordSearch(i, wsTerm);
        var expressionString = expression.ToString();

        return (expressionString, wordSearch);
    }

    public override bool Equals(object? obj)
    {
        return obj is ExpressionWord w && Expression.Equals(w.Expression);
    }

    public override int GetHashCode() => Expression.GetHashCode();

    public static bool operator ==(ExpressionWord left, ExpressionWord right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ExpressionWord left, ExpressionWord right)
    {
        return !(left == right);
    }
}

public readonly struct WordSearch
{
    public readonly ushort WordLength;
    public readonly (char c, ushort index)? Letter;

    public WordSearch(ushort wordLength, (char c, ushort index)? letter)
    {
        WordLength = wordLength;
        Letter = letter;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return WordLength * 11  + (Letter?.GetHashCode()??0 * 17);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is WordSearch ws &&
               WordLength == ws.WordLength &&
               (
                   (!Letter.HasValue && !ws.Letter.HasValue)
                   ||
                   (Letter.HasValue && ws.Letter.HasValue && Letter.Value.index == ws.Letter.Value.index && Letter.Value.c == ws.Letter.Value.c)
               );
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Letter.HasValue)
        {
            return new string(Enumerable.Repeat('?', Letter.Value.index).ToArray())
                   + Letter.Value.c +  new string(Enumerable.Repeat('?', WordLength - 1 - Letter.Value.index).ToArray());
        }
        else
        {
            return new string(Enumerable.Repeat('?', WordLength).ToArray());
        }
    }

    public static bool operator ==(WordSearch left, WordSearch right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(WordSearch left, WordSearch right)
    {
        return !(left == right);
    }
}
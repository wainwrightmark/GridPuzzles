using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WeCantSpell.Hunspell;

namespace Words;

public class DictionaryHelper
{
    private static Stream StringToStream(string s)
    {
        var byteArray = Encoding.ASCII.GetBytes(s);
        var stream = new MemoryStream(byteArray);
        return stream;
    }

    public DictionaryHelper()
    {
        HunspellWordList = new Lazy<WordList>(() =>
        {
            using var dictionaryStream = StringToStream(Resources.index1);
            using var affixStream = StringToStream(Resources.index_aff);

            var wl = WordList.CreateFromStreams(dictionaryStream, affixStream);

            return wl;
        });

        AllWords = new Lazy<IReadOnlyCollection<string>>(() =>
            GenerateAllWords(HunspellWordList.Value)
                .Distinct()
                .Where(x => x.All(char.IsLetter) && x.Length > 2).ToList());

        NormalWords = new Lazy<IReadOnlyCollection<string>>(() =>
            AllWords.Value.Where(x => x.All(char.IsLower)).ToList());

        ProperNouns = new Lazy<IReadOnlyCollection<string>>(() =>
            AllWords.Value.Where(x => x.Any(char.IsLower) && x.Any(char.IsUpper)).ToList());

        Acronyms = new Lazy<IReadOnlyCollection<string>>(() =>
            AllWords.Value.Where(x => x.All(char.IsUpper)).ToList());

        MostCommonWords = new Lazy<IReadOnlyCollection<string>>(
            () =>
                Resources.MostCommonWords.Split("\r\n", StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList());
    }

    public readonly Lazy<WordList> HunspellWordList;

    public readonly Lazy<IReadOnlyCollection<string>> AllWords;
    public readonly Lazy<IReadOnlyCollection<string>> NormalWords;
    public readonly Lazy<IReadOnlyCollection<string>> Acronyms;
    public readonly Lazy<IReadOnlyCollection<string>> ProperNouns;
    public readonly Lazy<IReadOnlyCollection<string>> MostCommonWords;

    private IEnumerable<string> GenerateAllWords(WordList wordList)
    {
        return wordList.RootWords.SelectMany(GetAllVariations);
    }

    public IEnumerable<string> GetAllVariations(string word)
    {
        yield return word;

        var wordEntryDetail = HunspellWordList.Value[word];

        var allPrefixesForWord =
            HunspellWordList.Value.Affix.Prefixes.Where(p => wordEntryDetail.Any(x => x.ContainsFlag(p.AFlag)))
                .ToList();

        string combined;
        foreach (var prefixEntry in allPrefixesForWord.SelectMany(p => p.Entries))
            if (TryAppend(prefixEntry, word, out combined))
                yield return combined;

        var allSuffixesForWord =
            HunspellWordList.Value.Affix.Suffixes.Where(s => wordEntryDetail.Any(x => x.ContainsFlag(s.AFlag)))
                .ToList();

        foreach (var suffixEntry in allSuffixesForWord.SelectMany(s => s.Entries))
            if (TryAppend(suffixEntry, word, out combined))
                yield return combined;

        foreach (var prefixEntry in allPrefixesForWord.Where(p => p.Options.HasFlag(AffixEntryOptions.CrossProduct))
                     .SelectMany(p => p.Entries))
        {
            if (!TryAppend(prefixEntry, word, out var withPrefix)) continue;
            foreach (var suffixEntry in allSuffixesForWord.Where(s => s.Options.HasFlag(AffixEntryOptions.CrossProduct))
                         .SelectMany(s => s.Entries))
                if (TryAppend(suffixEntry, withPrefix, out combined))
                    yield return combined;
        }
    }

    private static bool TryAppend(PrefixEntry prefix, string word, out string result)
    {
        if (prefix.Conditions.IsStartingMatch(word) && word.StartsWith(prefix.Strip))
        {
            result = prefix.Append + word[prefix.Strip.Length..];
            return true;
        }

        result = null!;
        return false;
    }

    private static bool TryAppend(SuffixEntry suffix, string word, out string result)
    {
        if (suffix.Conditions.IsEndingMatch(word) && word.EndsWith(suffix.Strip))
        {
            result = word[..^suffix.Strip.Length] + suffix.Append;
            return true;
        }

        result = null!;
        return false;
    }
}
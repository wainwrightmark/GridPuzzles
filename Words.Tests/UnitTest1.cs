using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Words.Tests;
public class UnitTest1
{
    public UnitTest1(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }

    public ITestOutputHelper TestOutputHelper { get; set; }

    [Theory]
    [InlineData(4, true)]
    [InlineData(4, false)]
    [InlineData(5, true)]
    [InlineData(5, false)]
    [InlineData(6, true)]
    [InlineData(6, false)]
    [InlineData(7, true)]
    [InlineData(7, false)]
    [InlineData(8, true)]
    [InlineData(8, false)]
    public void PrintWordsWithLength(int length, bool commonOnly)
    {
        var dh = new DictionaryHelper();

        IReadOnlyCollection<string> foundWords;

        if (commonOnly)
        {
            foundWords = dh.MostCommonWords.Value;
        }
        else
        {
            foundWords = dh.NormalWords.Value.SelectMany(x=> dh.GetAllVariations(x)).ToList();
        }

        var filteredFoundWords = foundWords.Where(x => x.Length == length && x.All(char.IsLetter))
            .Distinct()
            .ToList();

        TestOutputHelper.WriteLine(filteredFoundWords.Count + " Words");

        foreach (var filteredFoundWord in filteredFoundWords)
        {
            TestOutputHelper.WriteLine(filteredFoundWord);
        }
        



    }
}
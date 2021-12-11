using System.Collections.Generic;

namespace GridPuzzles.Blazor;

public static class GridTypeHelper
{
    public static IEnumerable<(string variantName, string href)> MenuGrids2
    {
        get
        {
            foreach (var (variantName, type) in MenuNumberGrids) yield return (variantName, $"numberGrid/{type}");

            foreach (var (variantName, size) in MenuWordGrids)
                yield return (variantName, $"wordGrid/{size}");
        }
    }

    private static IEnumerable<(string variantName, string type)> MenuNumberGrids
    {
        get
        {
            yield return ("Standard", "9");
            yield return ("Mini", "6");
            yield return ("Micro", "4");
        }
    }

    private static IEnumerable<(string variantName, string type)> MenuWordGrids
    {
        get
        {
            yield return ("21x21", "21");
            yield return ("15x15", "15");
            yield return ("11x11", "11");
            yield return ("5x5", "5");
        }
    }
}
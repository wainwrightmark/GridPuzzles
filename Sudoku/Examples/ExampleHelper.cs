using System.Collections;
using System.Globalization;
using System.Text;

namespace Sudoku.Examples;

public static class ExampleHelper
{
    public static IEnumerable<Example> SudokuExamples
    {
        get
        {
            var resourceSet =
                ExampleResource.ResourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true)!;

            foreach (DictionaryEntry entry in resourceSet)
            {
                if (entry.Value is byte[] ba)
                {

                    var yaml = Encoding.UTF8.GetString(ba);
                    yaml = yaml.TrimStart(new char[]{'\uFEFF'});
                   yield return new Example(entry.Key?.ToString(), yaml);
                }


                
            }
        }
    }
}

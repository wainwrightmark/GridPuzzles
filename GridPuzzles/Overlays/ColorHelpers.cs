using System.Drawing;

namespace GridPuzzles.Overlays;

public static class ColorHelpers
{
    public static string ToSVGColor(this Color color)
    {
        if (color.IsNamedColor)
            return color.Name;
        return $"#{color.R.ToHex()}{color.G.ToHex()}{color.B.ToHex()}";
    }

    public static string ToHex(this byte num) //Stolen from AngleSharp
    {
        char[] chArray = new char[2];
        var num1 = num >> 4;
        chArray[0] = (char) (num1 + (num1 < 10 ? 48 : 55));
        var num2 = num - 16 * num1;
        chArray[1] = (char) (num2 + (num2 < 10 ? 48 : 55));
        return new string(chArray);
    }
}
using System;

namespace Sudoku.Clues;

public static class ClueColors
{
    public static readonly Color DiagonalClueColor = Color.Red;
    public static readonly Color NearDiagonalClueColor = Color.DarkRed;
    public static readonly Color MirrorColor = Color.CornflowerBlue;


    public static Color GetUniqueSumColor(int sum)
    {
        var changingColor = Math.Min(255, ((Math.Abs(sum) * 255) / 44));

        return (Math.Abs(sum) % 6) switch
        {
            0 => Color.FromArgb(changingColor, 128, 0),
            1 => Color.FromArgb(changingColor, 0, 128),
            2 => Color.FromArgb(0, changingColor, 128),
            3 => Color.FromArgb(128, changingColor, 0),
            4 => Color.FromArgb(0, 128, changingColor),
            5 => Color.FromArgb(128, 0, changingColor),
            _ => throw new ArgumentException($"Modulus fail with {sum}")
        };
    }

    //public static readonly Color UniqueSumColor = Color.GreenYellow;
    public static readonly Color MagicSquareColor = Color.Green;

    public static readonly Color ThermometerColor = Color.MistyRose;
    public static readonly Color ThermometerHeadColor = Color.SteelBlue;

    public static readonly Color RestrictedValueColor = Color.DimGray;

    public static readonly Color BlockColor = Color.Black;
    public static readonly Color SnakeColor = Color.ForestGreen;

    public static readonly Color ArrowHeadColor = Color.BlueViolet;
    public static readonly Color ArrowTailColor = Color.Orange;

}
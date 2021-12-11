namespace GridPuzzles.SVG;

public static class CellStyleHelpers
{
    public const int CellSize = 60;

    public static double GetX(this Position position, bool centre) => position.GetX(centre, CellSize);
    public static double GetY(this Position position, bool centre) => position.GetY(centre, CellSize);

    public static int GetHeight(IGrid gridView)
    {
        return  (gridView.MaxPosition.Row + 2) * CellSize;
    }

    public static int GetWidth(IGrid gridView)
    {
        return  (gridView.MaxPosition.Column + 2) * CellSize;
    }
}
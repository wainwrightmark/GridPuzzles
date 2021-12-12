using System.Drawing;
using GridPuzzles.Enums;

namespace GridPuzzles.Overlays;

public static class CellOverlays
{
    public static ICellOverlay CreateOnePositionText(Position p1, string text) =>
        new TextCellOverlay(p1, 1, 1, text, Color.Black, Color.White);

    public static Maybe<ICellOverlay> TryCreateTwoPositionText(Position p1, Position p2, string text)
        => TryCreateTwoPositionText(p1, p2, _ => (text, null));

    public static Maybe<ICellOverlay> TryCreateTwoPositionText(Position p1, Position p2, Func<CompassDirection, (string text, int? rotationDegrees)> getText)
    {
        var direction = p1.GetAdjacentDirection(p2);

        if (direction.HasNoValue) return Maybe<ICellOverlay>.None;

        var topLeft = new Position(Math.Min(p1.Column, p2.Column), Math.Min(p1.Row, p2.Row));
        var width = p1.Column == p2.Column ? 1 : 2;
        var height = p1.Row == p2.Row ? 1 : 2;

        var (text, rotation) = getText(direction.Value);

        return new TextCellOverlay(topLeft, width, height, text,  Color.Black, Color.White, rotation);
    }

    public static Maybe<ICellOverlay> TryCreateSquareText(IReadOnlyCollection<Position> positions, string text)
    {
        if (positions.Count != 4 || !positions.GetFirstFour().FormSquare())
            return Maybe<ICellOverlay>.None;

        var topLeftPosition =
            new Position(positions.Select(x => x.Column).Min(), positions.Select(x => x.Row).Min());
        return new TextCellOverlay(topLeftPosition, 2, 2, text, Color.Black, Color.White);
    }
}
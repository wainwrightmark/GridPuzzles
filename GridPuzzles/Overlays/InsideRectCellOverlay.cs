using System.Collections.Generic;
using System.Drawing;
using SVGElements;

namespace GridPuzzles.Overlays;

public record InsideRectCellOverlay(Position Position, Color Color) : ICellSVGElementOverlay
{
    /// <inheritdoc />
    public IEnumerable<SVGElement> SVGDefinitions(double scale)
    {
        yield break;
    }

    /// <inheritdoc />
    public IEnumerable<SVGElement> SVGElements(double scale, bool selected)
    {
        yield return new SVGRectangle(
            Position + Color.ToSVGColor() + "Rect",
            Position.GetX(false, scale)+ scale / 10, 
            Position.GetY(false, scale)+ scale / 10, 
            1,1,
            scale * 4/5, scale * 4/5, 
            Color.ToSVGColor(), 
            Stroke:"none",
            PointerEvents:PointerEvents.none);
    }

    /// <inheritdoc />
    public int ZIndex => 1;
}
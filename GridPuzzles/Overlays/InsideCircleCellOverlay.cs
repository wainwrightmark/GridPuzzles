using System.Collections.Generic;
using System.Drawing;
using SVGElements;

namespace GridPuzzles.Overlays;

public record InsideCircleCellOverlay(Position Position, Color Color) : ICellSVGElementOverlay
{
    /// <inheritdoc />
    public IEnumerable<SVGElement> GetSVGDefinitions(double scale)
    {
        yield break;
    }

    /// <inheritdoc />
    public IEnumerable<SVGElement> GetSVGElements(double scale)
    {
        yield return new SVGCircle(
            Position + Color.ToSVGColor() + "Circle",
            scale * 2 / 5,  
            Position.GetX(true, scale), 
            Position.GetY(true, scale), 
                 
            Color.ToSVGColor(), Stroke:"none",
            PointerEvents:PointerEvents.none);
    }

    /// <inheritdoc />
    public int ZIndex => 1;
}
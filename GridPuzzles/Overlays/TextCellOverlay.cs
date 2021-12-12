using System.Drawing;
using SVGElements;

namespace GridPuzzles.Overlays;

public record TextCellOverlay(Position TopLeftPosition,
        int Width, 
        int Height, 
        string Text, 
        Color StrokeColor,
        Color FillColor,
        int? RotateDegrees = 0 ) 
    : ICellSVGElementOverlay
{
    /// <inheritdoc />
    public IEnumerable<SVGElement> SVGDefinitions(double scale)
    {
        yield break;
    }

    /// <inheritdoc />
    public IEnumerable<SVGElement> SVGElements(double scale, bool selected)
    {
        yield return new SVGText(
            TopLeftPosition + "Text" + Width + "-" + Height + Text.GetHashCode(),
            Text,
            null,
            TopLeftPosition.GetX(false, scale) + (scale * Width / 2),
            TopLeftPosition.GetY(false, scale)  + (scale * Height / 2),
            PointerEvents: PointerEvents.none,
            TextAnchor: TextAnchor.middle,
            DominantBaseline: DominantBaseline.middle,
            FontSize: "x-large",
            FontWeight:"bolder",
            Fill:FillColor.ToSVGColor(),
            Stroke:StrokeColor.ToSVGColor(),
            Rotate:RotateDegrees
        );
    }

    /// <inheritdoc />
    public int ZIndex => 3;
}
using System.Drawing;
using MoreLinq;
using SVGElements;

namespace GridPuzzles.Overlays;

public record LineCellOverlay(IReadOnlyList<Position> Positions, Color Color) : ICellSVGElementOverlay
{
    /// <inheritdoc />
    public IEnumerable<SVGElement> SVGDefinitions(double scale)
    {
        yield break;
    }

    public string GetPointsString(double scale) =>
        string.Join(" ", Positions.Select(x => $"{(x.Column * scale) + scale / 2}, {(x.Row * scale) + scale / 2}"));

    /// <inheritdoc />
    public IEnumerable<SVGElement> SVGElements(double scale, bool selected)
    {
        yield return new SVGPolyLine(
            "Line" + Positions.ToDelimitedString("") + Color.Name,
            GetPointsString(scale),
            Fill: "none",
            StrokeWidth:3,
            Children:selected? Animations.IsSelectedOpacity : null,
            Stroke:Color.ToSVGColor(),
            PointerEvents: PointerEvents.none
        );
    }

    /// <inheritdoc />
    public int ZIndex => 2;
}
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MoreLinq;
using SVGElements;

namespace GridPuzzles.Overlays;

public record LineCellOverlay(IReadOnlyList<Position> Positions, Color Color) : ICellSVGElementOverlay
{
    /// <inheritdoc />
    public IEnumerable<SVGElement> GetSVGDefinitions(double scale)
    {
        yield break;
    }

    public string GetPointsString(double scale) =>
        string.Join(" ", Positions.Select(x => $"{(x.Column * scale) + scale / 2}, {(x.Row * scale) + scale / 2}"));

    /// <inheritdoc />
    public IEnumerable<SVGElement> GetSVGElements(double scale)
    {
        yield return new SVGPolyLine(
            "Line" + Positions.ToDelimitedString("") + Color.Name,
            GetPointsString(scale),
            Fill: "none",
            StrokeWidth:3,
            Stroke:Color.ToSVGColor(),
            PointerEvents: PointerEvents.none
        );
    }

    /// <inheritdoc />
    public int ZIndex => 2;
}
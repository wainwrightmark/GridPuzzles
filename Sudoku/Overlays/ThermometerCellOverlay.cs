using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GridPuzzles;
using GridPuzzles.Overlays;
using MoreLinq;
using SVGElements;

namespace Sudoku.Overlays;

public record ThermometerCellOverlay(IReadOnlyList<Position> Positions, Color Color) : ICellSVGElementOverlay
{
    public string GetPointsString(double scale) =>
        string.Join(" ", Positions.Select(x => $"{(x.Column * scale) + scale / 2}, {(x.Row * scale) + scale / 2}"));

    /// <inheritdoc />
    public IEnumerable<SVGElement> GetSVGDefinitions(double scale)
    {
        yield return new SVGMarker(
            "thermoCircle" +Color.ToSVGColor(),
            "auto",
            scale / 2,
            scale / 2,
                
            RefX: scale / 4,
            RefY: scale / 4,
            Children: new[]
            {
                new SVGCircle(
                    "circle",
                    scale / 30,
                    CentreX: scale / 4,
                    CentreY: scale / 4,
                    Stroke: "none",
                    Fill: Color.ToSVGColor()
                )
            }
        );
    }

    /// <inheritdoc />
    public IEnumerable<SVGElement> GetSVGElements(double scale)
    {
        yield return new SVGPolyLine(
            "Thermo" + Positions.ToDelimitedString(""),

            GetPointsString(scale),
            Fill: "none",
            Stroke:Color.ToSVGColor(),
            StrokeWidth:10,
            MarkerStart: $"url(#thermoCircle{Color.ToSVGColor()})",
            PointerEvents: PointerEvents.none,
            StrokeLinecap: StrokeLinecap.round
        );
    }

    /// <inheritdoc />
    public int ZIndex => 2;
}
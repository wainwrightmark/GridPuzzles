using System.Collections.Generic;
using SVGElements;

namespace GridPuzzles.Overlays;

public interface ICellSVGElementOverlay : ICellOverlay
{
    IEnumerable<SVGElement> SVGDefinitions(double scale);
    IEnumerable<SVGElement> SVGElements(double scale, bool selected);

    public int ZIndex { get; }
}
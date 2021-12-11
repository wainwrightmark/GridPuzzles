using System.Collections.Generic;
using SVGElements;

namespace GridPuzzles.Overlays;

public interface ICellSVGElementOverlay : ICellOverlay
{
    IEnumerable<SVGElement> GetSVGDefinitions(double scale);
    IEnumerable<SVGElement> GetSVGElements(double scale);

    public int ZIndex { get; }
}
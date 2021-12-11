using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SVGElements
{
    [System.CodeDom.Compiler.GeneratedCode("SVGSourceGenerator", "1.0")]
    public sealed partial record SVGLine
    {
        public override IEnumerable<(string propertyName, int index, object value)> GetProperties()
        {
            if (XStart is not null)
                yield return ("x1", 1, XStart);
            if (XEnd is not null)
                yield return ("x2", 2, XEnd);
            if (YStart is not null)
                yield return ("y1", 3, YStart);
            if (YEnd is not null)
                yield return ("y2", 4, YEnd);
            if (Transform is not null)
                yield return ("transform", 5, Transform);
            if (Opacity is not null)
                yield return ("opacity", 6, Opacity);
            if (MarkerStart is not null)
                yield return ("marker-start", 7, MarkerStart);
            if (MarkerMid is not null)
                yield return ("marker-mid", 8, MarkerMid);
            if (MarkerEnd is not null)
                yield return ("marker-end", 9, MarkerEnd);
            if (Stroke is not null)
                yield return ("stroke", 10, Stroke);
            if (StrokeWidth is not null)
                yield return ("stroke-width", 11, StrokeWidth);
            if (StrokeLinecap is not null)
                yield return ("stroke-linecap", 12, StrokeLinecap);
            if (StrokeDashArray is not null)
                yield return ("stroke-dasharray", 13, StrokeDashArray);
            if (Id is not null)
                yield return ("id", 14, Id);
            if (TabIndex is not null)
                yield return ("tabindex", 15, TabIndex);
            if (Class is not null)
                yield return ("class", 16, Class);
            if (Style is not null)
                yield return ("style", 17, Style);
            if (PointerEvents is not null)
                yield return ("pointer-events", 18, PointerEvents);
            yield break;
        }
    }
}
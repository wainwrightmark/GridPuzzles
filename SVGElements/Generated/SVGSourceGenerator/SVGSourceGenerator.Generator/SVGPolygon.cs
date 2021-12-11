using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SVGElements
{
    [System.CodeDom.Compiler.GeneratedCode("SVGSourceGenerator", "1.0")]
    public sealed partial record SVGPolygon
    {
        public override IEnumerable<(string propertyName, int index, object value)> GetProperties()
        {
            if (Points is not null)
                yield return ("points", 1, Points);
            if (PathLength is not null)
                yield return ("pathLength", 2, PathLength);
            if (MarkerStart is not null)
                yield return ("marker-start", 3, MarkerStart);
            if (MarkerMid is not null)
                yield return ("marker-mid", 4, MarkerMid);
            if (MarkerEnd is not null)
                yield return ("marker-end", 5, MarkerEnd);
            if (Stroke is not null)
                yield return ("stroke", 6, Stroke);
            if (StrokeWidth is not null)
                yield return ("stroke-width", 7, StrokeWidth);
            if (StrokeLinecap is not null)
                yield return ("stroke-linecap", 8, StrokeLinecap);
            if (StrokeDashArray is not null)
                yield return ("stroke-dasharray", 9, StrokeDashArray);
            if (Id is not null)
                yield return ("id", 10, Id);
            if (TabIndex is not null)
                yield return ("tabindex", 11, TabIndex);
            if (Class is not null)
                yield return ("class", 12, Class);
            if (Style is not null)
                yield return ("style", 13, Style);
            if (PointerEvents is not null)
                yield return ("pointer-events", 14, PointerEvents);
            yield break;
        }
    }
}
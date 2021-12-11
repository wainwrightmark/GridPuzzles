using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SVGElements
{
    [System.CodeDom.Compiler.GeneratedCode("SVGSourceGenerator", "1.0")]
    public sealed partial record SVGPolyLine
    {
        public override IEnumerable<(string propertyName, int index, object value)> GetProperties()
        {
            if (Points is not null)
                yield return ("points", 1, Points);
            if (PathLength is not null)
                yield return ("pathLength", 2, PathLength);
            if (Fill is not null)
                yield return ("fill", 3, Fill);
            if (Transform is not null)
                yield return ("transform", 4, Transform);
            if (Opacity is not null)
                yield return ("opacity", 5, Opacity);
            if (MarkerStart is not null)
                yield return ("marker-start", 6, MarkerStart);
            if (MarkerMid is not null)
                yield return ("marker-mid", 7, MarkerMid);
            if (MarkerEnd is not null)
                yield return ("marker-end", 8, MarkerEnd);
            if (Stroke is not null)
                yield return ("stroke", 9, Stroke);
            if (StrokeWidth is not null)
                yield return ("stroke-width", 10, StrokeWidth);
            if (StrokeLinecap is not null)
                yield return ("stroke-linecap", 11, StrokeLinecap);
            if (StrokeDashArray is not null)
                yield return ("stroke-dasharray", 12, StrokeDashArray);
            if (Id is not null)
                yield return ("id", 13, Id);
            if (TabIndex is not null)
                yield return ("tabindex", 14, TabIndex);
            if (Class is not null)
                yield return ("class", 15, Class);
            if (Style is not null)
                yield return ("style", 16, Style);
            if (PointerEvents is not null)
                yield return ("pointer-events", 17, PointerEvents);
            yield break;
        }
    }
}
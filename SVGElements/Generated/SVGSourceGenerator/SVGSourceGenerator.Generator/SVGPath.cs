using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SVGElements
{
    [System.CodeDom.Compiler.GeneratedCode("SVGSourceGenerator", "1.0")]
    public sealed partial record SVGPath
    {
        public override IEnumerable<(string propertyName, int index, object value)> GetProperties()
        {
            if (Draw is not null)
                yield return ("d", 1, Draw);
            if (Fill is not null)
                yield return ("fill", 2, Fill);
            if (Transform is not null)
                yield return ("transform", 3, Transform);
            if (Opacity is not null)
                yield return ("opacity", 4, Opacity);
            if (MarkerStart is not null)
                yield return ("marker-start", 5, MarkerStart);
            if (MarkerMid is not null)
                yield return ("marker-mid", 6, MarkerMid);
            if (MarkerEnd is not null)
                yield return ("marker-end", 7, MarkerEnd);
            if (Stroke is not null)
                yield return ("stroke", 8, Stroke);
            if (StrokeWidth is not null)
                yield return ("stroke-width", 9, StrokeWidth);
            if (StrokeLinecap is not null)
                yield return ("stroke-linecap", 10, StrokeLinecap);
            if (StrokeDashArray is not null)
                yield return ("stroke-dasharray", 11, StrokeDashArray);
            if (Id is not null)
                yield return ("id", 12, Id);
            if (TabIndex is not null)
                yield return ("tabindex", 13, TabIndex);
            if (Class is not null)
                yield return ("class", 14, Class);
            if (Style is not null)
                yield return ("style", 15, Style);
            if (PointerEvents is not null)
                yield return ("pointer-events", 16, PointerEvents);
            yield break;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SVGElements
{
    [System.CodeDom.Compiler.GeneratedCode("SVGSourceGenerator", "1.0")]
    public sealed partial record SVGGroup
    {
        public override IEnumerable<(string propertyName, int index, object value)> GetProperties()
        {
            if (FontSize is not null)
                yield return ("font-size", 1, FontSize);
            if (FontFamily is not null)
                yield return ("font-family", 2, FontFamily);
            if (TextAnchor is not null)
                yield return ("text-anchor", 3, TextAnchor);
            if (DominantBaseline is not null)
                yield return ("dominant-baseline", 4, DominantBaseline);
            if (Fill is not null)
                yield return ("fill", 5, Fill);
            if (Transform is not null)
                yield return ("transform", 6, Transform);
            if (Stroke is not null)
                yield return ("stroke", 7, Stroke);
            if (StrokeWidth is not null)
                yield return ("stroke-width", 8, StrokeWidth);
            if (StrokeLinecap is not null)
                yield return ("stroke-linecap", 9, StrokeLinecap);
            if (StrokeDashArray is not null)
                yield return ("stroke-dasharray", 10, StrokeDashArray);
            if (Id is not null)
                yield return ("id", 11, Id);
            if (TabIndex is not null)
                yield return ("tabindex", 12, TabIndex);
            if (Class is not null)
                yield return ("class", 13, Class);
            if (Style is not null)
                yield return ("style", 14, Style);
            if (PointerEvents is not null)
                yield return ("pointer-events", 15, PointerEvents);
            yield break;
        }
    }
}
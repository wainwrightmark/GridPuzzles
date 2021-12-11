using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SVGElements
{
    [System.CodeDom.Compiler.GeneratedCode("SVGSourceGenerator", "1.0")]
    public sealed partial record SVGCircle
    {
        public override IEnumerable<(string propertyName, int index, object value)> GetProperties()
        {
            yield return ("r", 1, Radius);
            if (CentreX is not null)
                yield return ("cx", 2, CentreX);
            if (CentreY is not null)
                yield return ("cy", 3, CentreY);
            if (Fill is not null)
                yield return ("fill", 4, Fill);
            if (Transform is not null)
                yield return ("transform", 5, Transform);
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SVGElements
{
    [System.CodeDom.Compiler.GeneratedCode("SVGSourceGenerator", "1.0")]
    public sealed partial record SVGRectangle
    {
        public override IEnumerable<(string propertyName, int index, object value)> GetProperties()
        {
            if (X is not null)
                yield return ("x", 1, X);
            if (Y is not null)
                yield return ("y", 2, Y);
            if (RadiusX is not null)
                yield return ("rx", 3, RadiusX);
            if (RadiusY is not null)
                yield return ("ry", 4, RadiusY);
            if (Width is not null)
                yield return ("width", 5, Width);
            if (Height is not null)
                yield return ("height", 6, Height);
            if (Fill is not null)
                yield return ("fill", 7, Fill);
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
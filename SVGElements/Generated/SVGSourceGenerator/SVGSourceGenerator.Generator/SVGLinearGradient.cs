using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SVGElements
{
    [System.CodeDom.Compiler.GeneratedCode("SVGSourceGenerator", "1.0")]
    public sealed partial record SVGLinearGradient
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
            if (GradientTransform is not null)
                yield return ("gradientTransform", 5, GradientTransform);
            if (Id is not null)
                yield return ("id", 6, Id);
            if (TabIndex is not null)
                yield return ("tabindex", 7, TabIndex);
            if (Class is not null)
                yield return ("class", 8, Class);
            if (Style is not null)
                yield return ("style", 9, Style);
            if (PointerEvents is not null)
                yield return ("pointer-events", 10, PointerEvents);
            yield break;
        }
    }
}
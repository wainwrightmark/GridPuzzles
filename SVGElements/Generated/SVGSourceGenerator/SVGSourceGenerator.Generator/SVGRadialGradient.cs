using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SVGElements
{
    [System.CodeDom.Compiler.GeneratedCode("SVGSourceGenerator", "1.0")]
    public sealed partial record SVGRadialGradient
    {
        public override IEnumerable<(string propertyName, int index, object value)> GetProperties()
        {
            if (CircleRadius is not null)
                yield return ("r", 1, CircleRadius);
            if (CircleCentreX is not null)
                yield return ("cx", 2, CircleCentreX);
            if (CircleCentreY is not null)
                yield return ("cy", 3, CircleCentreY);
            if (FocusRadius is not null)
                yield return ("fr", 4, FocusRadius);
            if (FocusCentreX is not null)
                yield return ("fx", 5, FocusCentreX);
            if (FocusCentreY is not null)
                yield return ("fy", 6, FocusCentreY);
            if (Id is not null)
                yield return ("id", 7, Id);
            if (TabIndex is not null)
                yield return ("tabindex", 8, TabIndex);
            if (Class is not null)
                yield return ("class", 9, Class);
            if (Style is not null)
                yield return ("style", 10, Style);
            if (PointerEvents is not null)
                yield return ("pointer-events", 11, PointerEvents);
            yield break;
        }
    }
}
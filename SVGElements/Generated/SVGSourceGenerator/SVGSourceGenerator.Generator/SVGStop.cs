using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SVGElements
{
    [System.CodeDom.Compiler.GeneratedCode("SVGSourceGenerator", "1.0")]
    public sealed partial record SVGStop
    {
        public override IEnumerable<(string propertyName, int index, object value)> GetProperties()
        {
            if (Offset is not null)
                yield return ("offset", 1, Offset);
            if (StopColor is not null)
                yield return ("stop-color", 2, StopColor);
            if (StopOpacity is not null)
                yield return ("stop-opacity", 3, StopOpacity);
            if (Id is not null)
                yield return ("id", 4, Id);
            if (TabIndex is not null)
                yield return ("tabindex", 5, TabIndex);
            if (Class is not null)
                yield return ("class", 6, Class);
            if (Style is not null)
                yield return ("style", 7, Style);
            if (PointerEvents is not null)
                yield return ("pointer-events", 8, PointerEvents);
            yield break;
        }
    }
}
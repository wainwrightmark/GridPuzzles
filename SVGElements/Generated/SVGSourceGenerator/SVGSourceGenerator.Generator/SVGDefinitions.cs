using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SVGElements
{
    [System.CodeDom.Compiler.GeneratedCode("SVGSourceGenerator", "1.0")]
    public sealed partial record SVGDefinitions
    {
        public override IEnumerable<(string propertyName, int index, object value)> GetProperties()
        {
            if (Id is not null)
                yield return ("id", 1, Id);
            if (TabIndex is not null)
                yield return ("tabindex", 2, TabIndex);
            if (Class is not null)
                yield return ("class", 3, Class);
            if (Style is not null)
                yield return ("style", 4, Style);
            if (PointerEvents is not null)
                yield return ("pointer-events", 5, PointerEvents);
            yield break;
        }
    }
}
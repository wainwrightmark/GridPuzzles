using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SVGElements
{
    [System.CodeDom.Compiler.GeneratedCode("SVGSourceGenerator", "1.0")]
    public sealed partial record SVGAnimate
    {
        public override IEnumerable<(string propertyName, int index, object value)> GetProperties()
        {
            if (AttributeName is not null)
                yield return ("attributeName", 1, AttributeName);
            if (AttributeType is not null)
                yield return ("attributeType", 2, AttributeType);
            if (From is not null)
                yield return ("from", 3, From);
            if (To is not null)
                yield return ("to", 4, To);
            if (Dur is not null)
                yield return ("dur", 5, Dur);
            if (RepeatCount is not null)
                yield return ("repeatCount", 6, RepeatCount);
            if (Fill is not null)
                yield return ("fill", 7, Fill);
            if (Values is not null)
                yield return ("values", 8, Values);
            if (KeyTimes is not null)
                yield return ("keyTimes", 9, KeyTimes);
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
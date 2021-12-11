using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SVGElements
{
    [System.CodeDom.Compiler.GeneratedCode("SVGSourceGenerator", "1.0")]
    public sealed partial record SVGAnimateTransform
    {
        public override IEnumerable<(string propertyName, int index, object value)> GetProperties()
        {
            if (AttributeName is not null)
                yield return ("attributeName", 1, AttributeName);
            if (AttributeType is not null)
                yield return ("attributeType", 2, AttributeType);
            if (Type is not null)
                yield return ("type", 3, Type);
            if (By is not null)
                yield return ("by", 4, By);
            if (Dur is not null)
                yield return ("dur", 5, Dur);
            if (RepeatCount is not null)
                yield return ("repeatCount", 6, RepeatCount);
            if (Values is not null)
                yield return ("values", 7, Values);
            if (KeyTimes is not null)
                yield return ("keyTimes", 8, KeyTimes);
            if (Id is not null)
                yield return ("id", 9, Id);
            if (TabIndex is not null)
                yield return ("tabindex", 10, TabIndex);
            if (Class is not null)
                yield return ("class", 11, Class);
            if (Style is not null)
                yield return ("style", 12, Style);
            if (PointerEvents is not null)
                yield return ("pointer-events", 13, PointerEvents);
            yield break;
        }
    }
}
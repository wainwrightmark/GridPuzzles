using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SVGElements
{
    [System.CodeDom.Compiler.GeneratedCode("SVGSourceGenerator", "1.0")]
    public sealed partial record SVG
    {
        public override IEnumerable<(string propertyName, int index, object value)> GetProperties()
        {
            if (ViewBox is not null)
                yield return ("viewBox", 1, ViewBox);
            if (PreserveAspectRatio is not null)
                yield return ("preserveAspectRatio", 2, PreserveAspectRatio);
            if (xmlns is not null)
                yield return ("xmlns", 3, xmlns);
            if (transform is not null)
                yield return ("transform", 4, transform);
            if (Width is not null)
                yield return ("width", 5, Width);
            if (Height is not null)
                yield return ("height", 6, Height);
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
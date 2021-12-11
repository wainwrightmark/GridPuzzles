using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SVGElements
{
    [System.CodeDom.Compiler.GeneratedCode("SVGSourceGenerator", "1.0")]
    public sealed partial record SVGMarker
    {
        public override IEnumerable<(string propertyName, int index, object value)> GetProperties()
        {
            if (Orient is not null)
                yield return ("orient", 1, Orient);
            if (MarkerHeight is not null)
                yield return ("markerHeight", 2, MarkerHeight);
            if (MarkerWidth is not null)
                yield return ("markerWidth", 3, MarkerWidth);
            if (MarkerUnits is not null)
                yield return ("markerUnits", 4, MarkerUnits);
            if (PreserveAspectRatio is not null)
                yield return ("preserveAspectRatio", 5, PreserveAspectRatio);
            if (RefX is not null)
                yield return ("refX", 6, RefX);
            if (RefY is not null)
                yield return ("refY", 7, RefY);
            if (Fill is not null)
                yield return ("fill", 8, Fill);
            if (Transform is not null)
                yield return ("transform", 9, Transform);
            if (Opacity is not null)
                yield return ("opacity", 10, Opacity);
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
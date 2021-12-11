using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SVGElements
{
    [System.CodeDom.Compiler.GeneratedCode("SVGSourceGenerator", "1.0")]
    public sealed partial record SVGTextSpan
    {
        public override IEnumerable<(string propertyName, int index, object value)> GetProperties()
        {
            if (X is not null)
                yield return ("x", 1, X);
            if (Y is not null)
                yield return ("y", 2, Y);
            if (DX is not null)
                yield return ("dx", 3, DX);
            if (DY is not null)
                yield return ("dy", 4, DY);
            if (TextLength is not null)
                yield return ("textLength", 5, TextLength);
            if (LengthAdjust is not null)
                yield return ("lengthAdjust", 6, LengthAdjust);
            if (FontSize is not null)
                yield return ("font-size", 7, FontSize);
            if (FontWeight is not null)
                yield return ("font-weight", 8, FontWeight);
            if (FontFamily is not null)
                yield return ("font-family", 9, FontFamily);
            if (TextAnchor is not null)
                yield return ("text-anchor", 10, TextAnchor);
            if (DominantBaseline is not null)
                yield return ("dominant-baseline", 11, DominantBaseline);
            if (Fill is not null)
                yield return ("fill", 12, Fill);
            if (Stroke is not null)
                yield return ("stroke", 13, Stroke);
            if (Opacity is not null)
                yield return ("opacity", 14, Opacity);
            if (Transform is not null)
                yield return ("transform", 15, Transform);
            if (Rotate is not null)
                yield return ("rotate", 16, Rotate);
            if (TextDecoration is not null)
                yield return ("text-decoration", 17, TextDecoration);
            if (Id is not null)
                yield return ("id", 18, Id);
            if (TabIndex is not null)
                yield return ("tabindex", 19, TabIndex);
            if (Class is not null)
                yield return ("class", 20, Class);
            if (Style is not null)
                yield return ("style", 21, Style);
            if (PointerEvents is not null)
                yield return ("pointer-events", 22, PointerEvents);
            yield break;
        }
    }
}
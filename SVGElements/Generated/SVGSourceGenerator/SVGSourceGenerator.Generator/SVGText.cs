using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SVGElements
{
    [System.CodeDom.Compiler.GeneratedCode("SVGSourceGenerator", "1.0")]
    public sealed partial record SVGText
    {
        public override IEnumerable<(string propertyName, int index, object value)> GetProperties()
        {
            if (X is not null)
                yield return ("x", 1, X);
            if (Y is not null)
                yield return ("y", 2, Y);
            if (TextLength is not null)
                yield return ("textLength", 3, TextLength);
            if (LengthAdjust is not null)
                yield return ("lengthAdjust", 4, LengthAdjust);
            if (FontSize is not null)
                yield return ("font-size", 5, FontSize);
            if (FontWeight is not null)
                yield return ("font-weight", 6, FontWeight);
            if (FontFamily is not null)
                yield return ("font-family", 7, FontFamily);
            if (TextAnchor is not null)
                yield return ("text-anchor", 8, TextAnchor);
            if (DominantBaseline is not null)
                yield return ("dominant-baseline", 9, DominantBaseline);
            if (Fill is not null)
                yield return ("fill", 10, Fill);
            if (Stroke is not null)
                yield return ("stroke", 11, Stroke);
            if (Opacity is not null)
                yield return ("opacity", 12, Opacity);
            if (Transform is not null)
                yield return ("transform", 13, Transform);
            if (Rotate is not null)
                yield return ("rotate", 14, Rotate);
            if (Id is not null)
                yield return ("id", 15, Id);
            if (TabIndex is not null)
                yield return ("tabindex", 16, TabIndex);
            if (Class is not null)
                yield return ("class", 17, Class);
            if (Style is not null)
                yield return ("style", 18, Style);
            if (PointerEvents is not null)
                yield return ("pointer-events", 19, PointerEvents);
            yield break;
        }
    }
}
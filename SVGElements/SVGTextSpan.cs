using System;
using System.Collections.Generic;

namespace SVGElements;

public sealed partial record SVGTextSpan(
        string Id,
        String Content,
        [property: SVGProperty("x")] double? X = null,
        [property: SVGProperty("y")] double? Y = null,
        [property: SVGProperty("dx")] string? DX = null,
        [property: SVGProperty("dy")] string? DY = null,
        [property: SVGProperty("textLength")] double? TextLength = null,
        [property: SVGProperty("lengthAdjust")]
        LengthAdjust? LengthAdjust = null,
        [property: SVGProperty("font-size")] string? FontSize = null,
        [property: SVGProperty("font-weight")] string? FontWeight = null,
        [property: SVGProperty("font-family")] string? FontFamily = null,
        [property: SVGProperty("text-anchor")] TextAnchor? TextAnchor = null,
        [property: SVGProperty("dominant-baseline")]
        DominantBaseline? DominantBaseline = null,
        [property: SVGProperty("fill")] string? Fill = null,
        [property: SVGProperty("stroke")] string? Stroke = null,
        [property: SVGProperty("opacity")] string? Opacity = null,
        [property: SVGProperty("transform")] string? Transform = null,
        [property: SVGProperty("rotate")] int? Rotate = null,
        [property: SVGProperty("text-decoration")] string? TextDecoration = null,
        IReadOnlyList<ISVGEventHandler>? EventHandlers = null,
        int? TabIndex = null,
        string? Class = null,
        string? Style = null,
        PointerEvents? PointerEvents = null)
    : SVGElement("tspan", Id, Content, null, EventHandlers, TabIndex, Class, Style, PointerEvents
    );
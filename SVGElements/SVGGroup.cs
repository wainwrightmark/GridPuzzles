using System.Collections.Generic;

namespace SVGElements;

public sealed partial record SVGGroup(
    string Id,
    IReadOnlyList<SVGElement> Children,
    IReadOnlyList<ISVGEventHandler>? EventHandlers = null,
        
    int? TabIndex = null,
    string? Class = null,
    string? Style = null,
    PointerEvents? PointerEvents = null,
    string? Stroke = null,
    double? StrokeWidth = null,
    StrokeLinecap? StrokeLinecap = null,
    string? StrokeDashArray = null,

    [property:SVGProperty("font-size")] string? FontSize = null,
    [property:SVGProperty("font-family")] string? FontFamily = null,
    [property:SVGProperty("text-anchor")] TextAnchor? TextAnchor = null,
    [property:SVGProperty("dominant-baseline")] DominantBaseline? DominantBaseline = null,
    [property:SVGProperty("fill")] string? Fill = null,
    [property:SVGProperty("transform")] string? Transform = null
) : SVGStrokeElement("g",
    Id,
    null,
    Children,
    EventHandlers,
        
    TabIndex,
    Class,
    Style,
    PointerEvents,
    Stroke,
    StrokeWidth,
    StrokeLinecap,
    StrokeDashArray);
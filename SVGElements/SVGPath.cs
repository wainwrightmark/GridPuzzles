using System.Collections.Generic;

namespace SVGElements;

public sealed partial record SVGPath(
    string Id,
    [property:SVGProperty("d")] string Draw,
    [property:SVGProperty("fill")] string? Fill = null,
    [property:SVGProperty("transform")] string? Transform = null,
    [property:SVGProperty("opacity")] string? Opacity = null,
    [property:SVGProperty("marker-start")] string? MarkerStart = null,
    [property:SVGProperty("marker-mid")] string? MarkerMid = null,
    [property:SVGProperty("marker-end")] string? MarkerEnd = null,

    IReadOnlyList<ISVGEventHandler>? EventHandlers = null,
        
    int? TabIndex = null,
    string? Class = null,
    string? Style = null,
    PointerEvents? PointerEvents = null,
    string? Stroke = null,
    double? StrokeWidth = null,
    StrokeLinecap? StrokeLinecap = null,
    string? StrokeDashArray = null) : SVGStrokeElement("path",
    Id,
    null,
    null,
    EventHandlers,
        
    TabIndex,
    Class,
    Style,
    PointerEvents,
    Stroke,
    StrokeWidth,
    StrokeLinecap,
    StrokeDashArray);
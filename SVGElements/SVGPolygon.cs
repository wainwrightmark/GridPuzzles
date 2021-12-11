using System.Collections.Generic;

namespace SVGElements;

public sealed partial record SVGPolygon(
    string Id,
    [property:SVGProperty("points")] string Points,
    [property:SVGProperty("pathLength")] double? PathLength,

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
    string? StrokeDashArray = null) : SVGStrokeElement("polygon",
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
using System.Collections.Generic;

namespace SVGElements;

public sealed partial record SVGCircle(
    string Id,
    [property: SVGProperty("r")] double Radius,
    [property: SVGProperty("cx")] double? CentreX = null,
    [property: SVGProperty("cy")] double? CentreY = null,
    [property: SVGProperty("fill")] string? Fill = null,
    [property: SVGProperty("transform")] string? Transform = null,
    IReadOnlyList<SVGElement>? Children = null,
    IReadOnlyList<ISVGEventHandler>? EventHandlers = null,
        
    int? TabIndex = null,
    string? Class = null,
    string? Style = null,
    PointerEvents? PointerEvents = null,
    string? Stroke = null,
    double? StrokeWidth = null,
    StrokeLinecap? StrokeLinecap = null,
    string? StrokeDashArray = null
) : SVGStrokeElement(
    "circle",
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
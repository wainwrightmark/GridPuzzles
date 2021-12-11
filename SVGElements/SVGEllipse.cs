using System.Collections.Generic;

namespace SVGElements;

public sealed partial record SVGEllipse(
    string Id,
    [property: SVGProperty("rx")] string RadiusX,
    [property: SVGProperty("ry")] string RadiusY,
    IReadOnlyList<ISVGEventHandler>? EventHandlers = null,
        
    int? TabIndex= null,
    string? Class= null,
    string? Style= null,
    PointerEvents? PointerEvents= null,
    string? Stroke= null,
    double? StrokeWidth= null,
    StrokeLinecap? StrokeLinecap= null,
    string? StrokeDashArray= null,
    [property: SVGProperty("cx")] double? CentreX= null,
    [property: SVGProperty("cy")] double? CentreY= null,
    [property: SVGProperty("fill")] string? Fill= null,
    [property: SVGProperty("transform")] string? Transform= null


) : SVGStrokeElement(
    "ellipse",
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
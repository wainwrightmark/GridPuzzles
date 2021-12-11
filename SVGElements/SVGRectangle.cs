using System.Collections.Generic;

namespace SVGElements;

public sealed partial record SVGRectangle(
    string Id,
    [property:SVGProperty("x")] double? X = null,
    [property:SVGProperty("y")] double? Y = null,
    [property:SVGProperty("rx")] double? RadiusX = null,
    [property:SVGProperty("ry")] double? RadiusY = null,
    [property:SVGProperty("width")] double? Width = null,
    [property:SVGProperty("height")] double? Height = null,

    [property:SVGProperty("fill")] string? Fill = null,

    IReadOnlyList<ISVGEventHandler>? EventHandlers = null,
    IReadOnlyList<SVGElement>? Children = null,
        
    int? TabIndex = null,
    string? Class = null,
    string? Style = null,
    PointerEvents? PointerEvents = null,
    string? Stroke = null,
    double? StrokeWidth = null,
    StrokeLinecap? StrokeLinecap = null,
    string? StrokeDashArray = null) : SVGStrokeElement("rect",
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
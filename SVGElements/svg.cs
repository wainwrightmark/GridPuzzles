using System.Collections.Generic;

namespace SVGElements;

public sealed partial record SVG(
    string Id,
    IReadOnlyList<SVGElement> Children,
    [property: SVGProperty("viewBox")]
    string? ViewBox = null,
    [property: SVGProperty("preserveAspectRatio")]
    string? PreserveAspectRatio = null,
    [property: SVGProperty("xmlns")] string? xmlns = null,
    [property: SVGProperty("transform")]
    string? transform = null,
    [property: SVGProperty("width")] double? Width = null,
    [property: SVGProperty("height")]
    double? Height = null,
        
    IReadOnlyList<ISVGEventHandler>? EventHandlers = null,
    int? TabIndex = null,
    string? Class = null,
    string? Style = null,
    PointerEvents? PointerEvents = null) : SVGElement("svg",
    Id,
    null,
    Children,
    EventHandlers,
    TabIndex,
    Class,
    Style,
    PointerEvents);
using System.Collections.Generic;

namespace SVGElements;

public sealed partial record SVGImage(
    string Id,
    [property: SVGProperty("href")] string Href,
        
    IReadOnlyList<SVGElement>? Children = null,
    IReadOnlyList<ISVGEventHandler>? EventHandlers = null,
    int? TabIndex = null,
    string? Class = null,
    string? Style = null,
    PointerEvents? PointerEvents = null,
    [property: SVGProperty("x")] double? X = null,
    [property: SVGProperty("y")] double? Y = null,
    [property: SVGProperty("width")] double? Width = null,
    [property: SVGProperty("height")] double? Height = null,
    [property: SVGProperty("transform")] string? Transform = null,
    [property: SVGProperty("opacity")] double? Opacity = null
) :
    SVGElement("image",
        Id,
        null,
        Children,
        EventHandlers,
        TabIndex,
        Class,
        Style,
        PointerEvents);
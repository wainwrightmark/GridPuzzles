using System.Collections.Generic;

namespace SVGElements;

public sealed partial record SVGMarker(
    string Id,
    [property:SVGProperty("orient")] string? Orient = null,
    [property:SVGProperty("markerHeight")] double? MarkerHeight = null,
    [property:SVGProperty("markerWidth")] double? MarkerWidth = null,
    [property:SVGProperty("markerUnits")] string? MarkerUnits = null,
    [property:SVGProperty("preserveAspectRatio")] string? PreserveAspectRatio = null,
    [property:SVGProperty("refX")] double? RefX = null,
    [property:SVGProperty("refY")] double? RefY = null,
    [property:SVGProperty("fill")] string? Fill = null,
    [property:SVGProperty("transform")] string? Transform = null,
    [property:SVGProperty("opacity")] string? Opacity = null,

        
    IReadOnlyList<SVGElement>? Children = null,
    int? TabIndex = null,
    string? Class = null,
    string? Style = null,
    PointerEvents? PointerEvents = null) : SVGElement("marker",
    Id,
    null,
    Children,
    null,
    TabIndex,
    Class,
    Style,
    PointerEvents);
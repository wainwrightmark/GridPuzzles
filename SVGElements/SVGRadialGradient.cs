using System.Collections.Generic;

namespace SVGElements;

public sealed partial record SVGRadialGradient(
    string Id,
    IReadOnlyList<SVGStop> Stops,
    [property: SVGProperty("r")] double? CircleRadius = null,
    [property: SVGProperty("cx")] double? CircleCentreX = null,
    [property: SVGProperty("cy")] double? CircleCentreY = null,
    [property: SVGProperty("fr")] double? FocusRadius = null,
    [property: SVGProperty("fx")] double? FocusCentreX = null,
    [property: SVGProperty("fy")] double? FocusCentreY = null,
    string? Class = null,
    string? Style = null) : SVGElement("radialGradient",
    Id,
    null,
    Stops,
    null,
    null,
    Class,
    Style,
    null);
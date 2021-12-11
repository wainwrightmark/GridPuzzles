using System.Collections.Generic;

namespace SVGElements;

public sealed partial record SVGLinearGradient(
    string Id,
    IReadOnlyList<SVGStop> Stops,

    [property: SVGProperty("x1")] string? XStart = null,
    [property: SVGProperty("x2")] string? XEnd = null,
    [property: SVGProperty("y1")] string? YStart = null,
    [property: SVGProperty("y2")] string? YEnd = null,
    [property: SVGProperty("gradientTransform")] string? GradientTransform = null,
    string? Class = null,
    string? Style = null) : SVGElement("linearGradient",
    Id,
    null,
    Stops,
    null,
    null,
    Class,
    Style,
    null);
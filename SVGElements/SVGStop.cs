namespace SVGElements;

public sealed partial record SVGStop(
    string Id,
    [property:SVGProperty("offset")] string? Offset = null,
    [property:SVGProperty("stop-color")] string? StopColor = null,
    [property:SVGProperty("stop-opacity")] double? StopOpacity = null,
        
    string? Class = null,
    string? Style = null) : SVGElement("stop",
    Id,
    null,
    null,
    null,
    null,
    Class,
    Style,
    null);
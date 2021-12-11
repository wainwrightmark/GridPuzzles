namespace SVGElements;

/// <summary>
/// The SVG animate element provides a way to animate an attribute of an element over time.
/// </summary>
public  sealed partial record SVGAnimate(
        string Id,
        [property: SVGProperty("attributeName")]
        string? AttributeName= null,
        [property: SVGProperty("attributeType")]
        string? AttributeType= null,
        [property: SVGProperty("from")] double? From= null,
        [property: SVGProperty("to")] double? To= null,
        [property: SVGProperty("dur")] double? Dur= null,
        [property: SVGProperty("repeatCount")] string? RepeatCount= null,
        [property: SVGProperty("fill")] string? Fill= null,
        [property: SVGProperty("values")] string? Values= null,
        [property: SVGProperty("keyTimes")] string? KeyTimes= null
    )
    : SVGElement(
        "animate",
        Id);
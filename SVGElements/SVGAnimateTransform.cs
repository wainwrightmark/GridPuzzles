namespace SVGElements;

/// <summary>
/// The animateTransform element animates a transformation attribute on its target element, thereby allowing animations to control translation, scaling, rotation, and/or skewing.
/// </summary>
public sealed partial record SVGAnimateTransform(
        string Id,
        [property: SVGProperty("attributeName")]
        string? AttributeName= null,
        [property: SVGProperty("attributeType")]
        string? AttributeType= null,
        [property: SVGProperty("type")] string? Type= null,
        [property: SVGProperty("by")] double? By= null,
        [property: SVGProperty("dur")] double? Dur= null,
        [property: SVGProperty("repeatCount")] string? RepeatCount= null,
        [property: SVGProperty("values")] string? Values= null,
        [property: SVGProperty("keyTimes")] string? KeyTimes= null
    )
    : SVGElement(
        "animate",
        Id);
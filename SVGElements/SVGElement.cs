using System.Collections.Generic;

namespace SVGElements;

/// <summary>
/// Any SVG element
/// </summary>
public abstract record SVGElement(
    string ElementName,
    //The ID must be unique
    [property: SVGProperty("id")] string Id,
    string? Content = null, //custom implementation
    IReadOnlyList<SVGElement>? Children = null,
    IReadOnlyList<ISVGEventHandler>? EventHandlers = null,
    [property: SVGProperty("tabindex")] int? TabIndex = null,
    [property: SVGProperty("class")] string? Class = null,
    [property: SVGProperty("style")] string? Style = null,
    [property: SVGProperty("pointer-events")]
    PointerEvents? PointerEvents = null)
{
    public virtual IEnumerable<(string propertyName, int index, object value)> GetProperties()
    {
        yield break;
    }
}

public abstract record SVGStrokeElement(
        string ElementName,
        string Id,
        string? Content,
        IReadOnlyList<SVGElement>? Children = null,
        IReadOnlyList<ISVGEventHandler>? EventHandlers = null,

        int? TabIndex = null,
        string? Class = null,
        string? Style = null,
        PointerEvents? PointerEvents = null,
        [property: SVGProperty("stroke")] string? Stroke = null,
        [property: SVGProperty("stroke-width")]
        double? StrokeWidth = null,
        [property: SVGProperty("stroke-linecap")]
        StrokeLinecap? StrokeLinecap = null,
        [property: SVGProperty("stroke-dasharray")]
        string? StrokeDashArray = null
    )
    : SVGElement(ElementName, Id, Content, Children, EventHandlers, TabIndex, Class, Style, PointerEvents)
{
        
}
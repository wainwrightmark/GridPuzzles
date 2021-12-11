namespace SVGElements;

public sealed partial record SVGTitle(string Id, string Content, string? Class =null, string? Style = null) 
    : SVGElement("title", Id, Content, Class: Class, Style: Style)
{

}
using System;

namespace SVGElements;

[AttributeUsage(AttributeTargets.Property)]
public class SVGPropertyAttribute : Attribute
{
    public SVGPropertyAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
}
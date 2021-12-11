using SVGElements;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Components.Rendering;

namespace SVGHelper;

public static class SvgHelper
{
    public static void Render(this SVG svgElement, int k, RenderTreeBuilder builder, object eventReceiver)
    {
        RenderElement(svgElement, k, builder, eventReceiver);
    }

    private static void RenderElement(SVGElement svgElement, int k, RenderTreeBuilder builder, object eventReceiver)
    {
        builder.OpenRegion(k);
        builder.OpenElement(17, svgElement.ElementName);
        builder.SetKey(svgElement.Id);

        foreach (var (propertyName, index, value) in svgElement.GetProperties())
        {
            object value1;

            if (value is string s && string.IsNullOrWhiteSpace(s))
                continue;

            if (value is double d)
            {
                value1 = Math.Round(d, 2);
            }
            else if (value is Enum e)
            {
                value1 = e.GetType().GetMember(e.ToString()).First().GetCustomAttribute<DisplayAttribute>()?.Name ??
                         e.ToString();
            }
            else
            {
                value1 = value;
            }

            builder.AddAttribute(index, propertyName, value1.ToString());
        }

        if (svgElement.EventHandlers is not null)
        {
            for (var eventHandlerIndex = 0; eventHandlerIndex < svgElement.EventHandlers.Count; eventHandlerIndex++)
            {
                var svgElementEventHandler = svgElement.EventHandlers[eventHandlerIndex];

                if (svgElementEventHandler is IWebSVGEventHandler webSVGEventHandler)
                {
                    webSVGEventHandler.AddEventCallback(
                        builder, 1000 + eventHandlerIndex, eventReceiver
                    );
                }
            }
        }
            

        if (svgElement.Content is not null)
        {
            builder.AddContent(2045, svgElement.Content);
        }


        if (svgElement.Children is not null)
        {
            for (var index = 0; index < svgElement.Children.Count; index++)
            {
                var svgElementChild = svgElement.Children[index];
                RenderElement(svgElementChild, 3000 + index, builder, eventReceiver);
            }
        }


        builder.CloseElement();

        builder.CloseRegion();
    }
}
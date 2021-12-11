using GridPuzzles.SVG;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using SVGHelper;

#pragma warning disable 8618

namespace GridComponents;

public sealed class GridSVGComponent : ComponentBase
{
    [Parameter] public SVGBuilder SVGBuilder { get; set; }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var svg = SVGBuilder.ComposeSVG();
        svg.Render(179, builder, this);
    }
}
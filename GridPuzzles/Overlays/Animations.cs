using SVGElements;

namespace GridPuzzles.Overlays;

public static class Animations
{
    public static readonly IReadOnlyList<SVGElement> IsSelectedOpacity = new List<SVGElement>()
    {
        new SVGAnimate("SelectedAnimation", RepeatCount:"indefinite", Values:"1;0.25;1", Dur:3,AttributeName: "opacity")
    };
    
    public static readonly IReadOnlyList<SVGElement> IsSelectedStrokeOpacity = new List<SVGElement>()
    {
        new SVGAnimate("SelectedAnimation", RepeatCount:"indefinite", Values:"1;0.25;1", Dur:3,AttributeName: "stroke-opacity")
    };
}
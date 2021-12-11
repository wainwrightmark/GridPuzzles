using System.ComponentModel.DataAnnotations;

namespace SVGElements;

public enum PointerEvents
{
    none = 0,
    [Display(Name= "bounding-box")]
    bounding_box = 1,
    visiblePainted = 2,
    visibleFill = 3,
    visibleStroke = 4,
    visible = 5,
    painted = 6,
    fill = 7,
    stroke = 8,
    all = 9
}
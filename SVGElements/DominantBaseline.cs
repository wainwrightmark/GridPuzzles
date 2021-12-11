using System.ComponentModel.DataAnnotations;

namespace SVGElements;

public enum DominantBaseline
{
    auto = 0, 
    [Display(Name="text-bottom")]
    textBottom = 1, 
        
    alphabetic = 2, ideographic = 3, middle = 4, central = 5, mathematical = 6, hanging = 7,
    [Display(Name="text-top")]
    textTop = 8
}
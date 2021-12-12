using SVGElements;


namespace Sudoku.Overlays;

public record ArrowCellOverlay(IReadOnlyList<Position> Positions, Color Color) : ICellSVGElementOverlay
{
    public string GetPointsString(double scale)
    {
        return string.Join(" ", GetPoints());


        IEnumerable<string> GetPoints()
        {
            if (Positions.Count == 1)
            {
                yield return GetPositionString(Positions.Single());
                yield break;
            }

            var (p1, p2) = Positions.GetFirstTwo();
            var p1X = (p1.GetX(true, scale));
            var p2X = (p2.GetX(true, scale));
            var p1Y = (p1.GetY(true, scale));
            var p2Y = (p2.GetY(true, scale));
            double midX;
            double midY;
            if (p1.IsOrthogonal(p2))
            {
                midX = p1X + ((p2X - p1X) * 2 / 5);
                midY = p1Y + ((p2Y - p1Y) * 2 / 5);
            }
            else
            {
                midX = p1X + ((p2X - p1X) * 3 / 10);
                midY = p1Y + ((p2Y - p1Y) * 3 / 10);
            }


            yield return $"{midX}, {midY}";

            foreach (var position in Positions.Skip(1))
            {
                yield return GetPositionString(position);
            }
        }


        string GetPositionString(Position x)
        {
            return $"{x.GetX(true, scale)}, {x.GetY(true, scale)}";
        }
    }

    /// <inheritdoc />
    public IEnumerable<SVGElement> SVGDefinitions(double scale)
    {
        yield return new SVGMarker(
            "arrowHead" + Color.ToSVGColor(),
            "auto",
            scale / 2,
            scale / 2,
            RefX: 0,
            RefY: 5,
            Children: new[]
            {
                new SVGPath("arrowHeadPath",
                    "M 0,5 H 10 L 5,10 M 10,5 L 5,0",
                    Stroke: Color.ToSVGColor(),
                    Fill: "none")
            }
        );
    }

    private static readonly IReadOnlyList<SVGElement> SelectedAnimations = new List<SVGElement>()
    {
        new SVGAnimate("SelectedAnimation", RepeatCount:"indefinite", Values:"2;3;2", Dur:2,AttributeName: "stroke-width")
    };

    /// <inheritdoc />
    public IEnumerable<SVGElement> SVGElements(double scale, bool selected)
    {
        var animations = selected ? SelectedAnimations : null;

        yield return
            new SVGCircle(
                "circle",
                scale * 2 / 5,
                CentreX: Positions.First().GetX(true, scale),
                CentreY: Positions.First().GetY(true, scale),
                Stroke: Color.ToSVGColor(),
                Fill: "none",
                Children: animations
            );

        yield return new SVGPolyLine(
            "Arrow" + Positions.ToDelimitedString(""),
            GetPointsString(scale),
            Fill: "none",
            Stroke: Color.ToSVGColor(),
            StrokeWidth: 2,
            MarkerEnd: $"url(#arrowHead{Color.ToSVGColor()})",
            PointerEvents: PointerEvents.none,
            StrokeLinecap: StrokeLinecap.round,
            Children:animations
        );
    }

    /// <inheritdoc />
    public int ZIndex => 2;
}
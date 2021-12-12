using System;
using System.Text;
using CSharpFunctionalExtensions;
using GridPuzzles.Enums;
using GridPuzzles.Overlays;
using SVGElements;

namespace Sudoku.Overlays;

public record ShapeCellOverlay(IReadOnlyList<IReadOnlyList<ValueTuple<Position, Corner>>> Positions, Position TopLeft,
    string? Text) : ICellSVGElementOverlay
{
    /// <inheritdoc />
    public IEnumerable<SVGElement> SVGDefinitions(double scale)
    {
        yield break;
    }

    /// <inheritdoc />
    public IEnumerable<SVGElement> SVGElements(double scale, bool selected)
    {
        if (Text is not null)
        {
            yield return new SVGText(
                TopLeft + "ShapeText",
                Text,
                X: TopLeft.GetX(false, scale) +(scale / 5 *1) ,
                Y: TopLeft.GetY(false, scale) +(scale / 5 * 2)
            );
        }


        string pathDraw = DrawFullPath(Positions, scale);

        yield return new SVGPath(
            TopLeft + "ShapePath",
            Draw: pathDraw,
            Fill: "none",
            PointerEvents: PointerEvents.none,
            Stroke: "black",
            StrokeWidth: 1,
            StrokeDashArray: "8 4");
    }

    static string DrawFullPath(IReadOnlyList<IReadOnlyList<ValueTuple<Position, Corner>>> positions, double scale)
    {
        var sb = new StringBuilder();
        for (var index = 0; index < positions.Count; index++)
        {
            var list = positions[index];
            if (index > 0) sb.Append(' ');
            DrawSegment(list, scale, sb);
        }

        return sb.ToString();

        static void DrawSegment(IReadOnlyList<ValueTuple<Position, Corner>> positionCorners, double scale,
            StringBuilder sb)
        {
            var coordinate0 = GetCoordinate(positionCorners[0], scale);

            sb.Append($"M {coordinate0.x} {coordinate0.y}");

            for (var i = 1; i < positionCorners.Count; i++)
            {
                var coordinate = GetCoordinate(positionCorners[i], scale);
                sb.Append($"L {coordinate.x} {coordinate.y}");
            }

            sb.Append($"L {coordinate0.x} {coordinate0.y}");

            static (double x, double y) GetCoordinate(ValueTuple<Position, Corner> positionCorner, double scale)
            {
                var x = positionCorner.Item1.GetX(false, scale);
                var y = positionCorner.Item1.GetY(false, scale);

                const int ratio = 5;
                var leftOrTop = scale / ratio;
                var rightOrBottom = scale * (ratio - 1) / ratio;

                return positionCorner.Item2 switch
                {
                    Corner.TopLeft => (x + leftOrTop, y + leftOrTop),
                    Corner.TopRight => (x + rightOrBottom, y + leftOrTop),
                    Corner.BottomRight => (x + rightOrBottom, y + rightOrBottom),
                    Corner.BottomLeft => (x + leftOrTop, y + rightOrBottom),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
    }


    public static Maybe<ShapeCellOverlay> TryMake(
        IReadOnlyList<Position> positions, string? text
    )
    {
        if (!positions.Any()) return Maybe<ShapeCellOverlay>.None;

        var sections = GetPathSections(positions);
        if (sections.HasNoValue) return Maybe<ShapeCellOverlay>.None;

        return new ShapeCellOverlay(sections.Value, positions[0], text);
    }

    private static Maybe<IReadOnlyList<IReadOnlyList<ValueTuple<Position, Corner>>>> GetPathSections(
        IReadOnlyList<Position> positions)
    {
        var remainingLocations = positions.SelectMany(position =>
                Enum.GetValues<Corner>().Select(corner => (position, corner)))
            .ToHashSet();

        var pathComponents = new List<List<ValueTuple<Position, Corner>>>();

        var current = new List<ValueTuple<Position, Corner>>();
        var clockwise = true;
        while (remainingLocations.Any())
        {
            var location = remainingLocations.First();
            remainingLocations.Remove(location);
            var pathComplete = false;
            var pathIsTrivial = true;
            while (!pathComplete)
            {
                current.Add(location);

                var preferred = GetNextOption(location, clockwise, true);
                if (preferred == current.First())
                {
                    pathComplete = true;
                    continue;
                }

                if (remainingLocations.Remove(preferred))
                {
                    location = preferred;
                    continue;
                }

                var backup = GetNextOption(location, clockwise, false);
                pathIsTrivial = false;
                if (backup == current.First())
                {
                    pathComplete = true;
                    continue;
                }

                if (remainingLocations.Remove(backup))
                {
                    location = backup;
                    continue;
                }

                return Maybe<IReadOnlyList<IReadOnlyList<ValueTuple<Position, Corner>>>>.None; //Cannot Create a path
                        
            }

            if(!pathIsTrivial || current.Count > 4)
                pathComponents.Add(current);
            current = new List<ValueTuple<Position, Corner>>();
            clockwise = false; //anticlockwise for all but the first path
        }

        return pathComponents;
    }


    public static ValueTuple<Position, Corner> GetNextOption(ValueTuple<Position, Corner> current, bool clockwise, bool preferred)
    {
        if (preferred)
        {
            if (clockwise)
            {
                return current.Item2 switch
                {
                    Corner.TopLeft => (current.Item1.GetAdjacent(CompassDirection.North), Corner.BottomLeft),
                    Corner.TopRight => (current.Item1.GetAdjacent(CompassDirection.East), Corner.TopLeft),
                    Corner.BottomLeft => (current.Item1.GetAdjacent(CompassDirection.West), Corner.BottomRight),
                    Corner.BottomRight => (current.Item1.GetAdjacent(CompassDirection.South), Corner.TopRight),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            return current.Item2 switch
            {
                Corner.TopLeft => (current.Item1.GetAdjacent(CompassDirection.West), Corner.TopRight),
                Corner.TopRight => (current.Item1.GetAdjacent(CompassDirection.North), Corner.BottomRight),
                Corner.BottomLeft => (current.Item1.GetAdjacent(CompassDirection.South), Corner.TopLeft),
                Corner.BottomRight => (current.Item1.GetAdjacent(CompassDirection.East), Corner.BottomLeft),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        if (clockwise)
        {
            return current.Item2 switch
            {
                Corner.TopLeft => (current.Item1, Corner.TopRight),
                Corner.TopRight => (current.Item1, Corner.BottomRight),
                Corner.BottomLeft => (current.Item1, Corner.TopLeft),
                Corner.BottomRight => (current.Item1, Corner.BottomLeft),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        return current.Item2 switch
        {
            Corner.TopLeft => (current.Item1, Corner.BottomLeft),
            Corner.TopRight => (current.Item1, Corner.TopLeft),
            Corner.BottomLeft => (current.Item1, Corner.BottomRight),
            Corner.BottomRight => (current.Item1, Corner.TopRight),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <inheritdoc />
    public int ZIndex => 2;
}
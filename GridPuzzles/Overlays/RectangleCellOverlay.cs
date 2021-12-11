﻿using System.Collections.Generic;
using System.Drawing;
using SVGElements;

namespace GridPuzzles.Overlays;

public record RectangleCellOverlay
    (Position TopLeftPosition, int Width, int Height, Color Color, int Thickness) : ICellSVGElementOverlay
{
    /// <inheritdoc />
    public IEnumerable<SVGElement> GetSVGDefinitions(double scale)
    {
        yield break;
    }

    /// <inheritdoc />
    public IEnumerable<SVGElement> GetSVGElements(double scale)
    {
        yield return new SVGRectangle(
            TopLeftPosition + Color.ToSVGColor() + "Rectangle" + Width + "-" + Height,

            TopLeftPosition.GetX(false, scale),
            TopLeftPosition.GetY(false, scale),
            Width: scale * Width,
            Height: scale * Height,
            PointerEvents: PointerEvents.none,
            Stroke: Color.ToSVGColor(),
            StrokeWidth: Thickness,
            Fill: "transparent"
        );
    }

    /// <inheritdoc />
    public int ZIndex => 1;
}
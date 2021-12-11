using System.Drawing;

namespace GridPuzzles.Overlays;

public record CellColorOverlay(Color Color, Position Position) : ICellOverlay;
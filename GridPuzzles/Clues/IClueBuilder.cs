using GridPuzzles.Overlays;

namespace GridPuzzles.Clues;

public interface IClueBuilder
{
    public string Name { get; }
    public int Level { get; }
    IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition);
}

public interface IClueBuilder<T, TCell> : IClueBuilder where T :struct where TCell : ICell<T, TCell>, new()
{
    public IEnumerable<IClue<T, TCell>> CreateClues(
        Position minPosition,
        Position maxPosition,
        IValueSource<T, TCell> valueSource,
        IReadOnlyCollection<IClue<T, TCell>> lowerLevelClues);
}
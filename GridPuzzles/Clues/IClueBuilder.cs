using GridPuzzles.Overlays;

namespace GridPuzzles.Clues;

public interface IClueBuilder
{
    public string Name { get; }
    public int Level { get; }
    IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition);
}

public interface IClueBuilder<T> : IClueBuilder where T: notnull
{
    public IEnumerable<IClue<T>> CreateClues(
        Position minPosition,
        Position maxPosition,
        IValueSource<T> valueSource,
        IReadOnlyCollection<IClue<T>> lowerLevelClues);
}
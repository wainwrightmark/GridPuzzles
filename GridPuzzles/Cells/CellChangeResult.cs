namespace GridPuzzles.Cells;

public interface ICellChangeResult{}

public sealed record NoChange : ICellChangeResult
{
    private NoChange() {}

    public static NoChange Instance { get; } = new();
}
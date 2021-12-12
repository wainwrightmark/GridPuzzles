namespace GridPuzzles.Reasons;

public interface IUpdateReason
{
    string Text { get; }

    IEnumerable<Position> GetContributingPositions(IGrid grid);
}
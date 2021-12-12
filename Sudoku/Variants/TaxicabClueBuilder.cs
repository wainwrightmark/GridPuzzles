namespace Sudoku.Variants;

public class TaxicabClueBuilder : NoArgumentVariantBuilder<int>
{
    public static TaxicabClueBuilder Instance = new();

    private TaxicabClueBuilder()
    {
    }

    /// <inheritdoc />
    public override string Name => "Taxicab";

    /// <inheritdoc />
    public override int Level => 2;

    /// <inheritdoc />
    public override IEnumerable<IClue<int>> CreateClues(Position minPosition, Position maxPosition, IValueSource<int> valueSource,
        IReadOnlyCollection<IClue<int>> lowerLevelClues)
    {
        return minPosition.GetPositionsBetween(maxPosition, true).SelectMany(x=>x).Select(position => new TaxicabClue(position, minPosition, maxPosition));
    }
        

    /// <inheritdoc />
    public override bool OnByDefault => false;

    /// <inheritdoc />
    public override bool IsValid(Position maxPosition)
    {
        return true;
    }
}
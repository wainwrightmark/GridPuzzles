namespace Sudoku.Variants;

public class DifferByVariantBuilder : VariantBuilder
{
    private DifferByVariantBuilder()
    {
    }

    public static VariantBuilder Instance { get; } = new DifferByVariantBuilder();

    /// <inheritdoc />
    public override string Name => "Differ By";

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder>> TryGetClueBuilders1(
        IReadOnlyDictionary<string, string> arguments)
    {
        var sr = AmountArgument.TryGetFromDictionary(arguments);
        if (sr.IsFailure) return sr.ConvertFailure<IReadOnlyCollection<IClueBuilder>>();
        var pr = PositionArgument.TryGetFromDictionary(arguments);
        if (pr.IsFailure) return pr.ConvertFailure<IReadOnlyCollection<IClueBuilder>>();

        var l = new List<IClueBuilder>
        {
            new DifferByClueBuilder(pr.Value.Min(), pr.Value.Max(), sr.Value)
        };

        return l;
    }

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments => new VariantBuilderArgument[]
    {
        AmountArgument,
        PositionArgument
    };

    private static readonly IntArgument AmountArgument = new("Amount",
        1,
        8, 1);

    private static readonly ListPositionArgument PositionArgument = new("Positions",
        2,
        2);
}

public record DifferByClueBuilder(Position Position1, Position Position2, int Amount) : IClueBuilder
{

    /// <inheritdoc />
    public string Name => $"Differ By {Amount}";

    /// <inheritdoc />
    public int Level => 2;

    /// <inheritdoc />
    public IEnumerable<IClue<int, IntCell>> CreateClues(Position minPosition, Position maxPosition,
        IValueSource valueSource,
        IReadOnlyCollection<IClue<int, IntCell>> lowerLevelClues)
    {
        yield return RelationshipClue.Create(Position1, Position2, new DifferByConstraint(Amount));
    }

    /// <param name="minPosition"></param>
    /// <param name="maxPosition"></param>
    /// <inheritdoc />
    public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
    {
        if (Amount == 1 && CellOverlays.TryCreateTwoPositionText(Position1, Position2,"⬤").TryExtract(out var co))
        {
            yield return co;
        }
        else
        {
            yield return new CellColorOverlay(Color.BlueViolet, Position1);
            yield return new CellColorOverlay(Color.BlueViolet, Position2);
        }
    }
}
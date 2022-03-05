namespace Sudoku.Variants;

public class UnconstrainedPairsDifferVariantBuilder : VariantBuilder
{
    private UnconstrainedPairsDifferVariantBuilder() { }

    public static VariantBuilder Instance { get; } = new UnconstrainedPairsDifferVariantBuilder();

    /// <inheritdoc />
    public override string Name => "Unconstrained Pairs cannot differ by";

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder>> TryGetClueBuilders1(
        IReadOnlyDictionary<string, string> arguments)
    {
        var sr = AmountArgument.TryGetFromDictionary(arguments);
        if (sr.IsFailure)
            return sr.ConvertFailure<IReadOnlyCollection<IClueBuilder>>();

        return Result.Success<IReadOnlyCollection<IClueBuilder>>(new List<IClueBuilder>()
        {
            new UnconstrainedPairsDifferClueBuilder(sr.Value)
        });

    }



    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments => new VariantBuilderArgument[]
    {
        AmountArgument
    };

    private static readonly IntArgument AmountArgument = new("Amount", 1,8, 1);
        
}

public record UnconstrainedPairsDifferClueBuilder(int Amount) : IClueBuilder
{
    /// <inheritdoc />
    public string Name => $"Unconstrained Pairs cannot differ by {Amount}";

    /// <inheritdoc />
    public int Level => 3;

    /// <inheritdoc />
    public IEnumerable<IClue<int, IntCell>> CreateClues(Position minPosition, Position maxPosition, IValueSource valueSource,
        IReadOnlyCollection<IClue<int, IntCell>> lowerLevelClues)
    {

        var constrainedPairs =
            lowerLevelClues.OfType<RelationshipClue>()
                .Where(x => x.Constraint is DifferByConstraint || x.Constraint is XTimesConstraint)
                .Select(x => (x.Positions.Min(), x.Positions.Max())).ToHashSet();

        foreach (var lowerPosition in maxPosition.GetPositionsUpTo(true).SelectMany(x => x))
        {
            var adjacentHigherPositions = lowerPosition.GetAdjacentPositions(minPosition, maxPosition)
                .Where(x => lowerPosition < x);

            foreach (var adjacent in adjacentHigherPositions)
                if (constrainedPairs.Add((lowerPosition, adjacent)))
                    yield return new RelationshipClue(lowerPosition, adjacent, new DontDifferByConstraint(Amount));
        }
    }


    /// <param name="minPosition"></param>
    /// <param name="maxPosition"></param>
    /// <inheritdoc />
    public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
    {
        yield break;
    }
}
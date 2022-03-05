namespace Sudoku.Variants;

public class UnconstrainedPairsSumVariantBuilder : VariantBuilder
{
    private UnconstrainedPairsSumVariantBuilder() {}

    public static VariantBuilder Instance { get; } = new UnconstrainedPairsSumVariantBuilder();

    /// <inheritdoc />
    public override string Name => "Unconstrained Pairs cannot add to";

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder>> TryGetClueBuilders1(
        IReadOnlyDictionary<string, string> arguments)
    {
        var sr = ValuesArgument.TryGetFromDictionary(arguments);
        if (sr.IsFailure)
            return sr.ConvertFailure<IReadOnlyCollection<IClueBuilder>>();

        var values = ImmutableSortedSet.CreateBuilder<int>();

        foreach (var term in sr.Value.Split(','))
        {
            if(int.TryParse(term.Trim(), out var r))
                values.Add(r);
            else
                return Result.Failure<IReadOnlyCollection<IClueBuilder>>($"Could not parse '{term}'");
        }

        return Result.Success<IReadOnlyCollection<IClueBuilder>>(new List<IClueBuilder>()
        {
            new UnconstrainedPairsSumClueBuilder(values.ToImmutable())
        });

    }



    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments => new VariantBuilderArgument[]
    {
        ValuesArgument
    };

    private static readonly StringArgument ValuesArgument = new("Values", "5,10");
}

[Equatable]
public partial record UnconstrainedPairsSumClueBuilder([property:SetEquality] ImmutableSortedSet<int> BadSums) : IClueBuilder
{
    /// <inheritdoc />
    public string Name => "Unconstrained Pairs Sum";

    /// <inheritdoc />
    public int Level => 3;

    /// <inheritdoc />
    public IEnumerable<IClue<int, IntCell>> CreateClues(Position minPosition, Position maxPosition, IValueSource valueSource,
        IReadOnlyCollection<IClue<int, IntCell>> lowerLevelClues)
    {
        if (!BadSums.Any()) yield break;

        var name = $"Adjacent Cells must not add to {string.Join(",", BadSums)}";

        var constrainedPairs =
            lowerLevelClues.OfType<RelationshipClue>()
                .Where(x=>x.Constraint is SumConstraint)
                .Select(x => (x.Positions.Min(), x.Positions.Max())).ToHashSet();

        foreach (var lowerPosition in maxPosition.GetPositionsUpTo(true).SelectMany(x=>x))
        {
            var adjacentHigherPositions = lowerPosition.GetAdjacentPositions(minPosition, maxPosition)
                .Where(x => lowerPosition < x);

            var dict = ImmutableDictionary<Position, int>.Empty.Add(lowerPosition, 1);

            foreach (var adjacent in adjacentHigherPositions)
                if (constrainedPairs.Add((lowerPosition, adjacent)))
                    yield return SumClue.Create(name, BadSums, false, dict.Add(adjacent, 1), false);
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
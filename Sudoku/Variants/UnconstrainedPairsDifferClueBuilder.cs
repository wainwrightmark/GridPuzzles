﻿namespace Sudoku.Variants;

public class UnconstrainedPairsDifferVariantBuilder : VariantBuilder<int>
{
    private UnconstrainedPairsDifferVariantBuilder() { }

    public static VariantBuilder<int> Instance { get; } = new UnconstrainedPairsDifferVariantBuilder();

    /// <inheritdoc />
    public override string Name => "Unconstrained Pairs cannot differ by";

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder<int>>> TryGetClueBuilders1(
        IReadOnlyDictionary<string, string> arguments)
    {
        var sr = AmountArgument.TryGetFromDictionary(arguments);
        if (sr.IsFailure)
            return sr.ConvertFailure<IReadOnlyCollection<IClueBuilder<int>>>();

        return Result.Success<IReadOnlyCollection<IClueBuilder<int>>>(new List<IClueBuilder<int>>()
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

public record UnconstrainedPairsDifferClueBuilder(int Amount) : IClueBuilder<int>
{
    /// <inheritdoc />
    public string Name => $"Unconstrained Pairs cannot differ by {Amount}";

    /// <inheritdoc />
    public int Level => 3;

    /// <inheritdoc />
    public IEnumerable<IClue<int>> CreateClues(Position minPosition, Position maxPosition, IValueSource<int> valueSource,
        IReadOnlyCollection<IClue<int>> lowerLevelClues)
    {

        var constrainedPairs =
            lowerLevelClues.OfType<RelationshipClue<int>>()
                .Where(x => x.Constraint is DifferByConstraint || x.Constraint is XTimesConstraint)
                .Select(x => (x.Positions.Min(), x.Positions.Max())).ToHashSet();

        foreach (var lowerPosition in maxPosition.GetPositionsUpTo(true).SelectMany(x => x))
        {
            var adjacentHigherPositions = lowerPosition.GetAdjacentPositions(minPosition, maxPosition)
                .Where(x => lowerPosition < x);

            foreach (var adjacent in adjacentHigherPositions)
                if (constrainedPairs.Add((lowerPosition, adjacent)))
                    yield return new RelationshipClue<int>(lowerPosition, adjacent, new DontDifferByConstraint(Amount));
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
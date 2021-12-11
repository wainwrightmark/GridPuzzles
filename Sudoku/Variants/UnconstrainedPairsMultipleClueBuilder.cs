using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using GridPuzzles;
using GridPuzzles.Clues;
using GridPuzzles.Clues.Constraints;
using GridPuzzles.Overlays;
using GridPuzzles.VariantBuilderArguments;

namespace Sudoku.Variants;

public class UnconstrainedPairsMultipleVariantBuilder : VariantBuilder<int>
{
    private UnconstrainedPairsMultipleVariantBuilder() { }

    public static VariantBuilder<int> Instance { get; } = new UnconstrainedPairsMultipleVariantBuilder();

    /// <inheritdoc />
    public override string Name => "Unconstrained Pairs cannot be multiples";

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder<int>>> TryGetClueBuilders1(
        IReadOnlyDictionary<string, string> arguments)
    {
        var sr = AmountArgument.TryGetFromDictionary(arguments);
        if (sr.IsFailure)
            return sr.ConvertFailure<IReadOnlyCollection<IClueBuilder<int>>>();

        return Result.Success<IReadOnlyCollection<IClueBuilder<int>>>(new List<IClueBuilder<int>>()
        {
            new UnconstrainedPairsMultipleClueBuilder(sr.Value)
        });
    }

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments => new VariantBuilderArgument[]
    {
        AmountArgument
    };

    private static readonly IntArgument AmountArgument = new("Amount", 1, 8, 1);
}

public class UnconstrainedPairsMultipleClueBuilder : IClueBuilder<int>
{
    public int Amount { get; }

    public UnconstrainedPairsMultipleClueBuilder(int amount) => Amount = amount;

    /// <inheritdoc />
    public string Name => $"Unconstrained Pairs cannot be {Amount} times";

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
                    yield return new RelationshipClue<int>(lowerPosition, adjacent, new NotXTimesConstraint(Amount));
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
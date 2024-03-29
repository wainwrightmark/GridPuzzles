﻿using Sudoku.Overlays;

namespace Sudoku.Variants;

public partial class ArrowVariantBuilder : VariantBuilder
{
    private ArrowVariantBuilder() { }

    public static VariantBuilder Instance { get; } = new ArrowVariantBuilder();

    /// <inheritdoc />
    public override string Name => "Arrow";
        

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder>> TryGetClueBuilders1(IReadOnlyDictionary<string, string> arguments)
    {
        var headSizeArgumentResult = HeadSizeArgument.TryGetFromDictionary(arguments);
        if (headSizeArgumentResult.IsFailure)
            return headSizeArgumentResult.ConvertFailure<IReadOnlyCollection<IClueBuilder>>();


        var positionArgumentsResult = PositionArguments.TryGetFromDictionary(arguments);
        if (positionArgumentsResult.IsFailure)
            return positionArgumentsResult.ConvertFailure<IReadOnlyCollection<IClueBuilder>>();

        var heads = positionArgumentsResult.Value.Take(headSizeArgumentResult.Value).ToImmutableArray();
        var tails = positionArgumentsResult.Value.Skip(headSizeArgumentResult.Value).ToImmutableArray();

        if(heads.Length < 1)
            return Result.Failure<IReadOnlyCollection<IClueBuilder>>("Must be at least one head cell");

        if(tails.Length < 1)
            return Result.Failure<IReadOnlyCollection<IClueBuilder>>("Must be at least one tail cell");


        var l = new List<IClueBuilder>
        {
            new ArrowClueBuilder(heads, tails)
        };

        return l;
    }


    private static readonly IntArgument HeadSizeArgument = new("Head Size", 1, 5, 1);

    private static readonly ListPositionArgument PositionArguments = new("Positions",
        2,
        9);

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments => new VariantBuilderArgument[]
    {
        HeadSizeArgument,
        PositionArguments,
    };

    [Equatable]
    private partial record ArrowClueBuilder([property:OrderedEquality] ImmutableArray<Position> HeadPositions,[property:OrderedEquality] ImmutableArray<Position> TailPositions) : IClueBuilder
    {

        /// <inheritdoc />
        public string Name => "Sum along arrow";

        /// <inheritdoc />
        public int Level => 2;

        /// <inheritdoc />
        public IEnumerable<IClue<int, IntCell>> CreateClues(Position minPosition, Position maxPosition, IValueSource valueSource,
            IReadOnlyCollection<IClue<int, IntCell>> lowerLevelClues)
        {
            //Special case for length one arrows
            if (HeadPositions.Length == 1 && TailPositions.Length == 1)
            {
                yield return new RelationshipClue(HeadPositions.Single(), TailPositions.Single(),
                    AreEqualConstraint<int>.Instance);
                yield break;
            }

            //TODO allow multipliers
            var multipliers = HeadPositions.Select(x => (x, 1))
                .Concat(TailPositions.Select(x => (x, -1)))
                .GroupBy(x => x.x, x => x.Item2)
                .Select(x => new KeyValuePair<Position, int>(x.Key, x.Sum()))
                .Where(x => x.Value != 0)
                .ToImmutableDictionary();


            yield return SumClue.Create("Arrow", ImmutableSortedSet.Create(0), true, multipliers, false);
        }


        /// <param name="minPosition"></param>
        /// <param name="maxPosition"></param>
        /// <inheritdoc />
        public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
        {
            if (HeadPositions.Length == 1)
            {
                var path = TailPositions.BuildContiguousPath(HeadPositions.Single(), true);
                if (path.HasValue)
                {
                    yield return new ArrowCellOverlay(path.Value.ToList(), Color.Gray);
                    yield break;
                }
            }

            foreach (var headPosition in HeadPositions)
                yield return new CellColorOverlay(Color.Blue, headPosition);

            foreach (var tailPosition in TailPositions)
                yield return new CellColorOverlay(Color.Red, tailPosition);
        }
    }

}
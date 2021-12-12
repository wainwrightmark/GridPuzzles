using System.Drawing;
using CSharpFunctionalExtensions;
using Generator.Equals;
using GridPuzzles.Clues.Constraints;
using GridPuzzles.Overlays;
using GridPuzzles.VariantBuilderArguments;
using Sudoku.Overlays;

namespace Sudoku.Variants;

public partial class ArrowVariantBuilder : VariantBuilder<int>
{
    private ArrowVariantBuilder() { }

    public static VariantBuilder<int> Instance { get; } = new ArrowVariantBuilder();

    /// <inheritdoc />
    public override string Name => "Arrow";
        

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder<int>>> TryGetClueBuilders1(IReadOnlyDictionary<string, string> arguments)
    {
        var headSizeArgumentResult = HeadSizeArgument.TryGetFromDictionary(arguments);
        if (headSizeArgumentResult.IsFailure)
            return headSizeArgumentResult.ConvertFailure<IReadOnlyCollection<IClueBuilder<int>>>();


        var positionArgumentsResult = PositionArguments.TryGetFromDictionary(arguments);
        if (positionArgumentsResult.IsFailure)
            return positionArgumentsResult.ConvertFailure<IReadOnlyCollection<IClueBuilder<int>>>();

        var heads = positionArgumentsResult.Value.Take(headSizeArgumentResult.Value).ToImmutableList();
        var tails = positionArgumentsResult.Value.Skip(headSizeArgumentResult.Value).ToImmutableList();

        if(heads.Count < 1)
            return Result.Failure<IReadOnlyCollection<IClueBuilder<int>>>("Must be at least one head cell");

        if(tails.Count < 1)
            return Result.Failure<IReadOnlyCollection<IClueBuilder<int>>>("Must be at least one tail cell");


        var l = new List<IClueBuilder<int>>
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
    private partial record ArrowClueBuilder([property:OrderedEquality] ImmutableList<Position> HeadPositions,[property:OrderedEquality] ImmutableList<Position> TailPositions) : IClueBuilder<int>
    {

        /// <inheritdoc />
        public string Name => "Sum along arrow";

        /// <inheritdoc />
        public int Level => 2;

        /// <inheritdoc />
        public IEnumerable<IClue<int>> CreateClues(Position minPosition, Position maxPosition, IValueSource<int> valueSource,
            IReadOnlyCollection<IClue<int>> lowerLevelClues)
        {
            //Special case for length one arrows
            if (HeadPositions.Count == 1 && TailPositions.Count == 1)
            {
                yield return new RelationshipClue<int>(HeadPositions.Single(), TailPositions.Single(),
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


            yield return SumClue.Create("Arrow", ImmutableSortedSet.Create(0), true, multipliers);
        }


        /// <param name="minPosition"></param>
        /// <param name="maxPosition"></param>
        /// <inheritdoc />
        public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
        {
            if (HeadPositions.Count == 1)
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
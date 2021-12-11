using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using CSharpFunctionalExtensions;
using GridPuzzles;
using GridPuzzles.Clues;
using GridPuzzles.Clues.Constraints;
using GridPuzzles.Overlays;
using GridPuzzles.VariantBuilderArguments;

namespace Sudoku.Variants;

public class ConsecutiveGroupVariantBuilder : VariantBuilder<int>
{
    private ConsecutiveGroupVariantBuilder()
    {
    }

    public static VariantBuilder<int> Instance { get; } = new ConsecutiveGroupVariantBuilder();

    /// <inheritdoc />
    public override string Name => "Consecutive Group";


    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder<int>>> TryGetClueBuilders1(
        IReadOnlyDictionary<string, string> arguments)
    {
        var pr = PositionArgument.TryGetFromDictionary(arguments);
        if (pr.IsFailure) return pr.ConvertFailure<IReadOnlyCollection<IClueBuilder<int>>>();


        var l = new List<IClueBuilder<int>>
        {
            new ConsecutiveGroupClueBuilder(pr.Value)
        };

        return l;
    }
        

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments => new VariantBuilderArgument[]
    {
        PositionArgument
    };

    private static readonly ListPositionArgument PositionArgument = new("Positions",
        2,
        8);

    public class ConsecutiveGroupClueBuilder : IClueBuilder<int>
    {
        public ConsecutiveGroupClueBuilder(IEnumerable<Position> positions)
        {
            Positions = positions.Distinct().ToImmutableList();
        }

        public ImmutableList<Position> Positions { get; }

        /// <inheritdoc />
        public string Name => "Consecutive Group";

        /// <inheritdoc />
        public int Level => 1;

        /// <inheritdoc />
        public IEnumerable<IClue<int>> CreateClues(Position minPosition, Position maxPosition,
            IValueSource<int> valueSource,
            IReadOnlyCollection<IClue<int>> lowerLevelClues)
        {
            var amount = Positions.Count - 1;
            var constraint = new DifferByAtMostConstraint(amount);

            yield return new UniquenessClue<int>(Positions.ToImmutableSortedSet(), "Consecutive Group");

            foreach (var (p, q) in Positions.SelectMany(p => Positions.Select(q => (p, q))).Where(x => x.p != x.q))
            {
                yield return new RelationshipClue<int>(p, q, constraint);
            }
        }

        /// <param name="minPosition"></param>
        /// <param name="maxPosition"></param>
        /// <inheritdoc />
        public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
        {
            var path = Positions.BuildContiguousPath(true);

            if (path.HasValue)
            {
                yield return new LineCellOverlay(path.Value.ToList(), Color.Violet);
            }
            else
            {
                foreach (var position in Positions)
                {
                    yield return new CellColorOverlay(Color.Violet, position);
                }
            }
        }
    }
}
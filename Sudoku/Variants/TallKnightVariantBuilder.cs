using System.Collections.Generic;
using System.Linq;
using GridPuzzles;
using GridPuzzles.Clues;
using GridPuzzles.VariantBuilderArguments;

namespace Sudoku.Variants;

public class TallKnightVariantBuilder<T> : MutexVariantBuilder<T>where T : notnull
{
    public static TallKnightVariantBuilder<T> Instance = new();

    private TallKnightVariantBuilder()
    {
    }

    public override string Name => "Tall Knight";

    /// <inheritdoc />
    public override int Level => 3;

    /// <inheritdoc />
    protected override IEnumerable<UniquenessClue<T>> GetClues(Position minPosition, Position maxPosition)
    {
        var positionsChecked = new HashSet<Position>();

        foreach (var position in minPosition.GetPositionsBetween(maxPosition, true).SelectMany(x=>x))
        {

            positionsChecked.Add(position);

            var newTallKnightPositions =
                GetTallKnightAdjacentPositions(position, minPosition, maxPosition)
                    .Where(x => !positionsChecked.Contains(x));

            foreach (var p2 in newTallKnightPositions)
                yield return new UniquenessClue<T>(position, p2, "tall knight's move");
        }

    }

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments { get; } = new List<VariantBuilderArgument>();
        
    public static IEnumerable<Position> GetTallKnightAdjacentPositions(Position position, Position min, Position max)
    {
        var positions = TallKnightModifiers.Select(m =>
                (c: position.Column - m.col, r: position.Row - m.row))
            .Where(x => x.c >= min.Column && x.c <= max.Column)
            .Where(x => x.r >= min.Row && x.r <= max.Row)
            .Select(x => new Position(x.c, x.r));

        return positions;
    }

    // ReSharper disable once StaticMemberInGenericType
    private static readonly IReadOnlyCollection<(int col, int row)> TallKnightModifiers =
        new List<(int col, int row)>
        {
            (-3,1), (-3,-1), (-1,-3), (-1,3), (1,-3), (1,3), (3,1), (3,-1)
        };

}
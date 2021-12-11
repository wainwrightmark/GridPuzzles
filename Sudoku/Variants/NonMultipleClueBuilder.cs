using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GridPuzzles;
using GridPuzzles.Clues;
using GridPuzzles.Clues.Constraints;
using Sudoku.Clues;

namespace Sudoku.Variants;

public class NonMultipleClueBuilder : NoArgumentVariantBuilder<int>
{
    public static readonly NonMultipleClueBuilder Instance = new();

    private NonMultipleClueBuilder()
    {
    }

    /// <inheritdoc />
    public override string Name => "Non Multiple";

    /// <inheritdoc />
    public override  int Level => 4;

    /// <inheritdoc />
    public override IEnumerable<IClue<int>> CreateClues(Position minPosition, Position maxPosition, IValueSource<int> valueSource,
        IReadOnlyCollection<IClue<int>> lowerLevelClues)
    {
        var positionsChecked = new HashSet<Position>();

        foreach (var position in minPosition.GetPositionsBetween(maxPosition, true).SelectMany(x=>x))
        {
            positionsChecked.Add(position);

            var adjacentPositions = position.GetAdjacentPositions(minPosition, maxPosition)
                .Where(x => !positionsChecked.Contains(x));

            foreach (var adjacent in adjacentPositions)
            {
                yield return RelationshipClue<int>.Create(position, adjacent, NonMultipleConstraint.Instance);
            }
        }
    }

    /// <inheritdoc />
    public override bool OnByDefault => false;

    /// <inheritdoc />
    public override bool IsValid(Position maxPosition) => true;


        

}
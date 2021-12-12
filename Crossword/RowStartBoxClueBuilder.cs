using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GridPuzzles;
using GridPuzzles.Clues;

namespace Crossword;

public class RowStartBoxClueBuilder : NoArgumentVariantBuilder<char>
{
    private RowStartBoxClueBuilder()
    {
    }

    public static RowStartBoxClueBuilder Instance { get; } = new();

    /// <inheritdoc />
    public override string Name => "Start Boxes";

    /// <inheritdoc />
    public override int Level => 2;

    /// <inheritdoc />
    public override IEnumerable<IClue<char>> CreateClues(Position minPosition, Position maxPosition, IValueSource<char> valueSource,
        IReadOnlyCollection<IClue<char>> lowerLevelClues)
    {
        var blocks = lowerLevelClues.OfType<BlockClue>().SelectMany(x => x.Positions).ToImmutableHashSet();

        var positionLists = new List<(Position p1 , Position p2, Position p3)>();

        for (int column = minPosition.Column; column <= maxPosition.Column; column++)
        {
            positionLists.Add((new Position(column, 1),new Position(column, 2),new Position(column, 3)));
            positionLists.Add((new Position(column, maxPosition.Row),new Position(column, maxPosition.Row - 1),new Position(column, maxPosition.Row - 2)));
        }

        for (int row = minPosition.Row; row <= maxPosition.Row; row++)
        {
            positionLists.Add((new Position(1, row),new Position(2, row),new Position(3, row)));
            positionLists.Add((new Position(maxPosition.Column, row),new Position(maxPosition.Column - 1, row), new Position(maxPosition.Column - 2, row)));
        }

        foreach (var (p1, p2, p3) in positionLists)
        {
            if(!blocks.Contains(p1) && !blocks.Contains(p2) && !blocks.Contains(p3))
                yield return new RowStartBoxesClue(p1,p2,p3);
        }
    }

    /// <inheritdoc />
    public override bool OnByDefault => true;

    /// <inheritdoc />
    public override bool IsValid(Position maxPosition)
    {
        return true;
    }
}
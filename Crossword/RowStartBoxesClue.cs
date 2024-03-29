﻿namespace Crossword;

public class RowStartBoxesClue : IRuleClue
{
    public RowStartBoxesClue(Position position1, Position position2, Position position3)
    {
        Position1 = position1;
        Position2 = position2;
        Position3 = position3;
        Positions = new[] {Position1, Position2, Position3}.ToImmutableSortedSet();
    }

    public Position Position1 { get; }
    public Position Position2 { get; }
    public Position Position3 { get; }


    /// <inheritdoc />
    public string Name => "Row start blocks";

    /// <inheritdoc />
    public ImmutableSortedSet<Position> Positions { get; }

    /// <inheritdoc />
    public IEnumerable<ICellChangeResult> CalculateCellUpdates(Grid grid)
    {
        var cell1 = grid.GetCellKVP(Position1);
        var cell2 = grid.GetCellKVP(Position2);
        var cell3 = grid.GetCellKVP(Position3);

        if (cell3.Value.HasSingleValue()&& cell3.Value.Single() == CrosswordValueSource.BlockChar)
        {
            yield return (cell2.CloneWithOnlyValue(CrosswordValueSource.BlockChar, new CrosswordReason("Words must be at least 3 characters")));
            yield return (cell1.CloneWithOnlyValue(CrosswordValueSource.BlockChar, new CrosswordReason("Words must be at least 3 characters")));
        }
        else
        {
            if (cell2.Value.HasSingleValue()&& cell2.Value.Single() == CrosswordValueSource.BlockChar)
            {
                yield return (cell1.CloneWithOnlyValue(CrosswordValueSource.BlockChar, new CrosswordReason("Words must be at least 3 characters")));
            }
            else if (!cell2.Value.Contains(CrosswordValueSource.BlockChar))
            {
                yield return (cell3.CloneWithoutValue(CrosswordValueSource.BlockChar, new CrosswordReason("Words must be at least 3 characters")));
            }
            else if (!cell1.Value.Contains(CrosswordValueSource.BlockChar))
            {
                yield return (cell1.CloneWithoutValue(CrosswordValueSource.BlockChar, new CrosswordReason("Words must be at least 3 characters")));
                yield return (cell2.CloneWithoutValue(CrosswordValueSource.BlockChar, new CrosswordReason("Words must be at least 3 characters")));
            }
        }
    }
}
﻿namespace Crossword;

public class BlockClue : IRuleClue
{
    public BlockClue(ImmutableSortedSet<Position> positions)
    {
        Positions = positions;
    }

    /// <inheritdoc />
    public string Name => "Crossword Blocks";

    /// <inheritdoc />
    public ImmutableSortedSet<Position> Positions { get; }

    /// <inheritdoc />
    public IEnumerable<ICellChangeResult> CalculateCellUpdates(Grid grid)
    {
        var updates = Positions.Select(grid.GetCellKVP)
            .Select(x => x.CloneWithOnlyValue(CrosswordValueSource.BlockChar,
                    
                new CrosswordReason("Must be a block"))).ToList();

        foreach (var cellChangeResult in updates) yield return (cellChangeResult);
    }
}
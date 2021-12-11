using System;
using System.Collections.Generic;
using System.Linq;
using GridPuzzles.Cells;
using GridPuzzles.Reasons;

namespace GridPuzzles;

public sealed record Contradiction(IUpdateReason Reason, IReadOnlyList<Position> Positions) : ICellChangeResult
{
    /// <inheritdoc />
    public override string ToString()
    {
        if (Positions.Count == 1)
            return Reason.Text + " " + string.Join(", ", Positions);
        else
            return Reason.Text;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        if (!Positions.Any())
            return 0;

        return HashCode.Combine(Positions.Count, Positions.First());
    }

    /// <inheritdoc />
    public bool Equals(Contradiction? other)
    {
        return other is not null && Positions.SequenceEqual(other.Positions);
    }
}
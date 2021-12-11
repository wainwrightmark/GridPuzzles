using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MoreLinq;

namespace GridPuzzles.Reasons;

public sealed record ReasonList(ImmutableArray<ISingleReason> Reasons) : IUpdateReason
{
    /// <inheritdoc />
    public string Text => Reasons.Select(x=>x.Text).ToDelimitedString(", ");

    /// <inheritdoc />
    public IEnumerable<Position> GetContributingPositions(IGrid grid)
    {
        return Reasons.SelectMany(x => x.GetContributingPositions(grid)).Distinct();
    }

    public ReasonList Combine(IUpdateReason reason)
    {
        return reason switch
        {
            ReasonList rl => new ReasonList(Reasons.AddRange(rl.Reasons)),
            ISingleReason sr => new ReasonList(Reasons.Add(sr)),
            _ => throw new ArgumentOutOfRangeException(nameof(reason))
        };
    }
}
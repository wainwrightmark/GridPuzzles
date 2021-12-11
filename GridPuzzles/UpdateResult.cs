using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using GridPuzzles.Cells;
using GridPuzzles.Reasons;
using MoreLinq;

namespace GridPuzzles;

public interface IUpdateResult
{
    bool HasContradictions { get; }
    bool IsNotEmpty { get; }

    IEnumerable<Position> GetContradictionPositions(IGrid grid);
    IEnumerable<Position> UpdatedPositions { get; }


    IEnumerable<Position> GetContributingPositions(IGrid grid);


    /// <summary>
    /// Human readable message
    /// </summary>
    string Message { get; }

    IReadOnlyDictionary<Position, IUpdateReason> GetPositionReasons();
}


public sealed record UpdateResult<T>(ImmutableDictionary<Position, CellUpdate<T>> UpdatedCells,
        ImmutableHashSet<Contradiction> Contradictions)
    : IUpdateResult
{
    public bool IsNotEmpty => UpdatedCells.Any() || Contradictions.Any();
    public bool HasContradictions => Contradictions.Any();

    /// <inheritdoc />
    public IEnumerable<Position> GetContradictionPositions(IGrid grid)
    {
        return Contradictions.SelectMany(x => x.Reason.GetContributingPositions(grid)).Distinct();
    }

    /// <inheritdoc />
    public IEnumerable<Position> UpdatedPositions => UpdatedCells.Keys;


    public string Message
    {
        get
        {
            if (IsEmpty)
                return "No updates";

            if (Contradictions.Any())
                return Contradictions.First().ToString();

            string updatedCellsTerm;

            if (!UpdatedCells.Any())
                updatedCellsTerm = "";
            else if (UpdatedCells.Count == 1)
            {
                var (position, cellUpdate) = UpdatedCells.First();
                updatedCellsTerm = position + ": " + cellUpdate.NewCell + " - " + UpdatedCells.First().Value.Reason.Text;
            }
            else if (UpdatedCells.Select(x => x.Value.Reason).Distinct().CountBetween(1, 1))
            {
                updatedCellsTerm = $"{UpdatedCells.Count} updated cells" + " - " +
                                   UpdatedCells.First().Value.Reason.Text;
            }
            else
                updatedCellsTerm = $"{UpdatedCells.Count} updated cells";


            return $"{updatedCellsTerm}";
        }
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<Position, IUpdateReason> GetPositionReasons()
    {
        var messages =
            Contradictions.SelectMany(c => c.Positions.Select(p => (p, c.Reason)))
                .Concat(UpdatedCells.Values.Select(c => (c.Position, c.Reason)))
                .GroupBy(x => x.Item1, x => x.Reason)
                .ToDictionary(x => x.Key, 
                    x =>
                        x.Aggregate((a, b) => a.Combine(b)));

        return messages;
    }


    public static UpdateResult<T> Empty { get; } = new(ImmutableDictionary<Position, CellUpdate<T>>.Empty,
        ImmutableHashSet<Contradiction>.Empty);


    /// <inheritdoc />
    public override string ToString() => Message;


    [Pure] public bool IsEmpty => !(UpdatedCells.Any() || Contradictions.Any());

    [Pure]
    public UpdateResult<T> CloneWithContradiction(Contradiction c) =>
        Contradictions.Contains(c) ? this : new UpdateResult<T>(UpdatedCells, Contradictions.Add(c));

    [Pure]
    public UpdateResult<T> CloneWithCellChangeResult(ICellChangeResult r)
    {
        return r switch
        {
            NoChange => this,
            CellUpdate<T> cellUpdate => CloneWithCellUpdate(cellUpdate),
            Contradiction contradiction => CloneWithContradiction(contradiction),
            _ => throw new ArgumentOutOfRangeException(nameof(r))
        };
    }

    [Pure]
    public UpdateResult<T> CloneWithCellUpdate(CellUpdate<T> update)
    {
        if (!UpdatedCells.TryGetValue(update.Position, out var existing))
            return new UpdateResult<T>(UpdatedCells.Add(update.Position, update), Contradictions);

        var ccr = existing.TryCombine(update);

        return ccr switch
        {
            NoChange => this,
            CellUpdate<T> cellUpdate when cellUpdate.NewCell.PossibleValues.Count ==
                                          existing.NewCell.PossibleValues.Count => this,
            CellUpdate<T> cellUpdate => new UpdateResult<T>(UpdatedCells.SetItem(update.Position, cellUpdate),
                Contradictions),
            Contradiction contradiction => CloneWithContradiction(contradiction),
            _ => throw new ArgumentOutOfRangeException(nameof(ccr))
        };
    }

    [Pure]
    public UpdateResult<T> Combine(UpdateResult<T> other, out bool hasChanges)
    {
        hasChanges = false;
        if (other.IsEmpty) return this;
        if (IsEmpty)
        {
            hasChanges = true;
            return other;
        }

        ImmutableHashSet<Contradiction>.Builder contradictionsBuilder;

            
        var contradictionsChanged = false;

        if (Contradictions.IsEmpty)
        {
            contradictionsBuilder = other.Contradictions.ToBuilder();
            contradictionsChanged = other.Contradictions.Any();
        }
        else
        {
            contradictionsBuilder = Contradictions.ToBuilder();

            foreach (var c in other.Contradictions)
                contradictionsChanged |= contradictionsBuilder.Add(c);
        }

        ImmutableDictionary<Position, CellUpdate<T>> newUpdatedCells = 
            CombineUpdatedCells(other, contradictionsBuilder, ref hasChanges, ref contradictionsChanged);
            
        ImmutableHashSet<Contradiction> newContradictions =
            contradictionsChanged ? contradictionsBuilder.ToImmutable() : Contradictions;


        if (hasChanges || contradictionsChanged)
            return new UpdateResult<T>(newUpdatedCells, newContradictions);

        return this;
    }



    private ImmutableDictionary<Position, CellUpdate<T>> CombineUpdatedCells(UpdateResult<T> other,
        ImmutableHashSet<Contradiction>.Builder contradictionsBuilder,
        ref bool hasChanges,
        ref bool contradictionsChanged
    )
    {
        ImmutableDictionary<Position, CellUpdate<T>> newUpdatedCells;
        if (other.UpdatedCells.IsEmpty)
            newUpdatedCells = UpdatedCells;
        else if (UpdatedCells.IsEmpty)
        {
            newUpdatedCells = other.UpdatedCells;
            hasChanges = true;
        }
        else
        {
            var updateBuilder = UpdatedCells.ToBuilder();

            foreach (var otherUpdatedCell in other.UpdatedCells)
            {
                if (UpdatedCells.TryGetValue(otherUpdatedCell.Key, out var thisUpdatedCell))
                {
                    var ccr = thisUpdatedCell.TryCombine(otherUpdatedCell.Value);

                    switch (ccr)
                    {
                        case CellUpdate<T> cellUpdate:
                        {
                            var newCell = cellUpdate.NewCell;
                            if (newCell.PossibleValues.Count != thisUpdatedCell.NewCell.PossibleValues.Count)
                            {
                                updateBuilder[otherUpdatedCell.Key] = cellUpdate;
                                hasChanges = true;
                            }

                            break;
                        }
                        case Contradiction contradiction:
                        {
                            contradictionsChanged |= contradictionsBuilder.Add(contradiction);
                            break;
                        }
                                
                    }
                }
                else
                {
                    updateBuilder.Add(otherUpdatedCell);
                    hasChanges = true;
                }
            }

            newUpdatedCells = updateBuilder.ToImmutable();
        }

        return newUpdatedCells;
    }


    /// <inheritdoc />
    public IEnumerable<Position> GetContributingPositions(IGrid grid)
    {
        return UpdatedCells.Select(x => x.Value.Reason)
            .Concat(Contradictions.Select(x => x.Reason))
            .Distinct()
            .SelectMany(x => x.GetContributingPositions(grid))
            .Distinct()
            .Except(UpdatedPositions)
            .Except(GetContradictionPositions(grid));
    }
}
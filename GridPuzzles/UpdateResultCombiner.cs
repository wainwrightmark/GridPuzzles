using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GridPuzzles.Bifurcation;
using GridPuzzles.Cells;

namespace GridPuzzles;

public abstract record UpdateResultCombiner<T> where T : notnull
{
    public abstract UpdateResult<T> Combine(IEnumerable<ICellChangeResult> cellChangeResults);

    public abstract BifurcationResult<T> ToBifurcationResult(BifurcationNode<T> node);

    /// <summary>
    /// Gets full detail on contradictions
    /// </summary>
    private sealed record DefaultCombiner : UpdateResultCombiner<T>
    {
        private DefaultCombiner() {}

        public static DefaultCombiner Instance { get; } = new();

        /// <inheritdoc />
        public override UpdateResult<T> Combine(IEnumerable<ICellChangeResult> cellChangeResults)
        {
            var cellUpdates = ImmutableDictionary.CreateBuilder<Position, CellUpdate<T>>();
            var contradictions = ImmutableHashSet.CreateBuilder<Contradiction>();

            foreach (var cellChangeResult in cellChangeResults)
            {
                switch (cellChangeResult)
                {
                    case CellUpdate<T> update:
                    {
                        if (!cellUpdates.TryAdd(update.Position, update))
                        {
                            var old = cellUpdates[update.Position];
                            var combineResult = old.TryCombine(update);
                            if (combineResult is Contradiction contradiction)
                                contradictions.Add(contradiction);
                            if (combineResult is CellUpdate<T> combinedUpdate &&
                                combinedUpdate.NewCell.PossibleValues.Count != old.NewCell.PossibleValues.Count)
                                cellUpdates[update.Position] = combinedUpdate;
                        }

                        break;
                    }
                    case Contradiction contradiction:
                    {
                        contradictions.Add(contradiction);
                        break;
                    }
                }
            }

            if (contradictions.Any())
                return new UpdateResult<T>(ImmutableDictionary<Position, CellUpdate<T>>.Empty,
                    contradictions.ToImmutable());


            return new UpdateResult<T>(
                cellUpdates.ToImmutable(),
                contradictions.ToImmutable()
            );
        }

        /// <inheritdoc />
        public override BifurcationResult<T> ToBifurcationResult(BifurcationNode<T> node)
        {
            return SingleStep.ToBifurcationResult(node);
        }
    }

    private sealed record SingleStepCombiner : UpdateResultCombiner<T>
    {
        private SingleStepCombiner()
        {
        }

        public static SingleStepCombiner Instance { get; } = new();

        /// <inheritdoc />
        public override UpdateResult<T> Combine(IEnumerable<ICellChangeResult> cellChangeResults)
        {
            Dictionary<Position, CellUpdate<T>> dictionary = new();
            var contradictions = ImmutableHashSet<Contradiction>.Empty.ToBuilder();

            foreach (var cellChangeResult in cellChangeResults)
            {
                switch (cellChangeResult)
                {
                    case CellUpdate<T> update:
                    {
                        if (!dictionary.TryAdd(update.Position, update))
                        {
                            var old = dictionary[update.Position];
                            var combineResult = old.TryCombine(update);
                            if (combineResult is Contradiction contradiction)
                                contradictions.Add(contradiction);
                            if (combineResult is CellUpdate<T> combinedUpdate &&
                                combinedUpdate.NewCell.PossibleValues.Count != old.NewCell.PossibleValues.Count)
                                dictionary[update.Position] = combinedUpdate;
                        }

                        break;
                    }
                    case Contradiction contradiction:
                    {
                        contradictions.Add(contradiction);
                        break;
                    }
                }
            }


            if (contradictions.Any())
                return new UpdateResult<T>(ImmutableDictionary<Position, CellUpdate<T>>.Empty,
                    contradictions.ToImmutable());

            if(!dictionary.Any())
                return UpdateResult<T>.Empty;

            var bestUpdate =
                dictionary.GroupBy(x => x.Value.NewCell.PossibleValues.Count)
                    .OrderBy(x => x.Key)
                    .ThenBy(x => x.Count())
                    .First().First();

            if (bestUpdate.Value.NewCell.PossibleValues.Count <= 1)
            { //just return the one cell
                return new UpdateResult<T>(
                    ImmutableDictionary<Position, CellUpdate<T>>.Empty.Add(bestUpdate.Key, bestUpdate.Value),
                    contradictions.ToImmutable()
                );
            }
            else
            {
                var sameUpdates = dictionary
                    .Where(x => x.Value.Reason.Equals(bestUpdate.Value.Reason))
                    .ToImmutableDictionary();

                return new UpdateResult<T>(
                    sameUpdates,
                    contradictions.ToImmutable()
                );
            }

                
        }

        /// <inheritdoc />
        public override BifurcationResult<T> ToBifurcationResult(BifurcationNode<T> node)
        {
            if (node.UpdateResult.HasContradictions) return Fast.ToBifurcationResult(node);

            if (node.UpdateResult.UpdatedCells.Count <= 1)
                return Fast.ToBifurcationResult(node);

            var best = node.UpdateResult.UpdatedCells
                .OrderBy(x => x.Value.NewCell.PossibleValues.Count)
                .First();

            var newUpdateResult = new UpdateResult<T>(
                ImmutableDictionary<Position, CellUpdate<T>>.Empty.Add(best.Key, best.Value),

                ImmutableHashSet<Contradiction>.Empty);

            return new BifurcationResult<T>(newUpdateResult, null, node.LevelsDescended);
        }
    }

    public static UpdateResultCombiner<T> Fast => FastCombiner.Instance;
    public static UpdateResultCombiner<T> Default => DefaultCombiner.Instance;
    public static UpdateResultCombiner<T> SingleStep => SingleStepCombiner.Instance;

    /// <summary>
    /// Stops on first contradiction
    /// </summary>
    private sealed record FastCombiner : UpdateResultCombiner<T>
    {
        private FastCombiner() { }

        public static FastCombiner Instance { get; } = new();

        public override UpdateResult<T> Combine(IEnumerable<ICellChangeResult> cellChangeResults)
        {
            Dictionary<Position, CellUpdate<T>> dictionary = new();

            foreach (var cellChangeResult in cellChangeResults)
            {
                switch (cellChangeResult)
                {
                    case CellUpdate<T> update:
                    {
                        if (!dictionary.TryAdd(update.Position, update))
                        {
                            var old = dictionary[update.Position];
                            var combineResult = old.TryCombine(update);
                            if (combineResult is Contradiction contradiction)
                                return new UpdateResult<T>(dictionary.ToImmutableDictionary(),
                                    ImmutableHashSet<Contradiction>.Empty
                                        .Add(contradiction));

                            if (combineResult is CellUpdate<T> combinedUpdate &&
                                combinedUpdate.NewCell.PossibleValues.Count != old.NewCell.PossibleValues.Count)
                                dictionary[update.Position] = combinedUpdate;
                        }

                        break;
                    }
                    case Contradiction contradiction:
                    {
                        //Stop Immediately
                        return new UpdateResult<T>(dictionary.ToImmutableDictionary(),
                            ImmutableHashSet<Contradiction>.Empty
                                .Add(contradiction));
                    }
                }
            }


            return new UpdateResult<T>(
                dictionary.ToImmutableDictionary(),
                ImmutableHashSet<Contradiction>.Empty
            );
        }

        /// <inheritdoc />
        public override BifurcationResult<T> ToBifurcationResult(BifurcationNode<T> node)
        {
            return new BifurcationResult<T>(node.UpdateResult, node.CompleteGrids, node.LevelsDescended);
        }
    }
}
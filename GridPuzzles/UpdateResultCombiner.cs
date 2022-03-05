using GridPuzzles.Bifurcation;

namespace GridPuzzles;

public abstract record UpdateResultCombiner<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    public abstract UpdateResult<T, TCell> Combine(IEnumerable<ICellChangeResult> cellChangeResults);

    public abstract BifurcationResult<T, TCell> ToBifurcationResult(BifurcationNode<T, TCell> node);

    /// <summary>
    /// Gets full detail on contradictions
    /// </summary>
    private sealed record DefaultCombiner : UpdateResultCombiner<T, TCell>
    {
        private DefaultCombiner() {}

        public static DefaultCombiner Instance { get; } = new();

        /// <inheritdoc />
        public override UpdateResult<T, TCell> Combine(IEnumerable<ICellChangeResult> cellChangeResults)
        {
            var cellUpdates = ImmutableDictionary.CreateBuilder<Position, CellUpdate<T, TCell>>();
            var contradictions = ImmutableHashSet.CreateBuilder<Contradiction>();

            foreach (var cellChangeResult in cellChangeResults)
            {
                switch (cellChangeResult)
                {
                    case CellUpdate<T, TCell> update:
                    {
                        if (!cellUpdates.TryAdd(update.Position, update))
                        {
                            var old = cellUpdates[update.Position];
                            var combineResult = old.TryCombine(update);
                            if (combineResult is Contradiction contradiction)
                                contradictions.Add(contradiction);
                            if (combineResult is CellUpdate<T, TCell> combinedUpdate &&
                                !combinedUpdate.NewCell.Equals(old.NewCell))
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
                return new UpdateResult<T, TCell>(ImmutableDictionary<Position, CellUpdate<T, TCell>>.Empty,
                    contradictions.ToImmutable());


            return new UpdateResult<T, TCell>(
                cellUpdates.ToImmutable(),
                contradictions.ToImmutable()
            );
        }

        /// <inheritdoc />
        public override BifurcationResult<T, TCell> ToBifurcationResult(BifurcationNode<T, TCell> node)
        {
            return SingleStep.ToBifurcationResult(node);
        }
    }

    private sealed record SingleStepCombiner : UpdateResultCombiner<T, TCell>
    {
        private SingleStepCombiner()
        {
        }

        public static SingleStepCombiner Instance { get; } = new();

        /// <inheritdoc />
        public override UpdateResult<T, TCell> Combine(IEnumerable<ICellChangeResult> cellChangeResults)
        {
            Dictionary<Position, CellUpdate<T, TCell>> dictionary = new();
            var contradictions = ImmutableHashSet<Contradiction>.Empty.ToBuilder();

            foreach (var cellChangeResult in cellChangeResults)
            {
                switch (cellChangeResult)
                {
                    case CellUpdate<T, TCell> update:
                    {
                        if (!dictionary.TryAdd(update.Position, update))
                        {
                            var old = dictionary[update.Position];
                            var combineResult = old.TryCombine(update);
                            if (combineResult is Contradiction contradiction)
                                contradictions.Add(contradiction);
                            if (combineResult is CellUpdate<T, TCell> combinedUpdate &&
                                !combinedUpdate.NewCell.Equals(old.NewCell))
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
                return new UpdateResult<T, TCell>(ImmutableDictionary<Position, CellUpdate<T, TCell>>.Empty,
                    contradictions.ToImmutable());

            if(!dictionary.Any())
                return UpdateResult<T, TCell>.Empty;

            var bestUpdate =
                dictionary.GroupBy(x => x.Value.NewCell.Count())
                    .OrderBy(x => x.Key)
                    .ThenBy(x => x.Count())
                    .First().First();

            if (bestUpdate.Value.NewCell.IsEmpty())
            { //just return the one cell
                return new UpdateResult<T, TCell>(
                    ImmutableDictionary<Position, CellUpdate<T, TCell>>.Empty.Add(bestUpdate.Key, bestUpdate.Value),
                    contradictions.ToImmutable()
                );
            }
            else
            {
                var sameUpdates = dictionary
                    .Where(x => x.Value.Reason.Equals(bestUpdate.Value.Reason))
                    .ToImmutableDictionary();

                return new UpdateResult<T, TCell>(
                    sameUpdates,
                    contradictions.ToImmutable()
                );
            }

                
        }

        /// <inheritdoc />
        public override BifurcationResult<T, TCell> ToBifurcationResult(BifurcationNode<T, TCell> node)
        {
            if (node.UpdateResult.HasContradictions) return Fast.ToBifurcationResult(node);

            if (node.UpdateResult.UpdatedCells.Count <= 1)
                return Fast.ToBifurcationResult(node);

            var best = node.UpdateResult.UpdatedCells
                .OrderBy(x => x.Value.NewCell.Count())
                .First();

            var newUpdateResult = new UpdateResult<T, TCell>(
                ImmutableDictionary<Position, CellUpdate<T, TCell>>.Empty.Add(best.Key, best.Value),

                ImmutableHashSet<Contradiction>.Empty);

            return new BifurcationResult<T, TCell>(newUpdateResult, null, node.LevelsDescended);
        }
    }

    public static UpdateResultCombiner<T, TCell> Fast => FastCombiner.Instance;
    public static UpdateResultCombiner<T, TCell> Default => DefaultCombiner.Instance;
    public static UpdateResultCombiner<T, TCell> SingleStep => SingleStepCombiner.Instance;

    /// <summary>
    /// Stops on first contradiction
    /// </summary>
    private sealed record FastCombiner : UpdateResultCombiner<T, TCell>
    {
        private FastCombiner() { }

        public static FastCombiner Instance { get; } = new();

        public override UpdateResult<T, TCell> Combine(IEnumerable<ICellChangeResult> cellChangeResults)
        {
            Dictionary<Position, CellUpdate<T, TCell>> dictionary = new();

            foreach (var cellChangeResult in cellChangeResults)
            {
                switch (cellChangeResult)
                {
                    case CellUpdate<T, TCell> update:
                    {
                        if (!dictionary.TryAdd(update.Position, update))
                        {
                            var old = dictionary[update.Position];
                            var combineResult = old.TryCombine(update);
                            if (combineResult is Contradiction contradiction)
                                return new UpdateResult<T, TCell>(dictionary.ToImmutableDictionary(),
                                    ImmutableHashSet<Contradiction>.Empty
                                        .Add(contradiction));

                            if (combineResult is CellUpdate<T, TCell> combinedUpdate &&
                                !combinedUpdate.NewCell.Equals(old.NewCell))
                                dictionary[update.Position] = combinedUpdate;
                        }

                        break;
                    }
                    case Contradiction contradiction:
                    {
                        //Stop Immediately
                        return new UpdateResult<T, TCell>(dictionary.ToImmutableDictionary(),
                            ImmutableHashSet<Contradiction>.Empty
                                .Add(contradiction));
                    }
                }
            }


            return new UpdateResult<T, TCell>(
                dictionary.ToImmutableDictionary(),
                ImmutableHashSet<Contradiction>.Empty
            );
        }

        /// <inheritdoc />
        public override BifurcationResult<T, TCell> ToBifurcationResult(BifurcationNode<T, TCell> node)
        {
            return new BifurcationResult<T, TCell>(node.UpdateResult, node.CompleteGrids, node.LevelsDescended);
        }
    }
}
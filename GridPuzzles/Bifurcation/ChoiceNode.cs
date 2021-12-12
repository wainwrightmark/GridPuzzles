using System.Diagnostics.Contracts;
using MoreLinq;

namespace GridPuzzles.Bifurcation;

public class ChoiceNode<T> : BifurcationNode<T> where T :notnull
{
    /// <inheritdoc />
    public ChoiceNode(Grid<T> grid, UpdateResult<T> updateResult, int initialHeight,
        ImmutableHashSet<IBifurcationChoice<T>> choicesNotToTake,
        IReadOnlyCollection<IBifurcationOption<T>> options, IReadOnlyCollection<IBifurcationChoice<T>> choices) :
        base(grid, updateResult, choicesNotToTake)
    {
        Options = options;
        Choices = choices;
        InitialHeight = initialHeight;
            
    }

    /// <inheritdoc />
    public override int InitialHeight { get; }

    /// <summary>
    /// Options whose choices led to this node.
    /// </summary>
    public IReadOnlyCollection<IBifurcationOption<T>> Options { get; }

    /// <summary>
    /// The choices that led to this node.
    /// </summary>
    public IReadOnlyCollection<IBifurcationChoice<T>> Choices { get; }

    /// <inheritdoc />
    public override string ToString() => string.Join(" or ", Choices) + " " + StateString;


    public static void CombineAll(HashSet<ChoiceNode<T>> nodes)
    {
        var groups = nodes.GroupBy(x => x, ChoiceNodeComparer.Instance);
        var toRemove = new List<ChoiceNode<T>>();
        var toAdd = new List<ChoiceNode<T>>();

        foreach (var group in groups)
        {
            if (group.Count() == 1)
            {
            }
            else
            {
                var l = group.ToList();

                var combination = Combine(l);
                toRemove.AddRange(l);
                toAdd.Add(combination);
            }
        }

        nodes.ExceptWith(toRemove);
        nodes.UnionWith(toAdd);
    }

    [Pure]
    private static ChoiceNode<T> Combine(IReadOnlyCollection<ChoiceNode<T>> choiceNodes)
    {
        if (choiceNodes.Count == 1)
            return choiceNodes.Single();

        var choicesNotToTake =
            MoreLinq.Extensions.MinByExtension.MinBy(
                choiceNodes
                    .Select(x => x.ChoicesNotToTake),
                x=>x.Count
            ).First();

        var height = choiceNodes.Select(x => x.InitialHeight).Min();

        var options = choiceNodes.SelectMany(x => x.Options).ToHashSet();
        var choices = choiceNodes.SelectMany(x => x.Choices).ToHashSet();

        return new ChoiceNode<T>(choiceNodes.First().Grid, choiceNodes.First().UpdateResult, height,
            choicesNotToTake, options, choices);
    }


    /// <summary>
    /// Compares choice nodes on the basis of the update results they produce
    /// </summary>
    private class ChoiceNodeComparer : IEqualityComparer<ChoiceNode<T>>
    {
        private ChoiceNodeComparer()
        {
        }

        public static IEqualityComparer<ChoiceNode<T>> Instance { get; } = new ChoiceNodeComparer();

        /// <inheritdoc />
        public int GetHashCode(ChoiceNode<T> obj)
        {
            if (obj.UpdateResult.Contradictions.Any())
                return 42; //If they both have contradictions they are equal for this.

            return obj.UpdateResult.UpdatedCells.Count;
        }


        /// <inheritdoc />
        public bool Equals(ChoiceNode<T>? x, ChoiceNode<T>? y)
        {
            if (x is null || y is null)
                return false;

            if (ReferenceEquals(x, y))
                return true;

            if (x.UpdateResult.Contradictions.Any())
                return y.UpdateResult.Contradictions.Any();
            if (y.UpdateResult.Contradictions.Any())
                return false;

            var cellsMatch = CellsMatch(x.UpdateResult.UpdatedCells, y.UpdateResult.UpdatedCells);
            return cellsMatch;
        }

        private static bool CellsMatch(IReadOnlyDictionary<Position, CellUpdate<T>> r1,
            IReadOnlyDictionary<Position, CellUpdate<T>> r2)
        {
            if (r1.Count != r2.Count)
                return false;

            foreach (var (position, cellUpdate) in r1)
            {
                if (!r2.TryGetValue(position, out var cellUpdate2))
                    return false;

                if (!cellUpdate.NewCell.Equals(cellUpdate2.NewCell))
                    return false;
            }

            return true;
        }
    }
}
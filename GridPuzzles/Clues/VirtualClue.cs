using System.Drawing;
using GridPuzzles.Clues.Constraints;
using GridPuzzles.Overlays;

namespace GridPuzzles.Clues;

public abstract class VirtualClue //TODO this can completely change. We should get rid of clue interfaces and introduce a mixin system
{

    public static IClue<T, TCell> Create<T, TCell>(IClue<T, TCell> clue, Color color) where T :struct where TCell : ICell<T, TCell>, new()
    {
        var overlays = clue.Positions.Select(x => new CellColorOverlay(color, x)).ToList();

        if (clue is IRuleClue<T, TCell> ruleClue)
            return new VirtualRuleClue<T, TCell>(ruleClue,
                overlays);

        if (clue is IRelationshipClue<T, TCell> relationshipClue)
            return new VirtualRelationshipClue<T, TCell>(relationshipClue, overlays);

        throw new NotImplementedException($"Cannot Create Virtual clue for {clue}");
    }

    public class VirtualRelationshipClue<T, TCell> : IRelationshipClue<T, TCell>, IDynamicOverlayClue<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
    {
        public IReadOnlyCollection<ICellOverlay> CellOverlays { get; }
        public IRelationshipClue<T, TCell> Underlying { get; }

        public VirtualRelationshipClue(IRelationshipClue<T, TCell> underlying, IReadOnlyCollection<ICellOverlay> cellOverlays)
        {
            CellOverlays = cellOverlays;
            Underlying = underlying;
        }

        /// <inheritdoc />
        public IEnumerable<ICellOverlay> CreateCellOverlays(Grid<T, TCell> grid)
        {
            return CellOverlays;
        }

        /// <inheritdoc />
        public string Name => Underlying.Name;

        /// <inheritdoc />
        public ImmutableSortedSet<Position> Positions => Underlying.Positions;

        /// <inheritdoc />
        public Position Position1 => Underlying.Position1;

        /// <inheritdoc />
        public Position Position2 => Underlying.Position2;

        /// <inheritdoc />
        public IRelationshipClue<T, TCell> Flipped => Underlying.Flipped;

        /// <inheritdoc />
        public IRelationshipClue<T, TCell> UniqueVersion => Underlying.UniqueVersion;

        /// <inheritdoc />
        public (bool changed, TCell newSet1, TCell newSet2) FindValidValues(TCell set1,
            TCell set2)
        {
            return Underlying.FindValidValues(set1, set2);
        }

        /// <inheritdoc />
        public bool IsValidCombination(T p1Value, T p2Value)
        {
            return Underlying.IsValidCombination(p1Value, p2Value);
        }

        /// <inheritdoc />
        public ISingleReason Reason => Underlying.Reason;

        /// <inheritdoc />
        public Constraint<T> Constraint => Underlying.Constraint;
    }

    public class VirtualRuleClue<T, TCell> : IRuleClue<T, TCell>, IDynamicOverlayClue<T, TCell>
        where T :struct where TCell : ICell<T, TCell>, new()
    {
        public VirtualRuleClue(IRuleClue<T, TCell> underlying, IReadOnlyCollection<ICellOverlay> cellOverlays)
        {
            Underlying = underlying;
            CellOverlays = cellOverlays;
        }

        public IRuleClue<T, TCell> Underlying { get; }

        public IReadOnlyCollection<ICellOverlay> CellOverlays { get; }

        /// <inheritdoc />
        public IEnumerable<ICellChangeResult> CalculateCellUpdates(Grid<T, TCell> grid)
        {
            return Underlying.CalculateCellUpdates(grid);
        }

        /// <inheritdoc />
        public IEnumerable<ICellOverlay> CreateCellOverlays(Grid<T, TCell> grid)
        {
            return CellOverlays;
        }

        /// <inheritdoc />
        public string Name => "Virtual " + Underlying.Name;

        /// <inheritdoc />
        public ImmutableSortedSet<Position> Positions => Underlying.Positions;
    }
}
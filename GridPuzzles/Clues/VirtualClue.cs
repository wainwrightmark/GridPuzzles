using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using GridPuzzles.Cells;
using GridPuzzles.Clues.Constraints;
using GridPuzzles.Overlays;
using GridPuzzles.Reasons;

namespace GridPuzzles.Clues;

public abstract class VirtualClue //TODO this can completely change. We should get rid of clue interfaces and introduce a mixin system
{

    public static IClue<T> Create<T>(IClue<T> clue, Color color) where T :notnull
    {
        var overlays = clue.Positions.Select(x => new CellColorOverlay(color, x)).ToList();

        if (clue is IRuleClue<T> ruleClue)
            return new VirtualRuleClue<T>(ruleClue,
                overlays);

        if (clue is IRelationshipClue<T> relationshipClue)
            return new VirtualRelationshipClue<T>(relationshipClue, overlays);

        throw new NotImplementedException($"Cannot Create Virtual clue for {clue}");
    }

    public class VirtualRelationshipClue<T> : IRelationshipClue<T>, IDynamicOverlayClue<T> where T :notnull
    {
        public IReadOnlyCollection<ICellOverlay> CellOverlays { get; }
        public IRelationshipClue<T> Underlying { get; }

        public VirtualRelationshipClue(IRelationshipClue<T> underlying, IReadOnlyCollection<ICellOverlay> cellOverlays)
        {
            CellOverlays = cellOverlays;
            Underlying = underlying;
        }

        /// <inheritdoc />
        public IEnumerable<ICellOverlay> GetCellOverlays(Grid<T> grid)
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
        public IRelationshipClue<T> Flipped => Underlying.Flipped;

        /// <inheritdoc />
        public IRelationshipClue<T> UniqueVersion => Underlying.UniqueVersion;

        /// <inheritdoc />
        public (bool changed, ImmutableSortedSet<T> newSet1, ImmutableSortedSet<T> newSet2) GetValidValues(ImmutableSortedSet<T> set1,
            ImmutableSortedSet<T> set2)
        {
            return Underlying.GetValidValues(set1, set2);
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

    public class VirtualRuleClue<T> : IRuleClue<T>, IDynamicOverlayClue<T>
        where T :notnull
    {
        public VirtualRuleClue(IRuleClue<T> underlying, IReadOnlyCollection<ICellOverlay> cellOverlays)
        {
            Underlying = underlying;
            CellOverlays = cellOverlays;
        }

        public IRuleClue<T> Underlying { get; }

        public IReadOnlyCollection<ICellOverlay> CellOverlays { get; }

        /// <inheritdoc />
        public IEnumerable<ICellChangeResult> GetCellUpdates(Grid<T> grid)
        {
            return Underlying.GetCellUpdates(grid);
        }

        /// <inheritdoc />
        public IEnumerable<ICellOverlay> GetCellOverlays(Grid<T> grid)
        {
            return CellOverlays;
        }

        /// <inheritdoc />
        public string Name => "Virtual " + Underlying.Name;

        /// <inheritdoc />
        public ImmutableSortedSet<Position> Positions => Underlying.Positions;
    }
}
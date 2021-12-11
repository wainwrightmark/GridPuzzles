﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using CSharpFunctionalExtensions;
using GridPuzzles;
using GridPuzzles.Clues;
using GridPuzzles.Clues.Constraints;
using GridPuzzles.Overlays;
using GridPuzzles.VariantBuilderArguments;
using MoreLinq;

namespace Sudoku.Variants;

public class WhispersVariantBuilder : VariantBuilder<int>
{
    private WhispersVariantBuilder() { }

    public static VariantBuilder<int> Instance { get; } = new WhispersVariantBuilder();

    /// <inheritdoc />
    public override string Name => "Whispers";

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder<int>>> TryGetClueBuilders1(
        IReadOnlyDictionary<string, string> arguments)
    {
        var pr = Positions.TryGetFromDictionary(arguments);
        if (pr.IsFailure) return pr.ConvertFailure<IReadOnlyCollection<IClueBuilder<int>>>();

        var md = MinDistance.TryGetFromDictionary(arguments);
        if (md.IsFailure) return md.ConvertFailure<IReadOnlyCollection<IClueBuilder<int>>>();

        return new List<IClueBuilder<int>>
        {
            new WhispersClueBuilder(pr.Value.ToImmutableList(), md.Value)
        };
    }

    public static readonly IntArgument MinDistance = new("Minimum Distance", 1,9, 5);
    public static readonly ListPositionArgument Positions = new("Positions", 2,9);
        

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments => new VariantBuilderArgument[]  {MinDistance, Positions};

    public class WhispersClueBuilder : IClueBuilder<int>
    {
        /// <summary>
        /// Create a German Whispers line. Adjacent cells must differ by at least 5
        /// </summary>
        /// <param name="positions"></param>
        public WhispersClueBuilder(ImmutableList<Position> positions, int MinimumDistance)
        {
            Positions = positions;
            this.MinimumDistance = MinimumDistance;
        }

        /// <inheritdoc />
        public string Name  => "Whispers";

        /// <inheritdoc />
        public int Level => 2;

        public ImmutableList<Position> Positions { get; }
        public int MinimumDistance { get; }

        /// <inheritdoc />
        public IEnumerable<IClue<int>> CreateClues(Position minPosition, Position maxPosition, IValueSource<int> valueSource,
            IReadOnlyCollection<IClue<int>> lowerLevelClues)
        {

            foreach (var (p1, p2) in Positions.Pairwise((a,b)=>(a,b)))
            {
                yield return RelationshipClue<int>.Create(p1, p2, new DifferByAtLeastConstraint(MinimumDistance));   
            }
        }


        /// <param name="minPosition"></param>
        /// <param name="maxPosition"></param>
        /// <inheritdoc />
        public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
        {
            if (Positions.Pairwise((a,b)=> a.IsAdjacent(b)).All(x=>x))
            {
                yield return new LineCellOverlay(Positions, Color.Orange);
                yield break;
            }

            var pairs = Positions.Pairwise((a, b) => (a, b)).Select(x => (x.a, x.b));

            foreach (var (a, b) in pairs)
            {
                var overlay = CellOverlays.TryCreateTwoPositionText(a, b, $"|{MinimumDistance}|");
                if (overlay.HasValue)
                    yield return overlay.Value;
                else
                {
                    yield return new CellColorOverlay(Color.Orange, a);
                    yield return new CellColorOverlay(Color.Orange, b);
                }
            }
        }
    }
}
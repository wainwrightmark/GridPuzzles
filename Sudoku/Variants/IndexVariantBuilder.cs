﻿namespace Sudoku.Variants;

public class IndexVariantBuilder : VariantBuilder
{
    private IndexVariantBuilder()
    {
    }

    public static VariantBuilder Instance { get; } = new IndexVariantBuilder();

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder>> TryGetClueBuilders1(IReadOnlyDictionary<string, string> arguments)
    {
        var pr = Parallel.TryGetFromDictionary(arguments);
        if (pr.IsFailure) return pr.ConvertFailure<IReadOnlyCollection<IClueBuilder>>();

        var ir = Index.TryGetFromDictionary(arguments);
        if (ir.IsFailure) return ir.ConvertFailure<IReadOnlyCollection<IClueBuilder>>();

        return new List<IClueBuilder>
        {
            new IndexClueBuilder(pr.Value, ir.Value)
        };
    }

    /// <inheritdoc />
    public override string Name => "Index";

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments => new VariantBuilderArgument[]{Index, Parallel};

    public static readonly IntArgument Index = new (nameof(Index), 1, 9, Maybe<int>.None);

    public static readonly EnumArgument<Parallel> Parallel = new(nameof(Parallel), Maybe<Parallel>.None);

        
    public record IndexClueBuilder(Parallel ClueParallel,int ClueIndex) : IClueBuilder
    {

        /// <inheritdoc />
        public IEnumerable<IClue<int, IntCell>> CreateClues(Position minPosition, Position maxPosition, IValueSource valueSource,
            IReadOnlyCollection<IClue<int, IntCell>> lowerLevelClues)
        {
            yield return new IndexClue(ClueParallel, ClueIndex, maxPosition);
        }

        /// <inheritdoc />
        public string Name => "Index";

        /// <inheritdoc />
        public int Level => 2;

        /// <inheritdoc />
        public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
        {
            if (ClueParallel == GridPuzzles.Parallel.Column)
            {
                var topLeft = new Position(ClueIndex, minPosition.Row);
                yield return new RectangleCellOverlay(topLeft, 1, maxPosition.Row - minPosition.Row + 1, Color.Red,
                    10);
            }
            else
            {
                var topLeft = new Position(minPosition.Column, ClueIndex);
                yield return new RectangleCellOverlay(topLeft, maxPosition.Column - minPosition.Column + 1,1,  Color.Red,
                    10);
            }
                
        }

    }
}

public class IndexReason : ISingleReason
{
    public IndexReason(Maybe<IClue> clue, Position keyPosition, Position matchPosition)
    {
        Clue = clue;
        KeyPosition = keyPosition;
        MatchPosition = matchPosition;
    }

    /// <inheritdoc />
    public string Text => "Forced by index clue";
        
    /// <inheritdoc />
    public Maybe<IClue> Clue { get; }

    public Position KeyPosition { get; }
    public Position MatchPosition { get; }

    /// <inheritdoc />
    public IEnumerable<Position> GetContributingPositions(IGrid grid)
    {
        yield return KeyPosition;
        yield return MatchPosition;
    }
}

public class IndexClue : IRuleClue
{
    public IndexClue(Parallel parallel, int index, Position maxPosition)
    {
        Parallel = parallel;
        Index = index;
        Name = $"Index {Parallel} {Index}";

        Positions = maxPosition.GetPositionsUpTo(true).SelectMany(x=>x).ToImmutableSortedSet();
    }

    public Parallel Parallel { get; }

    public int Index { get; }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public ImmutableSortedSet<Position> Positions { get; }

    /// <inheritdoc />
    public IEnumerable<ICellChangeResult> CalculateCellUpdates(Grid grid)
    {
        if (Parallel == Parallel.Column)
        {
            for (int r = Position.Origin.Row; r <= grid.MaxPosition.Row; r++)
            {
                var keyPosition = new Position(Index, r);
                var keyCell = grid.GetCellKVP(keyPosition);

                if (keyCell.Value.HasSingleValue())
                {
                    var matchPosition = new Position(keyCell.Value.Single(), r);
                    var matchCell = grid.GetCellKVP(matchPosition);
                    yield return matchCell.CloneWithOnlyValue(Index, new IndexReason(this, keyPosition, matchPosition));

                }
                else
                {
                    for (int c = Position.Origin.Column; c <= grid.MaxPosition.Column; c++)
                    {
                        var matchPosition = new Position(c, r);
                        var matchCell = grid.GetCellKVP(matchPosition);

                        if (!keyCell.Value.Contains(c))
                        {
                            yield return matchCell.CloneWithoutValue(Index, new IndexReason(this, keyPosition, matchPosition));
                        }

                        if (!matchCell.Value.Contains(Index))
                        {
                            yield return keyCell.CloneWithoutValue(c, new IndexReason(this, keyPosition, matchPosition));
                        }
                    }
                }
            }
        }
        else
        {
            for (int c = Position.Origin.Column; c <= grid.MaxPosition.Column; c++)
            {
                var keyPosition = new Position(c, Index);
                var keyCell = grid.GetCellKVP(keyPosition);

                if (keyCell.Value.HasSingleValue())
                {
                    var matchPosition = new Position(c, keyCell.Value.Single());
                    var matchCell = grid.GetCellKVP(matchPosition);
                    yield return matchCell.CloneWithOnlyValue(Index, new IndexReason(this, keyPosition, matchPosition));

                }
                else
                {
                    for (int r = Position.Origin.Row; r <= grid.MaxPosition.Row; r++)
                    {
                        var matchPosition = new Position(c, r);
                        var matchCell = grid.GetCellKVP(matchPosition);

                        if (!keyCell.Value.Contains(r))
                        {
                            yield return matchCell.CloneWithoutValue(Index, new IndexReason(this, keyPosition, matchPosition));
                        }

                        if (!matchCell.Value.Contains(Index))
                        {
                            yield return keyCell.CloneWithoutValue(r, new IndexReason(this, keyPosition, matchPosition));
                        }
                    }
                }
            }
        }
    }
}
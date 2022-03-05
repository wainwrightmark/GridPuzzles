using System;
using GridPuzzles.Enums;

namespace Sudoku.Variants;

public class SlingshotVariantBuilder : VariantBuilder
{
    private SlingshotVariantBuilder()
    {
    }

    public static VariantBuilder Instance { get; } = new SlingshotVariantBuilder();

    /// <inheritdoc />
    public override string Name => "Slingshot";

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder>> TryGetClueBuilders1(
        IReadOnlyDictionary<string, string> arguments)
    {
        var positionArgumentsResult = SlingshotPosition.TryGetFromDictionary(arguments);
        if (positionArgumentsResult.IsFailure)
            return positionArgumentsResult.ConvertFailure<IReadOnlyCollection<IClueBuilder>>();


        var fromArgumentResult = FromArgument.TryGetFromDictionary(arguments);
        if (fromArgumentResult.IsFailure)
            return fromArgumentResult.ConvertFailure<IReadOnlyCollection<IClueBuilder>>();

        var toArgumentResult = ToArgument.TryGetFromDictionary(arguments);
        if (toArgumentResult.IsFailure)
            return toArgumentResult.ConvertFailure<IReadOnlyCollection<IClueBuilder>>();


        var l = new List<IClueBuilder>
        {
            new SlingshotClueBuilder(positionArgumentsResult.Value, fromArgumentResult.Value,
                toArgumentResult.Value)
        };

        return l;
    }

    public static readonly EnumArgument<CompassDirection> FromArgument = new("From", Maybe<CompassDirection>.None);
    public static readonly EnumArgument<CompassDirection> ToArgument = new("To", Maybe<CompassDirection>.None);

    public static readonly SinglePositionArgument SlingshotPosition = new("Position");

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments
    {
        get { return new VariantBuilderArgument[] { SlingshotPosition, FromArgument, ToArgument }; }
    }
}

public record SlingshotClueBuilder(Position CellPosition, CompassDirection FromDirection, CompassDirection ToDirection) : IClueBuilder
{

    /// <inheritdoc />
    public string Name => "Slingshot";

    /// <inheritdoc />
    public int Level => 2;

    /// <inheritdoc />
    public IEnumerable<IClue<int, IntCell>> CreateClues(Position minPosition, Position maxPosition,
        IValueSource valueSource,
        IReadOnlyCollection<IClue<int, IntCell>> lowerLevelClues)
    {
        yield return new SlingshotClue(CellPosition, FromDirection, ToDirection,
            ToDirection.GetMaxDistance(CellPosition, minPosition, maxPosition));
    }

    /// <param name="minPosition"></param>
    /// <param name="maxPosition"></param>
    /// <inheritdoc />
    public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
    {
        if (SymbolDictionary.TryGetValue((FromDirection, ToDirection), out var symbol))
        {
            yield return CellOverlays.CreateOnePositionText(CellPosition, symbol);
            yield break;
        }


        yield return new CellColorOverlay(Color.Green, CellPosition);

        yield return new CellColorOverlay( Color.BlueViolet,FromDirection.GetAdjacentPosition(CellPosition));

        var maxDistance = ToDirection.GetMaxDistance(CellPosition, Position.Origin, maxPosition);

        foreach (var p in ToDirection.GetAdjacentPositions(CellPosition, maxDistance))
            yield return new CellColorOverlay(Color.Red, p); 
    }

    private static readonly IReadOnlyDictionary<(CompassDirection from, CompassDirection to), string>
        SymbolDictionary = new Dictionary<(CompassDirection @from, CompassDirection to), string>()
        {
            { (CompassDirection.North, CompassDirection.West), "⮠" },
            { (CompassDirection.North, CompassDirection.East), "⮡" },
            { (CompassDirection.South, CompassDirection.West), "⮢" },
            { (CompassDirection.South, CompassDirection.East), "⮣" },
            { (CompassDirection.East, CompassDirection.North), "⮤" },
            { (CompassDirection.West, CompassDirection.North), "⮥" },
            { (CompassDirection.East, CompassDirection.South), "⮦" },
            { (CompassDirection.West, CompassDirection.South), "⮧" },

            { (CompassDirection.West, CompassDirection.West), "⮌" },
            { (CompassDirection.East, CompassDirection.East), "⮎" },
            { (CompassDirection.North, CompassDirection.North), "⮍" },
            { (CompassDirection.South, CompassDirection.South), "⮏" },

            { (CompassDirection.West, CompassDirection.East), "🠒" },
            { (CompassDirection.East, CompassDirection.West), "🠐" },
            { (CompassDirection.North, CompassDirection.South), "🠓" },
            { (CompassDirection.South, CompassDirection.North), "🠑" },
        };
}

public class SlingshotClue : IRuleClue
{
    public SlingshotClue(Position cellPosition, CompassDirection fromDirection, CompassDirection toDirection,
        int maxDistance)
    {
        CellPosition = cellPosition;
        FromDirection = fromDirection;
        ToDirection = toDirection;
        MaxDistance = maxDistance;

        Positions = ToDirection.GetAdjacentPositions(CellPosition, MaxDistance)
            .Prepend(FromDirection.GetAdjacentPosition(CellPosition))
            .Prepend(CellPosition)
            .ToImmutableSortedSet();
    }

    /// <inheritdoc />
    public string Name => "Slingshot";

    public Position CellPosition { get; }

    public CompassDirection FromDirection { get; }

    public CompassDirection ToDirection { get; }

    public int MaxDistance { get; }


    /// <inheritdoc />
    public ImmutableSortedSet<Position> Positions { get; }

    /// <inheritdoc />
    public IEnumerable<ICellChangeResult> CalculateCellUpdates(Grid grid)
    {
        var centreCell = grid.GetCellKVP(CellPosition);

        if (centreCell.Value.Any(x => x > MaxDistance))
            yield return centreCell.CloneWithOnlyValues<int, IntCell>(
                centreCell.Value.Where(x => x <= MaxDistance).ToIntCell(),
                new SlingshotReason(this)
                //"Maximum Slingshot length"
            );

        var newMax = Math.Min(MaxDistance, centreCell.Value.Max());

        var toCells = ToDirection.GetAdjacentPositions(CellPosition, newMax)
            .Select(grid.GetCellKVP)
            .Select((x, i) => (x, i + 1))
            .ToDictionary(x => x.Item2, x => x.x);

        var fromCell = grid.GetCellKVP(FromDirection.GetAdjacentPosition(CellPosition));

        //Set froms possible values to those implied by the centre cell

        var fromPossibles = centreCell.Value.Where(x => x <= MaxDistance)
            .SelectMany(x => toCells[x].Value).Distinct().ToIntCell();

        yield return fromCell.CloneWithOnlyValues<int, IntCell>(fromPossibles,
            new SlingshotReason(this)
            //"Possible Slingshot targets"
                
        );

        //if centre cell is set, to must equal from

        if (centreCell.Value.HasSingleValue())
        {
            if (toCells.TryGetValue(centreCell.Value.Single(), out var kvp))
            {
                yield return kvp
                    .CloneWithOnlyValues<int, IntCell>(fromCell.Value, 
                        new SlingshotReason(this)
                        //"Possible Slingshot sources"
                            
                    );
            }
            else
            {
                yield return new Contradiction(
                    new SlingshotReason(this),
                    //"Slingshot Target is outside the grid",
                    new []{CellPosition}
                );
            }
        }


        var fps = fromCell.Value.ToHashSet();

        //if only one cell in the from collection could share a value with two, that is the centre cell
        //Centre cell cannot have a value that puts to and from in the same uniqueness set

        var possibleToCells = toCells
            .Where(x => fps.Overlaps(x.Value.Value))
            .Where(x => !grid.ClueSource.UniquenessClueHelper.ArePositionsMutuallyUnique(fromCell.Key, x.Value.Key))
            .Select(x => x.Key).ToIntCell();

        yield return centreCell.CloneWithOnlyValues<int, IntCell>(possibleToCells,
            new SlingshotReason(this)
            //"Possible Slingshot lengths"
        );
    }
}

public sealed record SlingshotReason(SlingshotClue SlingshotClue) : ISingleReason
{
    /// <inheritdoc />
    public string Text => "Slingshot";

    /// <inheritdoc />
    public IEnumerable<Position> GetContributingPositions(IGrid grid)
    {
        return SlingshotClue.Positions;
    }

    /// <inheritdoc />
    public Maybe<IClue> Clue => SlingshotClue;
}
using System;

namespace Sudoku.Clues;

public class CompleteDiagonalVariantBuilder<T, TCell> : VariantBuilder<T, TCell>where T :struct where TCell : ICell<T, TCell>, new()
{
    private CompleteDiagonalVariantBuilder()
    {
    }

    public static IVariantBuilder<T, TCell> Instance { get; } = new CompleteDiagonalVariantBuilder<T, TCell>();

    /// <inheritdoc />
    public override string Name => "Complete Diagonal";

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder<T, TCell>>> TryGetClueBuilders1(IReadOnlyDictionary<string, string> arguments)
    {
        var dt = DiagonalTypeArgument.TryGetFromDictionary(arguments);

        if (dt.IsFailure) return dt.ConvertFailure<IReadOnlyCollection<IClueBuilder<T, TCell>>>();

        return dt.Value switch
        {
            DiagonalType.TopLeft => new[] {DiagonalClueBuilder<T, TCell>.TopLeft},
            DiagonalType.TopRight => new[] {DiagonalClueBuilder<T, TCell>.TopRight},
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public readonly EnumArgument<DiagonalType> DiagonalTypeArgument = new("Type", Maybe<DiagonalType>.From(DiagonalType.TopLeft));

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments  => new List<VariantBuilderArgument>()
    {
        DiagonalTypeArgument
    };

    /// <inheritdoc />
    public override bool IsValid(Position maxPosition)
    {
        return maxPosition.Column == maxPosition.Row && maxPosition.Column % 2 == 1;
    }
}

public record DiagonalClueBuilder<T, TCell>(bool TopToBottom) : IClueBuilder<T, TCell>where T :struct where TCell : ICell<T, TCell>, new()
{
    public static DiagonalClueBuilder<T, TCell> TopLeft = new( true);
    public static DiagonalClueBuilder<T, TCell> TopRight = new(false);
    public string Name => TopToBottom? "Top Left Diagonal":"Top Right Diagonal";

    
    /// <inheritdoc />
    public int Level => 2;

    /// <inheritdoc />
    public IEnumerable<IClue<T, TCell>> CreateClues(Position minPosition, Position maxPosition,
        IValueSource<T, TCell> valueSource,
        IReadOnlyCollection<IClue<T, TCell>> lowerLevelClues)
    {
        if (TopToBottom)
            yield return new DiagonalClue<T, TCell>(Name, minPosition, maxPosition);
        else
            yield return new DiagonalClue<T, TCell>(Name, new Position(minPosition.Column, maxPosition.Row),
                new Position(maxPosition.Column, minPosition.Row));
    }

    /// <param name="minPosition"></param>
    /// <param name="maxPosition"></param>
    /// <inheritdoc />
    public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
    {
        DiagonalClue<T, TCell> clue;
        if (TopToBottom)
        {
            clue = new DiagonalClue<T, TCell>(Name, Position.Origin, maxPosition);
        }
        else
            clue = new DiagonalClue<T, TCell>(Name, new Position(Position.Origin.Column, maxPosition.Row),
                new Position(maxPosition.Column, Position.Origin.Row));

        yield return new LineCellOverlay(clue.Positions, Color.Blue);
    }
}

public enum DiagonalType
{
    TopLeft,
    TopRight
}

public class CompleteOffsetDiagonalVariantBuilder<T, TCell> : VariantBuilder<T, TCell>where T :struct where TCell : ICell<T, TCell>, new()
{
    private CompleteOffsetDiagonalVariantBuilder()
    {
    }

    public static IVariantBuilder<T, TCell> Instance { get; } = new CompleteOffsetDiagonalVariantBuilder<T, TCell>();

    /// <inheritdoc />
    public override string Name => "Complete Offset Diagonal";

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder<T, TCell>>> TryGetClueBuilders1(IReadOnlyDictionary<string, string> arguments)
    {
        var dt = DiagonalTypeArgument.TryGetFromDictionary(arguments);

        if (dt.IsFailure) return dt.ConvertFailure<IReadOnlyCollection<IClueBuilder<T, TCell>>>();

        return dt.Value switch
        {
            DiagonalType.TopLeft => new[] {OffsetDiagonalClueBuilder<T, TCell>.TopLeft},
            DiagonalType.TopRight => new[] {OffsetDiagonalClueBuilder<T, TCell>.TopRight},
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public readonly EnumArgument<DiagonalType> DiagonalTypeArgument = new("Type", Maybe<DiagonalType>.From(DiagonalType.TopLeft));

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments  => new List<VariantBuilderArgument>()
    {
        DiagonalTypeArgument
    };

    /// <inheritdoc />
    public override bool IsValid(Position maxPosition)
    {
        return maxPosition.Column == maxPosition.Row && maxPosition.Column % 2 == 1;
    }
}

public record OffsetDiagonalClueBuilder<T, TCell>(bool TopIsLeft) : IClueBuilder<T, TCell>where T :struct where TCell : ICell<T, TCell>, new()
{
    public static OffsetDiagonalClueBuilder<T, TCell> TopLeft = new(true);
    public static OffsetDiagonalClueBuilder<T, TCell> TopRight = new(false);



    /// <inheritdoc />
    public string Name => TopIsLeft ? "Top Left Near Diagonal" : "Top Right Near Diagonal";

    /// <inheritdoc />
    public int Level => 2;

    /// <inheritdoc />
    public IEnumerable<IClue<T, TCell>> CreateClues(Position minPosition, Position maxPosition, IValueSource<T, TCell> valueSource,
        IReadOnlyCollection<IClue<T, TCell>> lowerLevelClues)
    {
        return CreateClues(minPosition, maxPosition);
    }
        

    private IEnumerable<IClue<T, TCell>> CreateClues(Position minPosition, Position maxPosition)
    {
        var centreColumn = (minPosition.Column + maxPosition.Column) / 2;
        var centreRow = (minPosition.Row + maxPosition.Row) / 2;

        var centre = new Position(centreColumn, centreRow);

        if (TopIsLeft)
        {
            var x1 = new Position(minPosition.Column + 1, minPosition.Row);
            var x2 = new Position(maxPosition.Column, maxPosition.Row - 1);
            yield return new NearDiagonalClue<T, TCell>(Name + 1, x1, x2, centre);

            var y1 = new Position(minPosition.Column, minPosition.Row + 1);
            var y2 = new Position(maxPosition.Column - 1, maxPosition.Row);
            yield return new NearDiagonalClue<T, TCell>(Name + 2, y1, y2, centre);
        }
        else
        {
            var x1 = new Position(minPosition.Column, maxPosition.Row - 1);
            var x2 = new Position(maxPosition.Column - 1, minPosition.Row);
            yield return new NearDiagonalClue<T, TCell>(Name + 1, x1, x2, centre);

            var y1 = new Position(minPosition.Column + 1, maxPosition.Row);
            var y2 = new Position(maxPosition.Column, minPosition.Row + 1);
            yield return new NearDiagonalClue<T, TCell>(Name + 2, y1, y2, centre);
        }
    }

    /// <param name="minPosition"></param>
    /// <param name="maxPosition"></param>
    /// <inheritdoc />
    public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
    {
        var clues = CreateClues(Position.Origin, maxPosition);

        foreach (var clue in clues)
            yield return new LineCellOverlay(clue.Positions, Color.Blue);
    }
}


public class NearDiagonalClue<T, TCell> : BasicClue<T, TCell>where T :struct where TCell : ICell<T, TCell>, new()
{
    /// <inheritdoc />
    public NearDiagonalClue(string domainName, Position left, Position right, Position centre) : base(domainName)
    {
        Positions = new []{centre}.Concat(left.GetDiagonalPositions(right)).ToImmutableSortedSet();
    }

    /// <inheritdoc />
    public override ImmutableSortedSet<Position> Positions { get; }
}

public class DiagonalClue<T, TCell> : BasicClue<T, TCell>where T :struct where TCell : ICell<T, TCell>, new()
{
    /// <inheritdoc />
    public DiagonalClue(string domainName, Position left, Position right) : base(domainName)
    {
        Positions = left.GetDiagonalPositions(right).ToImmutableSortedSet();
    }

    /// <inheritdoc />
    public override ImmutableSortedSet<Position> Positions { get; }
}
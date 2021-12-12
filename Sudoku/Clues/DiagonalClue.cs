using System;
using System.Drawing;
using CSharpFunctionalExtensions;
using GridPuzzles.Overlays;
using GridPuzzles.VariantBuilderArguments;

namespace Sudoku.Clues;

public class CompleteDiagonalVariantBuilder<T> : VariantBuilder<T>where T :notnull
{
    private CompleteDiagonalVariantBuilder()
    {
    }

    public static IVariantBuilder<T> Instance { get; } = new CompleteDiagonalVariantBuilder<T>();

    /// <inheritdoc />
    public override string Name => "Complete Diagonal";

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder<T>>> TryGetClueBuilders1(IReadOnlyDictionary<string, string> arguments)
    {
        var dt = DiagonalTypeArgument.TryGetFromDictionary(arguments);

        if (dt.IsFailure) return dt.ConvertFailure<IReadOnlyCollection<IClueBuilder<T>>>();

        return dt.Value switch
        {
            DiagonalType.TopLeft => new[] {DiagonalClueBuilder<T>.TopLeft},
            DiagonalType.TopRight => new[] {DiagonalClueBuilder<T>.TopRight},
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

public record DiagonalClueBuilder<T>(bool TopToBottom) : IClueBuilder<T>where T :notnull
{
    public static DiagonalClueBuilder<T> TopLeft = new( true);
    public static DiagonalClueBuilder<T> TopRight = new(false);
    public string Name => TopToBottom? "Top Left Diagonal":"Top Right Diagonal";

    
    /// <inheritdoc />
    public int Level => 2;

    /// <inheritdoc />
    public IEnumerable<IClue<T>> CreateClues(Position minPosition, Position maxPosition,
        IValueSource<T> valueSource,
        IReadOnlyCollection<IClue<T>> lowerLevelClues)
    {
        if (TopToBottom)
            yield return new DiagonalClue<T>(Name, minPosition, maxPosition);
        else
            yield return new DiagonalClue<T>(Name, new Position(minPosition.Column, maxPosition.Row),
                new Position(maxPosition.Column, minPosition.Row));
    }

    /// <param name="minPosition"></param>
    /// <param name="maxPosition"></param>
    /// <inheritdoc />
    public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
    {
        DiagonalClue<T> clue;
        if (TopToBottom)
        {
            clue = new DiagonalClue<T>(Name, Position.Origin, maxPosition);
        }
        else
            clue = new DiagonalClue<T>(Name, new Position(Position.Origin.Column, maxPosition.Row),
                new Position(maxPosition.Column, Position.Origin.Row));

        yield return new LineCellOverlay(clue.Positions, Color.Blue);
    }
}

public enum DiagonalType
{
    TopLeft,
    TopRight
}

public class CompleteOffsetDiagonalVariantBuilder<T> : VariantBuilder<T>where T :notnull
{
    private CompleteOffsetDiagonalVariantBuilder()
    {
    }

    public static IVariantBuilder<T> Instance { get; } = new CompleteOffsetDiagonalVariantBuilder<T>();

    /// <inheritdoc />
    public override string Name => "Complete Offset Diagonal";

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder<T>>> TryGetClueBuilders1(IReadOnlyDictionary<string, string> arguments)
    {
        var dt = DiagonalTypeArgument.TryGetFromDictionary(arguments);

        if (dt.IsFailure) return dt.ConvertFailure<IReadOnlyCollection<IClueBuilder<T>>>();

        return dt.Value switch
        {
            DiagonalType.TopLeft => new[] {OffsetDiagonalClueBuilder<T>.TopLeft},
            DiagonalType.TopRight => new[] {OffsetDiagonalClueBuilder<T>.TopRight},
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

public record OffsetDiagonalClueBuilder<T>(bool TopIsLeft) : IClueBuilder<T>where T :notnull
{
    public static OffsetDiagonalClueBuilder<T> TopLeft = new(true);
    public static OffsetDiagonalClueBuilder<T> TopRight = new(false);



    /// <inheritdoc />
    public string Name => TopIsLeft ? "Top Left Near Diagonal" : "Top Right Near Diagonal";

    /// <inheritdoc />
    public int Level => 2;

    /// <inheritdoc />
    public IEnumerable<IClue<T>> CreateClues(Position minPosition, Position maxPosition, IValueSource<T> valueSource,
        IReadOnlyCollection<IClue<T>> lowerLevelClues)
    {
        return CreateClues(minPosition, maxPosition);
    }
        

    private IEnumerable<IClue<T>> CreateClues(Position minPosition, Position maxPosition)
    {
        var centreColumn = (minPosition.Column + maxPosition.Column) / 2;
        var centreRow = (minPosition.Row + maxPosition.Row) / 2;

        var centre = new Position(centreColumn, centreRow);

        if (TopIsLeft)
        {
            var x1 = new Position(minPosition.Column + 1, minPosition.Row);
            var x2 = new Position(maxPosition.Column, maxPosition.Row - 1);
            yield return new NearDiagonalClue<T>(Name + 1, x1, x2, centre);

            var y1 = new Position(minPosition.Column, minPosition.Row + 1);
            var y2 = new Position(maxPosition.Column - 1, maxPosition.Row);
            yield return new NearDiagonalClue<T>(Name + 2, y1, y2, centre);
        }
        else
        {
            var x1 = new Position(minPosition.Column, maxPosition.Row - 1);
            var x2 = new Position(maxPosition.Column - 1, minPosition.Row);
            yield return new NearDiagonalClue<T>(Name + 1, x1, x2, centre);

            var y1 = new Position(minPosition.Column + 1, maxPosition.Row);
            var y2 = new Position(maxPosition.Column, minPosition.Row + 1);
            yield return new NearDiagonalClue<T>(Name + 2, y1, y2, centre);
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


public class NearDiagonalClue<T> : BasicClue<T>where T :notnull
{
    /// <inheritdoc />
    public NearDiagonalClue(string domainName, Position left, Position right, Position centre) : base(domainName)
    {
        Positions = new []{centre}.Concat(left.GetDiagonalPositions(right)).ToImmutableSortedSet();
    }

    /// <inheritdoc />
    public override ImmutableSortedSet<Position> Positions { get; }
}

public class DiagonalClue<T> : BasicClue<T>where T :notnull
{
    /// <inheritdoc />
    public DiagonalClue(string domainName, Position left, Position right) : base(domainName)
    {
        Positions = left.GetDiagonalPositions(right).ToImmutableSortedSet();
    }

    /// <inheritdoc />
    public override ImmutableSortedSet<Position> Positions { get; }
}
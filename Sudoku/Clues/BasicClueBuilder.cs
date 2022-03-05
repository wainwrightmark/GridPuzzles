using System;
using Sudoku.Variants;

namespace Sudoku.Clues;

public class DisjointBoxesVariantBuilder<T, TCell> : NoArgumentVariantBuilder<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    private DisjointBoxesVariantBuilder() { }

    public static DisjointBoxesVariantBuilder<T, TCell> Instance { get; } = new ();

    /// <inheritdoc />
    public override string Name => "Disjoint Boxes";

    /// <inheritdoc />
    public override int Level => 2;

    /// <inheritdoc />
    public override IEnumerable<IClue<T, TCell>> CreateClues(Position minPosition, Position maxPosition, IValueSource<T, TCell> valueSource,
        IReadOnlyCollection<IClue<T, TCell>> lowerLevelClues)
    {
        var boxClues = lowerLevelClues.OfType<BoxClue<T, TCell>>().ToList();

        if (boxClues.Count == maxPosition.Row && boxClues.Count == maxPosition.Column)
        {
            var minLength = boxClues.Min(x => x.Positions.Count);

            if (minLength == maxPosition.Row)
            {
                for (var i = 0; i < minLength; i++)
                {
                    var i1 = i;
                    var cells = boxClues.Select(x => x.Positions[i1]).ToImmutableSortedSet();

                    yield return new UniqueCompleteClue<T, TCell>($"Position {i + 1} of any Box", cells);
                }
            }
        }
    }

    /// <inheritdoc />
    public override bool OnByDefault => false;
}


public class CompleteRectangularBoxVariantBuilder<T, TCell> :VariantBuilder<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    private CompleteRectangularBoxVariantBuilder()
    {
    }

    public static IVariantBuilder<T, TCell> Instance { get; } = new CompleteRectangularBoxVariantBuilder<T, TCell>();

    /// <inheritdoc />
    public override string Name => "Complete Rectangular Boxes";

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder<T, TCell>>> TryGetClueBuilders1(IReadOnlyDictionary<string, string> arguments)
    {
        var widthResult = _boxWidthArgument.TryGetFromDictionary(arguments);

        if (widthResult.IsFailure) return widthResult.ConvertFailure<IReadOnlyCollection<IClueBuilder<T, TCell>>>();

        return new IClueBuilder<T, TCell>[]
        {
            new CompleteRectangularBoxClueBuilder(Convert.ToUInt16(widthResult.Value))
        };
    }
        
    private readonly IntArgument _boxWidthArgument = new("Box Width", 2, 8, Maybe<int>.From(2));

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments => new List<VariantBuilderArgument>
    {
        _boxWidthArgument
    };

    private record CompleteRectangularBoxClueBuilder(ushort BoxWidth) : IClueBuilder<T, TCell>
    {

        /// <inheritdoc />
        public string Name => $"Complete Boxes Width {BoxWidth}";

        /// <inheritdoc />
        public int Level => 1;

        /// <inheritdoc />
        public IEnumerable<IClue<T, TCell>> CreateClues(Position minPosition, Position maxPosition, IValueSource<T, TCell> valueSource,
            IReadOnlyCollection<IClue<T, TCell>> lowerLevelClues)
        {
            var height = Convert.ToUInt16(maxPosition.Column / BoxWidth);
            var index = 1;
                
            for (var row = minPosition.Row; row <= maxPosition.Row; row+=height)
            for (var column = minPosition.Column; column <= maxPosition.Column; column+=BoxWidth)
                yield return new BoxClue<T, TCell>(new Position(column, row),new Position(column + BoxWidth - 1, row + height - 1), index++);
        }
            
        /// <inheritdoc />
        public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
        {
            var height = Convert.ToUInt16(maxPosition.Column / BoxWidth);

                
            for (var row = minPosition.Row; row <= maxPosition.Row; row+=height)
            for (var column = minPosition.Column; column <= maxPosition.Column; column+=BoxWidth)
                yield return
                    new RectangleCellOverlay(
                        new Position(column, row),
                        BoxWidth,
                        height,
                        Color.Black, 3
                    );
        }
    }
}


public class CompleteSquareBoxesVariantBuilder<T, TCell> : NoArgumentVariantBuilder<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    private CompleteSquareBoxesVariantBuilder()
    {
    }

    public static NoArgumentVariantBuilder<T, TCell> Instance { get; } = new CompleteSquareBoxesVariantBuilder<T, TCell>();

    /// <inheritdoc />
    public override string Name => "Complete Square Boxes";

    /// <inheritdoc />
    public override int Level => 1;

    /// <inheritdoc />
    public override IEnumerable<IClue<T, TCell>> CreateClues(Position minPosition, Position maxPosition, IValueSource<T, TCell> valueSource,
        IReadOnlyCollection<IClue<T, TCell>> lowerLevelClues)
    {
        var boxSize = Convert.ToUInt16(Math.Sqrt(maxPosition.Column));
        int index = 1;
        for (var row = minPosition.Row; row <= maxPosition.Row; row+=boxSize)
        for (var column = minPosition.Column; column <= maxPosition.Column; column+=boxSize)
            
            yield return new BoxClue<T, TCell>(new Position(column, row),new Position(column + boxSize - 1, row + boxSize - 1), index++);
    }
        

    /// <inheritdoc />
    public override bool OnByDefault => true;

    /// <inheritdoc />
    public override bool IsValid(Position maxPosition)
    {
        return maxPosition.Row == maxPosition.Column && IsPerfectSquare(maxPosition.Row);
    }

    private static bool IsPerfectSquare(ushort input)
    {
        var sqrt = Math.Sqrt(input);
        return Math.Abs(Math.Ceiling(sqrt) - Math.Floor(sqrt)) < double.Epsilon;
    }

    /// <param name="minPosition"></param>
    /// <param name="maxPosition"></param>
    /// <inheritdoc />
    public override IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
    {
        var boxSize = Convert.ToUInt16(Math.Sqrt(maxPosition.Column));

        for (var row = minPosition.Row; row <= maxPosition.Row; row+=boxSize)
        for (var column = minPosition.Column; column <= maxPosition.Column; column+=boxSize)
            
            yield return
                new RectangleCellOverlay(
                    new Position(column, row),
                    boxSize,
                    boxSize,
                    Color.Black, 3
                );
    }
}

public class CompleteRowsVariantBuilder<T, TCell> : NoArgumentVariantBuilder<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    private CompleteRowsVariantBuilder()
    {
    }

    public static IVariantBuilder<T, TCell> Instance { get; } = new CompleteRowsVariantBuilder<T, TCell>();
        
    public override string Name => "Complete Rows";

    public override int Level => 1;

    public override IEnumerable<IClue<T, TCell>> CreateClues(Position minPosition, Position maxPosition, IValueSource<T, TCell> valueSource,
        IReadOnlyCollection<IClue<T, TCell>> lowerLevelClues)
    {
        for (var r = minPosition.Row; r <= maxPosition.Row; r++)
            yield return new RowClue<T, TCell>(r, minPosition.Column,maxPosition.Column, null);
    }

    /// <inheritdoc />
    public override bool OnByDefault { get; } = true;

    /// <inheritdoc />
    public override bool IsValid(Position maxPosition)
    {
        return true;
    }
}
public class CompleteColumnsVariantBuilder<T, TCell> : NoArgumentVariantBuilder<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    private CompleteColumnsVariantBuilder()
    {
    }

    public static IVariantBuilder<T, TCell> Instance { get; } = new CompleteColumnsVariantBuilder<T, TCell>();


    /// <inheritdoc />
    public override string Name => "Complete Columns";

    /// <inheritdoc />
    public override int Level => 1;

    /// <inheritdoc />
    public override IEnumerable<IClue<T, TCell>> CreateClues(Position minPosition, Position maxPosition, IValueSource<T, TCell> valueSource,
        IReadOnlyCollection<IClue<T, TCell>> lowerLevelClues)
    {
        for (var c = minPosition.Column; c <= maxPosition.Column; c++)
            yield return new ColumnClue<T, TCell>(c, minPosition.Row,maxPosition.Row);
    }

    /// <inheritdoc />
    public override bool OnByDefault { get; } = true;

    /// <inheritdoc />
    public override bool IsValid(Position maxPosition)
    {
        return true;
    }
}
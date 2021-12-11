using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using CSharpFunctionalExtensions;
using GridPuzzles;
using GridPuzzles.Clues;
using GridPuzzles.Overlays;
using GridPuzzles.VariantBuilderArguments;
using Sudoku.Variants;

namespace Sudoku.Clues;

public class DisjointBoxesVariantBuilder<T> : NoArgumentVariantBuilder<T> where T: notnull
{
    private DisjointBoxesVariantBuilder() { }

    public static DisjointBoxesVariantBuilder<T> Instance { get; } = new ();

    /// <inheritdoc />
    public override string Name => "Disjoint Boxes";

    /// <inheritdoc />
    public override int Level => 2;

    /// <inheritdoc />
    public override IEnumerable<IClue<T>> CreateClues(Position minPosition, Position maxPosition, IValueSource<T> valueSource,
        IReadOnlyCollection<IClue<T>> lowerLevelClues)
    {
        var boxClues = lowerLevelClues.OfType<BoxClue<T>>().ToList();

        if (boxClues.Count == maxPosition.Row && boxClues.Count == maxPosition.Column)
        {
            var minLength = boxClues.Min(x => x.Positions.Count);

            if (minLength == maxPosition.Row)
            {
                for (var i = 0; i < minLength; i++)
                {
                    var i1 = i;
                    var cells = boxClues.Select(x => x.Positions[i1]).ToImmutableSortedSet();

                    yield return new UniqueCompleteClue<T>($"Position {i + 1} of any Box", cells);
                }
            }
        }
    }

    /// <inheritdoc />
    public override bool OnByDefault => false;
}


public class CompleteRectangularBoxVariantBuilder<T> :VariantBuilder<T> where T:notnull
{
    private CompleteRectangularBoxVariantBuilder()
    {
    }

    public static IVariantBuilder<T> Instance { get; } = new CompleteRectangularBoxVariantBuilder<T>();

    /// <inheritdoc />
    public override string Name => "Complete Rectangular Boxes";

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder<T>>> TryGetClueBuilders1(IReadOnlyDictionary<string, string> arguments)
    {
        var widthResult = _boxWidthArgument.TryGetFromDictionary(arguments);

        if (widthResult.IsFailure) return widthResult.ConvertFailure<IReadOnlyCollection<IClueBuilder<T>>>();

        return new IClueBuilder<T>[]
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

    private class CompleteRectangularBoxClueBuilder : IClueBuilder<T>
    {
        public ushort BoxWidth { get; }

        public CompleteRectangularBoxClueBuilder(ushort boxWidth)
        {
            BoxWidth = boxWidth;
        }

        /// <inheritdoc />
        public string Name => $"Complete Boxes Width {BoxWidth}";

        /// <inheritdoc />
        public int Level => 1;

        /// <inheritdoc />
        public IEnumerable<IClue<T>> CreateClues(Position minPosition, Position maxPosition, IValueSource<T> valueSource,
            IReadOnlyCollection<IClue<T>> lowerLevelClues)
        {
            var height = Convert.ToUInt16(maxPosition.Column / BoxWidth);
            var index = 1;
                
            for (var row = minPosition.Row; row <= maxPosition.Row; row+=height)
            for (var column = minPosition.Column; column <= maxPosition.Column; column+=BoxWidth)
                yield return new BoxClue<T>(new Position(column, row),new Position(column + BoxWidth - 1, row + height - 1), index++);
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


public class CompleteSquareBoxesVariantBuilder<T> : NoArgumentVariantBuilder<T> where T :notnull
{
    private CompleteSquareBoxesVariantBuilder()
    {
    }

    public static NoArgumentVariantBuilder<T> Instance { get; } = new CompleteSquareBoxesVariantBuilder<T>();

    /// <inheritdoc />
    public override string Name => "Complete Square Boxes";

    /// <inheritdoc />
    public override int Level => 1;

    /// <inheritdoc />
    public override IEnumerable<IClue<T>> CreateClues(Position minPosition, Position maxPosition, IValueSource<T> valueSource,
        IReadOnlyCollection<IClue<T>> lowerLevelClues)
    {
        var boxSize = Convert.ToUInt16(Math.Sqrt(maxPosition.Column));
        int index = 1;
        for (var row = minPosition.Row; row <= maxPosition.Row; row+=boxSize)
        for (var column = minPosition.Column; column <= maxPosition.Column; column+=boxSize)
            
            yield return new BoxClue<T>(new Position(column, row),new Position(column + boxSize - 1, row + boxSize - 1), index++);
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

public class CompleteRowsVariantBuilder<T> : NoArgumentVariantBuilder<T> where T : notnull
{
    private CompleteRowsVariantBuilder()
    {
    }

    public static IVariantBuilder<T> Instance { get; } = new CompleteRowsVariantBuilder<T>();
        
    public override string Name => "Complete Rows";

    public override int Level => 1;

    public override IEnumerable<IClue<T>> CreateClues(Position minPosition, Position maxPosition, IValueSource<T> valueSource,
        IReadOnlyCollection<IClue<T>> lowerLevelClues)
    {
        for (var r = minPosition.Row; r <= maxPosition.Row; r++)
            yield return new RowClue<T>(r, minPosition.Column,maxPosition.Column, null);
    }

    /// <inheritdoc />
    public override bool OnByDefault { get; } = true;

    /// <inheritdoc />
    public override bool IsValid(Position maxPosition)
    {
        return true;
    }
}
public class CompleteColumnsVariantBuilder<T> : NoArgumentVariantBuilder<T> where T: notnull
{
    private CompleteColumnsVariantBuilder()
    {
    }

    public static IVariantBuilder<T> Instance { get; } = new CompleteColumnsVariantBuilder<T>();


    /// <inheritdoc />
    public override string Name => "Complete Columns";

    /// <inheritdoc />
    public override int Level => 1;

    /// <inheritdoc />
    public override IEnumerable<IClue<T>> CreateClues(Position minPosition, Position maxPosition, IValueSource<T> valueSource,
        IReadOnlyCollection<IClue<T>> lowerLevelClues)
    {
        for (var c = minPosition.Column; c <= maxPosition.Column; c++)
            yield return new ColumnClue<T>(c, minPosition.Row,maxPosition.Row);
    }

    /// <inheritdoc />
    public override bool OnByDefault { get; } = true;

    /// <inheritdoc />
    public override bool IsValid(Position maxPosition)
    {
        return true;
    }
}
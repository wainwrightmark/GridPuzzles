using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using GridPuzzles.Clues;
using GridPuzzles.Overlays;

namespace GridPuzzles;

public class Grid<T, TCell> : IGrid where T :struct where TCell : ICell<T, TCell>, new()
{
    private Grid(ImmutableArray<TCell> cells,
        ClueSource<T, TCell> clueSource,
        ImmutableArray<Position> allPositions)
    {
        CellArray = cells;
        AllPositions = allPositions;
        ClueSource = clueSource;
        NumberOfFixedCells = new Lazy<int>(() => cells.Count(x=> x.HasSingleValue()));
        UniqueString = new Lazy<string>(() =>
            string.Join("",
                CellArray.Select((x, i) => (x, i)).Where(x=>!Equals(x.x, clueSource.ValueSource.AnyValueCell))
                    .Select(x=>x.ToString())));
        MaxPosition = allPositions.Last();

        LazyOverlays = new Lazy<IReadOnlyList<CellOverlayWrapper>>(() =>
            ClueSource.FixedOverlays
                .Concat(ClueSource.DynamicOverlayClueHelper.CreateCellOverlays(this))
                    
                .ToList());

        LazyData = new Lazy<IReadOnlyDictionary<string, object>>(()=>ClueSource.GetLazyData(this));
    }

    public TCell GetCell(Position p) => CellArray[GetCellIndex(p, MaxPosition)];

    private ImmutableArray<TCell> CellArray { get; }

    public ClueSource<T, TCell> ClueSource { get; }
    public ImmutableArray<Position> AllPositions { get; }
    public Position MaxPosition { get; }
    public Lazy<int> NumberOfFixedCells { get; }
    public Lazy<string> UniqueString { get; }

    public Lazy<IReadOnlyDictionary<string, object>> LazyData { get; }
    private Lazy<IReadOnlyList<CellOverlayWrapper>> LazyOverlays { get; }

    public bool IsComplete => NumberOfFixedCells.Value >= AllPositions.Length;

    private static int GetCellIndex(Position position, Position maxPosition)
    {
        var r = ((position.Row - 1) * maxPosition.Column) + position.Column - 1;

        return r;
    }

    private static Position GetPositionFromIndex(int index, Position maxPosition)
    {
        var c = (index % maxPosition.Column) + 1;
        var r = (index / maxPosition.Row) + 1;
        return new Position(c, r);
    }

    /// <inheritdoc />
    [Pure]
    public IEnumerable<CellOverlayWrapper> GetOverlays() => LazyOverlays.Value;
    /// <summary>
    /// Creates a new grid with these cells. Other positions will have empty cells.
    /// </summary>
    public static Grid<T, TCell> Create(IEnumerable<KeyValuePair<Position, TCell>>? cells,
        Position maxPosition,
        ClueSource<T, TCell> clueSource)
    {

        var l = GetCellIndex(maxPosition, maxPosition) + 1;
        var array = Enumerable.Repeat(clueSource.ValueSource.AnyValueCell, l).ToArray();

        if (cells is not null)
        {
            foreach (var (key, value) in cells)
            {
                var index = GetCellIndex(key, maxPosition);
                array[index] = value;
            }
        }

        //var dict = (cells ?? Enumerable.Empty<KeyValuePair<Position, TCell>>())
        //    .Where(x => !x.Value.CouldHaveAnyValue(clueSource.ValueSource))
        //    .ToImmutableSortedDictionary(x => x.Key, x => x.Value);

        var allPositions = maxPosition
            .GetPositionsUpTo(true)
            .SelectMany(x => x).ToImmutableArray();

        return new Grid<T, TCell>(array.ToImmutableArray(), clueSource, allPositions);
    }

    [Pure]
    public Grid<T, TCell> CloneWithClueSource(ClueSource<T, TCell> clueSource)
    {
        var g = new Grid<T, TCell>(CellArray, clueSource, AllPositions).CloneWithUpdates(UpdateResult<T, TCell>.Empty, true);
        return g;
    }

    /// <summary>
    /// Clones the grid with only cells which have a fixed value and without any of the provided positions
    /// </summary>
    [Pure]
    public Grid<T, TCell> CloneWithoutCells(IEnumerable<Position> positions)
    {
        var array = CellArray.Select(x=> x.HasSingleValue()? x : ClueSource.ValueSource.AnyValueCell) .ToArray();

        foreach (var position in positions)
        {
            var index = GetCellIndex(position, MaxPosition);
            array[index] = ClueSource.ValueSource.AnyValueCell;
        }
        

        return new Grid<T, TCell>(array.ToImmutableArray(), ClueSource, AllPositions);
    }

    [Pure]
    public Grid<T, TCell> CloneWithUpdates(UpdateResult<T, TCell> updateResult, bool onlyFixedValues)
    {
        if (updateResult.IsEmpty && !onlyFixedValues)
            return this;


        var newCellArray = CellArray.ToArray();
        foreach (var (position, cellUpdate) in updateResult.UpdatedCells)
        {
            newCellArray[GetCellIndex(position, MaxPosition)] = cellUpdate.NewCell;
        }

        var newCells = newCellArray.ToImmutableArray();

        if (onlyFixedValues)
        {
            newCells = newCells.Select(x => x.HasSingleValue() ? x : ClueSource.ValueSource.AnyValueCell)
                .ToImmutableArray();
        }

        return new Grid<T, TCell>(newCells, ClueSource, AllPositions);
    }

    public string ToDisplayString()
    {
        var shortStrings = MaxPosition
            .GetPositionsUpTo(true).Select(row => row.Select(p => GetCell(p).ToString()!).ToList()).ToList();

        var longestString = shortStrings.SelectMany(x => x).Max(x => x.Length);

        var rows = shortStrings.Select(row => string.Join("", row.Select(PadString)));


        return string.Join("\r\n", rows);

        string PadString(string s)
        {
            var lPad = (longestString - s.Length) / 2;
            return (new string(' ', lPad) + s).PadRight(longestString);
        }
    }

    public string ToSimpleDisplayString()
    {
        var shortStrings = MaxPosition
            .GetPositionsUpTo(true).Select(row => row.Select(p => GetChar(GetCell(p))).ToList()).ToList();
        var rows = shortStrings.Select(row => string.Join("\t", row));

        return string.Join("\n", rows);

        static char GetChar(TCell cell)
        {
            return cell.HasSingleValue() ? cell.Single().ToString()!.First() : '-';
        }
    }

    public static Result<Grid<T, TCell>> CreateFromString(string s, ClueSource<T, TCell> clueSource, Position maxPosition)
    {
        s = Regex.Replace(s, @"\s", "");

        if (s.Length != maxPosition.GetTotalPositionsUpTo())
            return Result.Failure<Grid<T, TCell>>(
                $"Expected {maxPosition.GetTotalPositionsUpTo()} cells but got {s.Length}");
        

        var cells = s.Select(clueSource.ValueSource.TryParse)
            .Combine()
            .Map(x =>
                x.Select(a => a.Map(CellHelper.Create<T, TCell>).GetValueOrDefault(clueSource.ValueSource.AnyValueCell))
                    .ToImmutableArray());

        if (cells.IsFailure) return cells.ConvertFailure<Grid<T, TCell>>();

        
        var allPositions = maxPosition
            .GetPositionsUpTo(true)
            .SelectMany(x => x).ToImmutableArray();

        return new Grid<T, TCell>(cells.Value, clueSource, allPositions);
    }


    /// <inheritdoc />
    public override string ToString() => $"{NumberOfFixedCells.Value} Fixed Positions";
        
    public Dictionary<T, T>? GetSymmetries(int length)
    {
        var symmetries = new Dictionary<T, T>();

        for (var index = 0; index < CellArray.Length; index++)
        {
            var cell = CellArray[index];

            if(!cell.HasSingleValue())continue;


            var position = GetPositionFromIndex(index, MaxPosition);
            var opposite = position.GetOpposite(length);
            var oppositeCell = GetCell(opposite);
            if(!oppositeCell.HasSingleValue())continue;

            var val1 = cell.Single();
            var val2 = oppositeCell.Single();
                    
            if (symmetries.TryGetValue(val1, out var symmetry1))
            {
                if (!val2.Equals(symmetry1)) return null;
            }
            else
                symmetries.Add(val1, val2);

            if (symmetries.TryGetValue(val2, out var symmetry2))
            {
                if (!val1.Equals(symmetry2)) return null;
            }
            else
                symmetries.Add(val2, val1);

        }

        return symmetries;
    }


    /// <summary>
    /// Gets all cells, including those with the default value
    /// </summary>
    public IEnumerable<KeyValuePair<Position, TCell>> AllCells => AllPositions.Select(GetCellKVP);


    public IEnumerable<KeyValuePair<Position, TCell>> AllModifiedCells => AllCells.Where(x=>!x.Value.CouldHaveAnyValue(ClueSource.ValueSource));

    public KeyValuePair<Position, TCell> GetCellKVP(Position position) => new(position, GetCell(position));

    /// <inheritdoc />
    public override int GetHashCode() => UniqueString.Value.GetHashCode();

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Grid<T, TCell> g && UniqueString.Value.Equals(g.UniqueString.Value);

        
    public IEnumerable<(Position position, string text)> GetOuterIndicators()
    {
        for (var column = 1; column <= MaxPosition.Column; column++)
        {
            var position = new Position(column, 0);
            yield return (position, ((char)('A' + column - 1)).ToString());
        }

        for (var row = 1; row <= MaxPosition.Row; row++)
        {
            var position = new Position(0, row);
            yield return (position, row.ToString());
        }
    }

    /// <inheritdoc />
    public int GetPossibleValueCount(Position position) => GetCell(position).Count();
}

public record CellOverlayMetadata(Maybe<IClueBuilder> ClueBuilder, bool AlwaysSelected)
{
    public static CellOverlayMetadata Empty { get; } = new (Maybe<IClueBuilder>.None, false);
}

public record CellOverlayWrapper(ICellOverlay CellOverlay, CellOverlayMetadata Metadata);


public interface IGrid
{
    [Pure]
    IEnumerable<CellOverlayWrapper> GetOverlays();
        

    [Pure]
    ImmutableArray<Position> AllPositions { get; }

    [Pure] Position MaxPosition { get; }

    /// <summary>
    /// Get the external indicator numbers and letters
    /// </summary>
    [Pure]
    IEnumerable<(Position position, string text)> GetOuterIndicators();

    public int GetPossibleValueCount(Position position);
}
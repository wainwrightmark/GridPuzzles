using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using GridPuzzles.Clues;
using GridPuzzles.Overlays;

namespace GridPuzzles;

public class Grid<T, TCell> : IGrid where T :struct where TCell : ICell<T, TCell>, new()
{
    private Grid(IReadOnlyDictionary<Position, TCell> cells,
        ClueSource<T, TCell> clueSource,
        ImmutableArray<Position> allPositions)
    {
        Cells = cells;
        AllPositions = allPositions;
        ClueSource = clueSource;
        NumberOfFixedCells = new Lazy<int>(() => cells.Values.Count(x => x.HasSingleValue()));
        UniqueString = new Lazy<string>(() =>
            string.Join("",
                Cells.Select(x => x.ToString())
            ));
        MaxPosition = allPositions.Last();

        LazyOverlays = new Lazy<IReadOnlyList<CellOverlayWrapper>>(() =>
            ClueSource.FixedOverlays
                .Concat(ClueSource.DynamicOverlayClueHelper.CreateCellOverlays(this))
                    
                .ToList());
    }

    public TCell GetCell(Position p) => Cells.TryGetValue(p, out var c) ? c : ClueSource.ValueSource.AnyValueCell;
    public IReadOnlyDictionary<Position, TCell> Cells { get; }
    public ClueSource<T, TCell> ClueSource { get; }
    public ImmutableArray<Position> AllPositions { get; }
    public Position MaxPosition { get; }
    public Lazy<int> NumberOfFixedCells { get; }
    public Lazy<string> UniqueString { get; }
    private Lazy<IReadOnlyList<CellOverlayWrapper>> LazyOverlays { get; }

    public bool IsComplete => NumberOfFixedCells.Value >= AllPositions.Length;

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
        var dict = (cells ?? Enumerable.Empty<KeyValuePair<Position, TCell>>())
            .Where(x => !x.Value.CouldHaveAnyValue(clueSource.ValueSource))
            .ToImmutableSortedDictionary(x => x.Key, x => x.Value);

        var allPositions = maxPosition
            .GetPositionsUpTo(true)
            .SelectMany(x => x).ToImmutableArray();

        return new Grid<T, TCell>(dict, clueSource, allPositions);
    }

    [Pure]
    public Grid<T, TCell> CloneWithClueSource(ClueSource<T, TCell> clueSource)
    {
        var g = new Grid<T, TCell>(Cells, clueSource, AllPositions).CloneWithUpdates(UpdateResult<T, TCell>.Empty, true);
        return g;
    }

    [Pure]
    public Grid<T, TCell> CloneWithoutCells(IEnumerable<Position> positions)
    {
        var badPositions = positions.Concat(Cells.Where(x => x.Value.Count() > 1).Select(x=>x.Key)).ToHashSet();

        var newCells = Cells.Where(x=>!badPositions.Contains(x.Key))
            .ToDictionary(x=>x.Key, x=>x.Value);

        return new Grid<T, TCell>(newCells, ClueSource, AllPositions);
    }

    [Pure]
    public Grid<T, TCell> CloneWithUpdates(UpdateResult<T, TCell> updateResult, bool onlyFixedValues)
    {
        if (updateResult.IsEmpty && !onlyFixedValues)
            return this;

        IReadOnlyDictionary<Position, TCell> newCells;

        if (onlyFixedValues)
        {
            if (!updateResult.UpdatedCells.Any())
                newCells = Cells.Where(x=>x.Value.HasSingleValue())
                    .ToDictionary(x=>x.Key, x=>x.Value);
            else if (!Cells.Any())
                newCells = updateResult.UpdatedCells.Where(x => x.Value.NewCell.HasSingleValue())
                    .ToDictionary(x => x.Key, x => x.Value.NewCell);
            else
            {
                newCells = Cells
                    .Concat(updateResult.UpdatedCells.Select(x=> new KeyValuePair<Position, TCell>(x.Key, x.Value.NewCell)))
                    .Where(x=>x.Value.HasSingleValue())
                    .GroupBy(x=>x.Key, x=>x.Value) 
                    .ToDictionary(x=>x.Key, x=>x.Last());
            }
        }
        else
        {
            if (!updateResult.UpdatedCells.Any()) newCells = Cells;
            else if (!Cells.Any())
                newCells = updateResult.UpdatedCells
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value.NewCell);
            else
            {
                var intermediate = new Dictionary<Position, TCell>(Cells);

                foreach (var (position, value) in updateResult.UpdatedCells)
                {
                    intermediate[position] = value.NewCell;
                }

                newCells = intermediate;
            }
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

        ushort colNumber = 1;
        ushort rowNumber = 1;

        var cells = ImmutableSortedDictionary<Position, TCell>.Empty.ToBuilder();

        foreach (var parseResult in s.Select(clueSource.ValueSource.TryParse))
        {
            if (parseResult.IsFailure)
                return parseResult.ConvertFailure<Grid<T, TCell>>();

            var position = new Position(colNumber, rowNumber);
            if (parseResult.Value.HasValue)
                cells.Add(position, CellHelper.Create<T, TCell>(parseResult.Value.Value));

            colNumber++;
            if (colNumber > maxPosition.Column)
            {
                colNumber = 1;
                rowNumber++;
            }
        }

        var allPositions = maxPosition
            .GetPositionsUpTo(true)
            .SelectMany(x => x).ToImmutableArray();

        return new Grid<T, TCell>(cells.ToImmutable(), clueSource, allPositions);
    }


    /// <inheritdoc />
    public override string ToString() => $"{NumberOfFixedCells.Value} Fixed Positions";
        
    public Dictionary<T, T>? GetSymmetries(int length)
    {
        var symmetries = new Dictionary<T, T>();

        foreach (var (position, cell) in Cells.Where(x => x.Value.HasSingleValue()))
        {
            var opposite = position.GetOpposite(length);
            if (Cells.TryGetValue(opposite, out var other) && other.HasSingleValue())
            {
                var val1 = cell.Single();
                var val2 = other.Single();
                    
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
            else return null;
        }

        return symmetries;
    }


    /// <summary>
    /// Gets all cells, including those with the default value
    /// </summary>
    public IEnumerable<KeyValuePair<Position, TCell>> AllCells => AllPositions.Select(GetCellKVP);

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
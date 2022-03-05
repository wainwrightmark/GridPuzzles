using System.Threading;
using System.Threading.Tasks;
using GridPuzzles.Clues;
using GridPuzzles.VariantBuilderArguments;
using YamlDotNet.Serialization;

#pragma warning disable 8618

namespace GridPuzzles.Yaml;

public sealed class SerializableGrid
{
    [YamlMember(Order = 1)]
    public GridType GridType { get; set; }

    /// <summary>
    /// The number of columns.
    /// </summary>
    [YamlMember(Order = 2)]
    public int Columns { get;set; }

    /// <summary>
    /// The number of rows.
    /// </summary>
    [YamlMember(Order = 3)]
    public int Rows { get; set;}


    /// <summary>
    /// String representation of the grid.
    /// </summary>
    [YamlMember (Order = 4)]
    public string Grid { get; set;}


    [YamlMember(Order = 5)]
    public List<Variant> Variants { get; set; }

    [YamlMember(Order = 6)]
    public string? ExtraText { get; set; }
}

public enum GridType
{
    Number,
    Letter
}


public sealed class Variant
{
    [YamlMember(Order = 1)]
    public string Name { get; set; }

    [YamlMember(Order = 2)]
    public Dictionary<string, string>? Arguments { get; set; }
}


public static class YamlHelper
{
    public static string Serialize(this SerializableGrid grid)
    {
        var serializer = new Serializer();
        var result = serializer.Serialize(grid);

        return result;
    }

    public static Result<SerializableGrid> DeserializeGrid(string s)
    {
        var deserializer = new Deserializer();
        SerializableGrid grid;
        try
        {
            grid = deserializer.Deserialize<SerializableGrid>(s);
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception e)
        {
            return Result.Failure<SerializableGrid>(e.Message);
        }
#pragma warning restore CA1031 // Do not catch general exception types

        return grid;
    }

    public static SerializableGrid ToSerializable<T, TCell>(this Grid<T, TCell> grid,
        Func<Grid<T, TCell>, string?> getExtra,
        IEnumerable<VariantBuilderArgumentPair<T, TCell>> variantBuilderArgumentPairs)where T :struct where TCell : ICell<T, TCell>, new()
    {
        var gridType = grid switch
        {
            Grid<int, IntCell> _ => GridType.Number,
            Grid<char, CharCell> _ => GridType.Letter,
            _ => throw new ArgumentOutOfRangeException(nameof(grid))
        };

        var extraText = getExtra(grid);
        var gridText = grid.ToSimpleDisplayString();
        var variants = variantBuilderArgumentPairs
            .OrderBy(x=>x.VariantBuilder.Name)
                
            .Select(Make)
            .ToList();

        return new SerializableGrid
        {
            Columns = grid.MaxPosition.Column,
            Rows = grid.MaxPosition.Row,
            Grid = gridText,
            GridType = gridType,
            Variants = variants.Any()?variants : new List<Variant>(),
            ExtraText = extraText
        };
    }

    public static async Task<Result<(Grid<T, TCell> grid, IReadOnlyCollection<VariantBuilderArgumentPair<T, TCell>> variants)>>  Convert<T, TCell>(
        this SerializableGrid serializableGrid,
        IValueSource<T, TCell> vs,
        IReadOnlyDictionary<string, IVariantBuilder<T, TCell>> possibleVariantBuilders, CancellationToken cancellation)
        where T :struct where TCell : ICell<T, TCell>, new()
    {

        var variants = await serializableGrid.Variants
            .Select(x => DeserializeAsync(x, possibleVariantBuilders, cancellation))
            .Combine()
            .Map(x => x.ToList());
        if (variants.IsFailure)
            return variants.ConvertFailure<(Grid<T, TCell> grid, IReadOnlyCollection<VariantBuilderArgumentPair<T, TCell>> variants)>();

        var maxPosition = new Position(serializableGrid.Columns, serializableGrid.Rows);

        var clueSourceResult = await variants.Value
            .Select(x => x.VariantBuilder.TryGetClueBuildersAsync(x.Pairs, cancellation))
            .Combine()
            .Map(x => x.SelectMany(y => y))
            .Map(x => new ClueSource<T, TCell>(Position.Origin, maxPosition, vs, x.ToArray()));

        if (clueSourceResult.IsFailure)
            return clueSourceResult.ConvertFailure<(Grid<T, TCell> grid, IReadOnlyCollection<VariantBuilderArgumentPair<T, TCell>> variants)>();

        var gridResult = Grid<T, TCell>.CreateFromString(serializableGrid.Grid, clueSourceResult.Value, maxPosition);

        if(gridResult.IsFailure)
            return gridResult.ConvertFailure<(Grid<T, TCell> grid, IReadOnlyCollection<VariantBuilderArgumentPair<T, TCell>> variants)>();

        return Result.Success<(Grid<T, TCell> grid, IReadOnlyCollection<VariantBuilderArgumentPair<T, TCell>> variants)>((gridResult.Value, variants.Value));

    }

    private static async Task<Result<VariantBuilderArgumentPair<T, TCell>>> DeserializeAsync<T, TCell>(Variant variant,
        IReadOnlyDictionary<string, IVariantBuilder<T, TCell>> possibleVariantBuilders, CancellationToken cancellation)where T :struct where TCell : ICell<T, TCell>, new()
    {
        if (!possibleVariantBuilders.TryGetValue(variant.Name, out var variantBuilder))
            return Result.Failure<VariantBuilderArgumentPair<T, TCell>>($"Could not parse variant {variant.Name}");

        var arguments = variant.Arguments ?? new Dictionary<string, string>();

        var r = await variantBuilder.TryGetClueBuildersAsync(arguments, cancellation);
        return r.IsFailure ? r.ConvertFailure<VariantBuilderArgumentPair<T, TCell>>() :
            new VariantBuilderArgumentPair<T, TCell>(variantBuilder, arguments);

    }

    private static Variant Make<T, TCell>(VariantBuilderArgumentPair<T, TCell> variantBuilderArgumentPair)where T :struct where TCell : ICell<T, TCell>, new()
    {
        var arguments = variantBuilderArgumentPair.Pairs.ToDictionary(x => x.Key, x => x.Value);

        return new Variant
        {
            Name = variantBuilderArgumentPair.VariantBuilder.Name,
            Arguments = arguments.Any()? arguments : null
        };
    }


}
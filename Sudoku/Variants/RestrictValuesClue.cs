using Sudoku.Clues;

namespace Sudoku.Variants;

public class RestrictValuesVariantBuilder<T, TCell> : VariantBuilder<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    private RestrictValuesVariantBuilder() { }

    public static IVariantBuilder<T, TCell> Instance { get; } = new RestrictValuesVariantBuilder<T, TCell>();

    /// <inheritdoc />
    public override string Name => "Restrict Values";

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder<T, TCell>>> TryGetClueBuilders1(IReadOnlyDictionary<string, string> arguments)
    {
        var positionArgumentsResult = ListPositionArgument.TryGetFromDictionary(arguments);
        if (positionArgumentsResult.IsFailure)
            return positionArgumentsResult.ConvertFailure<IReadOnlyCollection<IClueBuilder<T, TCell>>>();


        var valueArgumentResult = ValuesArgument.TryGetFromDictionary(arguments).Bind(DeserializeValues);
        if (valueArgumentResult.IsFailure)
            return valueArgumentResult.ConvertFailure<IReadOnlyCollection<IClueBuilder<T, TCell>>>();



        var l = new List<IClueBuilder<T, TCell>>
        {
            new RestrictValuesClueBuilder<T, TCell>(positionArgumentsResult.Value.ToImmutableSortedSet(), valueArgumentResult.Value)
        };

        return l;
    }

    private static Result<ImmutableHashSet<T>> DeserializeValues(string s)
    {
        if (typeof(T) == typeof(int))
        {
            var values = s.Split(",")
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x =>
                    int.TryParse(x, out var i)
                        ? Result.Success(i)
                        : Result.Failure<int>($"{i} is not a number"))
                .Combine()
                .Map(x => x.ToImmutableHashSet())
                .Map(x =>  (x as ImmutableHashSet<T>)!);

            return values;
        }
        else if (typeof(T) == typeof(char))
        {
            var values = s.Split(",")
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x =>
                    char.TryParse(x, out var i)
                        ? Result.Success(i)
                        : Result.Failure<char>($"{i} is not a character"))
                .Combine()
                .Map(x => x.ToImmutableHashSet())
                .Map(x => (x as ImmutableHashSet<T>)!);

            return values;
        }

        return Result.Failure<ImmutableHashSet<T>>($"Cannot make list of {typeof(T).Name}");
    }

    public static readonly ListPositionArgument ListPositionArgument = new("Positions", 1, 81);

    public static readonly StringArgument ValuesArgument = new("Allowed Values (Comma Separated)", Maybe<string>.None);

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments =>
        new List<VariantBuilderArgument>()
        {
            ListPositionArgument,
            ValuesArgument
        };
}

[Equatable]
public partial record RestrictValuesClueBuilder<T, TCell>([property:SetEquality] ImmutableSortedSet<Position> Positions,[property:SetEquality] ImmutableHashSet<T> Values ) : IClueBuilder<T, TCell>where T :struct where TCell : ICell<T, TCell>, new()
{

    /// <inheritdoc />
    public string Name => "Restrict Values";

    /// <inheritdoc />
    public int Level => 2;

    /// <inheritdoc />
    public IEnumerable<IClue<T, TCell>> CreateClues(Position minPosition, Position maxPosition, IValueSource<T, TCell> valueSource,
        IReadOnlyCollection<IClue<T, TCell>> lowerLevelClues)
    {
        yield return new RestrictedValuesClue<T, TCell>("Restricted Values", Positions, Values);
    }

    /// <param name="minPosition"></param>
    /// <param name="maxPosition"></param>
    /// <inheritdoc />
    public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
    {
        if (Values is IReadOnlyCollection<int> intValues  && Values.Count >= maxPosition.Column / 2)
        {
            if (intValues.All(x => x % 2 == 0))
            {
                foreach (var position in Positions)
                {
                    yield return new InsideRectCellOverlay(position, Color.Gray);
                }
                yield break;
            }
            else if (intValues.All(x => x % 2 == 1))
            {
                foreach (var position in Positions)
                {
                    yield return new InsideCircleCellOverlay(position, Color.Gray);
                }
                yield break;
            }
        }
        foreach (var position in Positions)
        {
            yield return new CellColorOverlay(ClueColors.RestrictedValueColor, position);
        }
    }
}
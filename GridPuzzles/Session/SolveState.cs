using System.Diagnostics.Contracts;
using System.Drawing;
using GridPuzzles.Clues;
using GridPuzzles.Overlays;
using GridPuzzles.VariantBuilderArguments;
using SVGElements;

namespace GridPuzzles.Session;

public interface ISolveState
{
    IUpdateResult UpdateResult { get; }

    ChangeType ChangeType { get; }
    string Message { get; }
    TimeSpan Duration { get; }
    IGrid Grid { get; }
    IEnumerable<Position> FixedValuePositions { get; }

    Maybe<SVGText> GetCellText(Position position, int minimumValuesToShow, int scale);
}

public enum ChangeType
{
    NoChange,
    Error,
    InitialState,
    LogicalMove,
    Undo,
    ManualChange,
    RandomMove,
    VariantChange,
    GoToState
}

public record SolveState<T>(Grid<T> Grid,
        ImmutableHashSet<VariantBuilderArgumentPair<T>> VariantBuilders,
        UpdateResult<T> UpdateResult,
        ChangeType ChangeType,
        string Message,
        TimeSpan Duration,
        ImmutableSortedDictionary<Position, T> FixedValues,
        Grid<T>? Previous)
    : ISolveState  where T:notnull
{
    /// <inheritdoc />
    IUpdateResult ISolveState.UpdateResult => UpdateResult;

    /// <inheritdoc />
    IGrid ISolveState.Grid => Grid;

    /// <inheritdoc />
    public IEnumerable<Position> FixedValuePositions => FixedValues.Keys;

    private static IEnumerable<(string text, bool strikeThrough, bool isOnNewLine)> GroupText(
        IValueSource<T> valueSource, 
        ImmutableSortedSet<T> goodValues,
        ImmutableSortedSet<T> badValues)
    {
        var strikeThrough = true;
        var totalCharacters = 0;
        var nextIsNewLine = true;
        var s = "";
        foreach (var value in valueSource.AllValues)
        {
            if (totalCharacters % 5 == 0)
            {
                if (!string.IsNullOrWhiteSpace(s)) //end this line
                {
                    yield return (s, strikeThrough, nextIsNewLine);
                }
                        
                s ="";
                nextIsNewLine = true;
            }

            bool? cgStrikeThrough = null;
            if (badValues.Contains(value))
                cgStrikeThrough = true;
            else if (goodValues.Contains(value))
                cgStrikeThrough = false;

            if (cgStrikeThrough.HasValue)
            {
                if (cgStrikeThrough.Value == strikeThrough)
                {
                    s += value;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        yield return (s, strikeThrough, nextIsNewLine);
                        nextIsNewLine = false;
                    }

                    s = "";
                    s += value;
                    strikeThrough = cgStrikeThrough.Value;
                }
            }
            else if(!s.EndsWith(' '))
            {
                s += ' ';
            }

            totalCharacters++;
        }

        if (!string.IsNullOrWhiteSpace(s))
            yield return (s, strikeThrough, nextIsNewLine);
    }

    [Pure]
    public Maybe<SVGText> GetCellText(Position position, int minimumValuesToShow, int scale)
    {
        var cell = Grid.GetCell(position);

        if (Previous is not null)
        {
            if (UpdateResult.UpdatedCells.ContainsKey(position))
            {

                var previousCell = Previous.GetCell(position);
                var deletedPositions = previousCell.PossibleValues.Except(cell.PossibleValues);
                if (deletedPositions.Any() && previousCell.PossibleValues.Count <= minimumValuesToShow)
                {
                    var xPosition = position.GetX(true, scale);
                    var groups = GroupText(Grid.ClueSource.ValueSource,
                        cell.PossibleValues, deletedPositions);

                    var spans = groups.Select((x, i) =>
                        {
                            var (text, strikeThrough, newLine) = x;
                            if (previousCell.PossibleValues.Count < 5)
                                newLine = false;
                            var id = strikeThrough ? $"st{i}" : $"norm{i}";
                            if (newLine) id += "nl";
                            var fill = strikeThrough ? "red" : "white";
                            var td = strikeThrough ? "line-through" : null;
                            double? spanXPosition = newLine ? xPosition : null;
                            string? dy = newLine && i != 0 ? "1.2em" : null;
                            return new SVGTextSpan(id, 

                                text, 
                                X:spanXPosition,
                                DY:dy,
                                Fill: fill,
                                TextDecoration: td);
                        }
                    ).ToList();

                    var fontSize = GetFontSize(previousCell.PossibleValues.Count);

                    return
                        new SVGText(
                            "CellTextU" + position,
                            null,
                            spans,
                            xPosition,
                            position.GetY(true, scale),
                            PointerEvents: PointerEvents.none,
                            TextAnchor: TextAnchor.middle, DominantBaseline: DominantBaseline.middle,
                            FontSize: fontSize,
                            FontWeight: "bolder",
                            Stroke: "black"
                        );
                }

            }
        }

            

        if (cell.PossibleValues.Count == 1)
        {
            var color = Grid.ClueSource.ValueSource.GetColor(cell.PossibleValues.Single());
            if(color is null && FixedValues.ContainsKey(position))
                color = Color.Blue;


            return  SimpleString(cell.PossibleValues.Single().ToString()!.Trim(), position, scale, color);
        }
                
        if (cell.PossibleValues.Count <= minimumValuesToShow)
            return SimpleString(cell.GetDisplayString(Grid.ClueSource.ValueSource), position, scale, null);

        return Maybe<SVGText>.None;

        static string GetFontSize(int length)
        {
            return length switch
            {
                1 => "xx-large",
                2 => "x-large",
                3 => "large",
                < 10 => "medium",
                _ => "small"
            };
        }

        static Maybe<SVGText> SimpleString(string s, Position position, int scale, Color? color)
        {
            if (string.IsNullOrWhiteSpace(s))
                return Maybe<SVGText>.None;

            string fontSize = GetFontSize(s.Length);

            string fill = color?.ToSVGColor()??"white";

            return
                new SVGText(
                    "CellText" + position,
                    s,
                    null,
                    position.GetX(true, scale),
                    position.GetY(true, scale),
                    PointerEvents: PointerEvents.none,
                    TextAnchor: TextAnchor.middle, DominantBaseline: DominantBaseline.middle,
                    FontSize: fontSize,
                    FontWeight: "bolder",
                    Fill: fill,
                    Stroke: "black"
                );
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using GridPuzzles;
using GridPuzzles.Cells;

namespace Crossword;

public static class Extensions
{
    public static bool MustNotBeABlock(this KeyValuePair<Position, Cell<char>> cell) =>
        cell.Value.MustNotBeABlock();

    public static bool MustNotBeABlock(this Cell<char> cell) =>
        !cell.PossibleValues.Contains(CrosswordValueSource.BlockChar);


    public static bool MustBeABlock(this Cell<char> cell) =>
        cell.PossibleValues.Count == 1 &&
        cell.PossibleValues.Single().Equals(CrosswordValueSource.BlockChar);

    public static bool CouldBeAnyLetter(this Cell<char> cell) =>
        cell.CouldHaveAnyValue(CrosswordValueSource.Instance) ||
        (!cell.CouldBeBlock() && cell.PossibleValues.Count == 26);

    public static bool CouldBeBlock(this Cell<char> cell) => cell.PossibleValues.Contains(CrosswordValueSource.BlockChar);

    public static bool MustBeABlock(this KeyValuePair<Position, Cell<char>> cell) => cell.Value.MustBeABlock();


    public static bool CouldBeBlock(this KeyValuePair<Position, Cell<char>> cell) => cell.Value.CouldBeBlock();


    public static bool CouldBeAnyLetter(this KeyValuePair<Position, Cell<char>> cell) => cell.Value.CouldBeBlock();
}
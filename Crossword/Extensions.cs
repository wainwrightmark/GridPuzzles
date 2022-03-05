namespace Crossword;

public static class Extensions
{
    public static bool MustNotBeABlock(this KeyValuePair<Position, CharCell> cell) =>
        cell.Value.MustNotBeABlock();

    public static bool MustNotBeABlock(this CharCell cell) =>
        !cell.Contains(CrosswordValueSource.BlockChar);


    public static bool MustBeABlock(this CharCell cell) =>
        cell.HasSingleValue() &&
        cell.Single().Equals(CrosswordValueSource.BlockChar);

    public static bool CouldBeAnyLetter(this CharCell cell) =>
        cell.CouldHaveAnyValue<char, CharCell>(CrosswordValueSource.Instance) ||
        (!cell.CouldBeBlock() && cell.Count() == 26);

    public static bool CouldBeBlock(this CharCell cell) => cell.Contains(CrosswordValueSource.BlockChar);

    public static bool MustBeABlock(this KeyValuePair<Position, CharCell> cell) => cell.Value.MustBeABlock();


    public static bool CouldBeBlock(this KeyValuePair<Position, CharCell> cell) => cell.Value.CouldBeBlock();


    public static bool CouldBeAnyLetter(this KeyValuePair<Position, CharCell> cell) => cell.Value.CouldBeBlock();
}
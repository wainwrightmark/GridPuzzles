namespace Crossword;

public class WordsClue : IClue<char, CharCell>
{
    public WordsClue(IEnumerable<(ushort priority, IReadOnlyCollection<string>)> words)
    {
        Words = words
            .Select(x => (x.priority, x.Item2.Where(w => w.All(CharCell.CharIsLegal)).ToList() as IReadOnlyCollection<string>));
        Positions = ImmutableSortedSet<Position>.Empty;
    }

    public IEnumerable<(ushort priority, IReadOnlyCollection<string>)>  Words { get; }

    /// <inheritdoc />
    public ImmutableSortedSet<Position> Positions { get; }


    /// <inheritdoc />
    public string Name => "Crossword Words";
}
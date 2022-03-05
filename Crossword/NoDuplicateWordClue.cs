using System.Text;
using CSharpFunctionalExtensions;
using GridPuzzles.Reasons;

namespace Crossword;

public class NoDuplicateVariantBuilder : NoArgumentVariantBuilder
{
    private NoDuplicateVariantBuilder()
    {
    }

    public static IVariantBuilder<char, CharCell> Instance { get; } = new NoDuplicateVariantBuilder();

    /// <inheritdoc />
    public override string Name => "No Duplicate Words";

    /// <inheritdoc />
    public override int Level => 1;


    /// <inheritdoc />
    public override IEnumerable<IClue<char, CharCell>> CreateClues(Position minPosition, Position maxPosition,
        IValueSource<char, CharCell> valueSource,
        IReadOnlyCollection<IClue<char, CharCell>> lowerLevelClues)
    {
        yield return new NoDuplicateWordClue(maxPosition.GetPositionsUpTo(true).SelectMany(x => x));
    }


    /// <inheritdoc />
    public override bool OnByDefault => true;
}

public class NoDuplicateWordClue : IRuleClue
{
    public NoDuplicateWordClue(IEnumerable<Position> positions)
    {
        Positions = positions.ToImmutableSortedSet();
    }

    /// <inheritdoc />
    public string Name => "No Duplicate Words";

    /// <inheritdoc />
    public ImmutableSortedSet<Position> Positions { get; }

    /// <inheritdoc />
    public IEnumerable<ICellChangeResult> CalculateCellUpdates(Grid grid)
    {
        var parallels =
            grid.MaxPosition.GetPositionsUpTo(true)
                .Concat(grid.MaxPosition.GetPositionsUpTo(false));

        var dict = new Dictionary<string, ImmutableSortedSet<Position>>();

        var currentWord = new StringBuilder();


        foreach (var parallel1 in parallels)
        {
            var parallel = parallel1.ToArray();


            for (var index = 0; index < parallel.Length; index++)
            {
                var position = parallel[index];
                var value = grid.GetCell(position);

                if (value.HasSingleValue())
                {
                    if (value.MustBeABlock())
                    {
                        if (currentWord is not null && currentWord.Length > 1)
                        {
                            foreach (var contradiction1 in TryAddWord(currentWord.ToString(), index, parallel,
                                         dict))
                                yield return contradiction1;
                        }

                        currentWord = new StringBuilder();
                    }
                    else
                        currentWord?.Append(value.Single());
                }
                else
                    currentWord = null;
            }

            if (currentWord is not null && currentWord.Length > 1)
            {
                foreach (var contradiction1 in TryAddWord(currentWord.ToString(), parallel.Length, parallel,
                             dict))
                    yield return contradiction1;
            }

            currentWord = new StringBuilder();
        }
    }

    private IEnumerable<Contradiction> TryAddWord(string word, int index, Position[] parallel,
        Dictionary<string, ImmutableSortedSet<Position>> dict)
    {
        var positions = parallel[(index - word.Length)..index].ToImmutableSortedSet();

        if (!dict.TryAdd(word, positions))
        {
            yield return new Contradiction(new DuplicateWordReason(word, this), positions.Concat(dict[word]).ToImmutableArray());
        }
    }
}

public sealed record DuplicateWordReason(string Word, NoDuplicateWordClue NDWClue) : ISingleReason
{
    /// <inheritdoc />
    public string Text => $"Duplicate word {Word}";

    /// <inheritdoc />
    public IEnumerable<Position> GetContributingPositions(IGrid grid)
    {
        yield break;
    }

    /// <inheritdoc />
    public Maybe<IClue> Clue => NDWClue;
}
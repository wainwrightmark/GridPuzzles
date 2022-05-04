using System;
using GridPuzzles.Bifurcation;
using GridPuzzles.Reasons;
using MoreLinq;

namespace Crossword;

public class ParallelWordClue : IRuleClue, IBifurcationClue<char, CharCell>
{
    /// <inheritdoc />
    public string Name => "Parallels should contain words";

    public ParallelWordClue(PossibleWordList wordList, Position min, Position max, ushort number, bool across,
        bool symmetrical, bool allowDuplicates)
    {
        WordList = wordList;
        Number = number;
        Across = across;
        Symmetrical = symmetrical;
        AllowDuplicates = allowDuplicates;
        PositionList = across
            ? Enumerable.Range(min.Column, max.Column - min.Column + 1).Select(x => new Position(x, number))
                .ToList()
            : Enumerable.Range(min.Row, max.Row - min.Row + 1).Select(x => new Position(number, x)).ToList();

        Positions = PositionList.ToImmutableSortedSet();
    }

    public IReadOnlyList<Position> PositionList { get; }

    /// <inheritdoc />
    public ImmutableSortedSet<Position> Positions { get; }

    public PossibleWordList WordList { get; }
    public ushort Number { get; }
    public bool Across { get; }
    public bool Symmetrical { get; }

    public bool AllowDuplicates { get; }

    private const int MinWordLength = 3;

    /// <inheritdoc />
    public override string ToString()
    {
        return Across ? $"Row {Number}" : $"Column {Number}";
    }

    /// <inheritdoc />
    public IEnumerable<ICellChangeResult> CalculateCellUpdates(Grid grid)
    {
        var cells = PositionList.Select(grid.GetCellKVP).ToList();

        IReadOnlyDictionary<string, ImmutableArray<Position>>? existingWords;
        if (AllowDuplicates) existingWords = null;
        else
        {
            var r = NoDuplicateWordClue.TryGetAllWords(grid, null);
            if(r.IsFailure)existingWords = null;
            else if(r.Value.Any())
            {
                existingWords = r.Value;
            }
            else existingWords = null;
        }

        var (rightOptions, leftOptions, fullLengthOptions, changes) =
            GetAllWordOptions(grid, cells, existingWords);

        foreach (var cellChangeResult in changes)
        {
            yield return cellChangeResult;
        }

        if ((rightOptions.Any() && leftOptions.Any()) || fullLengthOptions.Any())
        {
            for (var index = 0; index < cells.Count; index++)
            {
                var cell = cells[index];
                var index1 = index + 1;
                var impossibleValues = cell.Value
                    .Where(x => x != CrosswordValueSource.BlockChar)
                    .Where(pv => !
                        fullLengthOptions.Concat(leftOptions).Concat(rightOptions)
                            .Any(w => w.CouldHaveCharacterAtIndex(pv, index1))).ToCharCell();

                if (impossibleValues.Any())
                    yield return cell.CloneWithoutValues<char, CharCell>(impossibleValues,
                        new CrosswordReason("No words exist using this letter here."));
            }
        }
    }

    /// <inheritdoc />
    public IEnumerable<IBifurcationOption<char, CharCell>> FindBifurcationOptions(Grid grid, int maxChoices)
    {
        var cells = PositionList.Select(grid.GetCellKVP).ToList();

        IReadOnlyDictionary<string, ImmutableArray<Position>>? existingWords;
        if (AllowDuplicates) existingWords = null;
        else
        {
            var r = NoDuplicateWordClue.TryGetAllWords(grid, null);
            if(r.IsFailure)existingWords = null;
            else if(r.Value.Any())
            {
                existingWords = r.Value;
            }
            else existingWords = null;
        }


        var (rightOptions, leftOptions, fullLengthOptions, changes) =
            GetAllWordOptions(grid, cells, existingWords);

        if (changes.OfType<Contradiction>().Any())
            yield break;

        if ((rightOptions.Any() && leftOptions.Any()) || fullLengthOptions.Any())
        {
            var option = leftOptions.Select(o => (left: true, o))
                .Concat(rightOptions.Select(o => (left: false, o)))
                .Concat(fullLengthOptions.Select(o => (left: true, o)))
                .GroupBy(x => (x.left, x.o.Priority), x => x.o)
                .Select(x => new WordOptionList(x.ToList(), Across, Number))
                .Where(x => x.ChoiceCount > 1)
                .OrderByDescending(x => x.Priority) //higher priority
                .ThenBy(x => x.ChoiceCount) //lower count
                .FirstOrDefault();

            if (option != null)
            {
                yield return option;
            }
        }
    }


    private (IReadOnlyList<WordOption> rightOptions, IReadOnlyList<WordOption> leftOptions,
        IReadOnlyList<WordOption> fullLengthOptions, IReadOnlyList<ICellChangeResult> changes)
        GetAllWordOptions(Grid grid, IReadOnlyList<KeyValuePair<Position, CharCell>> cells,
            IReadOnlyDictionary<string, ImmutableArray<Position>>? existingWords)
    {
        //if (cells.All(x => x.Value.HasSingleValue))
        //    return (ImmutableArray<WordOption>.Empty, ImmutableArray<WordOption>.Empty,
        //        ImmutableArray<WordOption>.Empty, ImmutableArray<CellChangeResult<char>>.Empty);

        if (cells.All(x => !x.Value.HasSingleValue()) && !Symmetrical)
            return (ImmutableArray<WordOption>.Empty, ImmutableArray<WordOption>.Empty,
                ImmutableArray<WordOption>.Empty, ImmutableArray<ICellChangeResult>.Empty);

        var (minLeftBlocks, maxLeftBlocks, minRightBlocks, maxRightBlocks) = GetPossibleBlocks(cells, Symmetrical);

        IEnumerable<(KeyValuePair<Position, CharCell> cell, int i)> possibleBlocks;

        var changes = new List<ICellChangeResult>();

        if (Symmetrical)
        {
            var centre = (cells.Count - 1) / 2;

            var (centreBlocks, nonBlocks) = cells.Select((cell, i) => (cell, i))
                .Skip(MinWordLength).SkipLast(MinWordLength)
                .Where(x => x.cell.CouldBeBlock())
                .Partition(x => x.i == centre);

            possibleBlocks = centreBlocks;

            changes.AddRange(nonBlocks.Select(x => x.cell)
                .Select(cell => cell.CloneWithoutValue(CrosswordValueSource.BlockChar,
                    new CrosswordReason("Must not be a block, by symmetry"))));
        }
        else
        {
            possibleBlocks = cells.Select((cell, i) => (cell, i))
                .Skip(MinWordLength).SkipLast(MinWordLength)
                .Where(x => x.cell.CouldBeBlock()).ToList();
        }

        var allLeftOptions = new List<WordOption>();
        var allRightOptions = new List<WordOption>();

        //also think about having no blocks
        foreach (var (blockCell, blockIndex) in possibleBlocks)
        {
            var leftOptions =
                minLeftBlocks.RangeTo(maxLeftBlocks)
                    .SelectMany(eb => GetWordOptions(cells, eb, blockIndex - eb, eb, 1, existingWords)).ToList();

            if (leftOptions.Count == 0)
            {
                changes.Add(blockCell.CloneWithoutValue(CrosswordValueSource.BlockChar,
                    new CrosswordReason("if this is a block, no word could fit to its left")));
                continue;
            }

            var rightOptions = minRightBlocks.RangeTo(maxRightBlocks).SelectMany(eb =>
                GetWordOptions(cells, blockIndex + 1, PositionList.Count - blockIndex - eb - 1, 1, eb, existingWords)).ToList();
            if (rightOptions.Count == 0)
            {
                changes.Add(blockCell.CloneWithoutValue(CrosswordValueSource.BlockChar,
                    new CrosswordReason("if this is a block, no word could fit to its right")));
                continue;
            }

            allLeftOptions.AddRange(leftOptions);
            allRightOptions.AddRange(rightOptions);
        }

        //Full length word with no central blocks
        var fullLengthOptions =
            minLeftBlocks.RangeTo(maxLeftBlocks)
                .SelectMany(lBlocks =>
                    minRightBlocks.RangeTo(maxRightBlocks)
                        .Select(rBlocks => (lBlocks, rBlocks)))
                .SelectMany(
                    p => GetWordOptions(cells, p.lBlocks, PositionList.Count - p.rBlocks - p.lBlocks, p.lBlocks,
                        p.rBlocks, existingWords))
                .ToList();


        if (allLeftOptions.Count == 0 && fullLengthOptions.Count == 0)
            changes.Add(new Contradiction(new CrosswordReason("No possible left words"), Positions));
        else if (allLeftOptions.Count + fullLengthOptions.Count == 1 &&
                 allLeftOptions.Concat(fullLengthOptions).Single().PossibleWords.Count == 1)
            changes.AddRange(
                allLeftOptions.Concat(fullLengthOptions).Single().GetBifurcationWords(Across, Number).Single()
                    .GetChanges(grid)
            );

        if (allRightOptions.Count == 0 && fullLengthOptions.Count == 0)
            changes.Add(new Contradiction(new CrosswordReason("No possible right words"), Positions));
        else if (allRightOptions.Count + fullLengthOptions.Count == 1 &&
                 allRightOptions.Concat(fullLengthOptions).Single().PossibleWords.Count == 1)
            changes.AddRange(allRightOptions.Concat(fullLengthOptions).Single().GetBifurcationWords(Across, Number)
                .Single()
                .GetChanges(grid));

        return (allRightOptions, allLeftOptions, fullLengthOptions, changes);
    }

    private IEnumerable<WordOption> GetWordOptions(IEnumerable<KeyValuePair<Position, CharCell>> cells, int skip,
        int wordLength, int startBlocks, int endBlocks, IReadOnlyDictionary<string, ImmutableArray<Position>>? existingWords)
    {
        var cellList = cells.Skip(skip).Take(wordLength).ToList();

        var cellsToCheck = cellList
            .Select((x, i) => (cell: x.Value, i))
            .Where(x => !x.cell.CouldBeAnyLetter() && !x.cell.HasSingleValue()).ToList();

        var word = ExpressionWord.TryCreate(cellList);

        if (word.HasValue)
        {
            var searchResult = WordList.Search(word.Value);

            foreach (var (priority, words) in searchResult)
            {
                var words2 = Filter(words);
                if (words2.Any())
                    yield return new WordOption(word.Value, skip + 1, startBlocks, endBlocks, priority, words2,
                        wordLength);
            }
        }

        IReadOnlyList<Word> Filter(IReadOnlyList<Word> words)
        {
            if (!cellsToCheck.Any())
                return words;

            var filteredWords = words.Where(w =>

                cellsToCheck.All(x => x.cell.Contains(w.NormalizedText[x.i])));

            if (existingWords is not null)
                filteredWords = filteredWords.Where(w =>
            {
                if (existingWords.TryGetValue(w.NormalizedText, out var positions))
                {

                    if (!positions.All(p => cellList.Select(x => x.Key).Contains(p)))
                    {
                        return false; //This word already exists in a different place    
                    }

                }

                return true;
            });

            return filteredWords.ToList();
        }
    }


    private static (ushort minLeftBlocks, ushort maxLeftBlocks, ushort minRightBlocks, ushort maxRightBlocks)
        GetPossibleBlocks(
            IReadOnlyList<KeyValuePair<Position, CharCell>> keyValuePairs, bool symmetrical)
    {
        ushort minLeftBlocks;
        ushort maxLeftBlocks;
        ushort minRightBlocks;
        ushort maxRightBlocks;

        if (keyValuePairs[2].MustBeABlock()) minLeftBlocks = 3;
        else if (keyValuePairs[1].MustBeABlock()) minLeftBlocks = 2;
        else if (keyValuePairs[0].MustBeABlock()) minLeftBlocks = 1;
        else minLeftBlocks = 0;

        if (keyValuePairs[^3].MustBeABlock()) minRightBlocks = 3;
        else if (keyValuePairs[^2].MustBeABlock()) minRightBlocks = 2;
        else if (keyValuePairs[^1].MustBeABlock()) minRightBlocks = 1;
        else minRightBlocks = 0;

        if (keyValuePairs[0].MustNotBeABlock()) maxLeftBlocks = 0;
        else if (keyValuePairs[1].MustNotBeABlock()) maxLeftBlocks = 1;
        else if (keyValuePairs[2].MustNotBeABlock()) maxLeftBlocks = 2;
        else maxLeftBlocks = 3;

        if (keyValuePairs[^1].MustNotBeABlock()) maxRightBlocks = 0;
        else if (keyValuePairs[^2].MustNotBeABlock()) maxRightBlocks = 1;
        else if (keyValuePairs[^3].MustNotBeABlock()) maxRightBlocks = 2;
        else maxRightBlocks = 3;


        if (symmetrical)
        {
            minLeftBlocks = Math.Max(minLeftBlocks, minRightBlocks);
            minRightBlocks = minLeftBlocks;

            maxLeftBlocks = Math.Min(maxLeftBlocks, maxRightBlocks);
            maxRightBlocks = maxLeftBlocks;
        }


        return (minLeftBlocks, maxLeftBlocks, minRightBlocks, maxRightBlocks);
    }

    private class WordOption
    {
        public WordOption(ExpressionWord expressionWord, int firstCharacterIndex, int startBlocks, int endBlocks,
            ushort priority, IReadOnlyList<Word> possibleWords, int wordLength)
        {
            ExpressionWord = expressionWord;
            FirstCharacterIndex = Convert.ToUInt16(firstCharacterIndex);
            StartBlocks = Convert.ToUInt16(startBlocks);
            EndBlocks = Convert.ToUInt16(endBlocks);
            Priority = priority;
            PossibleWords = possibleWords;
            WordLength = Convert.ToUInt16(wordLength);
        }

        public ExpressionWord ExpressionWord { get; }

        public ushort FirstCharacterIndex { get; }
        public ushort StartBlocks { get; }
        public ushort EndBlocks { get; }
        public ushort Priority { get; }

        public ushort WordLength { get; }

        public IReadOnlyList<Word> PossibleWords { get; }

        public bool CouldHaveCharacterAtIndex(char c, int i)
        {
            var wordIndex = i - FirstCharacterIndex;
            if (wordIndex < 0 || wordIndex >= WordLength) return false;

            return PossibleWords.Any(w => w.NormalizedText[wordIndex] == c);
        }

        public IEnumerable<BifurcationWord> GetBifurcationWords(bool across, ushort parallelIndex)
        {
            return PossibleWords.Select(possibleWord => BifurcationWord.Create(possibleWord, across, parallelIndex,
                FirstCharacterIndex, StartBlocks, EndBlocks));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ExpressionWord.Expression;
        }
    }

    private class WordOptionList : IBifurcationOption
    {
        public WordOptionList(IReadOnlyList<WordOption> wordOptions, bool across, ushort parallelIndex)
        {
            WordOptions = wordOptions;
            Across = across;
            ParallelIndex = parallelIndex;
            Priority = wordOptions.Select(x => x.Priority).Distinct().Single();
            ChoiceCount = wordOptions.Sum(x => x.PossibleWords.Count);
        }

        public bool Across { get; }
        public ushort ParallelIndex { get; }
        public IReadOnlyList<WordOption> WordOptions { get; }

        /// <inheritdoc />
        public int Priority { get; }

        /// <inheritdoc />
        public ISingleReason Reason => new WordOptionReason(ParallelIndex, Across);

        /// <inheritdoc />
        public IEnumerable<IBifurcationChoice<char, CharCell>> Choices
        {
            get { return WordOptions.SelectMany(x => x.GetBifurcationWords(Across, ParallelIndex)); }
        }

        /// <inheritdoc />
        public int ChoiceCount { get; }

        /// <inheritdoc />
        public IBifurcationChoice<char, CharCell> this[int index]
        {
            get
            {
                var current = index;

                foreach (var wordOption in WordOptions)
                {
                    if (current < wordOption.PossibleWords.Count)
                    {
                        var choice = wordOption.PossibleWords[current];
                        return BifurcationWord.Create(choice, Across, ParallelIndex, wordOption.FirstCharacterIndex,
                            wordOption.StartBlocks, wordOption.EndBlocks);
                    }

                    current -= wordOption.PossibleWords.Count;
                }

                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ChoiceCount + " Choices";
        }

        public sealed record WordOptionReason(int ParallelIndex, bool Across) : ISingleReason
        {
            /// <inheritdoc />
            public string Text => Across ? $"{ParallelIndex} Across" : $"{ParallelIndex} Down";

            /// <inheritdoc />
            public IEnumerable<Position> GetContributingPositions(IGrid grid)
            {
                yield break;
            }

            /// <inheritdoc />
            public Maybe<IClue> Clue => Maybe<IClue>.None;
        }
    }

    private sealed class BifurcationWord : IBifurcationChoice<char, CharCell>
    {
        //Note this is a bit dodgy, but I'm cool with it
        public static BifurcationWord Create(Word word, bool across, ushort parallelIndex, ushort firstLetterIndex,
            ushort startBlocks, ushort endBlocks)
        {
            Position startPosition;
            if (across)
                startPosition = new Position(firstLetterIndex - startBlocks, parallelIndex);
            else startPosition = new Position(parallelIndex, firstLetterIndex - startBlocks);

            var text = new string(CrosswordValueSource.BlockChar, startBlocks) + word.NormalizedText +
                       new string(CrosswordValueSource.BlockChar, endBlocks);

            return new BifurcationWord(startPosition, across, text);
        }

        public BifurcationWord(Position startPosition, bool across, string text)
        {
            StartPosition = startPosition;
            Across = across;
            Text = text;
        }

        public Position StartPosition { get; }
        public bool Across { get; }

        /// <summary>
        /// Will include box characters on the sides
        /// </summary>
        public string Text { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return new { StartPosition, Across, Word = Text }.ToString()!;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(StartPosition, Across, Text);
        }

        public IEnumerable<ICellChangeResult> GetChanges(Grid grid)
        {
            for (var i = 0; i < Text.Length; i++)
            {
                var character = Text[i];
                var position = Across
                    ? new Position(StartPosition.Column + i, StartPosition.Row)
                    : new Position(StartPosition.Column, StartPosition.Row + i);


                var ccr = grid.GetCellKVP(position).CloneWithOnlyValue(character,
                    new CrosswordReason("Part of " + Text.Trim(CrosswordValueSource.BlockChar))
                );

                yield return (ccr);
            }
        }

        /// <inheritdoc />
        public UpdateResult UpdateResult
        {
            get
            {
                var updateResult = UpdateResult.Empty;


                for (var i = 0; i < Text.Length; i++)
                {
                    var character = Text[i];
                    var position = Across
                        ? new Position(StartPosition.Column + i, StartPosition.Row)
                        : new Position(StartPosition.Column, StartPosition.Row + i);


                    var cellUpdate = CellHelper.Create<char, CharCell>(character, position,
                        new CrosswordReason("Part of " + Text.Trim(CrosswordValueSource.BlockChar))
                    );


                    updateResult = updateResult.CloneWithCellUpdate(cellUpdate);
                }

                return updateResult;
            }
        }

        /// <inheritdoc />
        public int CompareTo(IBifurcationChoice<char, CharCell>? other)
        {
            if (other is BifurcationWord bsc)
                return string.CompareOrdinal(Text, bsc.Text);

            return 0;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is BifurcationWord bw && StartPosition.Equals(bw.StartPosition) &&
                   Across.Equals(bw.Across) && Text.Equals(bw.Text);
        }
    }
}

public sealed record CrosswordReason(string Text) : ISingleReason //TODO fix this
{
    /// <inheritdoc />
    public IEnumerable<Position> GetContributingPositions(IGrid grid)
    {
        yield break;
    }

    /// <inheritdoc />
    public Maybe<IClue> Clue => Maybe<IClue>.None;
}
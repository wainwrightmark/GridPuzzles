﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GridPuzzles;
using GridPuzzles.Clues;
using MoreLinq;

namespace Crossword;

public class ParallelWordClueBuilder : NoArgumentVariantBuilder<char>
{
    public static ParallelWordClueBuilder Instance = new();

    private ParallelWordClueBuilder()
    {
    }
    /// <inheritdoc />
    public override string Name => "Letters must form words";

    /// <inheritdoc />
    public override int Level => 4;

    /// <inheritdoc />
    public override IEnumerable<IClue<char>> CreateClues(Position minPosition, Position maxPosition, IValueSource<char> valueSource,
        IReadOnlyCollection<IClue<char>> lowerLevelClues)
    {
        var blocks = lowerLevelClues.OfType<BlockClue>().SelectMany(x => x.Positions).ToImmutableHashSet();

        var wordList = lowerLevelClues.OfType<WordListClue>().Single();


        var partition = lowerLevelClues.OfType<CrosswordSymmetryClue>()
            .SelectMany(x => x.SymmetricalParallels)
            .Distinct().Partition(x=>x.horizontal);

        var symmetricalHorizontals = partition.True.Select(x => x.index).ToHashSet();
        var symmetricalVerticals = partition.False.Select(x => x.index).ToHashSet();


        var wordClues = new List<ParallelWordClue>();

        //Columns
        for (var i = minPosition.Column; i <= maxPosition.Column; i++)
            wordClues.Add(new ParallelWordClue(wordList.PossibleWordList, minPosition, maxPosition, i, true, symmetricalVerticals.Contains(i)));

        //Rows
        for (var i = minPosition.Row; i <= maxPosition.Row; i++)
            wordClues.Add(new ParallelWordClue(wordList.PossibleWordList, minPosition, maxPosition, i, false, symmetricalHorizontals.Contains(i)));

        var trueClues = wordClues.Where(x => !blocks.Overlaps(x.Positions)).ToList();

        return trueClues;
    }

    /// <inheritdoc />
    public override bool OnByDefault => true;

    /// <inheritdoc />
    public override bool IsValid(Position maxPosition)
    {
        return true;
    }
}
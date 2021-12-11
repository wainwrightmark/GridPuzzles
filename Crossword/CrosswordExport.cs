using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GridPuzzles;

namespace Crossword;

public static class CrosswordExport
{
    public static string ExportCrossword(this Grid<char> grid)
    {
        if (!grid.IsComplete)
            return "not Complete";


        var words = new List<(string word, int number, Position start, bool across)>();
        var visitedAcross = new HashSet<Position>();
        var visitedDown = new HashSet<Position>();
        var sb = new StringBuilder();

        var cellsByRow = grid.MaxPosition
            .GetPositionsUpTo(true)
            .Select(row => row.Select(grid.GetCellKVP))
            .ToList();

        var  currentNumber = 1;
        foreach (var row in cellsByRow)
        {
            foreach (var (position, _) in row)
            {
                var acrossWord = GetWord(position, grid, visitedAcross, x => new Position(x.Column + 1, x.Row));
                var downWord = GetWord(position, grid, visitedDown, x => new Position(x.Column, x.Row + 1));

                if (IsWord(acrossWord) || IsWord(downWord))
                {
                    if (IsWord(acrossWord)) words.Add((acrossWord, currentNumber, position, true));
                    if (IsWord(downWord)) words.Add((downWord, currentNumber, position, false));

                    currentNumber++;
                }

            }
        }

        var numbersDict = words.GroupBy(x => x.start)
            .ToDictionary(x => x.Key, x => x.First().number);


        foreach (var row in cellsByRow)
        {
            foreach (var (key, value) in row)
            {
                if(value.PossibleValues.Single() == CrosswordValueSource.BlockChar) sb.Append('.');
                else if (numbersDict.TryGetValue(key, out var v)) sb.Append(NumbersArray[v]);
                else sb.Append(' ');
                sb.Append('\t');
            }

            sb.Append('\n');
        }

        sb.Append('\n');

        foreach (var (word, number, _, across) in words.OrderByDescending(x=>x.across).ThenBy(x=>x.number))
        {
            sb.Append($"{number}\t{(across ? 'A' : 'D')}\t{word}\t({word.Length})");
            sb.Append('\n');
        }

        var result = sb.ToString();

        return result;

        static bool IsWord(string? s)
        {
            return !string.IsNullOrWhiteSpace(s) && s.Length > 1;
        }

        static string GetWord(Position start, Grid<char> grid, ISet<Position> visited, Func<Position, Position> getNextPosition)
        {
            if (!visited.Add(start))
                return "";

            var position = start;
            var sb = new StringBuilder();

            while (true)
            {
                var value = grid.GetCell(position).PossibleValues.Single();
                if (value == CrosswordValueSource.BlockChar)
                    break;

                sb.Append(value);
                visited.Add(position);
                position = getNextPosition(position);
                if (!position.IsWithin(Position.Origin, grid.MaxPosition))
                    break;

            }

            return sb.ToString();
        }
    }

    private static readonly char[] NumbersArray = "⓪①②③④⑤⑥⑦⑧⑨⑩⑪⑫⑬⑭⑮⑯⑰⑱⑲⑳㉑㉒㉓㉔㉕㉖㉗㉘㉙㉚㉛㉜㉝㉞㉟㊱㊲㊳㊴㊵㊶㊷㊸㊹㊺㊻㊼㊽㊾㊿".ToCharArray();

}
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GridPuzzles;
using GridPuzzles.Bifurcation;
using GridPuzzles.Cells;
using GridPuzzles.Clues;
using GridPuzzles.Reasons;

namespace Sudoku.Clues;

public interface IParallelClue<T> : ICompletenessClue<T> where T: notnull
{
    Parallel Parallel { get; }
    ushort Index { get; }
}


public abstract class BasicClue<T> :
    ICompletenessClue<T> ,
    IBifurcationClue<T>  where T: notnull
{
    protected BasicClue(string domain) => Domain = domain;

    public string Domain { get; }

    /// <inheritdoc />
    public string Name => Domain;

    public abstract ImmutableSortedSet<Position> Positions { get; }


    /// <inheritdoc />
    public IEnumerable<IBifurcationOption<T>> GetBifurcationOptions(Grid<T> grid, int maxChoices)
    {
        var groups = Positions
            .Select(grid.GetCellKVP)
            .Where(x => x.Value.PossibleValues.Count != 1)
            .SelectMany(cell => cell.Value.PossibleValues.Select(v => (v, cell.Key)))
            .GroupBy(x => x.v, x => x)
            .Where(x =>
            {
                var c = x.Count();
                return 1 < c && c <= maxChoices;
            });

        foreach (var group in groups)
        {
            var choices = group.Select(t1 =>
                new BifurcationCellChoice<T>(CellHelper.Create(group.Key, t1.Key, BifurcationAttemptReason.Instance)) as
                    IBifurcationChoice<T>).ToArray();

            var bo = new BifurcationOption<T>(0, new MustExistsReason<T>(group.Key, this), choices);
            yield return bo;
        }
    }

    /// <inheritdoc />
    public override string ToString() => Domain;
}
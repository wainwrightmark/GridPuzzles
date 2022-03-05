

namespace Sudoku.Clues;

public interface IParallelClue<T, TCell> : ICompletenessClue<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    Parallel Parallel { get; }
    ushort Index { get; }
}


public abstract class BasicClue<T, TCell> :
    ICompletenessClue<T, TCell> ,
    IBifurcationClue<T, TCell>  where T :struct where TCell : ICell<T, TCell>, new()
{
    protected BasicClue(string domain) => Domain = domain;

    public string Domain { get; }

    /// <inheritdoc />
    public string Name => Domain;

    public abstract ImmutableSortedSet<Position> Positions { get; }


    /// <inheritdoc />
    public IEnumerable<IBifurcationOption<T, TCell>> FindBifurcationOptions(Grid<T, TCell> grid, int maxChoices)
    {
        var groups = Positions
            .Select(grid.GetCellKVP)
            .Where(x => !x.Value.HasSingleValue())
            .SelectMany(cell => cell.Value.Select(v => (v, cell.Key)))
            .GroupBy(x => x.v, x => x)
            .Where(x =>
            {
                var c = x.Count();
                return 1 < c && c <= maxChoices;
            });

        foreach (var group in groups)
        {
            var choices = group.Select(t1 =>
                new BifurcationCellChoice<T, TCell>(CellHelper.Create<T, TCell>(group.Key, t1.Key, BifurcationAttemptReason.Instance)) as
                    IBifurcationChoice<T, TCell>).ToArray();

            var bo = new BifurcationOption<T, TCell>(0, new MustExistsReason<T, TCell>(group.Key, this), choices);
            yield return bo;
        }
    }

    /// <inheritdoc />
    public override string ToString() => Domain;
}
using System.Threading;
using System.Threading.Tasks;
using GridPuzzles.Bifurcation;
using GridPuzzles.Clues;
using GridPuzzles.Yaml;

namespace GridPuzzles;

public static class Solver
{
    public static async Task<SolveResult<T, TCell>> TrySolveAsync<T, TCell>(string gridYaml,
        int currentBifurcationDepth,
        int maxBifurcationDepth,
        Func<(int columns, int rows), IValueSource<T, TCell>> getValueSource,
        IReadOnlyDictionary<string, IVariantBuilder<T, TCell>> variantBuilders, CancellationToken cancellation) where T :struct where TCell : ICell<T, TCell>, new()
    {
        var deserializationResult = await YamlHelper.DeserializeGrid(gridYaml).Bind(x=> x.Convert(
            getValueSource((x.Columns, x.Rows)),
            variantBuilders,
            cancellation )).Map(x=>x.grid);

        if(deserializationResult.IsFailure)
            throw new Exception(deserializationResult.Error);

        return Solve(deserializationResult.Value, currentBifurcationDepth, maxBifurcationDepth);
    }

    public static SolveResult<T, TCell> Solve<T, TCell>(Grid<T, TCell> grid, int currentBifurcationDepth, int maxBifurcationDepth) where T :struct where TCell : ICell<T, TCell>, new()
    {
        var current = grid;

        var mostRecentUpdate = UpdateResult<T, TCell>.Empty;


        while (true)
        {
            (current, mostRecentUpdate) = current.IterateRepeatedly(UpdateResultCombiner<T, TCell>.Fast, currentBifurcationDepth, mostRecentUpdate);

            if(mostRecentUpdate.Contradictions.Any())
                return new SolveResult<T, TCell>(current,  mostRecentUpdate.Contradictions, null);


            if (current.IsComplete)
                return new SolveResult<T, TCell>(current, null, new[] {current});

            if (maxBifurcationDepth <= currentBifurcationDepth)
                break;


            var bifurcationResult = current.Bifurcate(maxBifurcationDepth, true, UpdateResultCombiner<T, TCell>.Fast);

            if (bifurcationResult.UpdateResult.Contradictions.Any())
                return new SolveResult<T, TCell>(current,
                    bifurcationResult.UpdateResult.Contradictions, null);

            if (!bifurcationResult.UpdateResult.IsNotEmpty)
                break;

            mostRecentUpdate = bifurcationResult.UpdateResult;
            current = current.CloneWithUpdates(bifurcationResult.UpdateResult, false);
        }

        return new SolveResult<T, TCell>(current, null, null); //We failed to find any possible changes
    }

}



public class SolveResult<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{

    public SolveResult(Grid<T, TCell> grid,  IReadOnlyCollection<Contradiction>? contradictions, IReadOnlyCollection<Grid<T, TCell>>? completeGrids)
    {
        Contradictions = contradictions?? new List<Contradiction>();
        MostAdvancedGrid = grid;
        CompleteGrids = completeGrids;
    }

    public Grid<T, TCell> MostAdvancedGrid { get; }

    public IReadOnlyCollection<Grid<T, TCell>>? CompleteGrids { get; }

    public IReadOnlyCollection<Contradiction> Contradictions { get; }

    public bool IsSuccess => !Contradictions.Any() && MostAdvancedGrid.IsComplete;
}
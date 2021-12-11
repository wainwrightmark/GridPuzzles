using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using GridPuzzles.Bifurcation;
using GridPuzzles.Clues;
using GridPuzzles.Yaml;

namespace GridPuzzles;

public static class Solver
{
    public static async Task<SolveResult<T>> TrySolveAsync<T>(string gridYaml, int maxBifurcationDepth,
        Func<(int columns, int rows), IValueSource<T>> getValueSource,
        IReadOnlyDictionary<string, IVariantBuilder<T>> variantBuilders, CancellationToken cancellation) where T:notnull
    {
        var deserializationResult = await YamlHelper.DeserializeGrid(gridYaml).Bind(x=> x.Convert(
            getValueSource((x.Columns, x.Rows)),
            variantBuilders,
            cancellation )).Map(x=>x.grid);

        if(deserializationResult.IsFailure)
            throw new Exception(deserializationResult.Error);

        return Solve(deserializationResult.Value, maxBifurcationDepth);
    }

    public static SolveResult<T> Solve<T>(Grid<T> grid, int maxBifurcationDepth) where T:notnull
    {
        var current = grid;

        UpdateResult<T> mostRecentUpdate = UpdateResult<T>.Empty;


        while (true)
        {
            (current, mostRecentUpdate) = current.IterateRepeatedly(UpdateResultCombiner<T>.Fast, mostRecentUpdate);

            if(mostRecentUpdate.Contradictions.Any())
                return new SolveResult<T>(current,  mostRecentUpdate.Contradictions, null);


            if (current.IsComplete)
                return new SolveResult<T>(current, null, new[] {current});

            if (maxBifurcationDepth <= 0)
                break;


            var bifurcationResult = current.Bifurcate(maxBifurcationDepth, true, UpdateResultCombiner<T>.Fast);

            if (bifurcationResult.UpdateResult.Contradictions.Any())
                return new SolveResult<T>(current,
                    bifurcationResult.UpdateResult.Contradictions, null);

            if (!bifurcationResult.UpdateResult.IsNotEmpty)
                break;

            mostRecentUpdate = bifurcationResult.UpdateResult;
            current = current.CloneWithUpdates(bifurcationResult.UpdateResult, false);
        }

        return new SolveResult<T>(current, null, null); //We failed to find any possible changes
    }

}



public class SolveResult<T> where T:notnull
{

    public SolveResult(Grid<T> grid,  IReadOnlyCollection<Contradiction>? contradictions, IReadOnlyCollection<Grid<T>>? completeGrids)
    {
        Contradictions = contradictions?? new List<Contradiction>();
        MostAdvancedGrid = grid;
        CompleteGrids = completeGrids;
    }

    public Grid<T> MostAdvancedGrid { get; }

    public IReadOnlyCollection<Grid<T>>? CompleteGrids { get; }

    public IReadOnlyCollection<Contradiction> Contradictions { get; }

    public bool IsSuccess => !Contradictions.Any() && MostAdvancedGrid.IsComplete;
}
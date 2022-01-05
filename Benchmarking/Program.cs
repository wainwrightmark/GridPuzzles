using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;
using GridPuzzles.Yaml;
using Sudoku;
using Sudoku.Examples;

namespace GridPuzzles.Benchmarking;


public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<SudokuBenchmark>();
    }
}

[SimpleJob(RunStrategy.Monitoring, invocationCount:1)]

public class SudokuBenchmark
{
    private static byte[][] examplesToTest = new[]
    {
        ExampleResource.EasySudoku,
        ExampleResource.HardSudoku,
        ExampleResource.TatooineSunset,
        ExampleResource.ArrowKiller,
        //ExampleResource.Miracle2,
        //ExampleResource.BubbleBath,
        ExampleResource.Syzgy,
        //ExampleResource.MonstrousKiller,
        //ExampleResource.Miracle1,
        ExampleResource.ThermoArrows,
    };

    private static Grid<int> GetGrid(byte[] ba)
    {
        var yaml = ExampleHelper.GetYaml(ba);
        var deserializationResult = YamlHelper.DeserializeGrid(yaml);

        var grid = 
            deserializationResult
                .Value
                .Convert(NumbersValueSource.Sources[9], SudokuVariant.SudokuVariantBuildersDictionary,
                    CancellationToken.None).Result.Value.grid;

        return grid;
    }

    private static readonly List<Grid<int>> Grids = examplesToTest.Select(GetGrid).ToList();


    [GlobalSetup]
    public void GlobalSetup()
    {
        if (Grids.Count == 0) throw new Exception("Grids is empty");
    }

    [Params(true, false)]
    public bool EnablePlausibleSumChecking { get; set; }

    [Benchmark]
    public bool TestSudoku()
    {
        ExperimentalFeatures.EnablePlausibleSumChecking = EnablePlausibleSumChecking;

        foreach (var grid in Grids)
        {
            var solveResult = Solver.Solve(grid, 3);

            if (!solveResult.IsSuccess)
                throw new Exception("Solve result should be success");


        }

        return true;
    }

}
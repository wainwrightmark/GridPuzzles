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
        //var benchmark = new SudokuBenchmark();
        //benchmark.GlobalSetup();
        //benchmark.TestSudoku();

        var summary = BenchmarkRunner.Run<SudokuBenchmark>();
    }
}

[SimpleJob(RunStrategy.Monitoring, invocationCount: 1, targetCount: 2)]
public class SudokuBenchmark
{
    private static byte[][] examplesToTest = new[]
    {ExampleResource.EasySudoku,
        ExampleResource.TenKnights,
        //ExampleResource.KillerXXL,
        //ExampleResource.MonstrousKiller,
        //ExampleResource.SumsAndDoublingGroups,
        //ExampleResource.SumsAndUniqueSums,
        //ExampleResource.MoreHardArrows,
        //ExampleResource.MonstrousKiller,
        //ExampleResource.HardSudoku,
        //ExampleResource.TatooineSunset,
        //ExampleResource.ArrowKiller,
        ////ExampleResource.Miracle2,
        ////ExampleResource.BubbleBath,
        //ExampleResource.Syzgy,
        ////ExampleResource.Miracle1,
        //ExampleResource.ThermoArrows,
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

    //[Params(true, false)] public bool EnableArithmeticConsistency { get; set; }


    [Params(true, false)] public bool EnableParallelDescent { get; set; }

    [Benchmark]
    public bool TestSudoku()
    {
        //NOTE: Make sure to change settings that come into play during solving, not during grid construction

        //ExperimentalFeatures.EnableArithmeticConsistency = EnableArithmeticConsistency;
        ExperimentalFeatures.ParallelDescent = EnableParallelDescent;

        foreach (var grid in Grids)
        {
            var solveResult = Solver.Solve(grid, 0,3);

            if (!solveResult.IsSuccess)
                throw new Exception("Solve result should be success");
        }

        return true;
    }
}
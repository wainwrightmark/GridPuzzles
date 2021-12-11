using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Sudoku;
using Xunit;
using Xunit.Abstractions;

namespace GridPuzzles.Tests;

public class YamlTests
{
    private readonly ITestOutputHelper _output;

    public YamlTests(ITestOutputHelper output)
    {
        _output = output;
    }


    private static TheoryData<string, string, int> MakeGridData(string pathSuffix, int depth)
    {
        string workingDirectory = Environment.CurrentDirectory;
        string projectDirectory = Directory.GetParent(workingDirectory)!.Parent!.FullName;

        var path = Path.Combine(projectDirectory, pathSuffix);

        var theoryData = new TheoryData<string, string, int>();

        {
            var files = Directory.EnumerateFiles(path, "*.yml");

            foreach (var file in files)
            {
                var text = File.ReadAllText(file);

                var fileName = Path.GetFileNameWithoutExtension(file);
                theoryData.Add(fileName, text, depth);
            }
        }

        return theoryData;
    }


    public static readonly TheoryData<string, string, int> EasyGridData = MakeGridData(@"..\EasyNumberGridTests", 1);
    public static readonly TheoryData<string, string, int> MediumGridData = MakeGridData(@"..\MediumNumberGridTests", 3);
    public static readonly TheoryData<string, string, int> HardGridData = MakeGridData(@"..\HardNumberGridTests", 4);


    [Theory]
    [MemberData(nameof(EasyGridData))]
    public async Task TestEasyNumberGrid(string name, string yaml, int maxBifurcationDepth) => await Test(name, yaml, maxBifurcationDepth);

    [Theory]
    [MemberData(nameof(MediumGridData))]
    public async Task TestMediumNumberGrid(string name, string yaml, int maxBifurcationDepth) => await Test(name, yaml, maxBifurcationDepth);

    [Theory]
    [MemberData(nameof(HardGridData))]
    public async Task TestHardNumberGrid(string name, string yaml, int maxBifurcationDepth) => await Test(name, yaml, maxBifurcationDepth);

    private async Task Test(string _, string yaml, int maxBifurcationDepth)
    {
        var sw = Stopwatch.StartNew();
        var solveResult = await Solver.TrySolveAsync(yaml, maxBifurcationDepth,
            x=> NumbersValueSource.Sources[Math.Max(x.columns, x.rows)],
            SudokuVariant.SudokuVariantBuildersDictionary, CancellationToken.None);

        sw.Stop();
        _output.WriteLine(sw.ElapsedMilliseconds + " milliseconds");
        _output.WriteLine(solveResult.MostAdvancedGrid.ToDisplayString());

        _output.WriteLine("");
        _output.WriteLine("");
        _output.WriteLine(yaml);
        _output.WriteLine(sw.ElapsedMilliseconds + " milliseconds");

        if (!solveResult.IsSuccess)
        {
            solveResult.Contradictions.Should().BeEmpty();
            Assert.False(true, "Could not solve");
        }


        solveResult.IsSuccess.Should().BeTrue();
    }

    //[Fact]
    //public void RunEasyTests()
    //{
    //    throw new Exception("BlaH");
    //}

}
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace GridPuzzles.Tests;

public class MultipleSolutionTests
{
    private readonly ITestOutputHelper _output;

    public MultipleSolutionTests(ITestOutputHelper output)
    {
        _output = output;
    }

#pragma warning disable CA1034 // Nested types should not be visible
    public class MultipleSolutionTest
#pragma warning restore CA1034 // Nested types should not be visible
    {
        public MultipleSolutionTest(string testName, string grid, int expectedNumberOfSolutions)
        {
            TestName = testName;
            Grid = grid;
            ExpectedNumberOfSolutions = expectedNumberOfSolutions;
        }

        public string TestName { get; }

        public string Grid { get; }
        public int ExpectedNumberOfSolutions { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return TestName;
        }
    }

    //[Theory]
    //[MemberData(nameof(TestCases))]
    //public void TestSolutions(MultipleSolutionTest fgt)
    //{
    //    var sw = Stopwatch.StartNew();

    //    var grid = fgt.Grid.Trim().Replace("\t", "");

    //    var solveResult = Solver.TrySolveStandard(grid, 2);

    //    sw.Stop();
    //    _output.WriteLine(sw.ElapsedMilliseconds + " milliseconds");
    //    _output.WriteLine(solveResult.MostAdvancedGrid.ToDisplayString());
    //    _output.WriteLine(solveResult.CompleteGrids?.Count + " solutions");

    //    solveResult.Contradictions.Should().BeEmpty();
    //    solveResult.CompleteGrids.Should().NotBeNull();
    //    solveResult.CompleteGrids?.Select(x=>x.IsComplete).Should().AllBeEquivalentTo(true, "Result grids should all be complete");

    //    solveResult.CompleteGrids?.Count.Should().Be(fgt.ExpectedNumberOfSolutions);
    //}

    public static readonly IReadOnlyCollection<object[]> TestCases =
        new List<MultipleSolutionTest>
        {
            new("Two solutions Grid",
                @"
16273-4-5
73-4-5162
4-516273-
6273-4-51
3-4-51627
-516273-4
273-4-516
-4-516273
516273-4-", 2)

//                new MultipleSolutionTest("Six Solutions Grid",
//                    @"
//16273-4--
//73-4--162
//4--16273-
//6273-4--1
//3-4--1627
//--16273-4
//273-4--16
//-4--16273
//-16273-4-", 2)



        }.Select(x => new object[] {x}).ToList();




}
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GridPuzzles.Clues;
using Sudoku;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace GridPuzzles.Tests;

public class SymmetryTests
{
#pragma warning disable IDE0052 // Remove unread private members
    private readonly ITestOutputHelper _output;
#pragma warning restore IDE0052 // Remove unread private members

    public SymmetryTests(ITestOutputHelper output)
    {
        _output = output;
    }

#pragma warning disable CA1034 // Nested types should not be visible
    public class SymmetryTest
#pragma warning restore CA1034 // Nested types should not be visible
    {
        public SymmetryTest(string testName, string grid)
        {
            TestName = testName;
            Grid = grid;
        }

        public string TestName { get; }

        public string Grid { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return TestName;
        }
    }

    [Theory]
    [MemberData(nameof(SymmetryTestCases))]
    public async Task TestFullGrid(SymmetryTest fgt)
    {
        var gridText = fgt.Grid.Trim().Replace("\t","");

        var maxPosition = Position.NineNine;

        var clueSource = await ClueSource<int>
            .TryCreateAsync(
                SudokuVariant.SudokuVariantBuilders.GetVariantBuilderArgumentPairs(maxPosition),
                    
                maxPosition, NumbersValueSource.Sources[9], CancellationToken.None);

        if (clueSource.IsFailure)
            throw new XunitException(clueSource.Error);


        var grid = Grid<int>.CreateFromString(gridText, clueSource.Value, maxPosition);

        grid.IsSuccess.Should().BeTrue();

        var symmetry = grid.Value.GetSymmetries(9);

        symmetry.Should().NotBeNull().And.NotBeEmpty();
    }

    public static readonly IReadOnlyCollection<object[]> SymmetryTestCases =
            new List<SymmetryTest>
            {
                new("1 vs 2",
                    @"
1--------
---------
---------
---------
---------
---------
---------
---------
--------2"),
                new("1 vs 1",
                    @"
1--------
---------
---------
---------
---------
---------
---------
---------
--------1"),
                new("Centre",
                    @"
1--------
---------
---------
---------
----5----
---------
---------
---------
--------2"),

                new("More Cells",
                    @"
1--------
-------3-
---------
---------
----5----
---------
---------
-4-------
--------2")


            }.Select(x=>new object[]{x}).ToList()
        ;




}
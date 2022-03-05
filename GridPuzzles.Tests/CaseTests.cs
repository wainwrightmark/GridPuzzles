using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GridPuzzles.Cells;
using GridPuzzles.Clues;
using Sudoku;
using Xunit;
using Xunit.Sdk;

namespace GridPuzzles.Tests;

public class CaseTests
{
#pragma warning disable CA1034 // Nested types should not be visible
    public class SingleCellTestCase
#pragma warning restore CA1034 // Nested types should not be visible
    {
        public SingleCellTestCase(string testName, string grid, Position positionToCheck, IReadOnlyCollection<int> expectedPossibleValues)
        {
            TestName = testName;
            Grid = grid;
            PositionToCheck = positionToCheck;
            ExpectedPossibleValues = expectedPossibleValues;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return TestName;
        }

        public string TestName { get; }

        public string Grid { get; }

        public Position PositionToCheck { get; }

        public IReadOnlyCollection<int> ExpectedPossibleValues { get; }
    }


    public static readonly List<object[]> Cases = new List<SingleCellTestCase>
    {
        new("Test naked single in row",

            @"
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
1	2	3	4	5	6	7	8	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-",

            new Position(9, 5), new List<int>(){9}
        ),
        new("Test naked single in Column",

            @"
1	-	-	-	-	-	-	-	-
2	-	-	-	-	-	-	-	-
3	-	-	-	-	-	-	-	-
4	-	-	-	-	-	-	-	-
5	-	-	-	-	-	-	-	-
6	-	-	-	-	-	-	-	-
7	-	-	-	-	-	-	-	-
8	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-",

            new Position(1, 9), new List<int>(){9}
        ),

        new("Test naked single in Box",

            @"
1	2	3	-	-	-	-	-	-
4	5	6	-	-	-	-	-	-
7	8	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-",

            new Position(3, 3), new List<int>(){9}
        ),

        new("Test naked single pinned",

            @"
1	2	3	-	-	-	-	-	-
-	-	-	-	-	5	-	-	-
-	-	-	-	4	6	-	-	-
-	-	-	7	-	-	-	-	-
-	-	-	8	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-",

            new Position(4, 1), new List<int>(){9}
        ),

        new("Test Conjugate Pair",

            @"
1	2	3	4	-	-	-	-	9
-	-	-	7	-	-	-	-	-
-	-	-	8	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
",
            new Position(7, 1),
            new []{7,8}
        ),

        new("Test Conjugate Trio",

            @"
1	2	3	-	-	-	-	-	9
-	-	-	7	-	-	-	-	-
-	-	-	8	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
",
            new Position(7, 1),
            new []{7,8}
        ),

        new("Test Conjugate quad",

            @"
1	2	-	-	-	-	-	-	9
-	-	8	7	-	-	-	-	-
-	-	7	8	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
",
            new Position(7, 1),
            new []{7,8}
        ),

        new("Test Hidden Twin",

            @"
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	1	2	3	4	-	-
-	-	1	-	-	-	-	-	-
-	-	2	-	-	-	-	-	-
-	-	3	-	-	-	-	-	-
-	2	4	-	-	-	-	-	-
-	3	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
",
            new Position(1, 1),
            new []{2,3}
        ),
        new("Test Hidden Trio",

            @"
-	-	-	-	-	-	-	-	-
-	-	-	1	2	3	-	-	-
-	-	-	-	-	-	1	2	3
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
",
            new Position(1, 1),
            new []{1, 2,3}
        ),
//            new SingleCellTestCase("Test Hidden Quad",

//                @"
//-	-	-	-	-	-	-	-	-
//-	-	-	-	-	-	-	-	4
//-	-	-	-	-	-	1	2	3
//-	-	-	-	-	4	-	-	-
//-	-	-	-	-	3	-	-	-
//-	-	-	-	1	2	-	-	-
//-	-	-	-	2	1	-	-	-
//-	-	-	-	3	-	-	-	-
//-	-	-	-	4	-	-	-	-
//",
//                new Position(1, 1),
//                new []{1, 2,3,4}
//            ),

//            new SingleCellTestCase("Test Hidden Quad 2",

//                @"
//-	-	-	-	-	-	-	-	-
//-	-	-	-	-	-	-	-	4
//-	-	-	-	-	-	1	2	3
//-	-	-	-	-	4	-	-	-
//-	-	-	-	-	3	-	-	-
//-	-	-	-	1	2	-	-	-
//-	-	-	-	2	1	-	-	-
//-	-	-	-	3	-	-	-	-
//1	-	-	-	4	-	-	-	-
//",
//                new Position(1, 1),
//                new []{2,3,4}
//            ),

        new("Test X-Wing",

            @"
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
1	-	3	-	5	6	-	8	9
4	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
8	-	5	-	1	2	-	3	6
-	4	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
",
            new Position(4,1),
            new []{1,2,3,5,6,7,8,9}
        ),

        new("Test Swordfish",

            @"
-	-	2	-	-	3	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	-	-	1	-
-	-	4	-	-	-	-	-	5
-	-	-	-	1	-	-	-	-
-	-	-	-	-	-	-	-	-
-	-	-	-	-	6	-	-	7
-	1	-	-	-	-	-	-	-
-	-	-	-	-	-	-	-	-
",
            new Position(1, 6),
            new []{2,3,5,6,7,8,9}
        ),


    }.Select(x=> new object[]{x}).ToList();


    [Theory]
    [MemberData(nameof(Cases))]
    public async Task TestCase(SingleCellTestCase myCase)
    {
        var maxPosition = Position.NineNine;

        var variantBuilders =
            SudokuVariant.SudokuVariantBuilders.GetVariantBuilderArgumentPairs(maxPosition);

        var gridString = myCase.Grid.Trim().Replace("\t", "");

        var clueSource = await ClueSource<int, IntCell>.TryCreateAsync(
            variantBuilders, maxPosition, NumbersValueSource.Sources[9], CancellationToken.None);

        if(clueSource.IsFailure)
            throw new XunitException(clueSource.Error);

        var gridResult = Grid<int, IntCell>.CreateFromString(gridString,clueSource.Value, maxPosition);

        gridResult.IsSuccess.Should().BeTrue();


        var r = Solver.Solve(gridResult.Value, 0,0);// Note: some of these test cases lead to impossible grids

        r.Contradictions.Should().BeEmpty();

        r.MostAdvancedGrid.GetCell(myCase.PositionToCheck).Should().BeEquivalentTo(myCase.ExpectedPossibleValues);
    }






}
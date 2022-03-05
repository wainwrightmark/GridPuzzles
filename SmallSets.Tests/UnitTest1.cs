using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace SmallSets.Tests;
public class UnitTest1
{
    public static IEnumerable<object[]> GetListListTestCases()
    {
        return GetListListTestCases1().Select(x => new[] { x.l1, x.l2 });

        static IEnumerable<(int[] l1, int[] l2)> GetListListTestCases1()
        {
            //Empty Empty
            yield return (Array.Empty<int>(), Array.Empty<int>());

            //Empty Some

            yield return (Array.Empty<int>(), new int[]{1,2,3});

            //Some Empty

            yield return (new int[]{1,2,3}, Array.Empty<int>());

            // Some SomeOther

            yield return (new int[]{1,2,3}, new int[]{5,4,3});

            //Some SomeCompletelyDifferent

            yield return (new int[]{1,2,3}, new int[]{6, 5,4});

            //Some SomeSame

            yield return (new int[]{1,2,3}, new int[]{1,2,3});

            //Some SomePlusOne
            
            yield return (new int[]{1,2,3}, new int[]{4, 1,2,3});

            //Some SomeMinusOne
            yield return (new int[]{1,2,3}, new int[]{1,3});

            //BiggerNumbers 

            yield return (new int[]{0, 8, 16, 31}, new int[]{0, 8, 16, 31});
        }
    }
    
    
    public static IEnumerable<object[]> GetListIntTestCases()
    {
        return GetListIntTestCases1().Select(x => new object[] { x.l1, x.i });

        static IEnumerable<(int[] l1, int i)> GetListIntTestCases1()
        {
            //Empty Empty
            yield return (Array.Empty<int>(), 1);
            yield return (new int[]{1,2,3}, 1);
            yield return (new int[]{1,2,3}, 4);

            //BiggerNumbers 

            yield return (new int[]{0, 8, 16, 31}, 1);
            yield return (new int[]{1,2,3}, 31);
        }
    }
    

    [Theory] [MemberData(nameof(GetListIntTestCases))] public void TestCount(int[] l1, int l2) => Check(l1,  (x)=>x.Count);
    [Theory] [MemberData(nameof(GetListIntTestCases))] public void TestIsEmpty(int[] l1, int l2) => Check(l1,  (x)=>x.IsEmpty(), x=>x.Count == 0);
    [Theory] [MemberData(nameof(GetListIntTestCases))] public void TestMin(int[] l1, int l2) => Check(l1,  (x)=>x.Min(), x=>x.Min);
    [Theory] [MemberData(nameof(GetListIntTestCases))] public void TestMax(int[] l1, int l2) => Check(l1,  (x)=>x.Max(), x=>x.Max);
    [Theory] [MemberData(nameof(GetListIntTestCases))] public void TestAdd(int[] l1, int l2) => Check(l1, l2, (x, i)=>x.Add(i));
    [Theory] [MemberData(nameof(GetListIntTestCases))] public void TestContains(int[] l1, int l2) => Check(l1, l2, (x, i)=>x.Contains(i));
    [Theory] [MemberData(nameof(GetListIntTestCases))] public void TestRemove(int[] l1, int l2) => Check(l1, l2, (x, i)=>x.Remove(i));
    [Theory] [MemberData(nameof(GetListIntTestCases))] public void TestTryGetValue(int[] l1, int l2) => Check(l1, l2, (x, i)=>x.TryGetValue(i, out _));



    [Theory] [MemberData(nameof(GetListListTestCases))] public void TestClear(int[] l1, int[] l2) => Check(l1, (x)=>x.Clear());
    [Theory] [MemberData(nameof(GetListListTestCases))] public void TestExcept(int[] l1, int[] l2) => Check(l1, l2, (x, l2)=>x.Except(l2));
    [Theory] [MemberData(nameof(GetListListTestCases))] public void TestProperSubset(int[] l1, int[] l2) => Check(l1, l2, (x, l2)=>x.IsProperSubsetOf(l2));
    [Theory] [MemberData(nameof(GetListListTestCases))] public void TestProperSuperset(int[] l1, int[] l2) => Check(l1, l2, (x, l2)=>x.IsProperSupersetOf(l2));
    [Theory] [MemberData(nameof(GetListListTestCases))] public void TestSubset(int[] l1, int[] l2) => Check(l1, l2, (x, l2)=>x.IsSubsetOf(l2));
    [Theory] [MemberData(nameof(GetListListTestCases))] public void TestSuperset(int[] l1, int[] l2) => Check(l1, l2, (x, l2)=>x.IsSupersetOf(l2));
    [Theory] [MemberData(nameof(GetListListTestCases))] public void TestSetEquals(int[] l1, int[] l2) => Check(l1, l2, (x, l2)=>x.SetEquals(l2));
    [Theory] [MemberData(nameof(GetListListTestCases))] public void TestOverlaps(int[] l1, int[] l2) => Check(l1, l2, (x, l2)=>x.Overlaps(l2));
    [Theory] [MemberData(nameof(GetListListTestCases))] public void TestSymmetricExcept(int[] l1, int[] l2) => Check(l1, l2, (x, l2)=>x.SymmetricExcept(l2));
    [Theory] [MemberData(nameof(GetListListTestCases))] public void TestIntersect(int[] l1, int[] l2) => Check(l1, l2, (x, l2)=>x.Intersect(l2));
    [Theory] [MemberData(nameof(GetListListTestCases))] public void TestUnion(int[] l1, int[] l2) => Check(l1, l2, (x, l2)=>x.Union(l2));

    [Theory] [MemberData(nameof(GetListListTestCases))] public void TestIntersectSS(int[] l1, int[] l2) => Check(l1, l2, (x, l2)=>x.Intersect(l2));
    [Theory] [MemberData(nameof(GetListListTestCases))] public void TestExceptSS(int[] l1, int[] l2) => CheckSS(l1, l2, (x, l2)=>x.Except(l2));
    [Theory] [MemberData(nameof(GetListListTestCases))] public void TestProperSubsetSS(int[] l1, int[] l2) => CheckSS(l1, l2, (x, l2)=>x.IsProperSubsetOf(l2));
    [Theory] [MemberData(nameof(GetListListTestCases))] public void TestProperSupersetSS(int[] l1, int[] l2) => CheckSS(l1, l2, (x, l2)=>x.IsProperSupersetOf(l2));
    [Theory] [MemberData(nameof(GetListListTestCases))] public void TestSubsetSS(int[] l1, int[] l2) => CheckSS(l1, l2, (x, l2)=>x.IsSubsetOf(l2));
    [Theory] [MemberData(nameof(GetListListTestCases))] public void TestSupersetSS(int[] l1, int[] l2) => CheckSS(l1, l2, (x, l2)=>x.IsSupersetOf(l2));


    private void Check<T>(int[] l1, int[] l2, Func<IImmutableSet<int>, int[], T> func)
    {
        var s1 = l1.ToSmallSet();
        var r1 = func(s1, l2);

        var s2 = l1.ToImmutableSortedSet();
        var r2 = func(s2, l2);

        if (r1 is IEnumerable<int> e1 && r2 is IEnumerable<int> e2)
            string.Join(",", e1).Should().Be(string.Join(",", e2));
        else
        {
            r1 .Should().Be(r2);
        }
    }
    
    private void CheckSS<T>(int[] l1, int[] l2, Func<IImmutableSet<int>, IEnumerable<int>, T> func)
    {
        var s1 = l1.ToSmallSet();
        var r1 = func(s1, l2.ToSmallSet());

        var s2 = l1.ToImmutableSortedSet();
        var r2 = func(s2, l2);

        if (r1 is IEnumerable<int> e1 && r2 is IEnumerable<int> e2)
            string.Join(",", e1).Should().Be(string.Join(",", e2));
        else
        {
            r1 .Should().Be(r2);
        }
    }
    
    private void Check<T>(int[] l1, int i, Func<IImmutableSet<int>, int, T> func)
    {
        var s1 = l1.ToSmallSet();
        var r1 = func(s1, i);

        var s2 = l1.ToImmutableSortedSet();
        var r2 = func(s2, i);

        if (r1 is IEnumerable<int> e1 && r2 is IEnumerable<int> e2)
            string.Join(",", e1).Should().Be(string.Join(",", e2));
        else
        {
            r1 .Should().Be(r2);
        }
    }
    
    private void Check<T>(int[] l1, Func<IImmutableSet<int>, T> func)
    {
        var s1 = l1.ToSmallSet();
        var r1 = func(s1);

        var s2 = l1.ToImmutableSortedSet();
        var r2 = func(s2);

        if (r1 is IEnumerable<int> e1 && r2 is IEnumerable<int> e2)
            string.Join(",", e1).Should().Be(string.Join(",", e2));
        else
        {
            r1 .Should().Be(r2);
        }
    }
    
    private void Check<T>(int[] l1, Func<SmallSet, T> func, Func<ImmutableSortedSet<int>, T> func2)
    {
        var s1 = l1.ToSmallSet();
        var r1 = func(s1);

        var s2 = l1.ToImmutableSortedSet();
        var r2 = func2(s2);

        if (r1 is IEnumerable<int> e1 && r2 is IEnumerable<int> e2)
            string.Join(",", e1).Should().Be(string.Join(",", e2));
        else
        {
            r1 .Should().Be(r2);
        }
    }
    
}


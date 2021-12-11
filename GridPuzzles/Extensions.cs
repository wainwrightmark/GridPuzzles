using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;

namespace GridPuzzles;

public static class Extensions
{
    public static IReadOnlySet<T> IntersectAllSets<T>(this IEnumerable<IReadOnlySet<T>> sets)
    {
        HashSet<T>? mainSet = null;
        foreach (var set in sets.OrderBy(x=>x.Count))
        {
            if (mainSet is null)
                mainSet = set.ToHashSet();
            else if (mainSet.Count == 0)
            {
                break;
            }
            else
            {
                var elementsToRemove = mainSet.Where(x => !set.Contains(x)).ToList();

                if (elementsToRemove.Count == mainSet.Count)
                    mainSet.Clear();
                else
                    foreach (var ele in elementsToRemove)
                        mainSet.Remove(ele);
            }
        }

        if(mainSet is null)
            return ImmutableHashSet<T>.Empty;
        return mainSet;
    }


    public static ImmutableSortedSet<T> UnionAllSortedSets<T>(this IEnumerable<ImmutableSortedSet<T>> sets)
    {
        return sets.Aggregate(ImmutableSortedSet<T>.Empty, (current1, e) => current1.Union(e));
    }


    public static Parallel GetOrthogonal(this Parallel d)
    {
        return d switch
        {
            Parallel.Row => Parallel.Column,
            Parallel.Column => Parallel.Row,
            _ => throw new ArgumentOutOfRangeException(nameof(d), d, null)
        };
    }

    public static ushort GetIndex(this Parallel parallel, Position p)
    {
        return parallel switch
        {
            Parallel.Row => p.Row,
            Parallel.Column => p.Column,
            _ => throw new ArgumentOutOfRangeException(nameof(parallel), parallel, null)
        };
    }
        

    public static bool TryExtract<T>(this Maybe<T> maybe, out T value)
    {
        if (maybe.HasValue)
        {
            value = maybe.Value;
            return true;
        }

        value = default!;
        return false;
    }

    public static (T t1, T t2) GetFirstTwo<T>(this IEnumerable<T> e)
    {
        using var enumerator = e.GetEnumerator();
        enumerator.MoveNext();
        var first = enumerator.Current;
        enumerator.MoveNext();
        var second = enumerator.Current;
        return (first, second);
    }

    public static (T t1, T t2, T t3) GetFirstThree<T>(this IEnumerable<T> e)
    {
        using var enumerator = e.GetEnumerator();
        enumerator.MoveNext();
        var first = enumerator.Current;
        enumerator.MoveNext();
        var second = enumerator.Current;
        enumerator.MoveNext();
        var third = enumerator.Current;
        return (first, second, third);
    }

    public static (T t1, T t2, T t3, T t4) GetFirstFour<T>(this IEnumerable<T> e)
    {
        using var enumerator = e.GetEnumerator();
        enumerator.MoveNext();
        var first = enumerator.Current;
        enumerator.MoveNext();
        var second = enumerator.Current;
        enumerator.MoveNext();
        var third = enumerator.Current;
        enumerator.MoveNext();
        var fourth = enumerator.Current;
        return (first, second, third, fourth);
    }


    public static IEnumerable<T[]> GetCombinations<T>(this IReadOnlyCollection<T> options, int length)
    {
        switch (length)
        {
            case 0:
                throw new ArgumentException(@"Cannot Get Combinations with length 0", nameof(length));
            case 1:
            {
                foreach (var o in options.Select(x => new[] {x}))
                    yield return o;

                break;
            }
            default:
            {
                foreach (var t in options)
                {
                    var o = new[] {t};

                    var combos = GetCombinations(options, length - 1);

                    foreach (var combo in combos)
                        yield return o.Concat(combo).ToArray();
                }

                break;
            }
        }
    }


    /// <summary>
    /// Returns the same enumerable but without the null elements
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
        where T : class
    {
        return source.Where(t => t != null)!;
    }


    /// <summary>
    /// Gets an inclusive range.
    /// </summary>
    public static IEnumerable<ushort> RangeTo(this ushort min, ushort max)
    {
        for (var i = min; i <= max; i++)
            yield return i;
    }
}
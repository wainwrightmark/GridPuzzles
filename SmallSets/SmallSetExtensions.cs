namespace SmallSets;

public static class SmallSetExtensions
{
    /// <summary>
    /// Create a small set from this enumerable.
    /// Make sure that every member is between 0 and 31 inclusive.
    /// </summary>
    public static SmallSet ToSmallSet(this IEnumerable<int> enumerable)
    {
        if (enumerable is SmallSet ss) return ss;
        return new SmallSet(enumerable);
    }
}
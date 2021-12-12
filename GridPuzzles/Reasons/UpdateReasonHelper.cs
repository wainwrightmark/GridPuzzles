namespace GridPuzzles.Reasons;

public static class UpdateReasonHelper
{
    public static IUpdateReason Combine(this IUpdateReason reason1, IUpdateReason reason2)
    {
        if (reason1.Equals(reason2))
            return reason1;
        if (reason1 is ReasonList reasonList)
            return reasonList.Combine(reason2);
        if (reason2 is ReasonList reasonList2)
            return reasonList2.Combine(reason1);
        if (reason1 is ISingleReason sr1 && reason2 is ISingleReason sr2)
            return new ReasonList(ImmutableArray.Create(sr1, sr2));

        throw new ArgumentOutOfRangeException(nameof(reason1));
    }
}
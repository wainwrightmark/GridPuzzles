using System.Threading;
using System.Threading.Tasks;
using GridPuzzles.VariantBuilderArguments;

namespace GridPuzzles.Clues;

public sealed class ClueSource<T> where T : notnull
{
    public ClueSource(Position minPosition, Position maxPosition, IValueSource<T> valueSource,
        params IClueBuilder<T>[] clueBuilders) :
        this(minPosition, maxPosition, valueSource, clueBuilders as IEnumerable<IClueBuilder<T>>)
    {
    }

    public static async Task<Result<ClueSource<T>>> TryCreateAsync(
        IEnumerable<VariantBuilderArgumentPair<T>> variantsInPlay, Position maxPosition,
        IValueSource<T> valueSource, CancellationToken cancellation)
    {
        var variants = await variantsInPlay
            .Select(x => x.VariantBuilder.TryGetClueBuildersAsync(x.Pairs, cancellation))
            .Combine().Map(x => x.SelectMany(y => y).ToList());

        if (variants.IsFailure) return variants.ConvertFailure<ClueSource<T>>();

        var clueSource = new ClueSource<T>(Position.Origin, maxPosition, valueSource, variants.Value);

        return clueSource;
    }

    public ClueSource(Position minPosition, Position maxPosition, IValueSource<T> valueSource,
        IEnumerable<IClueBuilder<T>> clueBuilders)
    {
        ValueSource = valueSource;

        var clues = new List<IClue<T>>();

        ClueBuilders = clueBuilders.ToList();

        foreach (var group in ClueBuilders.GroupBy(x => x.Level).OrderBy(x => x.Key))
        {
            var levelClues = new List<IClue<T>>();

            foreach (var clueBuilder in group)
                levelClues.AddRange(clueBuilder.CreateClues(minPosition, maxPosition, valueSource, clues));
            clues.AddRange(levelClues);
        }

        Clues = clues;

        UniquenessClueHelper = new UniquenessClueHelper<T>(clues);
        CompletenessClueHelper = new CompletenessClueHelper<T>(clues);
        RelationshipClueHelper = new RelationshipClueHelper<T>(UniquenessClueHelper, clues);
        RuleClueHelper = new RuleClueHelper<T>(clues);
        MetaRuleClueHelper = new MetaRuleClueHelper<T>(clues);

        BifurcationClueHelper = new BifurcationClueHelper<T>(clues);
        DynamicOverlayClueHelper = new DynamicOverlayClueHelper<T>(clues);
        FixedOverlays = ClueBuilders.SelectMany(clueBuilder => 
            clueBuilder.GetOverlays(minPosition, maxPosition)
            .Select(overlay=> new CellOverlayWrapper(overlay, new CellOverlayMetadata(Maybe<IClueBuilder>.From(clueBuilder), false))) )
            .ToList();
    }

    public IReadOnlyCollection<IClue<T>> Clues { get; }

    public IEnumerable<IClueBuilder<T>> ClueBuilders { get; }

    public IValueSource<T> ValueSource { get; }


    public RuleClueHelper<T> RuleClueHelper { get; }
    public MetaRuleClueHelper<T> MetaRuleClueHelper { get; }
    public UniquenessClueHelper<T> UniquenessClueHelper { get; }

    public CompletenessClueHelper<T> CompletenessClueHelper { get; }
    public BifurcationClueHelper<T> BifurcationClueHelper { get; }
    public RelationshipClueHelper<T> RelationshipClueHelper { get; }

    public DynamicOverlayClueHelper<T> DynamicOverlayClueHelper { get; }

    public IReadOnlyList<CellOverlayWrapper> FixedOverlays { get; }

    //TODO add more to these methods when we have option clues!
}
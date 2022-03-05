using System.Threading;
using System.Threading.Tasks;
using GridPuzzles.VariantBuilderArguments;

namespace GridPuzzles.Clues;

public sealed class ClueSource<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    public ClueSource(Position minPosition, Position maxPosition, IValueSource<T, TCell> valueSource,
        params IClueBuilder<T, TCell>[] clueBuilders) :
        this(minPosition, maxPosition, valueSource, clueBuilders as IEnumerable<IClueBuilder<T, TCell>>)
    {
    }

    public static async Task<Result<ClueSource<T, TCell>>> TryCreateAsync(
        IEnumerable<VariantBuilderArgumentPair<T, TCell>> variantsInPlay, Position maxPosition,
        IValueSource<T, TCell> valueSource, CancellationToken cancellation)
    {
        var variants = await variantsInPlay
            .Select(x => x.VariantBuilder.TryGetClueBuildersAsync(x.Pairs, cancellation))
            .Combine().Map(x => x.SelectMany(y => y).ToList());

        if (variants.IsFailure) return variants.ConvertFailure<ClueSource<T, TCell>>();

        var clueSource = new ClueSource<T, TCell>(Position.Origin, maxPosition, valueSource, variants.Value);

        return clueSource;
    }

    public ClueSource(Position minPosition, Position maxPosition, IValueSource<T, TCell> valueSource,
        IEnumerable<IClueBuilder<T, TCell>> clueBuilders)
    {
        ValueSource = valueSource;

        var clues = new List<IClue<T, TCell>>();

        ClueBuilders = clueBuilders.ToList();

        foreach (var group in ClueBuilders.GroupBy(x => x.Level).OrderBy(x => x.Key))
        {
            var levelClues = new List<IClue<T, TCell>>();

            foreach (var clueBuilder in group)
                levelClues.AddRange(clueBuilder.CreateClues(minPosition, maxPosition, valueSource, clues));
            clues.AddRange(levelClues);
        }

        Clues = clues;

        UniquenessClueHelper = new UniquenessClueHelper<T, TCell>(clues);
        CompletenessClueHelper = new CompletenessClueHelper<T, TCell>(clues);
        RelationshipClueHelper = new RelationshipClueHelper<T, TCell>(UniquenessClueHelper, clues);
        RuleClueHelper = new RuleClueHelper<T, TCell>(clues);
        MetaRuleClueHelper = new MetaRuleClueHelper<T, TCell>(clues);

        BifurcationClueHelper = new BifurcationClueHelper<T, TCell>(clues);
        DynamicOverlayClueHelper = new DynamicOverlayClueHelper<T, TCell>(clues);
        FixedOverlays = ClueBuilders.SelectMany(clueBuilder => 
            clueBuilder.GetOverlays(minPosition, maxPosition)
            .Select(overlay=> new CellOverlayWrapper(overlay, new CellOverlayMetadata(Maybe<IClueBuilder>.From(clueBuilder), false))) )
            .ToList();
    }

    public IReadOnlyCollection<IClue<T, TCell>> Clues { get; }

    public IEnumerable<IClueBuilder<T, TCell>> ClueBuilders { get; }

    public IValueSource<T, TCell> ValueSource { get; }


    public RuleClueHelper<T, TCell> RuleClueHelper { get; }
    public MetaRuleClueHelper<T, TCell> MetaRuleClueHelper { get; }
    public UniquenessClueHelper<T, TCell> UniquenessClueHelper { get; }

    public CompletenessClueHelper<T, TCell> CompletenessClueHelper { get; }
    public BifurcationClueHelper<T, TCell> BifurcationClueHelper { get; }
    public RelationshipClueHelper<T, TCell> RelationshipClueHelper { get; }

    public DynamicOverlayClueHelper<T, TCell> DynamicOverlayClueHelper { get; }

    public IReadOnlyList<CellOverlayWrapper> FixedOverlays { get; }

    //TODO add more to these methods when we have option clues!
}
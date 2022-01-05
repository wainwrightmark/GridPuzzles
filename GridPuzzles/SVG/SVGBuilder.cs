using System.Drawing;
using GridPuzzles.Clues;
using GridPuzzles.Overlays;
using GridPuzzles.Session;
using GridPuzzles.VariantBuilderArguments;
using SVGElements;

namespace GridPuzzles.SVG;

public sealed class GridPageGridSVG : SVGBuilder
{
    /// <inheritdoc />
    public GridPageGridSVG(ISolveState solveState, 
        IReadOnlySet<IClueBuilder> selectedClueBuilders,
        Func<Position, IEnumerable<ISVGEventHandler>> getEventHandlers,
        SessionSettings sessionSettings) : base(solveState, getEventHandlers)
    {
        SessionSettings = sessionSettings;
        SelectedClueBuilders = selectedClueBuilders;
    }

    public SessionSettings SessionSettings { get; }
    public IReadOnlySet<IClueBuilder> SelectedClueBuilders { get; }

    /// <inheritdoc />
    public override bool IsSelected(IClueBuilder clueBuilder)
    {
        return SelectedClueBuilders.Contains(clueBuilder);
    }

    /// <inheritdoc />
    protected override IEnumerable<CellOverlayWrapper> ExtraOverlays
    {
        get { yield break; }
    }

    /// <inheritdoc />
    protected override IReadOnlyDictionary<Position, IUpdateReason> CreatePositionReasonDictionary()
    {
        return SolveState.UpdateResult.GetPositionReasons();
    }

    /// <inheritdoc />
    protected override IEnumerable<SVGElement> Definitions
    {
        get
        {
            yield return new SVGLinearGradient("contradiction", new List<SVGStop>()
            {
                new("start", "0%", Color.Red.ToSVGColor(), 0.4),
                new("halfway", "50%", Color.IndianRed.ToSVGColor(), 0.4),
            });

            yield return new SVGLinearGradient("updatedCell", new List<SVGStop>()
            {
                new("start", "0%", Color.DodgerBlue.ToSVGColor(), 0.25),
                new("halfway", "50%", Color.DeepSkyBlue.ToSVGColor(), 0.25),
            });

            yield return new SVGLinearGradient("contributingCell", new List<SVGStop>()
            {
                new("start", "0%", Color.Yellow.ToSVGColor(), 0.25),
                new("halfway", "50%", Color.Gold.ToSVGColor(), 0.25),
            });

            yield return new SVGLinearGradient("completedCell", new List<SVGStop>()
            {
                new("start", "0%", Color.LimeGreen.ToSVGColor(), 0.3),
                new("halfway", "50%", Color.Green.ToSVGColor(), 0.3),
            });
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<SVGGroup> CreateExtraGroups()
    {
        var contradictions = new SVGGroup(Id: "Contradictions",
            Children:
            SolveState.UpdateResult.GetContradictionPositions(Grid)
                .Select(position =>
                    new SVGRectangle(
                        "UpdatedCell" + position,
                        position.GetX(false),
                        position.GetY(false),
                        PointerEvents: PointerEvents.none,
                        Fill: "url(#contradiction)",
                        StrokeWidth: 0,
                        Height: CellStyleHelpers.CellSize,
                        Width: CellStyleHelpers.CellSize
                    )
                ).ToList()
        );

        var contributing = new SVGGroup(Id: "Contributing",
            SolveState.UpdateResult.GetContributingPositions(Grid)
                .Select(position =>
                {
                    string fill = "url(#contributingCell)";

                    return new SVGRectangle(
                        "Contributor" + position,
                        position.GetX(false),
                        position.GetY(false),
                        5,
                        PointerEvents: PointerEvents.none,
                        Fill: fill,
                        StrokeWidth: 0,
                        Height: CellStyleHelpers.CellSize,
                        Width: CellStyleHelpers.CellSize
                    );
                }).ToList()
        );

        var updates =
            new SVGGroup(Id: "Updates",
                SolveState.UpdateResult.UpdatedPositions.Select(position =>
                {
                    string fill = Grid.GetPossibleValueCount(position) <= 1
                        ? "url(#completedCell)"
                        : "url(#updatedCell)";

                    return new SVGRectangle(
                        "Update" + position,
                        position.GetX(false),
                        position.GetY(false),
                        5,
                        PointerEvents: PointerEvents.none,
                        Fill: fill,
                        StrokeWidth: 0,
                        Height: CellStyleHelpers.CellSize,
                        Width: CellStyleHelpers.CellSize
                    );
                }).ToList());


        var group = new SVGGroup(Id: "StateIndicators", Children: new[] { contributing, updates, contradictions });

        yield return group;
    }


    /// <inheritdoc />
    protected override IEnumerable<SVGText> CreateCellTexts()
    {
        foreach (var position in Grid.AllPositions)
        {
            var r = SolveState.GetCellText(position, SessionSettings.MinimumValuesToShow,
                CellStyleHelpers.CellSize);
            if (r.HasValue)
                yield return r.Value;
        }
    }
}

public sealed class VariantBuilderFormGridSVG : SVGBuilder
{
    /// <inheritdoc />
    public VariantBuilderFormGridSVG(ISolveState solveState,
        Func<Position, IEnumerable<ISVGEventHandler>> getEventHandlers,
        IVariantBuilder variantBuilder,
        VariantBuilderArgument variantBuilderArgument,
        Dictionary<string, string> results)
        : base(solveState,   getEventHandlers)
    {
        VariantBuilder = variantBuilder;
        VariantBuilderArgument = variantBuilderArgument;
        Results = results;
    }

    public IVariantBuilder VariantBuilder { get; }

    public VariantBuilderArgument VariantBuilderArgument { get; }

    public Dictionary<string, string> Results { get; }

    /// <inheritdoc />
    public override bool IsSelected(IClueBuilder clueBuilder)
    {
        return false;
    }

    /// <inheritdoc />
    protected override IEnumerable<SVGText> CreateCellTexts()
    {
        foreach (var position in SolveState.FixedValuePositions)
        {
            var r = SolveState.GetCellText(position, 1, CellStyleHelpers.CellSize);
            if (r.HasValue)
                yield return r.Value;
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<SVGElement> Definitions
    {
        get { yield break; }
    }

    /// <inheritdoc />
    protected override IReadOnlyDictionary<Position, IUpdateReason> CreatePositionReasonDictionary()
    {
        return ImmutableDictionary<Position, IUpdateReason>.Empty;
    }

    /// <inheritdoc />
    protected override IEnumerable<SVGGroup> CreateExtraGroups()
    {
        yield break;
    }


    /// <inheritdoc />
    protected override IEnumerable<CellOverlayWrapper> ExtraOverlays
    {
        get
        {
            var r = VariantBuilder.TryGetClueBuilders(Results);
            if (r.IsSuccess)
            {
                foreach (var clueBuilder in r.Value)
                {
                    foreach (var cellOverlay in clueBuilder.GetOverlays(Position.Origin, Grid.MaxPosition))
                    {
                        yield return new CellOverlayWrapper(cellOverlay, new CellOverlayMetadata(Maybe<IClueBuilder>.None, true));
                    }
                }
            }
            else
            {
                foreach (var kvp in Results.Values
                             .SelectMany(ListPositionArgument.ParsePositions)
                             .Where(x => x.IsSuccess)
                             .Select(x => x.Value))
                {
                    yield return new CellOverlayWrapper(new TextCellOverlay(kvp, 1, 1, "✓", Color.Black, Color.Transparent), CellOverlayMetadata.Empty);
                }
            }
        }
    }
}

public abstract class SVGBuilder
{
    protected SVGBuilder(ISolveState solveState,
        Func<Position, IEnumerable<ISVGEventHandler>> getEventHandlers)
    {
        SolveState = solveState;
        EventHandlers = getEventHandlers;
    }

    public ISolveState SolveState { get; }
    public abstract bool IsSelected(IClueBuilder clueBuilder);
    public Func<Position, IEnumerable<ISVGEventHandler>> EventHandlers { get; }

    public IGrid Grid => SolveState.Grid;

    protected abstract IEnumerable<SVGText> CreateCellTexts();

    protected abstract IEnumerable<SVGGroup> CreateExtraGroups();


    protected abstract IEnumerable<SVGElement> Definitions { get; }

    protected abstract IEnumerable<CellOverlayWrapper> ExtraOverlays { get; }

    protected abstract IReadOnlyDictionary<Position, IUpdateReason> CreatePositionReasonDictionary();

    private static SVGLinearGradient CreateLinearGradient(string name, IGrouping<Position, CellColorOverlay> group)
    {
        var sliceSize = 100 / group.Count();

        var gradient = new SVGLinearGradient(name,
            group.Select((x, i) =>
                new SVGStop("stop" + i,
                    StopColor: x.Color.ToSVGColor(), Offset: $"{i * sliceSize}%")
            ).ToList()
        );

        return gradient;
    }

    private static SVGText CreateOuterIndicator((Position position, string text) arg)
    {
        var (position, text) = arg;
        return new SVGText(
            "outerIndicator" + text,
            text,
            null,
            X: position.GetX(true),
            Y: position.GetY(true),
            TextAnchor: TextAnchor.middle,
            DominantBaseline: DominantBaseline.middle,
            PointerEvents: PointerEvents.none,
            FontWeight: "bold"
        );
    }

    private SVGRectangle CreateRectangle(Position position, IReadOnlyDictionary<Position, string> fillsDict,
        IReadOnlyDictionary<Position, IUpdateReason> textDictionary)
    {
        var fill = fillsDict.TryGetValue(position, out var v) ? v : "white";
        var eventHandlers = EventHandlers(position).ToImmutableList();


        IReadOnlyList<SVGElement>? children = null;

        if (textDictionary.TryGetValue(position, out var reason))
        {
            children = new[]
            {
                new SVGTitle("Title" + position, reason.Text)
            };
        }

        return
            new SVGRectangle(
                "CellRect" + position,
                position.GetX(false),
                position.GetY(false),
                1,
                TabIndex: 0,
                Fill: fill,
                Stroke: "black",
                StrokeWidth: 1,
                Height: CellStyleHelpers.CellSize,
                Width: CellStyleHelpers.CellSize,
                EventHandlers: eventHandlers,
                Children: children
            );
    }

    public SVGElements.SVG ComposeSVG()
    {
        var children = ImmutableList<SVGElement>.Empty.ToBuilder();

        var allOverlays = Grid.GetOverlays()
            .Concat(ExtraOverlays).ToList();

        var definitions = allOverlays
            .Select(x=>x.CellOverlay)
            .OfType<ICellSVGElementOverlay>()
            .SelectMany(x => x.SVGDefinitions(CellStyleHelpers.CellSize))
            .Concat(Definitions)
            .DistinctBy(x => x.Id)
            .ToList();

        var fillsDict = new Dictionary<Position, string>();
        HashSet<string> addedGradients = new();

        foreach (var group in allOverlays
                     .Select(x=>x.CellOverlay)
                     .OfType<CellColorOverlay>()
                     .GroupBy(x => x.Position))
        {
            if (group.Count() == 1)
                fillsDict.Add(group.Key, group.Single().Color.ToSVGColor());

            else
            {
                var name = "Gradient" + string.Join("-", group.Select(x => x.Color.Name).OrderBy(x => x));
                if (addedGradients.Add(name))
                {
                    var gradient = CreateLinearGradient(name, group);
                    definitions.Add(gradient);
                }

                fillsDict.Add(group.Key, $"url('#{name}')");
            }
        }

        children.Add(new SVGDefinitions("definitions", definitions));

        var outerIndicatorGroup = new SVGGroup("OuterIndicators",
            Grid.GetOuterIndicators().Select(CreateOuterIndicator).ToList());

        var reasonDictionary = CreatePositionReasonDictionary();

        children.Add(outerIndicatorGroup);

        var rects = Grid.AllPositions.Select(position => CreateRectangle(position, fillsDict, reasonDictionary))
            .ToList();

        children.Add(new SVGGroup("Cells", rects));


        var overlayGroups = new List<SVGElement>();
        foreach (var overlayGroup in allOverlays
                     .Select(x=> AsSVGOverlay(x, IsSelected))
                     .SelectMany(x=>x.ToList())
                     .GroupBy(x => x.overlay.ZIndex).OrderBy(x => x.Key))
        {
            
            var group = new SVGGroup(
                Id: $"OverlayLevel{overlayGroup.Key}",
                overlayGroup.SelectMany(x => x.overlay.SVGElements(CellStyleHelpers.CellSize,
                    x.selected)).ToList()
            );
            overlayGroups.Add(group);
        }


        children.Add(new SVGGroup(Id: "Overlays", Children: overlayGroups));

        children.AddRange(CreateExtraGroups());


        children.Add(new SVGGroup(Id: "CellViews", Children: CreateCellTexts().ToList()));

        var svg = new SVGElements.SVG("GridSVG",
            children.ToImmutable(),
            $"{0} {0} {CellStyleHelpers.GetHeight(Grid)} {CellStyleHelpers.GetWidth(Grid)}",
            "xMinYMin meet"
        );

        return svg;

        static Maybe<(ICellSVGElementOverlay overlay, bool selected)> AsSVGOverlay(CellOverlayWrapper wrapper,
            Func<IClueBuilder, bool> isSelected) 
        {
            if (wrapper.CellOverlay is ICellSVGElementOverlay svgElementOverlay)
            {
                var selected = wrapper.Metadata.AlwaysSelected || (wrapper.Metadata.ClueBuilder.HasValue && isSelected(wrapper.Metadata.ClueBuilder.Value));
                return (svgElementOverlay, selected);
            }

            return Maybe<(ICellSVGElementOverlay overlay, bool selected)>.None;
        }
    }
}
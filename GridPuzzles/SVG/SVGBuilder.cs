using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using GridPuzzles.Overlays;
using GridPuzzles.Reasons;
using GridPuzzles.Session;
using GridPuzzles.VariantBuilderArguments;
using MoreLinq;
using SVGElements;

namespace GridPuzzles.SVG;

public sealed class GridPageGridSVG : SVGBuilder
{
    /// <inheritdoc />
    public GridPageGridSVG(ISolveState solveState, Func<Position, IEnumerable<ISVGEventHandler>> getEventHandlers,
        SessionSettings sessionSettings) : base(solveState, getEventHandlers)
    {
        SessionSettings = sessionSettings;
    }

    public SessionSettings SessionSettings { get; }


    /// <inheritdoc />
    protected override IEnumerable<ICellOverlay> ExtraOverlays
    {
        get { yield break; }
    }

    /// <inheritdoc />
    protected override IReadOnlyDictionary<Position, IUpdateReason> GetPositionReasonDictionary()
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
    protected override IEnumerable<SVGGroup> GetExtraGroups()
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
    protected override IEnumerable<SVGText> GetTexts()
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
        : base(solveState, getEventHandlers)
    {
        VariantBuilder = variantBuilder;
        VariantBuilderArgument = variantBuilderArgument;
        Results = results;
    }

    public IVariantBuilder VariantBuilder { get; }

    public VariantBuilderArgument VariantBuilderArgument { get; }

    public Dictionary<string, string> Results { get; }

    /// <inheritdoc />
    protected override IEnumerable<SVGText> GetTexts()
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
    protected override IReadOnlyDictionary<Position, IUpdateReason> GetPositionReasonDictionary()
    {
        return ImmutableDictionary<Position, IUpdateReason>.Empty;
    }

    /// <inheritdoc />
    protected override IEnumerable<SVGGroup> GetExtraGroups()
    {
        yield break;
    }


    /// <inheritdoc />
    protected override IEnumerable<ICellOverlay> ExtraOverlays
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
                        yield return cellOverlay;
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
                    yield return new TextCellOverlay(kvp, 1, 1, "✓", Color.Black, Color.Transparent);
                }
            }
        }
    }
}

public abstract class SVGBuilder
{
    protected SVGBuilder(ISolveState solveState, Func<Position, IEnumerable<ISVGEventHandler>> getEventHandlers)
    {
        SolveState = solveState;
        GetEventHandlers = getEventHandlers;
    }

    public ISolveState SolveState { get; }
    public Func<Position, IEnumerable<ISVGEventHandler>> GetEventHandlers { get; }

    public IGrid Grid => SolveState.Grid;

    protected abstract IEnumerable<SVGText> GetTexts();

    protected abstract IEnumerable<SVGGroup> GetExtraGroups();


    protected abstract IEnumerable<SVGElement> Definitions { get; }

    protected abstract IEnumerable<ICellOverlay> ExtraOverlays { get; }

    protected abstract IReadOnlyDictionary<Position, IUpdateReason> GetPositionReasonDictionary();

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

    private static SVGText GetOuterIndicator((Position position, string text) arg)
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
        string fill = fillsDict.TryGetValue(position, out var v) ? v : "white";
        var eventHandlers = GetEventHandlers(position).ToImmutableList();


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
            .OfType<ICellSVGElementOverlay>()
            .SelectMany(x => x.GetSVGDefinitions(CellStyleHelpers.CellSize))
            .Concat(Definitions)
            .DistinctBy(x => x.Id)
            .ToList();

        var fillsDict = new Dictionary<Position, string>();
        HashSet<string> addedGradients = new();

        foreach (var group in allOverlays
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
            Grid.GetOuterIndicators().Select(GetOuterIndicator).ToList());

        var reasonDictionary = GetPositionReasonDictionary();

        children.Add(outerIndicatorGroup);

        var rects = Grid.AllPositions.Select(position => CreateRectangle(position, fillsDict, reasonDictionary))
            .ToList();

        children.Add(new SVGGroup("Cells", rects));


        var overlayGroups = new List<SVGElement>();
        foreach (var overlayGroup in allOverlays
                     .OfType<ICellSVGElementOverlay>()
                     .GroupBy(x => x.ZIndex).OrderBy(x => x.Key))
        {
            var group = new SVGGroup(
                Id: $"OverlayLevel{overlayGroup.Key}",
                overlayGroup.SelectMany(overlay => overlay.GetSVGElements(CellStyleHelpers.CellSize)).ToList()
            );
            overlayGroups.Add(group);
        }


        children.Add(new SVGGroup(Id: "Overlays", Children: overlayGroups));

        children.AddRange(GetExtraGroups());


        children.Add(new SVGGroup(Id: "CellViews", Children: GetTexts().ToList()));

        var svg = new SVGElements.SVG("GridSVG",
            children.ToImmutable(),
            $"{0} {0} {CellStyleHelpers.GetHeight(Grid)} {CellStyleHelpers.GetWidth(Grid)}",
            "xMinYMin meet"
        );

        return svg;
    }
}
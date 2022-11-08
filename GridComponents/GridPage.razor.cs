using System.Collections.Immutable;
using System.Text;
using CSharpFunctionalExtensions;
using GridPuzzles;
using GridPuzzles.Bifurcation;
using GridPuzzles.Cells;
using GridPuzzles.Clues;
using GridPuzzles.Session;
using GridPuzzles.SVG;
using GridPuzzles.VariantBuilderArguments;
using GridPuzzles.Yaml;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using SVGElements;
using SVGHelper;
using Position = GridPuzzles.Position;

namespace GridComponents;

public partial class GridPage<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        SelectedColumns = Columns;
        SelectedRows = Rows;

        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (MyGridSession is null)
        {
            var createResult = await GridCreator
                .TryCreate(Columns, Rows, GridText, CancellationToken.None);

            var r = await createResult.Match(Task.FromResult, _=> GridCreator.GetDefault());

            MyGridSession = await GridSession<T, TCell>.CreateAsync(r.Grid,
                r.VariantBuilders, r.VariantsInPlay, CancellationToken.None);

            if (createResult.IsFailure)
                await MyGridSession.ReportError(createResult.Error, TimeSpan.Zero);
        }

        MyGridSession.StateHasChangedAction = StateHasChanged;

        await base.OnParametersSetAsync();
        StateHasChanged();
    }

#pragma warning disable 8618
    public GridSession<T, TCell> MyGridSession { get; private set; }
#pragma warning restore 8618

    [EditorRequired]
    [Parameter] public int Columns { get; set; }

    [EditorRequired]
    [Parameter] public int Rows { get; set; }

    [Parameter] public string? GridText { get; set; }

    [EditorRequired]
    [Parameter] public GridCreator<T, TCell> GridCreator { get; set; }

    [Parameter]
    public IReadOnlyList<Example>? Examples { get; set; }

    [Parameter] public Func<Grid<T, TCell>, string>? GetExtraFunc { get; set; }

    private MudChip[] _selectedVariantBuilderChips = Array.Empty<MudChip>();

    private IReadOnlySet<IClueBuilder> SelectedClueBuilders =>
        _selectedVariantBuilderChips.Select(x => x.Value)
            .Cast<VariantBuilderArgumentPair<T, TCell>>()
            .SelectMany(x =>
                x.VariantBuilder.TryGetClueBuilders(x.Pairs).OnFailureCompensate(_ => new List<IClueBuilder>()).Value)
            .ToHashSet();
        


    public async Task ChangeSize()
    {
        if (Columns == SelectedColumns && Rows == SelectedRows)
            return; //no change
        Columns = SelectedColumns;
        Rows = SelectedRows;

        var r = await GridCreator.TryCreate(Columns, Rows, GridText, CancellationToken.None);

        if (r.IsSuccess)
        {
            MyGridSession = await GridSession<T, TCell>.CreateAsync(r.Value.Grid,
                r.Value.VariantBuilders, r.Value.VariantsInPlay, CancellationToken.None);

            MyGridSession.StateHasChangedAction = StateHasChanged;
            StateHasChanged();
        }
        else
        {
            await MyGridSession.ReportError(r.Error, TimeSpan.Zero);   
        }
    }

    private int _selectedColumns;

    public int SelectedColumns
    {
        get => _selectedColumns;
        set
        {
            _selectedColumns = value;
            if (GridCreator.WidthMustMatchHeight)
                _selectedRows = _selectedColumns;
        }
    }

    private int _selectedRows;

    public int SelectedRows
    {
        get => _selectedRows;
        set
        {
            _selectedRows = value;
            if (GridCreator.WidthMustMatchHeight)
                _selectedColumns = _selectedRows;
        }
    }


    public SVGBuilder GetSVGBuilder() =>
        new GridPageGridSVG(MyGridSession.ChosenState,
            SelectedClueBuilders,
            p => new[]
        {
            SVGEventHandler.OnKeyPressAsync(kea => KeyWasPressed(kea, p))
        }, MyGridSession.SessionSettings);

    private async Task KeyWasPressed(KeyboardEventArgs kea, Position position)
    {
        var result = MyGridSession.SolveState.Grid.ClueSource.ValueSource.TryParse(kea.Key.First())
            .Match(x => x, _ => Maybe<T>.None);

        await MyGridSession.SetCell(result, position);

        //this.sel

        //TODO set focus
    }

    public async Task RemoveVariantBuilder(VariantBuilderArgumentPair<T, TCell> variantBuilderArgumentPair)
    {
        await MyGridSession.RemoveVariantBuilder(variantBuilderArgumentPair);
    }

    private async Task Export()
    {
        var data = MyGridSession.SolveState.Grid.ToSerializable(
            GetExtra,
            MyGridSession.SolveState.VariantBuilders).Serialize();

        try
        {
            await _clipboardService.WriteTextAsync(data);
        }
        catch (Exception e)
        {
            await MyGridSession.ReportError(e.Message, TimeSpan.Zero);
        }
    }

    private string? GetExtra(Grid<T, TCell> grid) => GetExtraFunc?.Invoke(grid);


    private bool DownloadPopoverOpen { get; set; } = false;
    private string DownloadFileName { get; set; } = "Puzzle";
    private DownloadFormat DownloadFormat { get; set; } = DownloadFormat.SVG;

    private async Task Download()
    {
        var svgBuilder = new GridPageGridSVG(
            MyGridSession.SolveState,
            SelectedClueBuilders,
            position => ImmutableStack<ISVGEventHandler>.Empty,
            MyGridSession.SessionSettings);

        var svg = svgBuilder.ComposeSVG();

        var fileName = $"{DownloadFileName}.{DownloadFormat.ToString().ToLower()}";
        var data = SVGFormatHelper.GetImageData(svg, DownloadFormat);

        var result = await _blazorDownloadFileService
            .DownloadFile(fileName, data, "application/octet-stream");


        if (!result.Succeeded)
        {
            await MyGridSession.ReportError(result.ErrorMessage, TimeSpan.Zero);
        }
    }

    //private void SolveToFile() //TODO change this a lot!
    //{
    //    const string folder = @"Exported";

    //    for (var i = 0; i < 10; i++)
    //    {
    //        while (true)
    //        {
    //            var fileName = Path.Combine(Environment.CurrentDirectory, folder, Guid.NewGuid() + ".txt");
    //            var result = MyGridSession.SolveState.Grid.RandomSolve(new Random());

    //            if (result.HasNoValue)
    //                continue;

    //            var text = result.Value.ToSerializable(GetExtra, MyGridSession.SolveState.VariantBuilders)
    //                .Serialize();
    //            File.WriteAllText(fileName, text);

    //            break;
    //        }
    //    }
    //}


    private async Task Import()
    {
        try
        {
            var yaml = await _clipboardService.ReadTextAsync();
            var result = await ImportYamlAsync(yaml, MyGridSession.SolveState.Grid.ClueSource.ValueSource,
                GridCreator.VariantBuilderList.ToDictionary(x=>x.Name));

            if (result.IsFailure)
                await MyGridSession.ReportError(result.Error, TimeSpan.Zero);
        }
        catch (Exception e)
        {
            await MyGridSession.ReportError(e.Message, TimeSpan.Zero);
        }
    }

    private async Task ImportExample(Example example)
    {
        try
        {
            var yaml = example.Yaml;
            var result = await ImportYamlAsync(yaml, MyGridSession.SolveState.Grid.ClueSource.ValueSource,
                GridCreator.VariantBuilderList.ToDictionary(x=>x.Name));

            if (result.IsFailure)
                await MyGridSession.ReportError(result.Error, TimeSpan.Zero);
        }
        catch (Exception e)
        {
            await MyGridSession.ReportError(e.Message, TimeSpan.Zero);
        }
    }

    private async Task<Result> ImportYamlAsync(string yaml, IValueSource<T, TCell> valueSource,
        IReadOnlyDictionary<string, IVariantBuilder<T, TCell>> variantBuilders)
    {
        var r = await YamlHelper.DeserializeGrid(yaml)
            .Bind(x => x.Convert(valueSource, variantBuilders, CancellationToken.None));

        if (!r.IsSuccess) return r;

        var newGridSession = await GridSession<T, TCell>.CreateAsync(r.Value.grid, variantBuilders.Values,
            r.Value.variants,
            CancellationToken.None);

        newGridSession.SessionSettings = MyGridSession.SessionSettings;
        newGridSession.StateHasChangedAction = StateHasChanged;

        MyGridSession = newGridSession;
        StateHasChanged();
        return Result.Success();
    }

    private async Task AddVariantBuilder(string? variantBuilder)
    {
        if (string.IsNullOrWhiteSpace(variantBuilder)) return;

        var builder =
            MyGridSession.PotentialVariantBuilders.SingleOrDefault(x => x.Name.Equals(variantBuilder));

        if (builder == null) return;


        if (builder.Arguments.Any())
        {
            Dictionary<string, string> dict = new();
            var @continue = true;

            while (@continue)
            {
                var mp = new DialogParameters
                {
                    { nameof(VariantBuilderArgumentForm.VariantBuilder), builder },
                    { nameof(VariantBuilderArgumentForm.SolveState), MyGridSession.SolveState },
                    { nameof(VariantBuilderArgumentForm.Results), dict }
                };


                var modalResult = await _dialogService.Show<VariantBuilderArgumentForm>(builder.Name, mp,
                    new DialogOptions
                    {
                        FullWidth = true,
                        MaxWidth = MaxWidth.Large
                    }).Result;

                if (modalResult.Cancelled) return;
                dict = (Dictionary<string, string>)modalResult.Data;

                @continue = dict.TryGetValue(VariantBuilderArgumentForm.AgainString, out var againString) &&
                            bool.TryParse(againString, out var again) && again;

                dict.Remove(VariantBuilderArgumentForm.AgainString);

                await MyGridSession.AddVariantBuilder(
                    new VariantBuilderArgumentPair<T, TCell>(builder, dict.ToImmutableDictionary()));

                if (@continue)
                    foreach (var variantBuilderArgument in builder.Arguments.Where(x => x.ClearOnAdded))
                        dict.Remove(variantBuilderArgument.Name);
            }
        }
        else
        {
            await MyGridSession.AddVariantBuilder(
                new VariantBuilderArgumentPair<T, TCell>(builder, new Dictionary<string, string>()));
        }
    }
}
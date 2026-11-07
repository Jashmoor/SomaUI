using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Linq.Expressions;

namespace SomaUI;

public abstract class ChartComponent : ComponentBase {
    private ChartDisplay? _chart;
    [CascadingParameter]
    public ChartDisplay? Chart {
        get => _chart;
        set {
            _chart = value;
            Initialise();
        }
    }
    [CascadingParameter(Name = "_context")] internal ChartContext? Context { get; set; }

    [Parameter, EditorRequired] public IReadOnlyList<object> Data { get; set; } = [];
    private IReadOnlyList<object>? _previous;

    [Parameter] public string? XDataKey { get; set; }

    protected override void OnParametersSet() {
        if (Context is null || Chart is null)
            return;

        if (!ReferenceEquals(_previous, Data))
        {
            Context.__Version++;
            _previous = Data;
        }

        Context.Width = Chart.Width ?? 0;
        Context.Height = Chart.Height ?? 0;
        Context.Data = Data;
        Context.XDataKey = XDataKey;
        BuildScales();
    }

    protected virtual void BuildScales() {
        if (Context == null || Data.Count == 0) return;

        var xKey = Context.XDataKey ?? XDataKey ?? "name";
        var categories = DataAccessor.GetAllStrings(Data, xKey);
        Context.XBandScale = new ScaleBand(categories, Context.PlotLeft, Context.PlotRight, 0, 0);
        Context.XDataKey = xKey;

        double yMin = 0, yMax = 0;
        foreach (var key in Context.Config.Keys)
        {
            var values = DataAccessor.GetAllDoubles(Data, key);
            foreach (var v in values)
            {
                if (v > yMax) yMax = v;
                if (v < yMin) yMin = v;
            }
        }

        var (niceMin, niceMax) = ChartMath.NiceExtent(yMin, yMax);
        Context.YScale = new ScaleLinear(niceMin, niceMax, Context.PlotBottom, Context.PlotTop);
    }

    protected virtual void Initialise() { }

    protected static string F(double v) => v.ToString("G6", CultureInfo.InvariantCulture);
}

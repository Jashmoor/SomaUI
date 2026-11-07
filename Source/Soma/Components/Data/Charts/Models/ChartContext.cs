using Microsoft.AspNetCore.Components;

namespace SomaUI;

public class ChartConfig : Dictionary<string, ChartConfigEntry> {
    public ChartConfig() : base(StringComparer.OrdinalIgnoreCase) { }
}

public record ChartConfigEntry {
    public string? Label { get; init; }
    public string? Color { get; init; }
}

public record ChartMargin {
    public double Top { get; init; } = 50;
    public double Right { get; init; } = 0;
    public double Bottom { get; init; } = 10;
    public double Left { get; init; } = 30;
}

internal class ChartContext {
    public ChartConfig Config { get; set; } = new();
    public int __Version { get; set; }
    public ScaleLinear? YScale { get; set; } = new ScaleLinear(0, 300, 0, 300);
    public ScaleBand? YBandScale { get; set; }
    public ScaleBand? XBandScale { get; set; } = new ScaleBand(Enumerable.Range(0, 8).Select(x => "load"), 0, 500);
    public ScaleLinear? XLinearScale { get; set; }
    public ChartMargin Margin { get; set; } = new();

    public ScaleBand? CategoryScale =>
    BarLayout == BarLayout.Horizontal ? YBandScale : XBandScale;

    public ScaleLinear? ValueScale =>
        BarLayout == BarLayout.Horizontal ? XLinearScale : YScale;

    public double PlotLeft => Margin.Left;
    public double PlotTop => Margin.Top;
    public double PlotWidth => Width - Margin.Left - Margin.Right;
    public double PlotHeight => Height - Margin.Top - Margin.Bottom;
    public double PlotRight => Width - Margin.Right;
    public double PlotBottom => Height - Margin.Bottom;
    public int ActiveIndex { get; set; } = -1;
    public double TooltipLeftPct { get; set; }
    public double TooltipTopPct { get; set; }
    public bool IsPieStyle { get; set; }
    public string? ValueDataKey { get; set; }
    public string? NameDataKey { get; set; }
    public string? XDataKey { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public BarLayout BarLayout { get; set; } = BarLayout.Vertical;


    public bool TooltipEnabled { get; set; }
    public bool TooltipHideLabel { get; set; }
    public bool TooltipHideIndicator { get; set; }
    public TooltipIndicator TooltipIndicator { get; set; } = TooltipIndicator.Dot;
    public string? TooltipLabelKey { get; set; }
    public string? TooltipNameKey { get; set; }
    public RenderFragment? TooltipCustomContent { get; set; }
    public string? TooltipClass { get; set; }

    public IReadOnlyList<object> Data { get; set; } = [];
    public Action? OnStateChanged { get; set; }

    public void SetActiveIndex(int index, double svgX, double svgY) {
        ActiveIndex = index;
        if (Width > 0) TooltipLeftPct = (svgX / Width) * 100;
        if (Height > 0) TooltipTopPct = (svgY / Height) * 100;
        OnStateChanged?.Invoke();
    }

    public void ClearActiveIndex() {
        if (ActiveIndex != -1)
        {
            ActiveIndex = -1;
            OnStateChanged?.Invoke();
        }
    }
}

namespace SomaUI;

public static class ChartMath {
    public static double[] NiceTicks(double min, double max, int desiredCount = 5) {
        if (max <= min) return [min];
        var range = NiceNumber(max - min, false);
        var spacing = NiceNumber(range / (desiredCount - 1), true);
        var niceMin = Math.Floor(min / spacing) * spacing;
        var niceMax = Math.Ceiling(max / spacing) * spacing;
        var ticks = new List<double>();
        for (var v = niceMin; v <= niceMax + spacing * 0.5; v += spacing)
        {
            ticks.Add(Math.Round(v, 10));
        }
        return [.. ticks];
    }

    public static double NiceNumber(double value, bool round) {
        var exponent = Math.Floor(Math.Log10(value));
        var fraction = value / Math.Pow(10, exponent);
        double niceFraction;
        if (round)
        {
            niceFraction = fraction < 1.5 ? 1 :
                           fraction < 3 ? 2 :
                           fraction < 7 ? 5 : 10;
        }
        else
        {
            niceFraction = fraction <= 1 ? 1 :
                           fraction <= 2 ? 2 :
                           fraction <= 5 ? 5 : 10;
        }
        return niceFraction * Math.Pow(10, exponent);
    }

    public static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
    public static double ToDegrees(double radians) => radians * 180.0 / Math.PI;
    public static double Clamp(double value, double min, double max) =>
        Math.Max(min, Math.Min(max, value));
    public static double Lerp(double a, double b, double t) => a + (b - a) * t;
    public static (double Min, double Max) Extent(IEnumerable<double> values) {
        double min = double.MaxValue, max = double.MinValue;
        bool any = false;
        foreach (var v in values)
        {
            if (v < min) min = v;
            if (v > max) max = v;
            any = true;
        }
        return any ? (min, max) : (0, 0);
    }
    public static (double Min, double Max) PadExtent(double min, double max, double fraction = 0.05) {
        var range = max - min;
        if (range == 0) range = Math.Abs(min) == 0 ? 1 : Math.Abs(min);
        var pad = range * fraction;
        return (min - pad, max + pad);
    }
    public static (double Min, double Max) NiceExtent(double min, double max, int tickCount = 5) {
        if (min >= 0) min = 0;
        var ticks = NiceTicks(min, max, tickCount);
        return (ticks[0], ticks[^1]);
    }
}

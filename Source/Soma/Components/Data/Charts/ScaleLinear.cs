namespace SomaUI;

public class ScaleLinear {
    public double DomainMin { get; private set; }
    public double DomainMax { get; private set; }
    public double RangeMin { get; private set; }
    public double RangeMax { get; private set; }

    public ScaleLinear(double domainMin, double domainMax, double rangeMin, double rangeMax) {
        DomainMin = domainMin;
        DomainMax = domainMax;
        RangeMin = rangeMin;
        RangeMax = rangeMax;
    }

    public double Scale(double value) {
        var domainRange = DomainMax - DomainMin;
        if (domainRange == 0) return RangeMin;
        var t = (value - DomainMin) / domainRange;
        return RangeMin + t * (RangeMax - RangeMin);
    }

    public double Invert(double pixel) {
        var rangeSpan = RangeMax - RangeMin;
        if (rangeSpan == 0) return DomainMin;
        var t = (pixel - RangeMin) / rangeSpan;
        return DomainMin + t * (DomainMax - DomainMin);
    }

    public double[] Ticks(int count = 5) =>
        ChartMath.NiceTicks(DomainMin, DomainMax, count);

    public ScaleLinear Nice(int tickCount = 5) {
        var (nMin, nMax) = ChartMath.NiceExtent(DomainMin, DomainMax, tickCount);
        return new ScaleLinear(nMin, nMax, RangeMin, RangeMax);
    }
}

public class ScaleBand {
    private readonly string[] _domain;
    private readonly double _rangeMin;
    private readonly double _rangeMax;
    private readonly double _paddingInner;
    private readonly double _paddingOuter;
    private readonly double _bandwidth;
    private readonly double _step;

    public ScaleBand(IEnumerable<string> domain, double rangeMin, double rangeMax,
        double paddingInner = 0.1, double paddingOuter = 0.1) {
        _domain = domain.ToArray();
        _rangeMin = rangeMin;
        _rangeMax = rangeMax;
        _paddingInner = paddingInner;
        _paddingOuter = paddingOuter;

        var n = _domain.Length;
        var totalRange = rangeMax - rangeMin;

        if (n == 0)
        {
            _step = 0;
            _bandwidth = 0;
            return;
        }
        _step = totalRange / (n - _paddingInner + 2 * _paddingOuter);
        _bandwidth = _step * (1 - _paddingInner);
    }

    public double Bandwidth => _bandwidth;
    public double Step => _step;
    public double Scale(string category) {
        var index = Array.IndexOf(_domain, category);
        if (index < 0) return _rangeMin;
        return _rangeMin + _paddingOuter * _step + index * _step;
    }
    public double ScaleCenter(string category) => Scale(category) + _bandwidth / 2;
    public IReadOnlyList<string> Domain => _domain;
}

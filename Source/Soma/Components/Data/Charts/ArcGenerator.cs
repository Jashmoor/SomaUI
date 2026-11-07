
namespace SomaUI;

public static class ArcGenerator {
    public static string Arc(double cx, double cy, double outerRadius, double innerRadius,
        double startAngle, double endAngle, double cornerRadius = 0) {
        var path = new SvgPathBuilder();
        var angleDiff = Math.Abs(endAngle - startAngle);

        if (angleDiff >= 2 * Math.PI - 1e-6)
        {
            return FullCircleArc(cx, cy, outerRadius, innerRadius);
        }

        var outerStartX = cx + outerRadius * Math.Sin(startAngle);
        var outerStartY = cy - outerRadius * Math.Cos(startAngle);
        var outerEndX = cx + outerRadius * Math.Sin(endAngle);
        var outerEndY = cy - outerRadius * Math.Cos(endAngle);

        var largeArc = angleDiff > Math.PI;

        path.MoveTo(outerStartX, outerStartY);
        path.ArcTo(outerRadius, outerRadius, 0, largeArc, true, outerEndX, outerEndY);

        if (innerRadius > 0)
        {
            var innerEndX = cx + innerRadius * Math.Sin(endAngle);
            var innerEndY = cy - innerRadius * Math.Cos(endAngle);
            var innerStartX = cx + innerRadius * Math.Sin(startAngle);
            var innerStartY = cy - innerRadius * Math.Cos(startAngle);

            path.LineTo(innerEndX, innerEndY);
            path.ArcTo(innerRadius, innerRadius, 0, largeArc, false, innerStartX, innerStartY);
        }
        else
        {
            path.LineTo(cx, cy);
        }

        path.ClosePath();
        return path.Build();
    }

    private static string FullCircleArc(double cx, double cy, double outerRadius, double innerRadius) {
        var path = new SvgPathBuilder();

        path.MoveTo(cx, cy - outerRadius);
        path.ArcTo(outerRadius, outerRadius, 0, false, true, cx, cy + outerRadius);
        path.ArcTo(outerRadius, outerRadius, 0, false, true, cx, cy - outerRadius);

        if (innerRadius > 0)
        {
            path.MoveTo(cx, cy - innerRadius);
            path.ArcTo(innerRadius, innerRadius, 0, false, false, cx, cy + innerRadius);
            path.ArcTo(innerRadius, innerRadius, 0, false, false, cx, cy - innerRadius);
        }

        path.ClosePath();
        return path.Build();
    }

    public static (double X, double Y) Centroid(double cx, double cy, double outerRadius,
        double innerRadius, double startAngle, double endAngle) {
        var midAngle = (startAngle + endAngle) / 2;
        var midRadius = (outerRadius + innerRadius) / 2;
        return (cx + midRadius * Math.Sin(midAngle), cy - midRadius * Math.Cos(midAngle));
    }
}

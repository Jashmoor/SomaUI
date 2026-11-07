namespace SomaUI;

public class Point {
    public double X { get; set; }
    public double Y { get; set; }
    public string Render() {
        return $"{X} {Y}";
    }
    public Point ToCartesian(double radius, double angle) {
        X = X + radius * Math.Cos(angle);
        Y = Y + radius * Math.Sin(angle);
        return this;
    }
}

public class Point<T> : Point {
    public T? Data { get; set; }
}

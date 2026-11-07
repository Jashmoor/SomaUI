namespace SomaUI;

public readonly record struct CalendarEventRange(DateTime Start, DateTime End);

public enum CalendarView {
    Month,
    Week,
    Chronological
}

internal sealed record EventBar<TEvent>(
    TEvent Event,
    int StartColumn,
    int Span,
    int Lane,
    bool ContinuesBefore,
    bool ContinuesAfter);

internal sealed class WeekRowLayout<TEvent> {
    public List<EventBar<TEvent>> Bars { get; } = new();
    public int VisibleLaneCount { get; set; }
    public int[] BarsPerColumn { get; } = new int[7];
    public int[] HiddenPerColumn { get; } = new int[7];
}

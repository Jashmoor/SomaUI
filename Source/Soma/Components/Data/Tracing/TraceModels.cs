using Microsoft.AspNetCore.Components;

namespace SomaUI.Tracing;

public enum TraceStatus { None, Running, Error, Pending }

public abstract record TraceNode(string Id, string Title) {
    public string? Description { get; init; }
    public TraceStatus Status { get; init; }
    public RenderFragment? Icon { get; init; }
}

public sealed record TraceStepNode(string Id, string Title) : TraceNode(Id, Title) {
    public RenderFragment? Detail { get; init; }
    public string? Href { get; init; }
}

public sealed record TraceGroupNode(string Id, string Title, IReadOnlyList<TraceNode> Children)
    : TraceNode(Id, Title);

public enum ToolCallState { Running, Completed, Failed }
public sealed record ToolCallPart(string Id, string Name, ToolCallState State, string? Output = null);
public sealed record ReasoningPart(string Text, bool IsStreaming);

internal static class Cn {
    public static string Merge(params string?[] c) => string.Join(' ', c.Where(x => !string.IsNullOrWhiteSpace(x)));
}

using Microsoft.AspNetCore.Components;

namespace SomaUI;

public sealed class ToastDefinition {
    public ToastDefinition()
        : this(Identifier.NewId(), DateTimeOffset.UtcNow) { }

    internal ToastDefinition(string id, DateTimeOffset time) {
        Id = id;
        Created = time;
    }

    public string ToastId { get; set; } = string.Empty;
    public string Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public RenderFragment? Content { get; set; }
    public ToastType Type { get; set; } = ToastType.Default;
    public int Duration { get; set; } = 4000;
    public bool Dismissable { get; set; } = true;
    public Func<ValueTask>? Action { get; set; }
    public Func<ValueTask>? OnDismiss { get; set; }
    public bool Mounted { get; set; } = true;
    public bool Removed { get; set; }
    public bool Promise { get; set; }
    public DateTimeOffset Created { get; set; }
}

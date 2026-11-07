using Microsoft.AspNetCore.Components;

namespace SomaUI;

public enum SheetSide {
    Left,
    Top,
    Right,
    Bottom
}

public sealed class SheetOptions {
    public string? Title { get; set; }
    public string? Description { get; set; }
    public SheetSide Side { get; set; } = SheetSide.Right;
    public bool Dismissable { get; set; } = true;
    public string? Class { get; set; }
}

public class SheetParameters : Dictionary<string, object> {
    public SheetParameters() { }
    public SheetParameters(IDictionary<string, object> dictionary) : base(dictionary) { }

    public new void Add(string name, object value) => base.Add(name, value);
    public T? Get<T>(string name) => this.TryGetValue(name, out var value) ? (T)value : default;
}

public class SheetReference {
    public Guid Id { get; } = Guid.NewGuid();
    public TaskCompletionSource<object?> TaskCompletionSource { get; } = new();
    public Type? ComponentType { get; }
    public SheetParameters Parameters { get; }
    public SheetOptions Options { get; }
    public RenderFragment? Content { get; }
    public bool IsClosing { get; set; }
    internal object? PendingResult { get; set; }

    public SheetReference(Type componentType, SheetParameters parameters, SheetOptions options)
        => (ComponentType, Parameters, Options) = (componentType, parameters, options);

    public SheetReference(RenderFragment content, SheetOptions options)
        => (Content, Options, Parameters) = (content, options, new());

    public void Close(object? result) {
        TaskCompletionSource.TrySetResult(result);
    }

    public void Dismiss() {
        TaskCompletionSource.TrySetResult(null);
    }
}

public class SheetContext(
    SheetReference reference,
    SheetService service
    ) {
    public readonly SheetReference Reference = reference;

    public void Close(object? result = null) {
        service.Close(Reference.Id, result);
    }

    public void Dismiss() {
        service.Close(Reference.Id, null);
    }
}

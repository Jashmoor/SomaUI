using Microsoft.AspNetCore.Components;

namespace SomaUI;

public class DialogOptions {
    public string? Title { get; set; } = "Are you Sure?";
    public string? Description { get; set; } = "This will do something";
    public bool Dismissable { get; set; } = true;
    public string? Class { get; set; }
}

public class DialogParameters : Dictionary<string, object> {
    public DialogParameters() { }
    public DialogParameters(IDictionary<string, object> dictionary)
        : base(dictionary) { }

    public new void Add(string name, object value)
        => base.Add(name, value);
    public T? Get<T>(string name) => TryGetValue(name, out var value)
        ? (T)value
        : default;
}

public class DialogReference {
    public Guid Id { get; } = Guid.NewGuid();
    public TaskCompletionSource<object?> TaskCompletionSource { get; } = new();
    public Type? ComponentType { get; }
    public DialogParameters Parameters { get; }
    public DialogOptions Options { get; }
    public RenderFragment? Content { get; }
    public bool IsClosing { get; set; }
    internal object? PendingResult { get; set; }

    public DialogReference(Type componentType, DialogParameters parameters, DialogOptions options)
        => (ComponentType, Parameters, Options) = (componentType, parameters, options);

    public DialogReference(RenderFragment content, DialogOptions options)
        => (Content, Options, Parameters) = (content, options, new());

    public void Close(object? result) {
        TaskCompletionSource.TrySetResult(result);
    }

    public void Dismiss() {
        TaskCompletionSource.TrySetResult(null);
    }
}

public class DialogContext(
        DialogReference reference,
        DialogService service
    ) {
    public DialogReference Reference => reference;

    public void Close(object? result = null) {
        service.Close(reference.Id, result);
    }

    public void Dismiss() {
        service.Close(reference.Id, null);
    }
}
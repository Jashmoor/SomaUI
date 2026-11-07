using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SomaUI;

public class BaseComponent : ComponentBase, IDisposable {
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? Attributes { get; set; }

    [Parameter] public EventCallback<ElementReference> MouseEnter { get; set; }
    [Parameter] public EventCallback<ElementReference> MouseLeave { get; set; }

    public ElementReference Element { get; protected internal set; }

    private DotNetObjectReference<BaseComponent>? _reference;
    protected DotNetObjectReference<BaseComponent> Reference {
        get => (_reference ??= DotNetObjectReference.Create(this));
    }

    public string? UniqueID { get; set; }

    protected override void OnInitialized() {
        UniqueID ??= Guid.NewGuid().ToString("N")[..8];
    }

    internal bool disposed;

    public virtual void Dispose() {
        disposed = true;
        _reference?.Dispose();
        _reference = null;
    }
}

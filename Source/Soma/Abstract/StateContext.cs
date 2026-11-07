using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace SomaUI;

internal abstract class ComponentState<T> where T : class {
    public ComponentState(T state, string prefix = "element") {
        State = state ?? throw new ArgumentNullException(nameof(state));
        Id = $"{prefix}-{Identifier.NewId(10)}";
    }

    public T State { get; set; }
    public string Id { get; set; }
    public string CreateChildId(string marker)
        => $"{Id}-{marker}";
}

internal abstract class StateContext<T> : ComponentState<T> where T : class {
    protected StateContext(T state, string prefix = "")
        : base(state, prefix) {
    }

    public event Action? OnStateChange;

    protected void NotifyStateChange()
        => OnStateChange?.Invoke();

    protected void Modify(Action<T> action, bool notify = true) {
        action(State);
        if (notify)
            NotifyStateChange();
    }
}

internal class ControllableState<T> {
    private T? _base;
    private readonly T defaultValue;

    public ControllableState(T value) {
        _base = value;
        defaultValue = value;
    }

    public T? ControlledValue { get; set; }
    public bool Controlled { get; set; }
    public EventCallback<T> OnValueChanged { get; set; }
    public T Value {
        get {
            return (Controlled
                ? ControlledValue
                : _base) ?? defaultValue;
        }
    }

    public async Task SetValueAsync(T value) {
        if (!Controlled)
            _base = value;

        if (OnValueChanged.HasDelegate)
            await OnValueChanged.InvokeAsync(value);
    }

    public void Reset() {
        if (!Controlled)
            _base = defaultValue;
    }
}

public class TriggerContext {
    private bool consumed;

    public Action? Toggle {
        get => Consume(field);
        init => field = value;
    }

    public Action? Open {
        get => Consume(field);
        init => field = value;
    }

    public Action? Close {
        get => Consume(field);
        init => field = value;
    }

    public Action? OnMouseEnter {
        get => Consume(field);
        init => field = value;
    }

    public Action? OnMouseLeave {
        get => Consume(field);
        init => field = value;
    }

    public Action? OnFocus {
        get => Consume(field);
        init => field = value;
    }

    public Action? OnBlur {
        get => Consume(field);
        init => field = value;
    }

    public Func<KeyboardEventArgs, Task>? OnKeyDown {
        get => Consume(field);
        init => field = value;
    }

    public Action<ElementReference>? SetTrigger {
        get => Consume(field);
        init => field = value;
    }

    private T Consume<T>(T value) {
        consumed = true;
        return value;
    }
}
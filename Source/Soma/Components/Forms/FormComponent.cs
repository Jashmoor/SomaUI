using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Linq.Expressions;

namespace SomaUI;

public class FormComponent<T> : BaseComponent, IAsyncDisposable {
    [CascadingParameter] public EditContext? EditContext { get; set; } = default!;
    [CascadingParameter] internal FieldContext? FieldContext { get; set; }
    [CascadingParameter] public Expression<Func<T>>? For { get; set; }
    [Parameter] public string? Name { get; set; }
    [Parameter] public int TabIndex { get; set; } = 0;
    [Parameter] public string? Placholder { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public Expression<Func<T>>? ValueExpression { get; set; }
    [Parameter] public EventCallback<T> ValueChanged { get; set; }
    [Parameter] public EventCallback<T> Change { get; set; }

    internal virtual T? Value {
        get => _value;
        set {
            if (!EqualityComparer<T>.Default.Equals(value, _value))
                _value = value;
        }
    }
    protected T? _value;

    protected FieldIdentifier _Field = new();

    protected override void OnParametersSet() {
        if (EditContext is not null && (For ?? ValueExpression) != null)
            _Field = FieldIdentifier.Create((For ?? ValueExpression)!);
    }

    public override Task SetParametersAsync(ParameterView parameters) {
        var result = base.SetParametersAsync(parameters);

        if (EditContext != null && (For ?? ValueExpression) != null)
        {
            EditContext.OnValidationStateChanged -= ValidationStateChanged;
            EditContext.OnValidationStateChanged += ValidationStateChanged;
        }
        return result;
    }

    internal virtual void ValidationStateChanged(object? sender, ValidationStateChangedEventArgs e) {
        InvokeAsync(StateHasChanged);
    }

    public virtual async ValueTask FocusAsync() {
        await Element.FocusAsync();
    }

    public override void Dispose() {
        base.Dispose();

        if (EditContext != null)
            EditContext.OnValidationStateChanged -= ValidationStateChanged;

        GC.SuppressFinalize(this);
    }

    public virtual async ValueTask DisposeAsync() {
        if (EditContext is not null)
            EditContext.OnValidationStateChanged -= ValidationStateChanged;

        GC.SuppressFinalize(this);
    }
}

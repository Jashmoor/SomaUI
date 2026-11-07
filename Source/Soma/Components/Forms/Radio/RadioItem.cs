using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace SomaUI;

public abstract class RadioItem<T> : ComponentBase {
    [CascadingParameter] public EditContext EditContext { get; set; } = default!;
    [CascadingParameter] internal RadioGroupContext<T> Context { get; set; } = default!;
    [Parameter, EditorRequired] public T? Value { get; set; }
    [Parameter] public string? Label { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? Id { get; set; }

    protected override void OnInitialized() {
        base.OnInitialized();
        if (string.IsNullOrWhiteSpace(Id))
            Id = $"radio-option={Guid.NewGuid():N}";
        Context?.Items.Add(this);
    }

    public bool IsChecked {
        get {
            if (Context == null)
                return false;
            return EqualityComparer<T>.Default.Equals(Context.Value, Value);
        }
    }

    protected async Task OnClick(MouseEventArgs args) {
        if (!IsDisabled && Context?.Select != null && Value != null)
            await Context.Select(Value);
    }


    protected bool IsDisabled
    => Disabled || (Context?.Disabled ?? false);

    protected string State
        => IsChecked ? "checked" : "unchecked";

    public void Dispose() {
        Context?.Items.Remove(this);
    }
}

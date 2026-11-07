using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace SomaUI;


public enum IconPosition {
    Start,
    End
}

public partial class Button : BaseWithChildComponent {

    [CascadingParameter(Name = "TriggerContext")] public TriggerContext? TriggerContext { get; set; }

    [Parameter] public bool Disabled { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter] public RenderFragment? Icon { get; set; }
    [Parameter] public bool Loading { get; set; }
    [Parameter] public string? LoadingText { get; set; }
    [Parameter] public RenderFragment? LoadingTemplate { get; set; }
    [Parameter] public string? Href { get; set; }
    [Parameter] public string? Target { get; set; }
    [Parameter] public IconPosition IconPosition { get; set; } = IconPosition.Start;
    [Parameter] public ButtonVariant Variant { get; set; } = ButtonVariant.Default;
    [Parameter] public ButtonSize Size { get; set; } = ButtonSize.Default;

    private bool IsDisabled => Disabled || _Loading || Loading;
    private bool IsLink => !string.IsNullOrEmpty(Href);
    private bool _Loading = false;
    private string? Rel => Target == "_blank" ? "noopener noreferrer" : null;

    private TriggerContext? _previous;
    protected override void OnAfterRender(bool firstRender) {
        if (TriggerContext?.SetTrigger is not null && (firstRender || TriggerContext != _previous))
        {
            TriggerContext.SetTrigger?.Invoke(Element);
            _previous = TriggerContext;
        }
    }

    private async Task HandleClick(MouseEventArgs e) {
        if (IsDisabled)
            return;

        if (TriggerContext is not null)
        {
            if (TriggerContext.Toggle == null && TriggerContext.Close != null)
                TriggerContext.Close?.Invoke();
            else
                TriggerContext.Toggle?.Invoke();
        }

        if (OnClick.HasDelegate)
        {
            try
            {
                _Loading = true;
                await InvokeAsync(StateHasChanged);

                await OnClick.InvokeAsync(e);
            }
            finally
            {
                _Loading = false;
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    private async Task HandleKeyDown(KeyboardEventArgs e) {
        if (IsDisabled)
            return;
        if (TriggerContext?.OnKeyDown != null)
            await TriggerContext.OnKeyDown.Invoke(e);
    }

    private void HandleMouseEnter()
        => TriggerContext?.OnMouseEnter?.Invoke();

    private void HandleMouseLeave()
        => TriggerContext?.OnMouseLeave?.Invoke();

    private void HandleFocus()
        => TriggerContext?.OnFocus?.Invoke();

    private void HandleBlur()
        => TriggerContext?.OnBlur?.Invoke();
}

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace SomaUI;

public partial class ChipComponent : BaseComponent {
    [Parameter] public RenderFragment Prefix { get; set; } = default!;
    [Parameter] public RenderFragment Suffix { get; set; } = default!;

    [Parameter] public bool Round { get; set; } = true;
    [Parameter] public bool Fill { get; set; } = false;
    [Parameter] public string? Content { get; set; }
    [Parameter] public bool Disabled { get; set; } = false;

    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; } = default!;

    private async Task HandleOnClick(MouseEventArgs args) {
        if (Disabled)
            return;
        if (OnClick.HasDelegate)
            await OnClick.InvokeAsync(args);
    }
}

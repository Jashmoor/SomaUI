using Microsoft.AspNetCore.Components;

namespace SomaUI;

public partial class Item : BaseComponent {

    [Parameter] public string? Title { get; set; } = default!;
    [Parameter] public RenderFragment ChildContent { get; set; } = default!;
    [Parameter] public ItemVariant Variant { get; set; } = ItemVariant.Default;
    [Parameter] public ItemDensity Density { get; set; } = ItemDensity.Normal;
    [Parameter] public bool Disabled { get; set; } = false;
    [Parameter] public bool Loading { get; set; } = false;
    [Parameter] public string? Link { get; set; } = null;
}

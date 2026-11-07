using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SomaUI;

public sealed class Defer : ComponentBase {
    [Parameter] public RenderFragment ChildContent { get; set; } = default!;
    protected override void BuildRenderTree(RenderTreeBuilder builder)
        => builder.AddContent(0, ChildContent);
}

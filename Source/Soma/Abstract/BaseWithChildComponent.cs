using Microsoft.AspNetCore.Components;

namespace SomaUI;

public class BaseWithChildComponent : BaseComponent {
    [Parameter] public RenderFragment ChildContent { get; set; } = default!;
}

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SomaUI;

public sealed class AnimationDefer : ComponentBase {
    [CascadingParameter] public Animate Animation { get; set; } = default!;
    [Parameter] public RenderFragment ChildContent { get; set; } = default!;

    private string GetAnimationSpeed(AnimationSpeed? speed) {
        return speed switch {
            AnimationSpeed.Fast => "fast",
            AnimationSpeed.Slow => "slow",
            AnimationSpeed.XtraSlow => "x-slow",
            _ => ""
        };
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder) {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "animate");
        builder.AddAttribute(2, Animation?.Current?.AnimationName ?? "fade-in");
        builder.AddAttribute(3, GetAnimationSpeed(Animation?.Current?.Speed));
        builder.AddContent(4, ChildContent);
        builder.CloseElement();
    }
}

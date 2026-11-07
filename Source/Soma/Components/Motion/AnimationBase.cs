using Microsoft.AspNetCore.Components;

namespace SomaUI;


public sealed class FadeIn : AnimationBase {
    public override string AnimationName => "fade-in";
}

public sealed class FadeInUp : AnimationBase {
    public override string AnimationName => "fade-in-up";
}

public sealed class FadeInLeft : AnimationBase {
    public override string AnimationName => "fade-in-left";
}

public sealed class FadeInRight : AnimationBase {
    public override string AnimationName => "fade-in-right";
}

public enum AnimationSpeed {
    Fast,
    Normal,
    Slow,
    XtraSlow
}

public abstract partial class AnimationBase : ComponentBase {
    [CascadingParameter] public Animate Animate { get; set; } = default!;
    [Parameter] public AnimationSpeed Speed { get; set; } = AnimationSpeed.Normal;
    public abstract string AnimationName { get; }

    protected override void OnInitialized() {
        Animate?.RegisterAnimation(this);
    }
}

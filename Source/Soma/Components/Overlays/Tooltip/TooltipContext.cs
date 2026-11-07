using Microsoft.AspNetCore.Components;

namespace SomaUI;

internal class TooltipState {
    public bool IsOpen { get; set; }
    public ElementReference? TriggerElement { get; set; }
    public string Placement { get; set; } = "top";
}

internal class TooltipContext : StateContext<TooltipState> {
    public TooltipContext() : base(new TooltipState(), "tooltip") {
    }

    public bool IsOpen => State.IsOpen;
    public string Placement => State.Placement;
    public string TriggerId => CreateChildId("trigger");
    public string TooltipId => CreateChildId("content");
    public void Open(ElementReference? triggerElement = null) {
        Modify(state => {
            if (triggerElement.HasValue)
                state.TriggerElement = triggerElement.Value;
            state.IsOpen = true;
        });
    }

    public void Close() =>
        Modify(state => state.IsOpen = false);

    public void SetPlacement(string placement) =>
        Modify(state => state.Placement = placement);

    public void SetTriggerElement(ElementReference triggerElement) {
        if (State.TriggerElement?.Id != triggerElement.Id)
            Modify(state => state.TriggerElement = triggerElement);
    }
}

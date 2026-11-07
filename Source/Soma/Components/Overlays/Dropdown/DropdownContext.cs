using Microsoft.AspNetCore.Components;

namespace SomaUI;

internal class DropdownState {
    public bool Open { get; set; }
    public ElementReference? Trigger { get; set; }
}

internal class DropdownContext : FloatingContext {
    internal DropdownContext()
        : base("dropdown") { }
}

internal class FloatingContext : StateContext<DropdownState> {
    internal FloatingContext(string value) : base(new(), value) {
    }

    public string TriggerId => CreateChildId("trigger");
    public string ContentId => CreateChildId("content");
    public bool IsOpen => State.Open;

    public void Open(ElementReference? trigger = null)
        => Modify(state => {
            state.Open = true;
            state.Trigger = trigger;
        });

    public void Close()
        => Modify(state => {
            state.Open = false;
        });

    public void Toggle(ElementReference? trigger = null) {
        if (State.Open)
            Close();
        else
            Open(trigger);
    }

    public void SetTriggerElement(ElementReference triggerElement) {
        if (State.Trigger?.Id != triggerElement.Id)
            Modify(state => {
                state.Trigger = triggerElement;
            });
    }
}

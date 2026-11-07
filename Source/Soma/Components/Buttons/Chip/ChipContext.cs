namespace SomaUI;

internal class ChipContext {
    private readonly List<Chip> Chips = [];
    internal bool Disabled { get; set; }
    internal Func<object?, bool> IsSelected { get; set; } = static _ => false;
    internal Func<object?, Task> ToggleAsync { get; set; } = static _ => Task.CompletedTask;

    internal void Register(Chip chip) {
        if (!Chips.Contains(chip))
            Chips.Add(chip);
    }

    internal void UnRegister(Chip chip) {
        Chips.Remove(chip);
    }

    internal void NotifyStateChange() {
        foreach(var chip in Chips)
        {
            chip.NotifySetStateChanged();
        }
    }
}

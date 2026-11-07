using Microsoft.AspNetCore.Components;

namespace SomaUI;

internal record FloatItem(string id, RenderFragment content);

internal class FloatingContentService {

    private HashSet<FloatItem> Items { get; set; } = [];
    internal List<FloatItem> Content => Items.ToList();

    internal Action? StateChange;

    internal void AddContent(string id, RenderFragment content) {
        Items.Add(new FloatItem(id, content));
        StateChange?.Invoke();
    }

}

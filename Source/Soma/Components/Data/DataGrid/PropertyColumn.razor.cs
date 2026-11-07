namespace SomaUI;

using Microsoft.AspNetCore.Components;

public partial class PropertyColumn<T> : ComponentBase
    where T : class {
    [CascadingParameter] internal DataGridContext<T> Context { get; set; } = default!;

    [Parameter] public string? Title { get; set; }
    [Parameter] public bool Sortable { get; set; } = false;
    [Parameter] public Func<T, object> ValueExpression { get; set; } = default!;
    [Parameter] public bool Searchable { get; set; } = false;

    [Parameter] public RenderFragment HeaderTemplate { get; set; } = default!;
    [Parameter] public RenderFragment<T> CellTemplate { get; set; } = default!;

    private void EnsureCompatible() {
        if (Context is null)
            throw new ArgumentException($"Column {(Title ?? "(unnamed)")} Is a Type mismatch from {typeof(T).FullName}");
    }
}

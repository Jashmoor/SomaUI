using Microsoft.AspNetCore.Components;

namespace SomaUI;

public partial class CategoryColumn<T> : ComponentBase
    where T : class {
    [CascadingParameter] internal DataGridContext<T> Context { get; set; } = default!;
    [Parameter] public string? Title { get; set; }
    [Parameter] public Func<T, bool> ValueExpression { get; set; } = default!;
}


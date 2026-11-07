using Microsoft.AspNetCore.Components.Forms;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace SomaUI;

internal class InputValidation : IAsyncDisposable {
    private readonly BaseComponent Owner;
    private Func<EditContext?> EditContext;
    private FieldIdentifier FieldIdentifier;
    private Func<bool> ShowValidation;
    private Func<string> ComponentId;

    internal InputValidation(
        BaseComponent owner,
        Func<EditContext?> editContext,
        Func<bool> showValidation,
        Func<string> component_id
        ) {
        Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        ComponentId = component_id;
        EditContext = editContext;
        ShowValidation = showValidation;
    }

    public void OnParametersSet<T>(Expression<Func<T>>? valueExpression) {
        if (valueExpression is null)
            return;

        FieldIdentifier = FieldIdentifier.Create(valueExpression);
        var editContext = EditContext();
    }

    public async Task<bool> HandleValidationStateChange() {
        var editContext = EditContext();
        if (editContext is null)
            return false;

        bool changes = false;
        if (editContext.Properties.TryGetValue("firstInvalidInputId", out var storedValue))
        {
            var errors = editContext.GetValidationMessages(FieldIdentifier).Any();
            if (storedValue as string == ComponentId() && !errors)
            {
                editContext.Properties.Remove("firstInvalidInputId");
                changes = true;
            }
        }
        return changes;
    }

    public async Task NotifyFieldChanged() {
        var editContext = EditContext();
        if (editContext is null)
            return;
        editContext.NotifyFieldChanged(FieldIdentifier);
        if (ShowValidation())
        {

        }
    }

    public async ValueTask DisposeAsync()
        => await ValueTask.CompletedTask;
}

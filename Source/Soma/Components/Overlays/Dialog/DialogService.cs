using Microsoft.AspNetCore.Components;

namespace SomaUI;

public sealed class DialogService {
    public event Action? OnChange;
    private readonly List<DialogReference> _dialogs = [];
    internal IReadOnlyList<DialogReference> Dialogs => _dialogs.AsReadOnly();

    public async Task<TResult?> ShowAsync<TComponent, TResult>(
        Dictionary<string, object> parameters,
        DialogOptions? options = null
        ) where TComponent : ComponentBase {
        var reference = new DialogReference(
            typeof(TComponent),
            new DialogParameters(),
            options ?? new DialogOptions()
        );
        _dialogs.Add(reference);
        OnChange?.Invoke();
        var result = await reference.TaskCompletionSource.Task;
        if (result is { })
            return (TResult?)result;
        return default;
    }

    public Task<bool?> ShowOption(
        DialogOptions? options = null
        ) {
        return ShowAsync<EmptyDialog, bool?>(new(), options);
    }

    public Task<TResult?> ShowAsync<TComponent, TResult, TInput>(
        TInput model,
        DialogOptions? options = null
        ) where TComponent : ComponentBase {
        var parameters = new Dictionary<string, object> { { "Data", model! } };
        return ShowAsync<TComponent, TResult>(parameters, options);
    }

    public void Close(Guid id, object? result) {
        var reference = _dialogs.FirstOrDefault(x => x.Id == id);
        if (reference == null || reference.IsClosing)
            return;
        reference.IsClosing = true;
        reference.PendingResult = result;
        OnChange?.Invoke();
        _ = RemoveAfterAnimation(reference);
    }

    public void FinalizeClose(Guid id, object? result) {
        if (_dialogs.FirstOrDefault(x => x.Id == id) is DialogReference reference)
        {
            _dialogs.Remove(reference);
            reference.TaskCompletionSource.TrySetResult(result);
            OnChange?.Invoke();
        }
    }

    private async Task RemoveAfterAnimation(DialogReference reference) {
        await Task.Delay(200);
        _dialogs.Remove(reference);
        reference.TaskCompletionSource.TrySetResult(reference.PendingResult);
        OnChange?.Invoke();
    }

    public void CloseAll() {
        var all = _dialogs.ToList();
        foreach (var reference in all)
        {
            if (!reference.IsClosing)
            {
                reference.IsClosing = true;
                reference.PendingResult = null;
                _ = RemoveAfterAnimation(reference);
            }
        }
        OnChange?.Invoke();
    }

}

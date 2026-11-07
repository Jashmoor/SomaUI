namespace SomaUI;

public interface IToastService {
    event Action? StateChange;

    ValueTask Dismiss(string? id = null, CancellationToken cancellationToken = default);
    ValueTask<IReadOnlyList<ToastDefinition>> GetToasts(CancellationToken cancellationToken = default);
    ValueTask<string> Loading(string title, Action<ToastOptions>? configure = null, CancellationToken cancellationToken = default);
    ValueTask Pause(string? toasterId, CancellationToken cancellationToken = default);
    ValueTask<string> Promise(Func<ValueTask> taskFactory, ToastPromiseOptions options, CancellationToken cancellationToken = default);
    ValueTask<string> Promise(ValueTask task, ToastPromiseOptions options, CancellationToken cancellationToken);
    ValueTask Resume(string? toasterId, CancellationToken cancellationToken = default);
    ValueTask<string> Success(string title, Action<ToastOptions>? configure = null, CancellationToken cancellationToken = default);
    ValueTask<string> Toast(string title, ToastType type, Action<ToastOptions>? configure = null, CancellationToken cancellation = default);
    ValueTask<string> UserPreventable(UserPreventableOptions options, CancellationToken cancellationToken = default);
}
using Microsoft.AspNetCore.Components;

namespace SomaUI;

public sealed class ToastOptions {
    public string? Id { get; set; }
    public string? Description { get; set; }
    public RenderFragment? Content { get; set; }
    public int? Duration { get; set; }
    public bool Dismissable { get; set; }
    public Func<ValueTask>? Action { get; set; }
    public Func<ValueTask>? OnDismiss { get; set; }
    public Func<ValueTask>? OnAutoClose { get; set; }
}

public sealed class ToastPromiseOptions {
    public string Loading { get; set; } = "Loading...";
    public string Success { get; set; } = "Done";
    public string Error { get; set; } = "Something went wrong";
    public string? Description { get; set; }
    public string? SuccessDescription { get; set; }
    public string? ErrorDescription { get; set; }
    public Action<ToastOptions>? ConfigureLoading { get; set; }
    public Action<ToastOptions>? ConfigureSuccess { get; set; }
    public Action<ToastOptions>? ConfigureError { get; set; }
}

public sealed class UserPreventableOptions {
    public string Loading { get; set; } = "Pending ...";
    public string? Description { get; set; }
    public int Delay { get; set; } = 3000;
    public Func<CancellationToken, Task>? OnInitial { get; set; }
    public Func<CancellationToken, Task>? OnSuccess { get; set; }
    public Func<CancellationToken, Task>? OnFailure { get; set; }
    public Action<ToastOptions>? ConfigureInitial { get; set; }
    public Action<ToastOptions>? ConfigureSuccess { get; set; }
    public Action<ToastOptions>? ConfigureError { get; set; }
}

internal sealed class ToastTimerState {
    public CancellationTokenSource? CancellationTokenSource { get; set; }
    public DateTimeOffset Started { get; set; }
    public int Remaining { get; set; }
    public bool Paused { get; set; }
}

internal sealed class ToastService : IToastService {

    public event Action? StateChange;
    public int DefaultDuration = 4000;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private List<ToastDefinition> Toasts = [];
    private readonly HashSet<string> _paused = [];
    private readonly Dictionary<string, ToastTimerState> _timers = new(StringComparer.Ordinal);
    private int MOUNT_DELAY = 200;
    private bool _disposed = false;

    public async ValueTask<IReadOnlyList<ToastDefinition>> GetToasts(CancellationToken cancellationToken = default) {
        if (Toasts.Count == 0)
            return Array.Empty<ToastDefinition>();

        var toasts = Toasts.ToArray();

        Array.Sort(toasts, static (left, right) => left.Created.CompareTo(right.Created));
        return toasts;
    }

    public ValueTask<string> Toast(string title, ToastType type, Action<ToastOptions>? configure = null, CancellationToken cancellation = default)
        => CreateOrUpdate(title, null, type, configure, cancellation);

    public ValueTask<string> Success(string title, Action<ToastOptions>? configure = null, CancellationToken cancellationToken = default)
        => CreateOrUpdate(title, null, ToastType.Success, configure, cancellationToken);

    public ValueTask<string> Information(string title, Action<ToastOptions>? configure = null, CancellationToken cancellation = default)
        => CreateOrUpdate(title, null, ToastType.Default, configure, cancellation);

    public ValueTask<string> Loading(string title, Action<ToastOptions>? configure = null, CancellationToken cancellationToken = default)
        => CreateOrUpdate(title, null, ToastType.Loading, configure, cancellationToken);

    public ValueTask<string> Promise(Func<ValueTask> taskFactory, ToastPromiseOptions options, CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        return Promise(taskFactory(), options, cancellationToken);
    }

    public async ValueTask<string> Promise(ValueTask task, ToastPromiseOptions options, CancellationToken cancellationToken) {
        var id = await Loading(
            options.Loading,
            toastOptions => {
                toastOptions.Description = options.Description;
                toastOptions.Duration = int.MaxValue;
                options.ConfigureLoading?.Invoke(toastOptions);
            }, cancellationToken);

        var existing = FindToast(id);
        if (existing is not null)
            existing.Promise = true;

        _ = HandlePromise(task, id, options, cancellationToken);
        return id;
    }

    public ValueTask<string> UserPreventable(UserPreventableOptions options, CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        return UserPreventableInternal(options, cancellationToken);
    }

    private async ValueTask<string> UserPreventableInternal(UserPreventableOptions options, CancellationToken cancellationToken) {
        CancellationTokenSource source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var id = await CreateOrUpdate(
            options.Loading,
            null,
            ToastType.Preventable,
            toastoptions => {
                toastoptions.Description = options.Description;
                toastoptions.Duration = options.Delay;
                toastoptions.Dismissable = true;
                toastoptions.OnDismiss = async () => await source.CancelAsync();
                options.ConfigureInitial?.Invoke(toastoptions);
            },
            source.Token);

        _ = HandleUserPreventable(id, options, source.Token);
        return id;
    }

    private async Task HandleUserPreventable(string id, UserPreventableOptions options, CancellationToken cancellationToken) {
        try
        {
            if (_disposed || cancellationToken.IsCancellationRequested)
                return;

            if (options.OnInitial is not null)
                await options.OnInitial(cancellationToken);

            await Task.Delay(options.Delay, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            if (options.OnSuccess is not null)
                await options.OnSuccess(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            if (options.OnFailure is not null)
                await options.OnFailure(cancellationToken);
        }
        catch
        {
        }
    }

    private async ValueTask<string> CreateOrUpdate(
        string? title,
        RenderFragment? content,
        ToastType type,
        Action<ToastOptions>? configure,
        CancellationToken token
        ) {
        try
        {
            await _lock.WaitAsync(token);

            var options = new ToastOptions { Dismissable = type != ToastType.Loading };
            configure?.Invoke(options);

            var id = options.Id ?? Identifier.NewId();

            ToastDefinition? previous = FindToast(id);

            bool is_mounted = previous is not null && !previous.Mounted;
            var toast = new ToastDefinition(id, previous?.Created ?? DateTimeOffset.Now) {
                Title = title,
                Description = options.Description,
                Type = type,
                Content = content,
                Duration = options.Duration ?? DefaultDuration,
                Mounted = is_mounted && previous is not null && previous.Mounted,
                Dismissable = options.Dismissable,
                Promise = previous?.Promise == true || type == ToastType.Loading || type == ToastType.Preventable,
                Removed = false,
                Action = options.Action,
                OnDismiss = options.OnDismiss
            };

            var index = Toasts.FindIndex(item => item.Id == id);
            if (index >= 0)
            {
                Toasts[index] = toast;
            }
            else
            {
                Toasts.Add(toast);
            }

            await RestartTimer(toast, token);
            NotifyStateChange();

            if (!is_mounted)
                _ = Mount(id);

            return id;

        }
        finally
        {
            _lock.Release();
        }

    }

    private async Task Mount(string id) {
        try
        {
            await Task.Delay(MOUNT_DELAY);

            if (_disposed)
                return;

            var toast = FindToast(id);

            if (toast is null || toast.Removed || toast.Mounted)
                return;

            toast.Mounted = true;

            NotifyStateChange();
        }
        catch (TaskCanceledException)
        {
        }
    }

    private async ValueTask HandlePromise(ValueTask task, string id, ToastPromiseOptions options, CancellationToken cancellationToken) {
        try
        {
            await task;

            await CreateOrUpdate(options.Success, null, ToastType.Success, toastOptions => {
                toastOptions.Id = id;
                toastOptions.Description = options.SuccessDescription ?? options.Description;
                options.ConfigureSuccess?.Invoke(toastOptions);
            }, cancellationToken);
        }
        catch
        {
            await CreateOrUpdate(options.Error, null, ToastType.Error, toastOptions => {
                toastOptions.Id = id;
                toastOptions.Description = options.ErrorDescription ?? options.Description;
                options.ConfigureError?.Invoke(toastOptions);
            }, cancellationToken);
        }
    }

    private ToastDefinition? FindToast(string id) {
        for (var i = 0; i < Toasts.Count; i++)
        {
            var toast = Toasts[i];
            if (toast.Id == id)
                return toast;
        }
        return null;
    }

    private async ValueTask RestartTimer(ToastDefinition toast, CancellationToken cancellationToken) {
        await CancelTimer(toast.Id, cancellationToken);

        if (toast.Duration <= 0 || toast.Type == ToastType.Loading)
            return;

        CancellationTokenSource? tokenSource = null;

        var timerState = new ToastTimerState {
            Remaining = toast.Duration,
            Paused = _paused.Contains(toast.Id)
        };

        _timers[toast.Id] = timerState;

        if (timerState.Paused)
            return;

        tokenSource = new CancellationTokenSource();
        timerState.CancellationTokenSource = tokenSource;
        timerState.Started = DateTimeOffset.UtcNow;

        if (tokenSource is not null)
            _ = AutoDismiss(toast.Id, toast.Duration, tokenSource.Token);
    }

    private async ValueTask CancelTimer(string id, CancellationToken cancellationToken = default) {
        CancellationTokenSource? cts = null;

        if (_timers.Remove(id, out var existing))
            cts = existing.CancellationTokenSource;

        if (cts is null)
            return;

        await cts.CancelAsync();
        cts.Dispose();
    }

    private static int GetRemainingMs(ToastTimerState timerState, DateTimeOffset now) {
        var elapsedMs = (int)Math.Ceiling((now - timerState.Started).TotalMilliseconds);
        var remainingMs = timerState.Remaining - elapsedMs;
        return remainingMs > 0 ? remainingMs : 0;
    }

    public async ValueTask Pause(string? toasterId, CancellationToken cancellationToken = default) {
        if (_disposed)
            return;

        List<CancellationTokenSource>? timersToCancel = null;
        var now = DateTimeOffset.UtcNow;

        foreach (var toast in Toasts)
        {
            if (toast.Removed)
                continue;

            if (!_timers.TryGetValue(toast.Id, out var timerState))
                continue;

            if (timerState.Paused || timerState.CancellationTokenSource is null)
                continue;

            timerState.Remaining = GetRemainingMs(timerState, now);
            timerState.Paused = true;
            (timersToCancel ??= []).Add(timerState.CancellationTokenSource);
            timerState.CancellationTokenSource = null;
        }

        if (timersToCancel is null)
            return;

        foreach (var timer in timersToCancel)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await timer.CancelAsync();
            timer.Dispose();
        }
    }

    public async ValueTask Resume(string? toasterId, CancellationToken cancellationToken = default) {
        if (_disposed)
            return;

        List<(string Id, int RemainingMs, CancellationTokenSource TokenSource)>? timersToStart = null;
        List<string>? toastsToDismiss = null;

        foreach (var toast in Toasts)
        {
            if (toast.Removed || toast.Type == ToastType.Loading)
                continue;

            if (!_timers.TryGetValue(toast.Id, out var timerState) || !timerState.Paused)
                continue;

            if (timerState.Remaining <= 0)
            {
                _timers.Remove(toast.Id);
                (toastsToDismiss ??= []).Add(toast.Id);
                continue;
            }

            var tokenSource = new CancellationTokenSource();
            timerState.CancellationTokenSource = tokenSource;
            timerState.Started = DateTimeOffset.UtcNow;
            timerState.Paused = false;
            (timersToStart ??= []).Add((toast.Id, timerState.Remaining, tokenSource));
        }

        if (timersToStart is not null)
        {
            foreach ((var id, var remainingMs, var tokenSource) in timersToStart)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _ = AutoDismiss(id, remainingMs, tokenSource.Token);
            }
        }

        if (toastsToDismiss is not null)
        {
            foreach (var id in toastsToDismiss)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _ = DismissCore(id, invokeAutoClose: true, cancellationToken);
            }
        }
    }

    private async ValueTask AutoDismiss(string id, int duration, CancellationToken cancellationToken) {
        try
        {
            await Task.Delay(duration, cancellationToken);

            if (cancellationToken.IsCancellationRequested || _disposed)
                return;

            await DismissCore(id, invokeAutoClose: true);
        }
        catch (TaskCanceledException)
        {
        }
    }

    public async ValueTask Dismiss(string? id = null, CancellationToken cancellationToken = default) {
        if (_disposed)
            return;

        if (id is not null)
        {
            await DismissCore(id!, invokeAutoClose: false, cancellationToken);
            return;
        }

        string[] ids = new string[Toasts.Count];

        for (var i = 0; i < Toasts.Count; i++)
        {
            ids[i] = Toasts[i].Id;
        }

        var tasks = new Task[ids.Length];

        for (var i = 0; i < ids.Length; i++)
        {
            tasks[i] = DismissCore(ids[i], invokeAutoClose: false, cancellationToken).AsTask();
        }

        await Task.WhenAll(tasks);
    }

    private async ValueTask DismissCore(string id, bool invokeAutoClose, CancellationToken cancellationToken = default) {
        ToastDefinition? toast;

        if (_disposed)
            return;

        toast = FindToast(id);
        if (toast is null || toast.Removed)
            return;

        await CancelTimer(id, cancellationToken);
        toast.Mounted = false;
        NotifyStateChange();

        await Task.Delay(200, cancellationToken);

        toast.Removed = true;
        bool removed = Toasts.Remove(toast);

        if (removed)
            NotifyStateChange();

        try
        {
            if (invokeAutoClose)
            {
                //if (toast.OnAutoClose is not null)
                //    await toast.OnAutoClose();
            }
            else if (toast.OnDismiss is not null)
            {
                await toast.OnDismiss();
            }
        }
        catch
        {
        }
    }


    private void NotifyStateChange()
        => StateChange?.Invoke();


    public async ValueTask DisposeAsync() {
        string[] ids;

        _disposed = true;

        //ids = new string[_timers.Count];
        //var index = 0;

        //foreach (var id in _timers.Keys)
        //{
        //    ids[index++] = id;
        //}

        Toasts.Clear();

        //foreach (var id in ids)
        //{
        //    await CancelTimer(id);
        //}
    }

    public void Dispose() {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

}

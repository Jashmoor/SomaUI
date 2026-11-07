namespace SomaUI;

internal class TooltipInteractionService : IDisposable {
    private readonly int UserReset = 2000;
    private System.Timers.Timer? Reset;

    public bool Interactive { get; private set; }

    public event Action? OnInteraction;

    public void NotifyOpen() {
        Interactive = true;
        Reset?.Stop();
        Reset?.Dispose();
        Reset = new System.Timers.Timer(UserReset);
        Reset.Elapsed += async (s, e) => {
            Interactive = false;
            Reset?.Dispose();
            Reset = null;
            OnInteraction?.Invoke();
        };
        Reset.AutoReset = false;
        Reset.Start();

        OnInteraction?.Invoke();
    }

    public void Dispose()
        => Reset?.Dispose();
}

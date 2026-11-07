namespace SomaUI;

internal class PaginationContext {
    public PaginationState? State { get; set; }
    public int[] PageSizes { get; set; } = [10, 20, 50, 100];
    public Func<int, Task>? OnPageChanged { get; set; }
    public Func<int, Task>? OnPageSizeChanged { get; set; }
}


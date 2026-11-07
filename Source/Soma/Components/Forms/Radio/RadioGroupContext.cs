namespace SomaUI;

internal class RadioGroupContext<T> {
    public bool Disabled { get; set; }
    public T? Value { get; set; }
    public bool Required { get; set; }
    public string? Name { get; set; }
    public Func<T, Task>? Select { get; set; }
    public List<RadioItem<T>> Items { get; set; } = [];
}

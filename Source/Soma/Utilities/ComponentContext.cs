
namespace SomaUI;


internal static class Identifier {
    private const int RANGE_FROM = 0x10000000;
    private const int RANGE_TO = int.MaxValue;
    private static readonly Random _rnd = new();

    public static IdentifierContext SequentialContext() => new(NewId(),(n) => $"{n:000}");

    public static string NewId() {
        const int LENGTH = 8;

        if (IdentifierContext.Current == null)
            return $"a-{_rnd.Next(RANGE_FROM, RANGE_TO)}"[..LENGTH];

        return IdentifierContext.Current.GenerateId();
    }

    public static string NewId(int length) {
        if (IdentifierContext.Current == null)
        {
            if (length > 16)
                throw new ArgumentOutOfRangeException(nameof(length), "length must be less than 16");

            var rnd = _rnd.Next(RANGE_FROM, RANGE_TO);

            if (length <= 8)
                return $"a-{rnd:x}"[..length];

            return $"a-{rnd:x}{rnd:x}"[..length];
        }

        return IdentifierContext.Current.GenerateId();
    }
}


internal sealed class IdentifierContext : IDisposable {
    private static readonly ThreadLocal<Stack<IdentifierContext>> _threadScopeStack = new(() => new Stack<IdentifierContext>());

    public IdentifierContext(string prefix, Func<uint, string> newId) {
        _threadScopeStack.Value?.Push(this);
        NewId = newId;
        Prefix = prefix;
        CurrentIndex = 0;
    }

    public static IdentifierContext? Current {
        get {
            if (_threadScopeStack.Value == null || _threadScopeStack.Value.Count == 0)
                return null;

            return _threadScopeStack.Value?.Peek();
        }
    }

    private string Prefix { get; set; }
    internal uint CurrentIndex { get; set; }
    private Func<uint, string> NewId { get; init; }

    internal string GenerateId() {
        var id = NewId.Invoke(CurrentIndex);

        CurrentIndex++;

        if (CurrentIndex >= 99999999)
            CurrentIndex = 0;

        return $"{Prefix}{id}";
    }

    public void Dispose() {
        _ = _threadScopeStack.Value?.TryPop(out _);
    }
}
using Microsoft.JSInterop;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace SomaUI;

internal static class ScriptModules {
    private static readonly ConditionalWeakTable<IJSRuntime, ConcurrentDictionary<string, ScriptScope>> Cache = new();

    public static Task<IJSObjectReference> ImportAsync(IJSRuntime runtime, string path) {
        ArgumentNullException.ThrowIfNull(runtime);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var modules = Cache.GetValue(runtime, static _ => new ConcurrentDictionary<string, ScriptScope>());
        return modules.GetOrAdd(path, static _ => new ScriptScope()).GetAsync(runtime, path);
    }

    internal sealed class DisposableReference(
        IJSObjectReference reference
        ) : IJSObjectReference {
        public ValueTask DisposeAsync()
            => ValueTask.CompletedTask;
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            => reference.InvokeAsync<TValue>(identifier, args);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
            => reference.InvokeAsync<TValue>(identifier, cancellationToken, args);
    }

    private sealed class ScriptScope {
        private readonly object sync = new();
        private Task<IJSObjectReference>? _active;

        internal Task<IJSObjectReference>? Loaded {
            get { lock (sync) { return _active; } }
        }

        internal Task<IJSObjectReference> GetAsync(IJSRuntime runtime, string path) {
            lock (sync)
            {
                if (_active is null || (_active.IsCompleted && !_active.IsCompletedSuccessfully))
                    _active = ImportAsync(runtime, path);
                return _active;
            }
        }

        internal static async Task<IJSObjectReference> ImportAsync(IJSRuntime runtime, string path) {
            var import = await runtime.InvokeAsync<IJSObjectReference>("import", path);
            return new DisposableReference(import);
        }
    }
}

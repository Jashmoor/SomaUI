using System.Collections;
using System.Reflection;

namespace SomaUI;

public static class DataAccessor {
    public static double GetDouble(object item, string key) {
        var raw = GetValue(item, key);
        return raw switch {
            null => 0,
            double d => d,
            int i => i,
            float f => f,
            long l => l,
            decimal m => (double)m,
            _ => Convert.ToDouble(raw)
        };
    }

    public static string GetString(object item, string key) {
        var raw = GetValue(item, key);
        return raw?.ToString() ?? string.Empty;
    }

    public static object? GetValue(object item, string key) {
        if (item is IDictionary<string, object> dict)
            return dict.TryGetValue(key, out var val) ? val : null;

        if (item is IDictionary nonGenericDict)
            return nonGenericDict.Contains(key) ? nonGenericDict[key] : null;

        var type = item.GetType();
        var prop = type.GetProperty(key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        return prop?.GetValue(item);
    }
    public static double[] GetAllDoubles(IEnumerable<object> data, string key) =>
        data.Select(item => GetDouble(item, key)).ToArray();

    public static string[] GetAllStrings(IEnumerable<object> data, string key) =>
        data.Select(item => GetString(item, key)).ToArray();
}

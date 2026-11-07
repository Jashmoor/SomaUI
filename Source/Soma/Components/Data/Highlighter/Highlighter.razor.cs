using Microsoft.AspNetCore.Components;
using System.Text;
using System.Text.RegularExpressions;

namespace SomaUI;

public partial class Highlighter : BaseComponent {
    private Memory<string> _Fragments;
    private string _regex = string.Empty;


    [Parameter] public string HighlightText { get; set; } = string.Empty;
    [Parameter] public string Content { get; set; } = string.Empty;
    [Parameter] public bool CaseSensitive { get; set; } = false;
    [Parameter] public bool UntilNextBoundary { get; set; } = false;

    protected override void OnParametersSet() {
        base.OnParametersSet();
        string[] text = { HighlightText };
        _Fragments = StringSplitter.GetFragments(Content, text, out _regex, CaseSensitive, UntilNextBoundary);
    }
}

internal class StringSplitter {
    private const string Boundary = ".*?\\b";
    private static StringBuilder? _builder;

    internal static Memory<string> GetFragments(
        string text,
        IEnumerable<string> highlighted,
        out string regex,
        bool case_sensitive = false,
        bool until_boundary = false
        ) {
        if (string.IsNullOrEmpty(text))
        {
            regex = string.Empty;
            return Memory<string>.Empty;
        }
        var builder = Interlocked.Exchange(ref _builder, null) ?? new();
        builder.Append("((?:");

        bool matches = false;
        if (highlighted is not null)
        {
            foreach (var t in highlighted)
            {
                if (string.IsNullOrEmpty(t))
                    continue;

                if (matches)
                    builder.Append(")|(?:");

                matches = true;
                var value = Regex.Escape(t);
                builder.Append(value);
                if (until_boundary)
                    builder.Append(Boundary);

            }
        }
        if (matches)
        {
            builder.Append("))");
        }
        else
        {
            builder.Clear();
            _builder = builder;
            regex = string.Empty;
            return new string[] { text };
        }

        regex = builder.ToString();
        builder.Clear();
        _builder = builder;

        var splits = Regex.Split(text, regex, case_sensitive ? RegexOptions.None : RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

        var length = 0;
        for (var i = 0; i < splits.Length; i++)
        {
            var s = splits[i];
            if (!string.IsNullOrEmpty(s))
                splits[length++] = s;
        }
        Array.Clear(splits, length, splits.Length - length);
        return splits.AsMemory(0, length);
    }
}


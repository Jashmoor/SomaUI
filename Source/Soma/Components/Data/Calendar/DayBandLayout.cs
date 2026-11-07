using System.Globalization;

namespace SomaUI;

internal static class DayBandLayout {
    internal const int LaneHeightPx = 22;
    internal const int BarHeightPx = 20;
    internal const int BarInsetPx = 3;

    internal static int Claim(List<bool[]> occupied, int columns, int startColumn, int span) {
        ArgumentNullException.ThrowIfNull(occupied);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(columns);
        ArgumentOutOfRangeException.ThrowIfNegative(startColumn);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(span);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(startColumn + span, columns);

        for (var lane = 0; ; lane++)
        {
            if (lane == occupied.Count)
            {
                occupied.Add(new bool[columns]);
            }

            var taken = occupied[lane];
            var free = true;
            for (var column = startColumn; column < startColumn + span; column++)
            {
                if (taken[column])
                {
                    free = false;
                    break;
                }
            }

            if (!free)
            {
                continue;
            }

            for (var column = startColumn; column < startColumn + span; column++)
            {
                taken[column] = true;
            }

            return lane;
        }
    }

    internal static string BarStyle(
        int startColumn,
        int span,
        int columns,
        int lane,
        int topBasePx,
        bool continuesBefore,
        bool continuesAfter) {
        var startInset = continuesBefore ? 0 : BarInsetPx;
        var endInset = continuesAfter ? 0 : BarInsetPx;
        var top = topBasePx + (lane * LaneHeightPx);
        var gaps = columns - 1;

        var left = SignedPixels(startColumn + startInset);
        var width = SignedPixels(span - 1 - startInset - endInset);

        return string.Create(
            CultureInfo.InvariantCulture,
            $"inset-inline-start:calc((100% - {gaps}px) * {startColumn} / {columns} {left});width:calc((100% - {gaps}px) * {span} / {columns} {width});top:{top}px;height:{BarHeightPx}px;");
    }

    internal static string LaneSpacerStyle(int laneCount) =>
        string.Create(CultureInfo.InvariantCulture, $"height:{laneCount * LaneHeightPx}px;");

    internal static string? JoinRounding(bool continuesBefore, bool continuesAfter) =>
        (continuesBefore, continuesAfter) switch {
            (true, true) => "bb:rounded-none",
            (true, false) => "bb:rounded-s-none",
            (false, true) => "bb:rounded-e-none",
            _ => null,
        };

    private static string SignedPixels(int value) =>
        string.Create(CultureInfo.InvariantCulture, $"{(value < 0 ? '-' : '+')} {Math.Abs(value)}px");
}

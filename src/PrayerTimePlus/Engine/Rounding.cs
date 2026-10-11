using PrayerTimePlus.Numerics;

namespace PrayerTimePlus.Engine;

internal static class Rounding
{
    internal static int? MinuteOfDay(double hours)
    {
        if (!double.IsFinite(hours))
        {
            return null;
        }

        // Retain this literal and the two floors: exact half-minutes depend on their evaluation order.
        var bumped = DegreeMath.FixHour(hours + 0.0083333333);
        var hour = Math.Floor(bumped);
        var minute = Math.Floor((bumped - hour) * 60.0);
        return (int)(hour * 60.0 + minute);
    }
}

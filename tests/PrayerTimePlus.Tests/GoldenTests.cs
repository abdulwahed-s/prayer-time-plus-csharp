using System.Globalization;
using Xunit;

namespace PrayerTimePlus.Tests;

public sealed class GoldenTests
{
    [Theory]
    [InlineData(CalculationMethod.MuslimWorldLeague, "03:59,05:27,12:16,15:37,19:05,19:05,20:28")]
    [InlineData(CalculationMethod.Oman, "03:59,05:27,12:21,15:42,19:05,19:10,20:35")]
    public void SoharMatchesExactLocalAndUtcValues(CalculationMethod method, string expected)
    {
        var offset = TimeSpan.FromHours(4);
        var times = new PrayerTimes(new Coordinates(24.3486, 56.6953, 5.0),
            new DateComponents(2026, 6, 28), method.GetParameters(), offset, "OM", "sohar");
        DateTimeOffset?[] values = [times.Fajr, times.Sunrise, times.Dhuhr, times.Asr,
            times.Sunset, times.Maghrib, times.Isha];
        var clocks = expected.Split(',');
        for (var i = 0; i < values.Length; i++)
        {
            var actual = Assert.IsType<DateTimeOffset>(values[i]);
            var clock = TimeOnly.ParseExact(clocks[i], "HH:mm", CultureInfo.InvariantCulture);
            var local = new DateTime(2026, 6, 28, clock.Hour, clock.Minute, 0, DateTimeKind.Unspecified);
            Assert.Equal(offset, actual.Offset);
            Assert.Equal(local, actual.DateTime);
            Assert.Equal(local.AddHours(-4), actual.UtcDateTime);
        }
    }
}

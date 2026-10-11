using Xunit;

namespace PrayerTimePlus.Tests;

public sealed class RamadanTests
{
    [Theory]
    [InlineData("SA", "makkah", true, "21:07")]
    [InlineData("sa", "makkah", true, "21:07")]
    [InlineData("SA", "makkah", false, "20:37")]
    [InlineData("AE", "makkah", true, "20:37")]
    [InlineData(" SA ", "makkah", true, "20:37")]
    [InlineData("SA", "custom", true, "20:37")]
    [InlineData("SA", null, true, "20:37")]
    public void MeccaMatchesSiblingGoldensAndRamadanGating(string country, string? key, bool ramadan, string isha)
    {
        var parameters = CalculationMethod.Makkah.GetParameters() with { Method = key, IsRamadan = ramadan };
        var times = new PrayerTimes(new Coordinates(21.42667, 39.82611), new DateComponents(2026, 6, 28),
            parameters, TimeSpan.FromHours(3), country);
        Assert.Equal("12:24", TestInputs.Clock(times.Dhuhr));
        Assert.Equal("19:07", TestInputs.Clock(times.Maghrib));
        Assert.Equal(isha, TestInputs.Clock(times.Isha));
    }
}

using PrayerTimePlus.Engine;
using Xunit;

namespace PrayerTimePlus.Tests;

public sealed class HighLatitudeTests
{
    private static CalculationParameters Parameters => CalculationMethod.MuslimWorldLeague.GetParameters();

    private static PrayerTimes Oslo(HighLatitudeRule rule, PrayerAdjustments? adjustments = null) => new(
        new Coordinates(59.9139, 10.7522), new DateComponents(2026, 6, 21),
        Parameters with { HighLatitudeRule = rule, Adjustments = adjustments ?? new PrayerAdjustments() }, TimeSpan.FromHours(2));

    [Fact]
    public void NoneLeavesTwilightUndefinedWhileHorizonEventsResolve()
    {
        var times = Oslo(HighLatitudeRule.None);
        Assert.Null(times.Fajr);
        Assert.Null(times.Isha);
        Assert.NotNull(times.Sunrise);
        Assert.NotNull(times.Sunset);
    }

    [Theory]
    [InlineData(HighLatitudeRule.Automatic)]
    [InlineData(HighLatitudeRule.MiddleOfTheNight)]
    [InlineData(HighLatitudeRule.SeventhOfTheNight)]
    [InlineData(HighLatitudeRule.TwilightAngle)]
    public void ExplicitNightRulesDefineTwilight(HighLatitudeRule rule)
    {
        var times = Oslo(rule);
        Assert.NotNull(times.Fajr);
        Assert.NotNull(times.Isha);
        Assert.NotEqual(Oslo(HighLatitudeRule.MiddleOfTheNight).Fajr, Oslo(HighLatitudeRule.SeventhOfTheNight).Fajr);
        Assert.NotEqual(Oslo(HighLatitudeRule.TwilightAngle).Isha, Oslo(HighLatitudeRule.SeventhOfTheNight).Isha);
    }

    [Fact]
    public void AutomaticIsExactlyTheOneShotSeventhRetry()
    {
        Assert.Equal(TestInputs.Values(Oslo(HighLatitudeRule.SeventhOfTheNight)), TestInputs.Values(Oslo(HighLatitudeRule.Automatic)));
        Assert.Equal(TestInputs.Values(TestInputs.Sohar(Parameters with { HighLatitudeRule = HighLatitudeRule.None })),
            TestInputs.Values(TestInputs.Sohar(Parameters)));
        var london = new PrayerTimes(new Coordinates(51.5080, -0.1281), new DateComponents(2026, 7, 9),
            AutoMethod.ForCountry("GB").GetParameters(), TimeSpan.FromHours(1), "GB", "London");
        Assert.NotNull(london.Fajr);
        Assert.NotNull(london.Isha);
    }

    [Theory]
    [InlineData(HighLatitudeRule.None)]
    [InlineData(HighLatitudeRule.Automatic)]
    [InlineData(HighLatitudeRule.MiddleOfTheNight)]
    [InlineData(HighLatitudeRule.SeventhOfTheNight)]
    [InlineData(HighLatitudeRule.TwilightAngle)]
    public void PolarDayDoesNotInventHorizonOrNightTimes(HighLatitudeRule rule)
    {
        var times = new PrayerTimes(new Coordinates(78.2232, 15.6469), new DateComponents(2026, 6, 21),
            Parameters with { HighLatitudeRule = rule }, TimeSpan.FromHours(2));
        Assert.Null(times.Sunrise);
        Assert.Null(times.Sunset);
        Assert.Null(times.Maghrib);
        Assert.Null(times.Fajr);
        Assert.Null(times.Isha);
        Assert.NotNull(times.Dhuhr);
    }

    [Fact]
    public void NightCorrectionsReapplyOnlyTheirOwnOffsets()
    {
        var baseline = Oslo(HighLatitudeRule.SeventhOfTheNight);
        var adjusted = Oslo(HighLatitudeRule.SeventhOfTheNight, new PrayerAdjustments { Fajr = 3, Isha = 4 });
        Assert.Equal(baseline.Fajr!.Value.AddMinutes(3), adjusted.Fajr);
        Assert.Equal(baseline.Isha!.Value.AddMinutes(4), adjusted.Isha);
    }

    [Fact]
    public void IntervalIshaUsesEighteenDegreesForTwilightFraction()
    {
        var parameters = Parameters with { IshaIsInterval = true, IshaValue = 300.0 };
        var calculator = new PrayerTimeCalculator(new Coordinates(59.9139, 10.7522), new DateComponents(2026, 6, 21),
            parameters, "", 2.0);
        var natural = calculator.Compute(HighLatitudeRule.None);
        var adjusted = calculator.Compute(HighLatitudeRule.TwilightAngle);
        var night = natural.Sunrise - natural.Sunset + 24.0;
        Assert.Equal(natural.Maghrib + 18.0 / 60.0 * night, adjusted.Isha, 10);
    }
}

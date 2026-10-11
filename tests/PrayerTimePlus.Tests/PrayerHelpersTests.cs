using Xunit;

namespace PrayerTimePlus.Tests;

public sealed class PrayerHelpersTests
{
    [Theory]
    [InlineData(Prayer.Fajr, Prayer.Sunrise)]
    [InlineData(Prayer.Sunrise, Prayer.Dhuhr)]
    [InlineData(Prayer.Dhuhr, Prayer.Asr)]
    [InlineData(Prayer.Asr, Prayer.Maghrib)]
    [InlineData(Prayer.Maghrib, Prayer.Isha)]
    [InlineData(Prayer.Isha, Prayer.None)]
    public void ExactBoundariesCompareInstantsAcrossOffsets(Prayer current, Prayer next)
    {
        var times = TestInputs.Sohar();
        var at = times.TimeForPrayer(current)!.Value;
        Assert.Equal(current, times.CurrentPrayer(at.ToOffset(TimeSpan.FromHours(-7))));
        Assert.Equal(next, times.NextPrayer(at.ToUniversalTime()));
        Assert.Equal(current, times.NextPrayer(at.AddTicks(-1)));
        Assert.Equal(current, times.CurrentPrayer(at.AddTicks(1)));
    }

    [Fact]
    public void BeforeFajrAndAfterIshaUseNoneAsDocumented()
    {
        var times = TestInputs.Sohar();
        Assert.Equal(Prayer.None, times.CurrentPrayer(times.Fajr!.Value.AddTicks(-1)));
        Assert.Equal(Prayer.Fajr, times.NextPrayer(times.Fajr.Value.AddTicks(-1)));
        Assert.Equal(Prayer.Isha, times.CurrentPrayer(times.Isha!.Value.AddDays(1)));
        Assert.Equal(Prayer.None, times.NextPrayer(times.Isha.Value.AddDays(1)));
        Assert.Null(times.TimeForPrayer(Prayer.None));
        Assert.Null(times.TimeForPrayer((Prayer)999));
    }

    [Fact]
    public void HelpersSkipUndefinedSolarEvents()
    {
        var times = new PrayerTimes(new Coordinates(59.9139, 10.7522), new DateComponents(2026, 6, 21),
            new CalculationParameters { HighLatitudeRule = HighLatitudeRule.None }, TimeSpan.FromHours(2));
        Assert.Null(times.TimeForPrayer(Prayer.Fajr));
        Assert.Null(times.TimeForPrayer(Prayer.Isha));
        Assert.Equal(Prayer.Sunrise, times.NextPrayer(times.Sunrise!.Value.AddHours(-1)));
        Assert.Equal(Prayer.Maghrib, times.CurrentPrayer(times.Maghrib!.Value.AddHours(1)));
        Assert.Equal(Prayer.None, times.NextPrayer(times.Maghrib.Value.AddHours(1)));
    }

    [Fact]
    public void HanafiChangesOnlyAsrWithExplicitFactors()
    {
        var shafi = TestInputs.Sohar();
        var hanafi = TestInputs.Sohar(shafi.CalculationParameters with { Madhab = Madhab.Hanafi });
        Assert.Equal(1, Madhab.Shafi.GetShadowFactor());
        Assert.Equal(2, Madhab.Hanafi.GetShadowFactor());
        Assert.True(hanafi.Asr > shafi.Asr);
        Assert.Equal(shafi.Fajr, hanafi.Fajr);
        Assert.Equal(shafi.Isha, hanafi.Isha);
        Assert.Throws<ArgumentOutOfRangeException>(() => ((Madhab)99).GetShadowFactor());
    }

    [Theory]
    [InlineData(2026, 6, 28)]
    [InlineData(2026, 12, 31)]
    [InlineData(2024, 2, 29)]
    public void SunnahSpansTomorrowAcrossCalendarBoundaries(int year, int month, int day)
    {
        var today = TestInputs.Sohar(date: new DateComponents(year, month, day));
        var tomorrowDate = today.DateComponents.ToDateOnly().AddDays(1);
        var tomorrow = TestInputs.Sohar(date: DateComponents.From(tomorrowDate));
        var sunnah = new SunnahTimes(today);
        var nightMinutes = (tomorrow.Fajr - today.Maghrib)!.Value.TotalMinutes;
        // Whole-minute boundaries yield midpoint and two-thirds rounded half-up.
        Assert.Equal(today.Maghrib!.Value.AddMinutes(Math.Floor(nightMinutes / 2 + 0.5)), sunnah.MiddleOfTheNight);
        Assert.Equal(today.Maghrib.Value.AddMinutes(Math.Floor(nightMinutes * 2 / 3 + 0.5)), sunnah.LastThirdOfTheNight);
        Assert.True(sunnah.MiddleOfTheNight > today.Maghrib);
        Assert.True(sunnah.LastThirdOfTheNight > sunnah.MiddleOfTheNight);
        Assert.True(sunnah.LastThirdOfTheNight < tomorrow.Fajr);
        Assert.Equal(today.UtcOffset, sunnah.LastThirdOfTheNight!.Value.Offset);
        Assert.Equal(tomorrowDate, DateOnly.FromDateTime(sunnah.LastThirdOfTheNight.Value.DateTime));
    }

    [Fact]
    public void SunnahRecomputesFajrRatherThanAddingTwentyFourHours()
    {
        var today = new PrayerTimes(new Coordinates(45, 0), new DateComponents(2026, 5, 1),
            new CalculationParameters(), TimeSpan.Zero);
        var tomorrow = new PrayerTimes(today.Coordinates, new DateComponents(2026, 5, 2), today.CalculationParameters, TimeSpan.Zero);
        Assert.NotEqual(today.Fajr!.Value.TimeOfDay, tomorrow.Fajr!.Value.TimeOfDay);
        var expected = today.Maghrib!.Value.AddSeconds((long)(tomorrow.Fajr.Value - today.Maghrib.Value).TotalSeconds / 2);
        expected = expected.AddSeconds(30);
        expected = expected.AddTicks(-(expected.Ticks % TimeSpan.TicksPerMinute));
        Assert.Equal(expected, new SunnahTimes(today).MiddleOfTheNight);
    }

    [Fact]
    public void SunnahNullBoundariesAndMaximumDateAreExplicit()
    {
        var times = new PrayerTimes(new Coordinates(78, 15), new DateComponents(2026, 6, 21),
            new CalculationParameters(), TimeSpan.FromHours(2));
        var sunnah = new SunnahTimes(times);
        Assert.Null(sunnah.MiddleOfTheNight);
        Assert.Null(sunnah.LastThirdOfTheNight);
        Assert.Throws<ArgumentNullException>(() => new SunnahTimes(null!));
        var maximum = TestInputs.Sohar(offset: TimeSpan.Zero, date: new DateComponents(9999, 12, 31));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SunnahTimes(maximum));
    }
}

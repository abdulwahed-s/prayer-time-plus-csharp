using Xunit;

namespace PrayerTimePlus.Tests;

public sealed class CustomMaghribTests
{
    [Fact]
    public void BareParametersTreatAnOmittedMaghribFlagAsAnAngle()
    {
        var times = TestInputs.Sohar(new CalculationParameters
        {
            FajrAngle = 18.0,
            MaghribValue = 4.0,
            IshaIsInterval = true,
            IshaValue = 90.0,
            HighLatitudeRule = HighLatitudeRule.None,
        });
        Assert.Equal("19:05", TestInputs.Clock(times.Sunset));
        Assert.Equal("19:21", TestInputs.Clock(times.Maghrib));
        Assert.Equal("20:51", TestInputs.Clock(times.Isha));
    }

    [Fact]
    public void BareParametersCanExplicitlySelectAnInterval()
    {
        var times = TestInputs.Sohar(new CalculationParameters
        {
            MaghribIsInterval = true,
            MaghribValue = 4.0,
            IshaIsInterval = true,
            IshaValue = 90.0,
            HighLatitudeRule = HighLatitudeRule.None,
        });
        Assert.Equal("19:09", TestInputs.Clock(times.Maghrib));
        Assert.Equal("20:39", TestInputs.Clock(times.Isha));
    }

    [Fact]
    public void BareZeroValueUsesSunsetAndTheDefaultIshaAngle()
    {
        var parameters = new CalculationParameters { HighLatitudeRule = HighLatitudeRule.None };
        Assert.False(parameters.MaghribIsInterval);
        var times = TestInputs.Sohar(parameters);
        Assert.Equal(times.Sunset, times.Maghrib);
        Assert.Equal("20:28", TestInputs.Clock(times.Isha));
    }

    private static CalculationParameters AngleParameters => CalculationMethod.Custom.GetParameters() with
    {
        HighLatitudeRule = HighLatitudeRule.None,
        MaghribIsInterval = false,
        MaghribValue = 4.0,
    };

    [Fact]
    public void PositiveAnglePreservesChronology()
    {
        var times = TestInputs.Sohar(AngleParameters);
        Assert.True(times.Fajr < times.Sunset);
        Assert.True(times.Sunset < times.Maghrib);
        Assert.True(times.Maghrib < times.Isha);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-4.0)]
    public void NonPositiveAnglesWithZeroOffsetsUseSunset(double angle)
    {
        var times = TestInputs.Sohar(AngleParameters with { MaghribValue = angle });
        Assert.Equal(times.Sunset, times.Maghrib);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-4.0)]
    [InlineData(20.0)]
    [InlineData(double.NaN)]
    public void InvalidOrNonChronologicalAnglesFallBackWithOffsets(double angle)
    {
        var times = TestInputs.Sohar(AngleParameters with
        {
            MaghribValue = angle,
            MethodAdjustments = new PrayerAdjustments { Maghrib = 2 },
            Adjustments = new PrayerAdjustments { Maghrib = 3 },
        });
        Assert.Equal(times.Sunset!.Value.AddMinutes(5), times.Maghrib);
    }

    [Fact]
    public void IntervalMaghribAddsFiveMinutes()
    {
        var times = TestInputs.Sohar(AngleParameters with { MaghribIsInterval = true, MaghribValue = 5.0 });
        Assert.Equal(times.Sunset!.Value.AddMinutes(5), times.Maghrib);
    }

    [Fact]
    public void IntervalIshaStartsAtFinalAngleMaghribAndCombinesTuningOnce()
    {
        var parameters = AngleParameters with { IshaIsInterval = true, IshaValue = 90.0 };
        var baseline = TestInputs.Sohar(parameters);
        var tuned = TestInputs.Sohar(parameters with
        {
            MethodAdjustments = new PrayerAdjustments { Maghrib = 2, Isha = 3 },
            Adjustments = new PrayerAdjustments { Maghrib = 3, Isha = 4 },
        });
        Assert.True(baseline.Maghrib > baseline.Sunset);
        Assert.Equal(baseline.Maghrib!.Value.AddMinutes(90), baseline.Isha);
        Assert.Equal(baseline.Maghrib.Value.AddMinutes(5), tuned.Maghrib);
        Assert.Equal(baseline.Isha!.Value.AddMinutes(12), tuned.Isha);
    }

    [Fact]
    public void AngleIshaReceivesOnlyItsOwnTuning()
    {
        var baseline = TestInputs.Sohar(AngleParameters);
        var tuned = TestInputs.Sohar(AngleParameters with { Adjustments = new PrayerAdjustments { Maghrib = 5, Isha = 7 } });
        Assert.Equal(baseline.Maghrib!.Value.AddMinutes(5), tuned.Maghrib);
        Assert.Equal(baseline.Isha!.Value.AddMinutes(7), tuned.Isha);
    }

    [Fact]
    public void UnavailableLondonAngleFallsBackBeforeIntervalIsha()
    {
        var times = new PrayerTimes(new Coordinates(51.5080, -0.1281), new DateComponents(2026, 7, 9),
            AngleParameters with { MaghribValue = 18.0, IshaIsInterval = true, IshaValue = 90.0 }, TimeSpan.FromHours(1));
        Assert.NotNull(times.Sunset);
        Assert.Equal(times.Sunset, times.Maghrib);
        Assert.Equal(times.Maghrib!.Value.AddMinutes(90), times.Isha);
    }

    [Fact]
    public void UnavailableAngleIshaDoesNotInvalidateOtherwiseValidMaghrib()
    {
        var times = TestInputs.Sohar(AngleParameters with { IshaValue = double.NaN });
        Assert.True(times.Maghrib > times.Sunset);
        Assert.Null(times.Isha);
    }
}

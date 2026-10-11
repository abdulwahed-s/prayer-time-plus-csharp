using PrayerTimePlus.Engine;
using PrayerTimePlus.Numerics;
using Xunit;

namespace PrayerTimePlus.Tests;

public sealed class AstronomyTests
{
    [Theory]
    [InlineData(2026, 6, 28, 2461219.5)]
    [InlineData(2000, 1, 1, 2451544.5)]
    [InlineData(2000, 2, 29, 2451603.5)]
    public void JulianDayUsesCivilComponents(int year, int month, int day, double expected) =>
        Assert.Equal(expected, Astronomical.JulianDay(new DateComponents(year, month, day)));

    [Theory]
    [InlineData(56.6953, 23.2676, -0.05468, 0.001, 0.0001)]
    [InlineData(39.82611, 23.262, -0.054885, 0.005, 0.001)]
    public void SolarIntermediatesMatchTolerances(double longitude, double declination, double equation,
        double declinationTolerance, double equationTolerance)
    {
        var position = Astronomical.SunPosition(2461219.5 - longitude / 360.0 + 12.0 / 24.0);
        Assert.InRange(position.Declination, declination - declinationTolerance, declination + declinationTolerance);
        Assert.InRange(position.EquationOfTime, equation - equationTolerance, equation + equationTolerance);
    }

    [Theory]
    [InlineData(-1.0, 23.0, 359.0)]
    [InlineData(-360.0, 0.0, 0.0)]
    [InlineData(361.0, 1.0, 1.0)]
    public void NormalizationUsesFloor(double input, double hour, double angle)
    {
        Assert.Equal(hour, DegreeMath.FixHour(input));
        Assert.Equal(angle, DegreeMath.FixAngle(input));
    }

    [Fact]
    public void DegreeWrappersPreserveBranchesAndUndefinedAngles()
    {
        Assert.Equal(1.0, DegreeMath.Sin(90.0), 12);
        Assert.Equal(0.5, DegreeMath.Cos(60.0), 12);
        Assert.Equal(1.0, DegreeMath.Tan(45.0), 12);
        Assert.Equal(30.0, DegreeMath.Asin(0.5), 12);
        Assert.Equal(60.0, DegreeMath.Acos(0.5), 12);
        Assert.Equal(135.0, DegreeMath.Acot(-1.0), 12);
        Assert.Equal(90.0, DegreeMath.Acot(0.0), 12);
        Assert.Equal(135.0, DegreeMath.Atan2(1.0, -1.0), 12);
        Assert.True(double.IsNaN(DegreeMath.Acos(1.001)));
    }

    [Theory]
    [InlineData(12.0, 720)]
    [InlineData(12.499, 750)]
    [InlineData(23.999, 0)]
    [InlineData(-0.1, 1434)]
    [InlineData(25.0, 60)]
    [InlineData(12.008333333333333, 720)]
    [InlineData(12.0083334, 721)]
    [InlineData(12.0083332, 720)]
    [InlineData(double.NaN, null)]
    [InlineData(double.PositiveInfinity, null)]
    [InlineData(double.NegativeInfinity, null)]
    public void RoundingUsesTheLiteralBumpAndSeparateFloors(double hours, int? expected) =>
        Assert.Equal(expected, Rounding.MinuteOfDay(hours));

    [Theory]
    [InlineData(0.0, 20.0, true)]
    [InlineData(0.01, 0.0, true)]
    [InlineData(1.0, 12.5, true)]
    [InlineData(1.0, 20.0, false)]
    [InlineData(double.NaN, 20.0, true)]
    [InlineData(1.0, double.NaN, true)]
    public void AutomaticDegeneracyUsesRoundedFajrAndIshaHours(double fajr, double isha, bool expected) =>
        Assert.Equal(expected, new SolarTimes(fajr, 6, 12, 15, 18, 18, isha).LooksDegenerate());
}

using System.Globalization;
using Xunit;

namespace PrayerTimePlus.Tests;

public sealed class DateAndOffsetTests
{
    [Theory]
    [InlineData(330, "05:29", "21:58")]
    [InlineData(345, "05:44", "22:13")]
    [InlineData(-210, "20:29", "12:58")]
    [InlineData(840, "13:59", "06:28")]
    [InlineData(-840, "09:59", "02:28")]
    public void FractionalAndExtremeOffsetsLocalizeThenWrapToTheRequestedDate(int minutes, string fajr, string isha)
    {
        var offset = TimeSpan.FromMinutes(minutes);
        var times = TestInputs.Sohar(offset: offset);
        Assert.Equal(fajr, TestInputs.Clock(times.Fajr));
        Assert.Equal(isha, TestInputs.Clock(times.Isha));
        Assert.All(TestInputs.Values(times), value =>
        {
            var actual = Assert.IsType<DateTimeOffset>(value);
            Assert.Equal(offset, actual.Offset);
            Assert.Equal(new DateTime(2026, 6, 28), actual.Date);
            Assert.Equal(actual.DateTime - offset, actual.UtcDateTime);
            Assert.Equal(DateTimeKind.Unspecified, actual.DateTime.Kind);
        });
    }

    [Theory]
    [InlineData(841, 0)]
    [InlineData(-841, 0)]
    [InlineData(0, 1)]
    [InlineData(0, -1)]
    public void InvalidOffsetsThrow(int minutes, int seconds)
    {
        var offset = TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds);
        Assert.Throws<ArgumentOutOfRangeException>(() => TestInputs.Sohar(offset: offset));
        Assert.Throws<ArgumentOutOfRangeException>(() => PrayerTimes.Today(new Coordinates(), new CalculationParameters(), offset));
    }

    [Theory]
    [InlineData(2025, 2, 29)]
    [InlineData(2026, 13, 1)]
    [InlineData(2026, 4, 31)]
    [InlineData(0, 1, 1)]
    [InlineData(10000, 1, 1)]
    public void InvalidCivilDatesThrow(int year, int month, int day) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new DateComponents(year, month, day));

    [Theory]
    [InlineData(2024, 2, 29)]
    [InlineData(2026, 12, 31)]
    [InlineData(2027, 1, 1)]
    [InlineData(1, 1, 2)]
    [InlineData(9999, 12, 30)]
    public void ValidCivilDatesRemainOnTheirRequestedDay(int year, int month, int day)
    {
        var date = new DateComponents(year, month, day);
        Assert.Equal(date, DateComponents.From(new DateOnly(year, month, day)));
        Assert.All(TestInputs.Values(TestInputs.Sohar(date: date)), time =>
            Assert.Equal(new DateTime(year, month, day), time!.Value.Date));
    }

    [Fact]
    public void StructDefaultsAndNullReferencesAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TestInputs.Sohar(date: default(DateComponents)));
        Assert.Throws<ArgumentNullException>(() => new PrayerTimes(new Coordinates(), new DateComponents(2026, 1, 1), null!, TimeSpan.Zero));
        Assert.Throws<ArgumentNullException>(() => TestInputs.Sohar(new CalculationParameters { Adjustments = null! }));
        Assert.Throws<ArgumentNullException>(() => TestInputs.Sohar(new CalculationParameters { MethodAdjustments = null! }));
        Assert.Throws<ArgumentNullException>(() => TestInputs.Sohar(country: null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => TestInputs.Sohar(new CalculationParameters { Madhab = (Madhab)77 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => TestInputs.Sohar(new CalculationParameters { HighLatitudeRule = (HighLatitudeRule)77 }));
    }

    [Theory]
    [InlineData(91, 0, 0)]
    [InlineData(0, -181, 0)]
    [InlineData(0, 0, -1)]
    [InlineData(double.NaN, 0, 0)]
    [InlineData(0, double.PositiveInfinity, 0)]
    [InlineData(0, 0, double.NaN)]
    public void CoordinateValidationRemainsOptIn(double latitude, double longitude, double altitude)
    {
        _ = new Coordinates(latitude, longitude, altitude);
        Assert.Throws<ArgumentOutOfRangeException>(() => Coordinates.Validated(latitude, longitude, altitude));
    }

    [Fact]
    public void PermissiveNonFiniteCoordinatesProduceNullsBeforeIntegerCasts()
    {
        var times = new PrayerTimes(new Coordinates(double.NaN, double.NaN), new DateComponents(2026, 1, 1),
            new CalculationParameters(), TimeSpan.Zero);
        Assert.All(TestInputs.Values(times), time => Assert.Null(time));
        Assert.Equal(new Coordinates(90, -180, 0), Coordinates.Validated(90, -180));
    }

    [Fact]
    public void CultureDoesNotChangeTheCivilCalendarOrOutput()
    {
        var previous = CultureInfo.CurrentCulture;
        var previousUi = CultureInfo.CurrentUICulture;
        try
        {
            var expected = TestInputs.Values(TestInputs.Sohar());
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ar-SA");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("tr-TR");
            Assert.Equal(expected, TestInputs.Values(TestInputs.Sohar()));
            Assert.Equal(CalculationMethod.Indonesia, AutoMethod.ForCountry("id"));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
            CultureInfo.CurrentUICulture = previousUi;
        }
    }

    [Fact]
    public void TodayUsesTheDateAtTheSuppliedOffset()
    {
        var offset = TimeSpan.FromHours(-14);
        var before = DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(offset).DateTime);
        var today = PrayerTimes.Today(new Coordinates(), new CalculationParameters(), offset);
        var after = DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(offset).DateTime);
        Assert.Contains(today.DateComponents.ToDateOnly(), new[] { before, after });
    }

    [Fact]
    public void ConcurrentCallsReturnTheSameImmutableResults()
    {
        var parameters = CalculationMethod.Oman.GetParameters();
        var expected = TestInputs.Values(TestInputs.Sohar(parameters));
        Parallel.For(0, 100, _ => Assert.Equal(expected, TestInputs.Values(TestInputs.Sohar(parameters))));
        var changed = parameters with { Adjustments = new PrayerAdjustments { Fajr = 2 } };
        Assert.Equal(new PrayerAdjustments(), parameters.Adjustments);
        Assert.Equal(expected[0]!.Value.AddMinutes(2), TestInputs.Sohar(changed).Fajr);
    }
}

namespace PrayerTimePlus;

/// <summary>Minute-rounded night portions spanning today's final Maghrib to tomorrow's recomputed Fajr.</summary>
/// <example><code>
/// var times = new PrayerTimes(new Coordinates(24.3486, 56.6953),
///     new DateComponents(2026, 6, 28), CalculationMethod.Oman.GetParameters(), TimeSpan.FromHours(4));
/// var sunnah = new SunnahTimes(times);
/// Console.WriteLine(sunnah.LastThirdOfTheNight);
/// </code></example>
public sealed class SunnahTimes
{
    /// <summary>Recomputes tomorrow's Fajr with the same input snapshot and UTC offset.</summary>
    /// <param name="prayerTimes">Today's calculated times; must not be null.</param>
    /// <exception cref="ArgumentNullException">The input is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Tomorrow or a resulting instant exceeds .NET date bounds.</exception>
    public SunnahTimes(PrayerTimes prayerTimes)
    {
        ArgumentNullException.ThrowIfNull(prayerTimes);
        var tomorrow = DateComponents.From(prayerTimes.DateComponents.ToDateOnly().AddDays(1));
        var next = new PrayerTimes(prayerTimes.Coordinates, tomorrow, prayerTimes.CalculationParameters,
            prayerTimes.UtcOffset, prayerTimes.CountryCode, prayerTimes.CityName);
        if (prayerTimes.Maghrib is not { } maghrib || next.Fajr is not { } fajr)
        {
            return;
        }

        // Divide integer seconds in this order before rounding the resulting instant.
        var seconds = (fajr - maghrib).Ticks / TimeSpan.TicksPerSecond;
        MiddleOfTheNight = RoundToMinute(maghrib.AddSeconds(seconds / 2L));
        LastThirdOfTheNight = RoundToMinute(maghrib.AddSeconds(seconds * 2L / 3L));
    }

    /// <summary>Gets the midpoint of the night, or null if either boundary is undefined.</summary>
    public DateTimeOffset? MiddleOfTheNight { get; }
    /// <summary>Gets the start of the final third, or null if either boundary is undefined.</summary>
    public DateTimeOffset? LastThirdOfTheNight { get; }

    private static DateTimeOffset RoundToMinute(DateTimeOffset time)
    {
        var bumped = time.AddSeconds(30);
        return new DateTimeOffset(bumped.Ticks - bumped.Ticks % TimeSpan.TicksPerMinute, bumped.Offset);
    }
}

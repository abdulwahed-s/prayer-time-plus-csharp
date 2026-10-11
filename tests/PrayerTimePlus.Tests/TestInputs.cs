using System.Globalization;

namespace PrayerTimePlus.Tests;

internal static class TestInputs
{
    internal static PrayerTimes Sohar(CalculationParameters? parameters = null, TimeSpan? offset = null,
        DateComponents? date = null, string country = "OM") => new(
        new Coordinates(24.3486, 56.6953, 5.0), date ?? new DateComponents(2026, 6, 28),
        parameters ?? CalculationMethod.MuslimWorldLeague.GetParameters(), offset ?? TimeSpan.FromHours(4), country);

    internal static DateTimeOffset?[] Values(PrayerTimes times) =>
        [times.Fajr, times.Sunrise, times.Dhuhr, times.Asr, times.Sunset, times.Maghrib, times.Isha];

    internal static string? Clock(DateTimeOffset? time) => time?.ToString("HH:mm", CultureInfo.InvariantCulture);

    internal static int Minute(DateTimeOffset? time) => time!.Value.Hour * 60 + time.Value.Minute;
}

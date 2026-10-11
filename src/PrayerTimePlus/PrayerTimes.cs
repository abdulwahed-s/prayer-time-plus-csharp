using PrayerTimePlus.Engine;

namespace PrayerTimePlus;

/// <summary>Seven nullable, minute-rounded daily solar times carrying the supplied UTC offset.</summary>
/// <remarks>Undefined solar events are null. Ordinary times wrap to the requested civil date.
/// No clock, machine time zone or network is used during construction.</remarks>
/// <example><code>
/// var times = new PrayerTimes(new Coordinates(24.3486, 56.6953, 5.0),
///     new DateComponents(2026, 6, 28), CalculationMethod.Oman.GetParameters(),
///     TimeSpan.FromHours(4), countryCode: "OM");
/// Console.WriteLine(times.Fajr?.ToString("HH:mm"));
/// </code></example>
public sealed class PrayerTimes
{
    /// <summary>Calculates prayer times eagerly from immutable input values.</summary>
    /// <param name="coordinates">Observer location; coordinate validation is opt-in.</param>
    /// <param name="dateComponents">Valid Gregorian civil date, with year in [1, 9999].</param>
    /// <param name="calculationParameters">Angles and rules; must not be null or contain null adjustments.</param>
    /// <param name="utcOffset">UTC offset including DST, in whole minutes within [-14, +14] hours.</param>
    /// <param name="countryCode">Optional country code for elevation and Ramadan rules; defaults to empty and must not be null.</param>
    /// <param name="cityName">Optional caller label, unused by the engine; defaults to empty and must not be null.</param>
    /// <exception cref="ArgumentNullException">Parameters, adjustments or a location label are null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The date, offset or enum is invalid, or a resulting UTC instant falls outside years 1 to 9999.</exception>
    public PrayerTimes(Coordinates coordinates, DateComponents dateComponents,
        CalculationParameters calculationParameters, TimeSpan utcOffset, string countryCode = "", string cityName = "")
    {
        ArgumentNullException.ThrowIfNull(calculationParameters);
        ArgumentNullException.ThrowIfNull(calculationParameters.MethodAdjustments);
        ArgumentNullException.ThrowIfNull(calculationParameters.Adjustments);
        ArgumentNullException.ThrowIfNull(countryCode);
        ArgumentNullException.ThrowIfNull(cityName);
        var date = dateComponents.ToDateOnly();
        ValidateOffset(utcOffset);
        if (!Enum.IsDefined(calculationParameters.Madhab) || !Enum.IsDefined(calculationParameters.HighLatitudeRule))
        {
            throw new ArgumentOutOfRangeException(nameof(calculationParameters), "Unsupported Asr or high-latitude rule.");
        }

        Coordinates = coordinates;
        DateComponents = dateComponents;
        CalculationParameters = calculationParameters;
        UtcOffset = utcOffset;
        CountryCode = countryCode;
        CityName = cityName;
        var calculator = new PrayerTimeCalculator(coordinates, dateComponents, calculationParameters, countryCode, utcOffset.TotalHours);
        var rule = calculationParameters.HighLatitudeRule;
        var solar = calculator.Compute(rule == HighLatitudeRule.Automatic ? HighLatitudeRule.None : rule);
        if (rule == HighLatitudeRule.Automatic && solar.LooksDegenerate())
        {
            solar = calculator.Compute(HighLatitudeRule.SeventhOfTheNight);
        }

        Fajr = BuildTime(solar.Fajr, date, utcOffset);
        Sunrise = BuildTime(solar.Sunrise, date, utcOffset);
        Dhuhr = BuildTime(solar.Dhuhr, date, utcOffset);
        Asr = BuildTime(solar.Asr, date, utcOffset);
        Sunset = BuildTime(solar.Sunset, date, utcOffset);
        Maghrib = BuildTime(solar.Maghrib, date, utcOffset);
        Isha = BuildTime(solar.Isha, date, utcOffset);
    }

    /// <summary>Gets the immutable observer location.</summary>
    public Coordinates Coordinates { get; }
    /// <summary>Gets the requested civil date.</summary>
    public DateComponents DateComponents { get; }
    /// <summary>Gets the immutable parameter snapshot used for calculation.</summary>
    public CalculationParameters CalculationParameters { get; }
    /// <summary>Gets the supplied UTC offset, including DST.</summary>
    public TimeSpan UtcOffset { get; }
    /// <summary>Gets the country code used for elevation and Ramadan rules.</summary>
    public string CountryCode { get; }
    /// <summary>Gets the caller's city label, unused by the calculation.</summary>
    public string CityName { get; }
    /// <summary>Gets Fajr at the supplied offset, or null when undefined.</summary>
    public DateTimeOffset? Fajr { get; }
    /// <summary>Gets Sunrise at the supplied offset, or null when undefined.</summary>
    public DateTimeOffset? Sunrise { get; }
    /// <summary>Gets Dhuhr at the supplied offset, or null when undefined.</summary>
    public DateTimeOffset? Dhuhr { get; }
    /// <summary>Gets Asr at the supplied offset, or null when undefined.</summary>
    public DateTimeOffset? Asr { get; }
    /// <summary>Gets Sunset before Maghrib adjustments, or null when undefined.</summary>
    public DateTimeOffset? Sunset { get; }
    /// <summary>Gets the final Maghrib at the supplied offset, or null when undefined.</summary>
    public DateTimeOffset? Maghrib { get; }
    /// <summary>Gets Isha at the supplied offset, or null when undefined.</summary>
    public DateTimeOffset? Isha { get; }

    /// <summary>Returns a prayer boundary, including Sunrise.</summary>
    /// <param name="prayer">The prayer to look up.</param>
    /// <returns>The time, or null for None, an unsupported enum or an undefined event.</returns>
    public DateTimeOffset? TimeForPrayer(Prayer prayer) => prayer switch
    {
        Prayer.Fajr => Fajr,
        Prayer.Sunrise => Sunrise,
        Prayer.Dhuhr => Dhuhr,
        Prayer.Asr => Asr,
        Prayer.Maghrib => Maghrib,
        Prayer.Isha => Isha,
        _ => null,
    };

    /// <summary>Finds the latest defined prayer boundary at or before an instant, including Sunrise.</summary>
    /// <param name="at">Comparison instant; null defaults to the current UTC clock.</param>
    /// <returns>The current prayer, or None before the first defined boundary.</returns>
    public Prayer CurrentPrayer(DateTimeOffset? at = null)
    {
        var instant = at ?? DateTimeOffset.UtcNow;
        var result = Prayer.None;
        DateTimeOffset? latest = null;
        for (var prayer = Prayer.Fajr; prayer < Prayer.None; prayer++)
        {
            if (TimeForPrayer(prayer) is { } time && time <= instant && (latest is null || time >= latest))
            {
                result = prayer;
                latest = time;
            }
        }

        return result;
    }

    /// <summary>Finds the earliest defined boundary strictly after an instant, including Sunrise.</summary>
    /// <param name="at">Comparison instant; null defaults to the current UTC clock.</param>
    /// <returns>The next prayer, or None after the last defined boundary.</returns>
    public Prayer NextPrayer(DateTimeOffset? at = null)
    {
        var instant = at ?? DateTimeOffset.UtcNow;
        var result = Prayer.None;
        DateTimeOffset? earliest = null;
        for (var prayer = Prayer.Fajr; prayer < Prayer.None; prayer++)
        {
            if (TimeForPrayer(prayer) is { } time && time > instant && (earliest is null || time < earliest))
            {
                result = prayer;
                earliest = time;
            }
        }

        return result;
    }

    /// <summary>Calculates today's civil date at the supplied offset using the current UTC clock.</summary>
    /// <param name="coordinates">Observer location.</param>
    /// <param name="parameters">Non-null immutable calculation settings.</param>
    /// <param name="utcOffset">Whole-minute UTC offset including DST, within [-14, +14] hours.</param>
    /// <param name="countryCode">Optional non-null country code; defaults to empty.</param>
    /// <param name="cityName">Optional non-null label; defaults to empty.</param>
    /// <returns>Calculated times for today's date at the supplied offset.</returns>
    /// <exception cref="ArgumentNullException">A required reference or adjustment is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The offset or enum value is invalid.</exception>
    public static PrayerTimes Today(Coordinates coordinates, CalculationParameters parameters, TimeSpan utcOffset,
        string countryCode = "", string cityName = "")
    {
        ValidateOffset(utcOffset);
        var date = DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(utcOffset).DateTime);
        return new PrayerTimes(coordinates, DateComponents.From(date), parameters, utcOffset, countryCode, cityName);
    }

    private static DateTimeOffset? BuildTime(double hours, DateOnly date, TimeSpan offset)
    {
        if (Rounding.MinuteOfDay(hours) is not { } minute)
        {
            return null;
        }

        var local = date.ToDateTime(new TimeOnly(minute / 60, minute % 60), DateTimeKind.Unspecified);
        return new DateTimeOffset(local, offset);
    }

    private static void ValidateOffset(TimeSpan offset)
    {
        if (offset.Ticks % TimeSpan.TicksPerMinute != 0 || offset.TotalHours is < -14.0 or > 14.0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "UTC offset must be whole minutes within +/-14 hours.");
        }
    }
}

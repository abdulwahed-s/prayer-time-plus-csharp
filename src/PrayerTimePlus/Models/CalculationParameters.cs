namespace PrayerTimePlus;

/// <summary>Immutable prayer calculation settings with angle-based Maghrib by default.</summary>
/// <remarks>Bare settings default Maghrib to a zero-degree angle, meaning Sunset.
/// The <see cref="CalculationMethod.Custom"/> preset explicitly uses a zero-minute interval instead.</remarks>
/// <example><code>
/// var parameters = CalculationMethod.Oman.GetParameters() with
/// {
///     Madhab = Madhab.Hanafi,
///     Adjustments = new PrayerAdjustments { Fajr = 2 }
/// };
/// </code></example>
public sealed record CalculationParameters
{
    /// <summary>Gets the case-sensitive preset key, or null for fully custom settings; defaults to null.</summary>
    /// <remarks>The key controls elevation-method selection and the Makkah Ramadan rule.</remarks>
    public string? Method { get; init; }
    /// <summary>Gets the Fajr depression angle in degrees; defaults to 18.</summary>
    public double FajrAngle { get; init; } = 18.0;
    /// <summary>Gets whether MaghribValue is an interval in minutes; defaults to false (angle mode).</summary>
    public bool MaghribIsInterval { get; init; }
    /// <summary>Gets minutes after Sunset, or an evening depression angle in degrees; defaults to zero.</summary>
    /// <remarks>Non-positive, unavailable or non-chronological angles fall back to Sunset plus adjustments.</remarks>
    public double MaghribValue { get; init; }
    /// <summary>Gets whether IshaValue is minutes after final Maghrib; defaults to false.</summary>
    public bool IshaIsInterval { get; init; }
    /// <summary>Gets the Isha interval in minutes or depression angle in degrees; defaults to 17 degrees.</summary>
    public double IshaValue { get; init; } = 17.0;
    /// <summary>Gets preset minute offsets; defaults to fresh zero offsets and must not be null.</summary>
    public PrayerAdjustments MethodAdjustments { get; init; } = new();
    /// <summary>Gets caller minute offsets added to preset offsets; defaults to fresh zero offsets and must not be null.</summary>
    public PrayerAdjustments Adjustments { get; init; } = new();
    /// <summary>Gets the Asr school; defaults to Shafi.</summary>
    public Madhab Madhab { get; init; } = Madhab.Shafi;
    /// <summary>Gets the twilight correction; defaults to Automatic.</summary>
    public HighLatitudeRule HighLatitudeRule { get; init; } = HighLatitudeRule.Automatic;
    /// <summary>Gets the caller-supplied Ramadan flag; defaults to false.</summary>
    /// <remarks>Only the makkah key in country SA adds 30 minutes to Isha, before high-latitude correction.</remarks>
    public bool IsRamadan { get; init; }
}

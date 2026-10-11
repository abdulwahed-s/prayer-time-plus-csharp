namespace PrayerTimePlus;

/// <summary>Immutable signed minute offsets, all defaulting to zero.</summary>
public sealed record PrayerAdjustments
{
    /// <summary>Gets the Fajr offset in minutes.</summary>
    public int Fajr { get; init; }
    /// <summary>Gets the Sunrise offset in minutes.</summary>
    public int Sunrise { get; init; }
    /// <summary>Gets the Dhuhr offset in minutes.</summary>
    public int Dhuhr { get; init; }
    /// <summary>Gets the Asr offset in minutes.</summary>
    public int Asr { get; init; }
    /// <summary>Gets the Maghrib offset in minutes; Sunset is unaffected.</summary>
    public int Maghrib { get; init; }
    /// <summary>Gets the Isha offset in minutes.</summary>
    public int Isha { get; init; }
}

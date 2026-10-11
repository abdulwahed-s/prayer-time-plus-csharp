namespace PrayerTimePlus;

/// <summary>Controls Fajr and Isha corrections when twilight is unavailable or too distant.</summary>
public enum HighLatitudeRule
{
    /// <summary>Try None, then retry once with SeventhOfTheNight for degenerate rounded Fajr or Isha; the default.</summary>
    Automatic,
    /// <summary>Leave astronomical results unchanged, including undefined events.</summary>
    None,
    /// <summary>Limit twilight to half the sunset-to-sunrise night.</summary>
    MiddleOfTheNight,
    /// <summary>Limit twilight using the literal fraction 0.14286 of the night.</summary>
    SeventhOfTheNight,
    /// <summary>Limit twilight to the depression angle divided by 60 times the night.</summary>
    TwilightAngle,
}

namespace PrayerTimePlus;

/// <summary>Prayer boundaries used by current and next prayer helpers; includes Sunrise.</summary>
public enum Prayer
{
    /// <summary>The dawn prayer.</summary>
    Fajr,
    /// <summary>Sunrise, ending the Fajr window.</summary>
    Sunrise,
    /// <summary>The midday prayer.</summary>
    Dhuhr,
    /// <summary>The afternoon prayer.</summary>
    Asr,
    /// <summary>The evening prayer.</summary>
    Maghrib,
    /// <summary>The night prayer.</summary>
    Isha,
    /// <summary>No matching boundary on the calculated day.</summary>
    None,
}

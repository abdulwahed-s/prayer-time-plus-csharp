namespace PrayerTimePlus;

/// <summary>Maps Asr schools to explicit shadow factors.</summary>
public static class MadhabExtensions
{
    /// <summary>Returns the additional shadow length as a multiple of object height.</summary>
    /// <param name="madhab">The Asr school.</param>
    /// <returns>One for Shafi or two for Hanafi.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The enum value is unsupported.</exception>
    public static int GetShadowFactor(this Madhab madhab) => madhab switch
    {
        Madhab.Shafi => 1,
        Madhab.Hanafi => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(madhab)),
    };
}

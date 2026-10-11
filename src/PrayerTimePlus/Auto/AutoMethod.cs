using PrayerTimePlus.Data;

namespace PrayerTimePlus;

/// <summary>Resolves country defaults from compiled data, with Muslim World League as fallback.</summary>
public static class AutoMethod
{
    /// <summary>Resolves an ISO country code using invariant case-insensitive matching without trimming.</summary>
    /// <param name="countryCode">The country code; null, empty and unknown codes use the fallback.</param>
    /// <returns>The country's supported preset, or Muslim World League.</returns>
    public static CalculationMethod ForCountry(string? countryCode) =>
        CalculationMethods.FromKey(AutoMethodResolution.ForCountry(countryCode)) ?? CalculationMethod.MuslimWorldLeague;
}

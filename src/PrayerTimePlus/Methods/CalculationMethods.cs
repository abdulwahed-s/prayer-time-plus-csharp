using PrayerTimePlus.Data;

namespace PrayerTimePlus;

/// <summary>Resolves case-sensitive stable method identifiers.</summary>
public static class CalculationMethods
{
    /// <summary>Looks up a stable key with exact case and whitespace matching.</summary>
    /// <param name="key">The key, or null.</param>
    /// <returns>The preset, or null for an unknown, unsupported or null key.</returns>
    public static CalculationMethod? FromKey(string? key) => MethodKeys.FromKey(key);
}

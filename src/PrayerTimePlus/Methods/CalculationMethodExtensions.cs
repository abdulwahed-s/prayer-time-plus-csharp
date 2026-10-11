using PrayerTimePlus.Data;

namespace PrayerTimePlus;

/// <summary>Stable preset keys and fresh immutable parameter values.</summary>
public static class CalculationMethodExtensions
{
    /// <summary>Returns the stable external identifier for a preset.</summary>
    /// <param name="method">The supported preset.</param>
    /// <returns>Its case-sensitive stable key.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The enum value is unsupported.</exception>
    public static string GetKey(this CalculationMethod method) => MethodKeys.GetKey(method);

    /// <summary>Creates fresh parameters from the preset's eleven numeric columns.</summary>
    /// <param name="method">The supported preset.</param>
    /// <returns>A fresh record with Shafi, Automatic, zero user offsets and Ramadan disabled.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The enum value is unsupported.</exception>
    public static CalculationParameters GetParameters(this CalculationMethod method)
    {
        var key = method.GetKey();
        var values = MethodParameters.ForKey(key);
        return new CalculationParameters
        {
            Method = key,
            FajrAngle = values[0],
            MaghribIsInterval = values[1] == 1.0,
            MaghribValue = values[2],
            IshaIsInterval = values[3] == 1.0,
            IshaValue = values[4],
            MethodAdjustments = new PrayerAdjustments
            {
                Fajr = (int)values[5],
                Sunrise = (int)values[6],
                Dhuhr = (int)values[7],
                Asr = (int)values[8],
                Maghrib = (int)values[9],
                Isha = (int)values[10],
            },
        };
    }
}

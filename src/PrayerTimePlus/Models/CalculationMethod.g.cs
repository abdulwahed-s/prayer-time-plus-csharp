// Generated from tools/data. Do not edit by hand.
// Regenerate: dotnet run --project tools/PrayerTimePlus.DataGenerator -c Release

namespace PrayerTimePlus;

/// <summary>Supported calculation presets with stable external keys and fresh parameter values.</summary>
public enum CalculationMethod
{
    /// <summary>The aachen preset: Fajr 18.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 17.0 degrees.</summary>
    Aachen,
    /// <summary>The algeria preset: Fajr 18.0 degrees, Maghrib 3.0 minutes after Sunset, Isha 17.0 degrees.</summary>
    Algeria,
    /// <summary>The austria preset: Fajr 18.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 17.0 degrees.</summary>
    Austria,
    /// <summary>The azrou preset: Fajr 19.1 degrees, Maghrib 0.0 degrees, Isha 17.0 degrees.</summary>
    Azrou,
    /// <summary>The belgium preset: Fajr 18.0 degrees, Maghrib 0.0 degrees, Isha 18.0 degrees.</summary>
    Belgium,
    /// <summary>The birmingham preset: Fajr 18.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 17.0 degrees.</summary>
    Birmingham,
    /// <summary>The blackburn preset: Fajr 18.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 17.0 degrees.</summary>
    Blackburn,
    /// <summary>The calgary preset: Fajr 15.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 15.0 degrees.</summary>
    Calgary,
    /// <summary>The custom preset: Fajr 18.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 17.0 degrees.</summary>
    Custom,
    /// <summary>The czech preset: Fajr 12.04 degrees, Maghrib 0.0 degrees, Isha 12.04 degrees.</summary>
    Czech,
    /// <summary>The dordrecht preset: Fajr 15.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 15.0 degrees.</summary>
    Dordrecht,
    /// <summary>The egypt preset: Fajr 19.5 degrees, Maghrib 0.0 minutes after Sunset, Isha 17.5 degrees.</summary>
    Egyptian,
    /// <summary>The eindhoven preset: Fajr 15.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 15.0 degrees.</summary>
    Eindhoven,
    /// <summary>The emirates preset: Fajr 18.5 degrees, Maghrib 2.0 minutes after Sunset, Isha 18.5 degrees.</summary>
    Emirates,
    /// <summary>The fribourg preset: Fajr 18.01 degrees, Maghrib 0.0 degrees, Isha 100.0 minutes after final Maghrib.</summary>
    Fribourg,
    /// <summary>The indonesia preset: Fajr 20.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 18.0 degrees.</summary>
    Indonesia,
    /// <summary>The iraq preset: Fajr 18.0 degrees, Maghrib 0.0 degrees, Isha 17.0 degrees.</summary>
    Iraq,
    /// <summary>The isna preset: Fajr 15.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 15.0 degrees.</summary>
    NorthAmerica,
    /// <summary>The jordan preset: Fajr 18.12 degrees, Maghrib 0.0 degrees, Isha 17.975 degrees.</summary>
    Jordan,
    /// <summary>The karachi preset: Fajr 18.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 18.0 degrees.</summary>
    Karachi,
    /// <summary>The kazakhstan preset: Fajr 14.97 degrees, Maghrib 0.0 degrees, Isha 14.96 degrees.</summary>
    Kazakhstan,
    /// <summary>The kuwait preset: Fajr 18.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 17.5 degrees.</summary>
    Kuwait,
    /// <summary>The libya preset: Fajr 18.3 degrees, Maghrib 0.0 degrees, Isha 18.35 degrees.</summary>
    Libya,
    /// <summary>The london preset: Fajr 18.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 17.0 degrees.</summary>
    London,
    /// <summary>The luxembourg preset: Fajr 18.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 17.0 degrees.</summary>
    Luxembourg,
    /// <summary>The lyon preset: Fajr 12.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 12.0 degrees.</summary>
    Lyon,
    /// <summary>The makkah preset: Fajr 18.5 degrees, Maghrib 0.0 minutes after Sunset, Isha 90.0 minutes after final Maghrib.</summary>
    Makkah,
    /// <summary>The malaysia preset: Fajr 20.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 18.0 degrees.</summary>
    Malaysia,
    /// <summary>The malaysia2 preset: Fajr 20.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 18.46 degrees.</summary>
    Malaysia2,
    /// <summary>The maldives preset: Fajr 19.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 19.0 degrees.</summary>
    Maldives,
    /// <summary>The mississauga preset: Fajr 15.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 15.0 degrees.</summary>
    Mississauga,
    /// <summary>The montreal preset: Fajr 15.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 15.0 degrees.</summary>
    Montreal,
    /// <summary>The morocco preset: Fajr 19.09 degrees, Maghrib 0.0 degrees, Isha 17.0 degrees.</summary>
    Morocco,
    /// <summary>The moscow preset: Fajr 16.0 degrees, Maghrib 0.0 degrees, Isha 15.1 degrees.</summary>
    Moscow,
    /// <summary>The munchen preset: Fajr 18.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 17.0 degrees.</summary>
    Munchen,
    /// <summary>The mwl preset: Fajr 18.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 17.0 degrees.</summary>
    MuslimWorldLeague,
    /// <summary>The none preset: Fajr 18.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 17.0 degrees.</summary>
    None,
    /// <summary>The nurnberg preset: Fajr 18.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 17.0 degrees.</summary>
    Nurnberg,
    /// <summary>The oman preset: Fajr 18.0 degrees, Maghrib 5.0 minutes after Sunset, Isha 18.0 degrees.</summary>
    Oman,
    /// <summary>The omanMuscat preset: Fajr 17.74 degrees, Maghrib 0.0 degrees, Isha 18.229 degrees.</summary>
    OmanMuscat,
    /// <summary>The orleans preset: Fajr 15.0 degrees, Maghrib 0.0 degrees, Isha 12.34 degrees.</summary>
    Orleans,
    /// <summary>The palestine preset: Fajr 20.11 degrees, Maghrib 0.0 degrees, Isha 17.9 degrees.</summary>
    Palestine,
    /// <summary>The paris preset: Fajr 12.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 12.0 degrees.</summary>
    Paris,
    /// <summary>The potsdam preset: Fajr 18.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 17.0 degrees.</summary>
    Potsdam,
    /// <summary>The qatar preset: Fajr 18.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 90.0 minutes after final Maghrib.</summary>
    Qatar,
    /// <summary>The rotterdam preset: Fajr 15.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 15.0 degrees.</summary>
    Rotterdam,
    /// <summary>The southkorea preset: Fajr 18.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 18.0 degrees.</summary>
    SouthKorea,
    /// <summary>The sudan preset: Fajr 18.12 degrees, Maghrib 0.0 degrees, Isha 17.88 degrees.</summary>
    Sudan,
    /// <summary>The switzerland preset: Fajr 17.99 degrees, Maghrib 0.0 degrees, Isha 100.0 minutes after final Maghrib.</summary>
    Switzerland,
    /// <summary>The syria preset: Fajr 19.5 degrees, Maghrib 0.0 minutes after Sunset, Isha 17.5 degrees.</summary>
    Syria,
    /// <summary>The tajikistan preset: Fajr 18.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 18.0 degrees.</summary>
    Tajikistan,
    /// <summary>The toulouse preset: Fajr 12.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 12.0 degrees.</summary>
    Toulouse,
    /// <summary>The tunisia preset: Fajr 18.0 degrees, Maghrib 1.0 minutes after Sunset, Isha 18.0 degrees.</summary>
    Tunisia,
    /// <summary>The turkey preset: Fajr 18.0 degrees, Maghrib 0.0 degrees, Isha 16.93 degrees.</summary>
    Turkey,
    /// <summary>The uoif preset: Fajr 12.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 12.0 degrees.</summary>
    Uoif,
    /// <summary>The windsor preset: Fajr 15.0 degrees, Maghrib 0.0 minutes after Sunset, Isha 15.0 degrees.</summary>
    Windsor,
    /// <summary>Dubai label using Muslim World League numeric fallback parameters.</summary>
    Dubai,
}

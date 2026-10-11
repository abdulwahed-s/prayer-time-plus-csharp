namespace PrayerTimePlus.Engine;

internal readonly record struct SolarTimes(
    double Fajr, double Sunrise, double Dhuhr, double Asr, double Sunset, double Maghrib, double Isha)
{
    internal bool LooksDegenerate()
    {
        var fajr = Rounding.MinuteOfDay(Fajr);
        var ishaHour = Rounding.MinuteOfDay(Isha) / 60;
        return fajr is null or 0 || ishaHour is null or 0 or 12;
    }
}

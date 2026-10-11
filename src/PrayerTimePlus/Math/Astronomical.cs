namespace PrayerTimePlus.Numerics;

internal static class Astronomical
{
    internal static double JulianDay(DateComponents date)
    {
        var year = date.Year;
        var month = date.Month;
        if (month <= 2)
        {
            year -= 1;
            month += 12;
        }

        var century = Math.Floor(year / 100.0);
        return Math.Floor((year + 4716) * 365.25)
            + Math.Floor((month + 1) * 30.6001)
            + date.Day
            + (2 - century + Math.Floor(century / 4.0))
            - 1524.5;
    }

    internal static SolarCoordinates SunPosition(double julianDate)
    {
        var days = julianDate - 2451545.0;
        var meanAnomaly = DegreeMath.FixAngle(0.98560028 * days + 357.529);
        var meanLongitude = DegreeMath.FixAngle(0.98564736 * days + 280.459);
        var eclipticLongitude = DegreeMath.FixAngle(meanLongitude
            + 1.915 * DegreeMath.Sin(meanAnomaly)
            + 0.02 * DegreeMath.Sin(2.0 * meanAnomaly));
        var obliquity = 23.439 - 0.00000036 * days;
        var declination = DegreeMath.Asin(DegreeMath.Sin(obliquity) * DegreeMath.Sin(eclipticLongitude));
        var rightAscension = DegreeMath.Atan2(DegreeMath.Cos(obliquity) * DegreeMath.Sin(eclipticLongitude),
            DegreeMath.Cos(eclipticLongitude)) / 15.0;
        var equationOfTime = meanLongitude / 15.0 - DegreeMath.FixHour(rightAscension);
        return new SolarCoordinates(declination, equationOfTime);
    }
}

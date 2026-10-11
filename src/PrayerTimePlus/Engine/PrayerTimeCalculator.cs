using PrayerTimePlus.Numerics;

namespace PrayerTimePlus.Engine;

internal sealed class PrayerTimeCalculator(
    Coordinates coordinates, DateComponents date, CalculationParameters parameters, string countryCode, double offsetHours)
{
    private readonly double _baseDate = Astronomical.JulianDay(date) - coordinates.Longitude / 360.0;
    private readonly double _fajrOffset = ((double)parameters.MethodAdjustments.Fajr + parameters.Adjustments.Fajr) / 60.0;
    private readonly double _sunriseOffset = ((double)parameters.MethodAdjustments.Sunrise + parameters.Adjustments.Sunrise) / 60.0;
    private readonly double _dhuhrOffset = ((double)parameters.MethodAdjustments.Dhuhr + parameters.Adjustments.Dhuhr) / 60.0;
    private readonly double _asrOffset = ((double)parameters.MethodAdjustments.Asr + parameters.Adjustments.Asr) / 60.0;
    private readonly double _maghribOffset = ((double)parameters.MethodAdjustments.Maghrib + parameters.Adjustments.Maghrib) / 60.0;
    private readonly double _ishaOffset = ((double)parameters.MethodAdjustments.Isha + parameters.Adjustments.Isha) / 60.0;

    internal SolarTimes Compute(HighLatitudeRule rule)
    {
        var dip = UsesElevation() ? 0.833 + 0.0347 * Math.Sqrt(coordinates.Altitude) : 0.833;
        var localisation = offsetHours - coordinates.Longitude / 15.0;
        var fajr = SunAngleTime(180.0 - parameters.FajrAngle, 5.0 / 24.0) + localisation;
        var sunrise = SunAngleTime(180.0 - dip, 6.0 / 24.0) + localisation;
        var dhuhr = MidDay(12.0 / 24.0) + localisation;
        var asr = AsrTime(parameters.Madhab.GetShadowFactor(), 13.0 / 24.0) + localisation;
        var sunset = SunAngleTime(dip, 18.0 / 24.0) + localisation;
        var angleMaghrib = SunAngleTime(parameters.MaghribValue, 18.0 / 24.0) + localisation + _maghribOffset;
        var angleIsha = SunAngleTime(parameters.IshaValue, 18.0 / 24.0) + localisation + _ishaOffset;
        fajr += _fajrOffset;
        sunrise += _sunriseOffset;
        dhuhr += _dhuhrOffset;
        asr += _asrOffset;

        // Candidate chronology is checked before twilight correction and before interval Isha is formed.
        var maghrib = ResolveMaghrib(sunset, angleMaghrib, angleIsha);
        var isha = parameters.IshaIsInterval
            ? maghrib + parameters.IshaValue / 60.0 + _ishaOffset
            : angleIsha;
        if (parameters.Method == "makkah" && countryCode.Equals("SA", StringComparison.OrdinalIgnoreCase) && parameters.IsRamadan)
        {
            isha += 0.5;
        }

        if (rule != HighLatitudeRule.None)
        {
            var night = DegreeMath.FixHour(sunrise - sunset);
            var fajrPortion = NightPortion(rule, parameters.FajrAngle) * night;
            if (double.IsNaN(fajr) || DegreeMath.FixHour(sunrise - fajr) > fajrPortion)
            {
                fajr = sunrise - fajrPortion + _fajrOffset;
            }

            var ishaAngle = parameters.IshaIsInterval ? 18.0 : parameters.IshaValue;
            var ishaPortion = NightPortion(rule, ishaAngle) * night;
            if (double.IsNaN(isha) || DegreeMath.FixHour(isha - maghrib) > ishaPortion)
            {
                isha = maghrib + ishaPortion + _ishaOffset;
            }
        }

        return new SolarTimes(fajr, sunrise, dhuhr, asr, sunset, maghrib, isha);
    }

    private double MidDay(double seed) => DegreeMath.FixHour(12.0 - Astronomical.SunPosition(_baseDate + seed).EquationOfTime);

    private double SunAngleTime(double angle, double seed)
    {
        var declination = Astronomical.SunPosition(_baseDate + seed).Declination;
        var noon = MidDay(seed);
        var numerator = -DegreeMath.Sin(angle) - DegreeMath.Sin(declination) * DegreeMath.Sin(coordinates.Latitude);
        var denominator = DegreeMath.Cos(declination) * DegreeMath.Cos(coordinates.Latitude);
        var hourAngle = DegreeMath.Acos(numerator / denominator) / 15.0;
        if (angle > 90.0)
        {
            hourAngle = -hourAngle;
        }

        return noon + hourAngle;
    }

    private double AsrTime(double shadowFactor, double seed)
    {
        var declination = Astronomical.SunPosition(_baseDate + seed).Declination;
        var altitude = DegreeMath.Acot(shadowFactor + DegreeMath.Tan(Math.Abs(coordinates.Latitude - declination)));
        return SunAngleTime(-altitude, seed);
    }

    private bool UsesElevation() => parameters.Method is "iraq" or "morocco" or "tunisia" or "jordan"
        or "orleans" or "sudan" or "belgium" or "kazakhstan"
        || countryCode.ToUpperInvariant() is "PS" or "IL" or "CZ" or "CH";

    private double ResolveMaghrib(double sunset, double angleMaghrib, double angleIsha)
    {
        var sunsetBasedMaghrib = sunset + _maghribOffset;
        if (parameters.MaghribIsInterval)
        {
            return sunsetBasedMaghrib + parameters.MaghribValue / 60.0;
        }

        var chronological = double.IsFinite(angleMaghrib) && angleMaghrib > sunset
            && (parameters.IshaIsInterval || !double.IsFinite(angleIsha) || angleMaghrib < angleIsha);
        return parameters.MaghribValue > 0.0 && chronological ? angleMaghrib : sunsetBasedMaghrib;
    }

    private static double NightPortion(HighLatitudeRule rule, double angle) => rule switch
    {
        HighLatitudeRule.MiddleOfTheNight => 0.5,
        // Use the literal 0.14286 to preserve the reference night fraction.
        HighLatitudeRule.SeventhOfTheNight or HighLatitudeRule.Automatic => 0.14286,
        HighLatitudeRule.TwilightAngle => angle / 60.0,
        _ => 0.0,
    };
}

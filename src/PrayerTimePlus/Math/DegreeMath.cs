namespace PrayerTimePlus.Numerics;

internal static class DegreeMath
{
    private const double RadiansPerDegree = Math.PI / 180.0;
    private const double DegreesPerRadian = 180.0 / Math.PI;

    internal static double Sin(double degrees) => Math.Sin(degrees * RadiansPerDegree);
    internal static double Cos(double degrees) => Math.Cos(degrees * RadiansPerDegree);
    internal static double Tan(double degrees) => Math.Tan(degrees * RadiansPerDegree);
    internal static double Asin(double value) => Math.Asin(value) * DegreesPerRadian;
    internal static double Acos(double value) => Math.Acos(value) * DegreesPerRadian;
    internal static double Atan2(double y, double x) => Math.Atan2(y, x) * DegreesPerRadian;
    internal static double Acot(double value) => Math.Atan2(1.0, value) * DegreesPerRadian;
    internal static double FixAngle(double angle) => Wrap(angle, 360.0);
    internal static double FixHour(double hour) => Wrap(hour, 24.0);

    private static double Wrap(double value, double modulus)
    {
        var remainder = value - modulus * Math.Floor(value / modulus);
        return remainder < 0.0 ? remainder + modulus : remainder;
    }
}

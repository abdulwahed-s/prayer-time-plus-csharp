namespace PrayerTimePlus;

/// <summary>An immutable observer location in degrees and metres, with east longitude positive.</summary>
/// <remarks>The constructor is permissive. Use <see cref="Validated"/> to check geographic bounds.</remarks>
public readonly record struct Coordinates
{
    /// <summary>Creates coordinates without geographic validation.</summary>
    /// <param name="latitude">Latitude in degrees, north positive.</param>
    /// <param name="longitude">Longitude in degrees, east positive.</param>
    /// <param name="altitude">Altitude above sea level in metres; defaults to zero.</param>
    public Coordinates(double latitude, double longitude, double altitude = 0.0)
    {
        Latitude = latitude;
        Longitude = longitude;
        Altitude = altitude;
    }

    /// <summary>Gets latitude in degrees, north positive.</summary>
    public double Latitude { get; }

    /// <summary>Gets longitude in degrees, east positive.</summary>
    public double Longitude { get; }

    /// <summary>Gets altitude above sea level in metres.</summary>
    public double Altitude { get; }

    /// <summary>Creates coordinates after checking finite values and geographic bounds.</summary>
    /// <param name="latitude">Finite latitude in [-90, 90] degrees.</param>
    /// <param name="longitude">Finite longitude in [-180, 180] degrees.</param>
    /// <param name="altitude">Finite, non-negative altitude in metres; defaults to zero.</param>
    /// <returns>The validated immutable location.</returns>
    /// <exception cref="ArgumentOutOfRangeException">A value is non-finite or outside its bounds.</exception>
    public static Coordinates Validated(double latitude, double longitude, double altitude = 0.0)
    {
        if (!double.IsFinite(latitude) || latitude is < -90.0 or > 90.0)
        {
            throw new ArgumentOutOfRangeException(nameof(latitude));
        }

        if (!double.IsFinite(longitude) || longitude is < -180.0 or > 180.0)
        {
            throw new ArgumentOutOfRangeException(nameof(longitude));
        }

        if (!double.IsFinite(altitude) || altitude < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(altitude));
        }

        return new Coordinates(latitude, longitude, altitude);
    }
}

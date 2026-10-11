using System.Globalization;
using PrayerTimePlus;

var coordinates = new Coordinates(24.3486, 56.6953, altitude: 5.0);
var date = new DateComponents(2026, 6, 28);
var offset = TimeSpan.FromHours(4);
var country = "OM";
if (args.Length != 0)
{
    if (args.Length != 5)
    {
        Console.Error.WriteLine("Usage: PrayerTimePlus.Example yyyy-MM-dd latitude longitude utc-offset-minutes country");
        return 1;
    }

    date = DateComponents.From(DateOnly.ParseExact(args[0], "yyyy-MM-dd", CultureInfo.InvariantCulture));
    coordinates = Coordinates.Validated(double.Parse(args[1], CultureInfo.InvariantCulture),
        double.Parse(args[2], CultureInfo.InvariantCulture));
    offset = TimeSpan.FromMinutes(int.Parse(args[3], CultureInfo.InvariantCulture));
    country = args[4];
}

var method = AutoMethod.ForCountry(country);
var times = new PrayerTimes(coordinates: coordinates, dateComponents: date,
    calculationParameters: method.GetParameters(), utcOffset: offset, countryCode: country, cityName: "sohar");
Console.WriteLine($"{date.ToDateOnly():yyyy-MM-dd} / {method.GetKey()} / UTC {offset}");
foreach (var prayer in Enum.GetValues<Prayer>())
{
    if (prayer != Prayer.None)
    {
        Console.WriteLine($"{prayer,-8} {times.TimeForPrayer(prayer)?.ToString("HH:mm", CultureInfo.InvariantCulture) ?? "undefined"}");
    }
}

var parameters = CalculationMethod.Oman.GetParameters() with
{
    Madhab = Madhab.Hanafi,
    HighLatitudeRule = HighLatitudeRule.SeventhOfTheNight,
    Adjustments = new PrayerAdjustments { Fajr = 2 },
};
var customised = new PrayerTimes(coordinates, date, parameters, offset, country);
Console.WriteLine($"Customised Asr: {customised.Asr:HH:mm}");

var custom = CalculationMethod.Custom.GetParameters() with
{
    HighLatitudeRule = HighLatitudeRule.None,
    MaghribIsInterval = false,
    MaghribValue = 4.0,
    IshaIsInterval = true,
    IshaValue = 90.0,
};
var angleTimes = new PrayerTimes(coordinates, date, custom, offset, country);
Console.WriteLine($"Angle Maghrib / interval Isha: {angleTimes.Maghrib:HH:mm} / {angleTimes.Isha:HH:mm}");
if (times.Dhuhr is { } dhuhr)
{
    Console.WriteLine($"At Dhuhr: {times.CurrentPrayer(dhuhr)}; next: {times.NextPrayer(dhuhr)}");
}

Console.WriteLine($"Sunnah last third: {new SunnahTimes(times).LastThirdOfTheNight:yyyy-MM-dd HH:mm zzz}");
return 0;

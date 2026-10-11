using System.Globalization;
using System.Text.Json;
using Xunit;

namespace PrayerTimePlus.Tests;

public sealed class FocusedConformanceTests
{
    public static IEnumerable<object[]> Vectors()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures/focused-sibling-goldens.json")));
        return document.RootElement.GetProperty("vectors").EnumerateArray()
            .Select(vector => new object[] { vector.GetProperty("input").GetProperty("id").GetString()!, vector.Clone() }).ToArray();
    }

    [Theory]
    [MemberData(nameof(Vectors))]
    public void IndependentFocusedCasesMatchTimesSunnahAndDeclaredHelperPolicy(string identifier, JsonElement vector)
    {
        Assert.NotEmpty(identifier);
        var input = vector.GetProperty("input");
        var location = input.GetProperty("coordinates").EnumerateArray().Select(value => value.GetDouble()).ToArray();
        var components = input.GetProperty("date").EnumerateArray().Select(value => value.GetInt32()).ToArray();
        var date = new DateComponents(components[0], components[1], components[2]);
        var parameters = input.GetProperty("construction").GetString() == "direct"
            ? new CalculationParameters()
            : CalculationMethods.FromKey(input.GetProperty("method").GetString())!.Value.GetParameters();
        var overrides = input.GetProperty("parameters");
        parameters = parameters with
        {
            Madhab = (Madhab)input.GetProperty("madhab").GetInt32(),
            HighLatitudeRule = (HighLatitudeRule)input.GetProperty("rule").GetInt32(),
            IsRamadan = input.GetProperty("ramadan").GetBoolean(),
            FajrAngle = Number(overrides, "fajrAngle", parameters.FajrAngle),
            MaghribIsInterval = Flag(overrides, "maghribIsInterval", parameters.MaghribIsInterval),
            MaghribValue = Number(overrides, "maghribValue", parameters.MaghribValue),
            IshaIsInterval = Flag(overrides, "ishaIsInterval", parameters.IshaIsInterval),
            IshaValue = Number(overrides, "ishaValue", parameters.IshaValue),
            Adjustments = Adjustments(input.GetProperty("adjustments"), new PrayerAdjustments()),
            MethodAdjustments = input.TryGetProperty("methodAdjustments", out var methodAdjustments)
                ? Adjustments(methodAdjustments, parameters.MethodAdjustments) : parameters.MethodAdjustments,
        };
        var offset = TimeSpan.FromMinutes(input.GetProperty("offsetMinutes").GetInt32());
        var times = new PrayerTimes(new Coordinates(location[0], location[1], location[2]), date, parameters,
            offset, input.GetProperty("country").GetString()!, input.GetProperty("city").GetString()!);
        var expected = vector.GetProperty("expected");
        var values = TestInputs.Values(times);
        var expectedTimes = expected.GetProperty("times").EnumerateArray().ToArray();
        for (var i = 0; i < values.Length; i++)
        {
            AssertStamp(expectedTimes[i], values[i], offset);
        }

        var sunnah = new SunnahTimes(times);
        var expectedSunnah = expected.GetProperty("sunnah").EnumerateArray().ToArray();
        AssertStamp(expectedSunnah[0], sunnah.MiddleOfTheNight, offset);
        AssertStamp(expectedSunnah[1], sunnah.LastThirdOfTheNight, offset);
        foreach (var query in vector.GetProperty("chronologicalHelpers").EnumerateArray())
        {
            var local = date.ToDateOnly().ToDateTime(new TimeOnly(query.GetProperty("localHour").GetInt32(), 0), DateTimeKind.Unspecified);
            var instant = new DateTimeOffset(local, offset).ToUniversalTime();
            Assert.Equal(ParsePrayer(query.GetProperty("current")), times.CurrentPrayer(instant));
            Assert.Equal(ParsePrayer(query.GetProperty("next")), times.NextPrayer(instant));
        }
    }

    [Fact]
    public void CountryDependentSunnahPreservesContextAndItsKnownDartDifference()
    {
        var times = new PrayerTimes(new Coordinates(50, 14, 3000), new DateComponents(2026, 6, 21),
            CalculationMethod.MuslimWorldLeague.GetParameters() with { HighLatitudeRule = HighLatitudeRule.SeventhOfTheNight },
            TimeSpan.FromHours(2), "CZ", "Prague");
        var sunnah = new SunnahTimes(times);
        Assert.Equal(new DateTimeOffset(2026, 6, 22, 0, 36, 0, times.UtcOffset), sunnah.MiddleOfTheNight);
        Assert.Equal(new DateTimeOffset(2026, 6, 22, 1, 36, 0, times.UtcOffset), sunnah.LastThirdOfTheNight);
    }

    private static double Number(JsonElement values, string key, double fallback) =>
        values.TryGetProperty(key, out var value) ? value.GetDouble() : fallback;

    private static bool Flag(JsonElement values, string key, bool fallback) =>
        values.TryGetProperty(key, out var value) ? value.GetBoolean() : fallback;

    private static Prayer ParsePrayer(JsonElement value) => Enum.Parse<Prayer>(value.GetString()!, ignoreCase: true);

    private static PrayerAdjustments Adjustments(JsonElement values, PrayerAdjustments defaults) => defaults with
    {
        Fajr = (int)Number(values, "fajr", defaults.Fajr),
        Sunrise = (int)Number(values, "sunrise", defaults.Sunrise),
        Dhuhr = (int)Number(values, "dhuhr", defaults.Dhuhr),
        Asr = (int)Number(values, "asr", defaults.Asr),
        Maghrib = (int)Number(values, "maghrib", defaults.Maghrib),
        Isha = (int)Number(values, "isha", defaults.Isha),
    };

    private static void AssertStamp(JsonElement expected, DateTimeOffset? actual, TimeSpan offset)
    {
        if (expected.ValueKind == JsonValueKind.Null)
        {
            Assert.Null(actual);
            return;
        }

        var value = Assert.IsType<DateTimeOffset>(actual);
        Assert.Equal(expected.GetProperty("local").GetString(), value.ToString("yyyy-MM-dd'T'HH:mm", CultureInfo.InvariantCulture));
        Assert.Equal(expected.GetProperty("utc").GetString(), value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm'Z'", CultureInfo.InvariantCulture));
        Assert.Equal(offset, value.Offset);
    }
}

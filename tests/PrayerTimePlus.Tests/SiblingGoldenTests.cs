using System.Text.Json;
using Xunit;

namespace PrayerTimePlus.Tests;

public sealed class SiblingGoldenTests
{
    public static IEnumerable<object[]> Vectors()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures/sibling-goldens.json")));
        return document.RootElement.GetProperty("vectors").EnumerateArray()
            .Select(vector => new object[] { vector.Clone() }).ToArray();
    }

    [Theory]
    [MemberData(nameof(Vectors))]
    public void MatchesCapturedSiblingMinutesAndOffsetInstants(JsonElement vector)
    {
        var location = vector.GetProperty("coordinates").EnumerateArray().Select(value => value.GetDouble()).ToArray();
        var date = vector.GetProperty("date").EnumerateArray().Select(value => value.GetInt32()).ToArray();
        var method = CalculationMethods.FromKey(vector.GetProperty("method").GetString())!.Value;
        var parameters = method.GetParameters() with
        {
            Madhab = (Madhab)vector.GetProperty("madhab").GetInt32(),
            HighLatitudeRule = (HighLatitudeRule)vector.GetProperty("rule").GetInt32(),
        };
        var offset = TimeSpan.FromMinutes(vector.GetProperty("offsetMinutes").GetInt32());
        var times = new PrayerTimes(new Coordinates(location[0], location[1], location[2]),
            new DateComponents(date[0], date[1], date[2]), parameters, offset, vector.GetProperty("country").GetString()!);
        var expected = vector.GetProperty("expected").EnumerateArray().Select(value => value.GetString()).ToArray();
        var actual = TestInputs.Values(times);
        Assert.Equal(expected, actual.Select(TestInputs.Clock).ToArray());
        for (var i = 0; i < actual.Length; i++)
        {
            if (actual[i] is { } value)
            {
                Assert.Equal(offset, value.Offset);
                var clock = expected[i]!.Split(':').Select(int.Parse).ToArray();
                var local = new DateTime(date[0], date[1], date[2], clock[0], clock[1], 0, DateTimeKind.Unspecified);
                Assert.Equal(local - offset, value.UtcDateTime);
            }
        }
    }
}

using System.Text.Json;
using PrayerTimePlus.Data;
using Xunit;

namespace PrayerTimePlus.Tests;

public sealed class MethodDataTests
{
    public static IEnumerable<object[]> Methods() => Enum.GetValues<CalculationMethod>().Select(method => new object[] { method });

    public static IEnumerable<object[]> Countries()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Data/auto_method_resolution.json")));
        return document.RootElement.GetProperty("country").EnumerateObject()
            .Select(entry => new object[] { entry.Name, entry.Value.GetString()! }).ToArray();
    }

    [Theory]
    [MemberData(nameof(Methods))]
    public void EveryPresetMatchesAllElevenColumnsAndReturnsFreshValues(CalculationMethod method)
    {
        var key = method.GetKey();
        var parameters = method.GetParameters();
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Data/method_parameters.json")));
        var data = document.RootElement.GetProperty("methods");
        var expected = data.GetProperty(key == "dubai" ? "mwl" : key).EnumerateArray().Select(value => value.GetDouble()).ToArray();
        double[] actual = [parameters.FajrAngle, parameters.MaghribIsInterval ? 1 : 0, parameters.MaghribValue,
            parameters.IshaIsInterval ? 1 : 0, parameters.IshaValue, parameters.MethodAdjustments.Fajr,
            parameters.MethodAdjustments.Sunrise, parameters.MethodAdjustments.Dhuhr, parameters.MethodAdjustments.Asr,
            parameters.MethodAdjustments.Maghrib, parameters.MethodAdjustments.Isha];
        Assert.Equal(expected, actual);
        Assert.Equal(method, CalculationMethods.FromKey(key));
        Assert.Equal(key, parameters.Method);
        Assert.NotSame(parameters, method.GetParameters());
        Assert.NotSame(parameters.MethodAdjustments, method.GetParameters().MethodAdjustments);
        Assert.NotSame(parameters.Adjustments, method.GetParameters().Adjustments);
        Assert.Equal(Madhab.Shafi, parameters.Madhab);
        Assert.Equal(HighLatitudeRule.Automatic, parameters.HighLatitudeRule);
        Assert.False(parameters.IsRamadan);
        Assert.Equal(new PrayerAdjustments(), parameters.Adjustments);
    }

    [Theory]
    [MemberData(nameof(Methods))]
    public void EveryPresetsOffsetsApplyExactlyOnce(CalculationMethod method)
    {
        var parameters = method.GetParameters() with { HighLatitudeRule = HighLatitudeRule.None };
        var baseline = TestInputs.Sohar(parameters with { MethodAdjustments = new PrayerAdjustments() });
        var actual = TestInputs.Sohar(parameters);
        var adjustment = parameters.MethodAdjustments;
        var maghribDelta = adjustment.Maghrib;
        if (parameters.MaghribIsInterval)
        {
            Assert.Equal(parameters.MaghribValue, (baseline.Maghrib - baseline.Sunset)!.Value.TotalMinutes);
        }

        int[] deltas = [adjustment.Fajr, adjustment.Sunrise, adjustment.Dhuhr, adjustment.Asr, 0, maghribDelta,
            adjustment.Isha + (parameters.IshaIsInterval ? maghribDelta : 0)];
        var baselineValues = TestInputs.Values(baseline);
        var actualValues = TestInputs.Values(actual);
        for (var i = 0; i < deltas.Length; i++)
        {
            Assert.Equal((TestInputs.Minute(baselineValues[i]) + deltas[i] + 1440) % 1440, TestInputs.Minute(actualValues[i]));
        }
    }

    [Theory]
    [MemberData(nameof(Countries))]
    public void EveryAutoEntryMatchesInBothCasesWithoutWhitespaceTrimming(string country, string key)
    {
        Assert.Equal(key, AutoMethod.ForCountry(country).GetKey());
        Assert.Equal(key, AutoMethod.ForCountry(country.ToLowerInvariant()).GetKey());
        Assert.Equal(CalculationMethod.MuslimWorldLeague, AutoMethod.ForCountry(" " + country + " "));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("unknown")]
    [InlineData("jafari")]
    [InlineData("tehran")]
    [InlineData("MWL")]
    [InlineData(" mwl ")]
    public void UnknownKeysReturnNullAndNumericTablesFallBackToMwl(string? key)
    {
        Assert.Null(CalculationMethods.FromKey(key));
        Assert.Equal(MethodParameters.ForKey("mwl"), MethodParameters.ForKey(key));
    }

    [Fact]
    public void CustomNoneAndDubaiKeepSharedDefaults()
    {
        var custom = CalculationMethod.Custom.GetParameters();
        Assert.Equal(new CalculationParameters { Method = "custom" }, custom);
        Assert.Equal(custom with { Method = "none" }, CalculationMethod.None.GetParameters());
        Assert.Equal(custom with { Method = "dubai" }, CalculationMethod.Dubai.GetParameters());
        Assert.Equal(CalculationMethod.MuslimWorldLeague, AutoMethod.ForCountry(null));
        Assert.Equal(CalculationMethod.MuslimWorldLeague, AutoMethod.ForCountry("ZZ"));
        Assert.Throws<ArgumentOutOfRangeException>(() => ((CalculationMethod)(-1)).GetParameters());
        Assert.Throws<ArgumentOutOfRangeException>(() => ((CalculationMethod)(-1)).GetKey());
    }
}

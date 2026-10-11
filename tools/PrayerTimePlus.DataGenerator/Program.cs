using System.Globalization;
using System.Text;
using System.Text.Json;

var check = args.Contains("--check", StringComparer.Ordinal);
var root = Path.GetFullPath(args.FirstOrDefault(arg => arg != "--check") ?? ".");
using var methodsDocument = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "tools/data/method_parameters.json")));
using var autoDocument = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "tools/data/auto_method_resolution.json")));
var methods = methodsDocument.RootElement.GetProperty("methods").EnumerateObject()
    .Where(entry => entry.Name is not "jafari" and not "tehran")
    .OrderBy(entry => entry.Name, StringComparer.Ordinal).ToArray();
foreach (var entry in methods)
{
    var values = entry.Value.EnumerateArray().ToArray();
    if (values.Length != 11 || values.Any(value => !double.IsFinite(value.GetDouble()))
        || values[1].GetDouble() is not 0.0 and not 1.0 || values[3].GetDouble() is not 0.0 and not 1.0
        || values.Skip(5).Any(value => value.GetDouble() != Math.Floor(value.GetDouble())))
    {
        throw new InvalidDataException($"Invalid eleven-column preset: {entry.Name}");
    }
}

var header = "// Generated from tools/data. Do not edit by hand.\n"
    + "// Regenerate: dotnet run --project tools/PrayerTimePlus.DataGenerator -c Release\n\n";
var parameters = new StringBuilder(header).Append("namespace PrayerTimePlus.Data;\n\ninternal static class MethodParameters\n{\n")
    .Append("    internal static double[] ForKey(string? key) => key switch\n    {\n");
var keys = new StringBuilder(header).Append("namespace PrayerTimePlus.Data;\n\ninternal static class MethodKeys\n{\n")
    .Append("    internal static string GetKey(CalculationMethod method) => method switch\n    {\n");
var enumeration = new StringBuilder(header).Append("namespace PrayerTimePlus;\n\n")
    .Append("/// <summary>Supported calculation presets with stable external keys and fresh parameter values.</summary>\n")
    .Append("public enum CalculationMethod\n{\n");
foreach (var entry in methods)
{
    var name = EnumName(entry.Name);
    var literals = entry.Value.EnumerateArray().Select(value => value.GetRawText());
    parameters.Append(CultureInfo.InvariantCulture, $"        \"{entry.Name}\" => [{string.Join(", ", literals)}],\n");
    keys.Append(CultureInfo.InvariantCulture, $"        CalculationMethod.{name} => \"{entry.Name}\",\n");
    var values = entry.Value.EnumerateArray().ToArray();
    var maghribUnit = values[1].GetDouble() == 1.0 ? "minutes after Sunset" : "degrees";
    var ishaUnit = values[3].GetDouble() == 1.0 ? "minutes after final Maghrib" : "degrees";
    enumeration.Append(CultureInfo.InvariantCulture,
        $"    /// <summary>The {entry.Name} preset: Fajr {values[0].GetRawText()} degrees, Maghrib {values[2].GetRawText()} {maghribUnit}, Isha {values[4].GetRawText()} {ishaUnit}.</summary>\n    {name},\n");
}
// Dubai has a supported label but no separate table: the numeric fallback is MWL.
keys.Append("        CalculationMethod.Dubai => \"dubai\",\n")
    .Append("        _ => throw new ArgumentOutOfRangeException(nameof(method)),\n    };\n\n")
    .Append("    internal static CalculationMethod? FromKey(string? key) => key switch\n    {\n");
foreach (var entry in methods)
{
    keys.Append(CultureInfo.InvariantCulture, $"        \"{entry.Name}\" => CalculationMethod.{EnumName(entry.Name)},\n");
}
keys.Append("        \"dubai\" => CalculationMethod.Dubai,\n        _ => null,\n    };\n}\n");
enumeration.Append("    /// <summary>Dubai label using Muslim World League numeric fallback parameters.</summary>\n    Dubai,\n}\n");
parameters.Append("        _ => [18.0, 1, 0.0, 0, 17.0, 0, 0, 0, 0, 0, 0],\n    };\n}\n");

var auto = new StringBuilder(header).Append("namespace PrayerTimePlus.Data;\n\ninternal static class AutoMethodResolution\n{\n")
    .Append("    internal static string ForCountry(string? countryCode) => countryCode?.ToUpperInvariant() switch\n    {\n");
foreach (var entry in autoDocument.RootElement.GetProperty("country").EnumerateObject().OrderBy(entry => entry.Name, StringComparer.Ordinal))
{
    var key = entry.Value.GetString()!;
    if (!methods.Any(method => method.Name == key))
    {
        throw new InvalidDataException($"Unsupported Auto preset: {key}");
    }

    auto.Append(CultureInfo.InvariantCulture, $"        \"{entry.Name}\" => \"{key}\",\n");
}
auto.Append("        _ => \"mwl\",\n    };\n}\n");

Write("src/PrayerTimePlus/Data/MethodParameters.g.cs", parameters.ToString());
Write("src/PrayerTimePlus/Data/MethodKeys.g.cs", keys.ToString());
Write("src/PrayerTimePlus/Data/AutoMethodResolution.g.cs", auto.ToString());
Write("src/PrayerTimePlus/Models/CalculationMethod.g.cs", enumeration.ToString());

var documentation = new StringBuilder("# Calculation methods\n\n")
    .Append("Generated from the committed inputs in `tools/data/`. Regenerate with:\n\n")
    .Append("```sh\ndotnet run --project tools/PrayerTimePlus.DataGenerator -c Release\n```\n\n")
    .Append("Keys are case-sensitive; `CalculationMethods.FromKey` returns null for unknown keys. ")
    .Append("Country Auto matching ignores case but preserves whitespace. Unknown countries use `mwl`. ")
    .Append("Each preset returns fresh parameters with Shafi Asr, Automatic high-latitude handling, ")
    .Append("zero user adjustments and Ramadan disabled.\n\n")
    .Append("Maghrib and Isha values are degrees when the interval flag is 0, and minutes when it is 1. ")
    .Append("A non-positive Maghrib angle uses Sunset. A positive angle must be finite, later than Sunset ")
    .Append("and earlier than angle-based Isha; otherwise it falls back to Sunset plus its offsets. ")
    .Append("Interval Isha starts from the final Maghrib. Offsets are signed minutes in ")
    .Append("Fajr, Sunrise, Dhuhr, Asr, Maghrib, Isha order.\n\n")
    .Append("| Key | C# preset | Fajr degrees | Maghrib interval | Maghrib value | Isha interval | Isha value | Fajr offset | Sunrise offset | Dhuhr offset | Asr offset | Maghrib offset | Isha offset |\n")
    .Append("|---|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|\n");
foreach (var entry in methods)
{
    documentation.Append(CultureInfo.InvariantCulture,
        $"| `{entry.Name}` | `{EnumName(entry.Name)}` | {string.Join(" | ", entry.Value.EnumerateArray().Select(value => value.GetRawText()))} |\n");
}
documentation.Append("\n`Dubai` has stable key `dubai` and uses the same numeric fallback as `mwl`. ")
    .Append("`None` is a placeholder with MWL values; `Custom` is the shared custom starting point. ")
    .Append("Shia presets are unsupported. City-labelled presets use the listed parameters without city tables or local tweaks.\n\n")
    .Append("The horizon depression is 0.833 degrees. Only methods `iraq`, `morocco`, `tunisia`, `jordan`, ")
    .Append("`orleans`, `sudan`, `belgium`, `kazakhstan`, or countries `PS`, `IL`, `CZ`, `CH`, ")
    .Append("add `0.0347 * sqrt(altitudeMetres)` to the depression. Country matching ignores case without trimming.\n\n")
    .Append("The `makkah` preset with country `SA` and `IsRamadan = true` adds 30 minutes to Isha before twilight correction.\n");
Write("METHODS.md", documentation.ToString());

void Write(string relativePath, string content)
{
    var path = Path.Combine(root, relativePath);
    if (check)
    {
        if (!File.Exists(path) || File.ReadAllText(path) != content)
        {
            throw new InvalidDataException($"Generated file differs: {relativePath}");
        }
    }
    else
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content, new UTF8Encoding(false));
    }
}

static string EnumName(string key) => key switch
{
    "mwl" => "MuslimWorldLeague",
    "egypt" => "Egyptian",
    "makkah" => "Makkah",
    "isna" => "NorthAmerica",
    "omanMuscat" => "OmanMuscat",
    "southkorea" => "SouthKorea",
    _ => char.ToUpperInvariant(key[0]) + key[1..],
};

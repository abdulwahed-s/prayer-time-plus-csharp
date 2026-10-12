# PrayerTimePlus for .NET

[![NuGet](https://img.shields.io/nuget/v/PrayerTimePlus.svg)](https://www.nuget.org/packages/PrayerTimePlus)

A dependency-free C# library for Islamic prayer times and Sunnah night portions.
The `net8.0` library uses immutable inputs, nullable `DateTimeOffset` results and
the caller's UTC offset. It implements the supported prayer-time calculations
of the Dart, Swift and Kotlin 0.3.0 siblings. The initial C# package is **0.3.0**,
aligned with that family version; helper and city-context differences are
described below.

> **Also available for [Dart / Flutter](https://github.com/abdulwahed-s/prayer_time_plus),
> [Swift](https://github.com/abdulwahed-s/prayer-time-plus-swift), and
> [Kotlin / JVM](https://github.com/abdulwahed-s/prayer-time-plus-kotlin).** All four
> are faithful ports of the same solar engine and match supported prayer-time
> calculations to the minute for identical inputs.
> See [Other platforms](#other-platforms).

## Install

Install [PrayerTimePlus from NuGet](https://www.nuget.org/packages/PrayerTimePlus)
from your consumer project's directory:

```sh
dotnet add package PrayerTimePlus --version 0.3.0
```

The library requires .NET 8 or later and has no third-party runtime dependencies.

Alternatively, download `PrayerTimePlus.0.3.0.nupkg` from
[GitHub Releases](https://github.com/abdulwahed-s/prayer-time-plus-csharp/releases)
into a local package folder and install from that folder:

```sh
dotnet add package PrayerTimePlus --version 0.3.0 --source /absolute/path/to/downloaded/packages
```

## Calculate a day

```csharp
using PrayerTimePlus;

var times = new PrayerTimes(
    coordinates: new Coordinates(24.3486, 56.6953, altitude: 5.0),
    dateComponents: new DateComponents(2026, 6, 28),
    calculationParameters: CalculationMethod.Oman.GetParameters(),
    utcOffset: TimeSpan.FromHours(4),
    countryCode: "OM",
    cityName: "sohar");

Console.WriteLine(times.Fajr?.ToString("HH:mm"));    // 03:59
Console.WriteLine(times.Maghrib?.ToString("HH:mm")); // 19:10
Console.WriteLine(times.Isha?.ToString("HH:mm"));    // 20:35
```

The seven properties are `Fajr`, `Sunrise`, `Dhuhr`, `Asr`, `Sunset`, `Maghrib`
and `Isha`. Each defined result carries the supplied offset; `.UtcDateTime`
gives its absolute UTC instant. Undefined solar events return `null`.

The date is a Gregorian civil date. A `DateOnly` can be adapted with
`DateComponents.From(date)`. UTC offsets must contain whole minutes and fall
within ±14 hours; include the DST adjustment for the requested date yourself.
Offsets such as `TimeSpan.FromMinutes(345)` (+05:45) are supported. The library
does not discover time zones or locations.

Ordinary prayer times round to a minute and wrap to the requested civil date,
even if an intermediate hour crosses midnight. Gregorian dates use years
1–9999. An invalid date, default `DateComponents`, invalid enum or invalid
offset throws an argument exception. At the date-range endpoints, a UTC
instant outside years 1–9999 also throws `ArgumentOutOfRangeException`.

`Coordinates` accepts arbitrary doubles. Choose
`Coordinates.Validated(latitude, longitude, altitude)` to require finite
latitude in [-90, 90], longitude in [-180, 180] and non-negative altitude in
metres. East longitude is positive. Elevation affects only the methods and
countries listed in [METHODS.md](METHODS.md). `cityName` is a caller label and
does not affect the calculation.

## Presets, Auto and adjustments

[METHODS.md](METHODS.md) lists every supported stable key, angle, interval and
minute offset. `CalculationMethods.FromKey("oman")` resolves exact keys;
unknown, null, case-altered or whitespace-padded keys return `null`.
`Custom` and the `None` placeholder use the shared MWL defaults. The `Dubai`
label uses MWL numeric fallback values.

```csharp
var method = AutoMethod.ForCountry("OM");
var parameters = method.GetParameters() with
{
    Madhab = Madhab.Hanafi,
    HighLatitudeRule = HighLatitudeRule.SeventhOfTheNight,
    Adjustments = new PrayerAdjustments { Fajr = 2 }
};
```

Auto uses the compiled country map, matches invariant case without trimming,
and falls back to Muslim World League for unknown, empty or null country codes.
Pass the country code to `PrayerTimes` too, for country-dependent elevation
and Ramadan behavior. Presets return fresh immutable records. Customize them
with `with`; `MethodAdjustments` and caller `Adjustments` are combined once.
Their signed integer values are minutes. Sunset has no adjustment slot.

Shafi is the default Asr school and uses shadow factor 1. Hanafi uses factor 2.
The default high-latitude rule is `Automatic`: calculate with `None`, then
retry once using the literal night fraction `0.14286` if rounded Fajr is null
or midnight, or Isha is null or in hour 0 or 12. Explicit rules are:

- `None`: preserve unavailable twilight as null.
- `MiddleOfTheNight`: half the sunset-to-sunrise night.
- `SeventhOfTheNight`: the literal fraction `0.14286` of the night.
- `TwilightAngle`: the prayer's depression angle divided by 60 of the night;
  interval Isha uses 18 degrees for this rule.

Night corrections limit twilight only when the natural result is undefined
or farther from its anchor than the permitted fraction. An unavailable
Sunrise or Sunset can still leave twilight null under any rule.

## Custom Maghrib and Isha

```csharp
var custom = CalculationMethod.Custom.GetParameters() with
{
    HighLatitudeRule = HighLatitudeRule.None,
    MaghribIsInterval = false,
    MaghribValue = 4.0,
    IshaIsInterval = true,
    IshaValue = 90.0
};
```

Fajr always uses a depression angle in degrees. In angle mode, positive
Maghrib values use a finite evening angle later than Sunset and earlier than
angle-based Isha. A non-positive, unavailable or non-chronological angle falls
back to Sunset plus Maghrib adjustments. In interval mode, Maghrib is Sunset
plus `MaghribValue` minutes and its adjustments. Interval Isha starts at the
final Maghrib, then adds `IshaValue` minutes and its own adjustments.

Bare `new CalculationParameters()` defaults `MaghribIsInterval` to false:
an omitted flag makes a positive `MaghribValue` an angle in degrees. In contrast,
the `Custom` preset explicitly uses interval=true with a zero-minute value.

The caller sets `IsRamadan`. With method key `makkah` and country `SA`
(case-insensitive), it adds 30 minutes to Isha before high-latitude correction.
The library performs no Hijri calendar conversion.

## Current, next and Sunnah times

```csharp
var current = times.CurrentPrayer(times.Dhuhr!.Value);
var next = times.NextPrayer(times.Dhuhr.Value); // Asr
var fajr = times.TimeForPrayer(Prayer.Fajr);
var sunnah = new SunnahTimes(times);
Console.WriteLine(sunnah.LastThirdOfTheNight);
```

Current/next compare absolute instants, skip undefined values and include
Sunrise. Current is the latest boundary at or before the supplied instant;
next is the earliest strictly after it. Current returns `None` before the
day's first defined boundary, and next returns `None` after the last. `None`
has no time. Omitting the instant reads the current UTC clock.

Helpers use actual timestamp order when midnight wrapping or adjustments make
the prayer names unordered. Equal timestamps select the later prayer name for
current and the earlier name for next, in Fajr → Sunrise → Dhuhr → Asr → Maghrib
→ Isha order. Kotlin and Dart 0.3.0 helpers scan that fixed name order instead,
so helper answers can differ for reordered schedules even when all seven
calculated prayer minutes match. For Oslo on 2026-06-21, +02:00, MWL/Shafi and
TwilightAngle, Isha is 00:12 on the requested date: C# selects Isha as next at
00:00, and Sunrise as current at 10:00. The sibling helpers select Fajr and
Isha respectively. `NextPrayer` only examines this constructed day; consumers
must calculate tomorrow with its correct DST offset after the day's last event.

Minute-level compatibility covers the supported solar calculations. C# and
Kotlin treat `cityName` as a label, while Dart applies city-specific tweaks for
some presets. Those application adjustments are outside the C# engine. C#
preserves country/city context when recomputing tomorrow for Sunnah, matching
Kotlin; Dart 0.3.0 omits that context and can produce different night portions.

`PrayerTimes.Today(coordinates, parameters, offset)` chooses today's date at
the supplied offset. Calculation for an explicit date never reads the clock.

Sunnah times run from today's final Maghrib to tomorrow's recomputed Fajr,
using the same inputs and offset. They can fall on the following date and
round to a minute after integer-second night division. Either missing boundary
makes both values null. Tomorrow beyond year 9999 throws an argument exception.
If DST changes overnight, calculate the required days and boundaries in your
application using their respective offsets.

## Build, example and verification

Install the stable .NET SDK pinned in [global.json](global.json). The library
targets `net8.0`; the example, tests and generator target `net10.0`. From the
repository root:

```sh
dotnet restore PrayerTimePlus.slnx
dotnet build PrayerTimePlus.slnx -c Release --no-restore
dotnet format PrayerTimePlus.slnx --verify-no-changes --no-restore
dotnet test tests/PrayerTimePlus.Tests/PrayerTimePlus.Tests.csproj -c Release --no-build --no-restore
dotnet run --project examples/PrayerTimePlus.Example -c Release --no-build
dotnet run --project tools/PrayerTimePlus.DataGenerator -c Release --no-build -- --check
dotnet pack src/PrayerTimePlus/PrayerTimePlus.csproj -c Release --no-build --no-restore -o artifacts/packages
```

The console example defaults to Sohar on 2026-06-28. For another date and
location, provide `yyyy-MM-dd latitude longitude utc-offset-minutes country`:

```sh
dotnet run --project examples/PrayerTimePlus.Example -c Release --no-build -- 2026-06-28 24.3486 56.6953 240 OM
```

The example also compiles and demonstrates customization and helper APIs.
`tools/Verify-Package.ps1` checks package contents and runs a fresh consumer
for each of .NET 8 and .NET 10, with isolated caches and package references
using only the local feed. Install both runtimes and their reference packs;
the SDKs include them. Each consumer asserts the actual runtime major version:

```powershell
pwsh -File tools/Verify-Package.ps1
```

Tests contain exact Sohar/Mecca goldens, custom-angle conformance and captured
fixed-input Dart sibling vectors across presets, both Asr schools, all
high-latitude choices, fractional offsets, leap dates and hemispheres. Fixtures
and generator inputs are committed; builds require no sibling repositories.
Focused custom/Sunnah cases include source revisions and normalized instants;
[the optional capture recipe](tools/conformance/README.md) records the selected
helper policy and the known Dart context exceptions. Generated code remains
subject to analyzers and formatting. Regenerate tables
and method documentation with:

```sh
dotnet run --project tools/PrayerTimePlus.DataGenerator -c Release
```

CI runs the checks on Windows, Linux and macOS. Runtime scope covers the
calculation library; Qibla, Shia presets, static city tables, seasonal changes,
application city tweaks, geolocation, scheduling and native UI are outside it.

## GitHub release pipeline

[The Release workflow](.github/workflows/release.yml) runs when a version tag is
pushed. The tag must match `<Version>` in the library project and have release
notes in [CHANGELOG.md](CHANGELOG.md). Both `v0.3.0` and `0.3.0` tag formats are
supported. After updating the version and changelog and committing the changes:

```sh
git tag v0.3.0
git push origin v0.3.0
```

The workflow reuses the Windows, Linux and macOS CI checks, verifies packaged
consumers on .NET 8 and .NET 10, then publishes the `.nupkg`, `.snupkg` and
`SHA256SUMS` as GitHub Release assets. Release notes come from the matching
changelog section. The built-in GitHub token supplies release permissions.

For an existing tag, **Actions → Release → Run workflow** can verify packages
without publishing; enable **publish** to create the release. Completed releases
are preserved on reruns, and interrupted draft uploads can be resumed. NuGet.org
publication remains a separate step using the released `.nupkg`.

## Other platforms

The same solar engine, ported idiomatically to four ecosystems, with matching
supported prayer-time calculations to the minute:

| Platform | Package | Repository |
|---|---|---|
| **C# / .NET** — you are here | [`PrayerTimePlus`](https://www.nuget.org/packages/PrayerTimePlus) | [prayer-time-plus-csharp](https://github.com/abdulwahed-s/prayer-time-plus-csharp) |
| Dart / Flutter | [`prayer_time_plus`](https://pub.dev/packages/prayer_time_plus) | [prayer_time_plus](https://github.com/abdulwahed-s/prayer_time_plus) |
| Swift · iOS, macOS, watchOS, tvOS, Linux | [Swift Package Index](https://swiftpackageindex.com/abdulwahed-s/prayer-time-plus-swift) | [prayer-time-plus-swift](https://github.com/abdulwahed-s/prayer-time-plus-swift) |
| Kotlin / JVM | [`io.github.abdulwahed-s:prayer-time-plus`](https://central.sonatype.com/artifact/io.github.abdulwahed-s/prayer-time-plus) | [prayer-time-plus-kotlin](https://github.com/abdulwahed-s/prayer-time-plus-kotlin) |

See [Current, next and Sunnah times](#current-next-and-sunnah-times) for helper
ordering and city-context compatibility details.

## License

Licensed under [MIT](LICENSE).

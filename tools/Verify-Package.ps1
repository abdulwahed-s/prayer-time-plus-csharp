[CmdletBinding()]
param(
    [string]$PackageDirectory = 'artifacts/packages'
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repositoryRoot 'src/PrayerTimePlus/PrayerTimePlus.csproj'
$project = [xml](Get-Content -LiteralPath $projectPath -Raw)
$version = [string]$project.Project.PropertyGroup.Version
if ([IO.Path]::IsPathRooted($PackageDirectory)) {
    $feed = (Resolve-Path -LiteralPath $PackageDirectory).Path
} else {
    $feed = (Resolve-Path -LiteralPath (Join-Path $repositoryRoot $PackageDirectory)).Path
}
$package = Join-Path $feed "PrayerTimePlus.$version.nupkg"
$symbols = Join-Path $feed "PrayerTimePlus.$version.snupkg"
if (-not (Test-Path -LiteralPath $package) -or -not (Test-Path -LiteralPath $symbols)) {
    throw 'Expected library and portable-symbol packages are missing.'
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead($package)
try {
    $entries = @($archive.Entries | ForEach-Object { $_.FullName })
    foreach ($required in @('lib/net8.0/PrayerTimePlus.dll', 'lib/net8.0/PrayerTimePlus.xml', 'README.md', 'LICENSE')) {
        if ($entries -notcontains $required) { throw "Missing package entry: $required" }
    }
    foreach ($entry in $entries) {
        if ($entry -notmatch '^(_rels/\.rels|\[Content_Types\]\.xml|PrayerTimePlus\.nuspec|README\.md|LICENSE|lib/net8\.0/PrayerTimePlus\.(dll|xml)|package/services/metadata/core-properties/[^/]+\.psmdcp)$') {
            throw "Unexpected package content: $entry"
        }
    }
    $reader = [IO.StreamReader]::new($archive.GetEntry('PrayerTimePlus.nuspec').Open())
    try { $manifest = [xml]$reader.ReadToEnd() } finally { $reader.Dispose() }
    $metadata = $manifest.package.metadata
    if ($metadata.id -ne 'PrayerTimePlus' -or $metadata.version -ne $version -or
        $metadata.authors -ne 'abdulwahed-s' -or $metadata.license.InnerText -ne 'MIT' -or
        $metadata.license.type -ne 'expression' -or $metadata.readme -ne 'README.md' -or
        $metadata.repository.url -ne 'https://github.com/abdulwahed-s/prayer-time-plus-csharp' -or
        $metadata.repository.type -ne 'git') {
        throw 'Package metadata does not match the library identity.'
    }
    if ($manifest.SelectNodes("//*[local-name()='dependency']").Count -ne 0) {
        throw 'The calculation package must have zero runtime dependencies.'
    }
} finally {
    $archive.Dispose()
}

$symbolArchive = [IO.Compression.ZipFile]::OpenRead($symbols)
try {
    if ($null -eq $symbolArchive.GetEntry('lib/net8.0/PrayerTimePlus.pdb')) {
        throw 'Portable PDB is missing from the symbol package.'
    }
} finally {
    $symbolArchive.Dispose()
}

$runRoot = Join-Path $repositoryRoot ('artifacts/package-smoke-' + [guid]::NewGuid().ToString('N'))
foreach ($runtimeMajor in @(8, 10)) {
    $smokeRoot = Join-Path $runRoot "net$runtimeMajor"
    New-Item -ItemType Directory -Path $smokeRoot | Out-Null
    $smokeProject = Join-Path $smokeRoot 'PackageSmoke.csproj'
    $cache = Join-Path $smokeRoot 'nuget-cache'
    $config = Join-Path $smokeRoot 'NuGet.Config'
    $utf8 = [Text.UTF8Encoding]::new($false)
    [IO.File]::WriteAllText($smokeProject, @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net$runtimeMajor.0</TargetFramework>
    <RollForward>LatestPatch</RollForward>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="PrayerTimePlus" Version="$version" />
  </ItemGroup>
</Project>
"@, $utf8)
    $escapedFeed = [Security.SecurityElement]::Escape($feed)
    [IO.File]::WriteAllText($config, @"
<configuration>
  <packageSources>
    <clear />
    <add key="local" value="$escapedFeed" />
  </packageSources>
</configuration>
"@, $utf8)
    [IO.File]::WriteAllText((Join-Path $smokeRoot 'Program.cs'), @'
using System.Globalization;
using PrayerTimePlus;

if (Environment.Version.Major != EXPECTED_RUNTIME_MAJOR)
{
    throw new InvalidOperationException($"Expected .NET EXPECTED_RUNTIME_MAJOR, actually running {Environment.Version}.");
}

var offset = TimeSpan.FromHours(4);
var location = new Coordinates(24.3486, 56.6953, altitude: 5.0);
var date = new DateComponents(2026, 6, 28);
var times = new PrayerTimes(location, date, CalculationMethod.Oman.GetParameters(), offset, "OM", "sohar");
string[] expected = ["03:59", "05:27", "12:21", "15:42", "19:05", "19:10", "20:35"];
DateTimeOffset?[] actual = [times.Fajr, times.Sunrise, times.Dhuhr, times.Asr, times.Sunset, times.Maghrib, times.Isha];
for (var index = 0; index < expected.Length; index++)
{
    if (actual[index] is not { } value || value.Offset != offset
        || value.ToString("HH:mm", CultureInfo.InvariantCulture) != expected[index]
        || value.UtcDateTime != value.DateTime - offset)
    {
        throw new InvalidOperationException($"Packaged golden mismatch at index {index}.");
    }
}
var custom = CalculationMethod.Custom.GetParameters() with
{
    HighLatitudeRule = HighLatitudeRule.None,
    MaghribIsInterval = false,
    MaghribValue = 4.0,
    IshaIsInterval = true,
    IshaValue = 90.0
};
var bare = new PrayerTimes(location, date, new CalculationParameters
{
    MaghribValue = 4.0,
    IshaIsInterval = true,
    IshaValue = 90.0,
    HighLatitudeRule = HighLatitudeRule.None
}, offset, "OM");
if (bare.Maghrib?.ToString("HH:mm", CultureInfo.InvariantCulture) != "19:21"
    || bare.Isha?.ToString("HH:mm", CultureInfo.InvariantCulture) != "20:51")
{
    throw new InvalidOperationException("Packaged bare Maghrib default mismatch.");
}
var baseline = new PrayerTimes(location, date, custom, offset, "OM");
var tuned = new PrayerTimes(location, date, custom with
{
    MethodAdjustments = new PrayerAdjustments { Maghrib = 2, Isha = 3 },
    Adjustments = new PrayerAdjustments { Maghrib = 3, Isha = 4 }
}, offset, "OM");
if (baseline.Maghrib <= baseline.Sunset || baseline.Isha != baseline.Maghrib!.Value.AddMinutes(90)
    || tuned.Maghrib != baseline.Maghrib.Value.AddMinutes(5)
    || tuned.Isha != baseline.Isha!.Value.AddMinutes(12))
{
    throw new InvalidOperationException("Packaged custom Maghrib / interval Isha mismatch.");
}
if (times.CurrentPrayer(times.Dhuhr!.Value) != Prayer.Dhuhr || times.NextPrayer(times.Dhuhr.Value) != Prayer.Asr
    || new SunnahTimes(times).LastThirdOfTheNight is null || AutoMethod.ForCountry("om") != CalculationMethod.Oman)
{
    throw new InvalidOperationException("Packaged helper mismatch.");
}
Console.WriteLine($"NuGet consumer passed on .NET {Environment.Version}: goldens, bare defaults, tuning, offsets and helpers.");
'@.Replace('EXPECTED_RUNTIME_MAJOR', [string]$runtimeMajor), $utf8)

    & dotnet restore $smokeProject --configfile $config --packages $cache
    if ($LASTEXITCODE -ne 0) { throw 'Local-feed consumer restore failed.' }
    & dotnet run --project $smokeProject -c Release --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'Packaged consumer failed.' }
    Write-Output "Package contents and metadata verified: $package"
    Write-Output "SHA256: $((Get-FileHash -LiteralPath $package -Algorithm SHA256).Hash)"
    Write-Output "Isolated consumer/cache: $smokeRoot"
}

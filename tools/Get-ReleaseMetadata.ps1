[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Tag
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$pattern = '^v?(?<version>(?:0|[1-9][0-9]*)\.(?:0|[1-9][0-9]*)\.(?:0|[1-9][0-9]*)(?:-[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?)$'
$tagMatch = [regex]::Match($Tag, $pattern)
if (-not $tagMatch.Success) {
    throw 'Use a semantic version tag such as v0.3.0 or 0.3.0, optionally with a prerelease suffix.'
}
$version = $tagMatch.Groups['version'].Value
$isPrerelease = $version.Contains('-')
if ($isPrerelease) {
    foreach ($identifier in $version.Split('-', 2)[1].Split('.')) {
        if ($identifier -match '^0[0-9]+$') {
            throw 'Numeric prerelease identifiers must not contain leading zeroes.'
        }
    }
}

$projectPath = Join-Path $repositoryRoot 'src/PrayerTimePlus/PrayerTimePlus.csproj'
$project = [xml](Get-Content -LiteralPath $projectPath -Raw -Encoding UTF8)
$projectVersion = [string]$project.Project.PropertyGroup.Version
if ($version -cne $projectVersion) {
    throw "Tag version $version does not match the project version $projectVersion."
}
$packageId = [string]$project.Project.PropertyGroup.PackageId
if ($packageId -ne 'PrayerTimePlus') {
    throw 'The release package ID must be PrayerTimePlus.'
}

$changelog = Get-Content -LiteralPath (Join-Path $repositoryRoot 'CHANGELOG.md') -Raw -Encoding UTF8
$escapedVersion = [regex]::Escape($version)
$section = [regex]::Match($changelog, "(?ms)^## $escapedVersion(?:[ \t]+[^\r\n]*)?\r?\n(?<notes>.*?)(?=^## |\z)")
if (-not $section.Success -or [string]::IsNullOrWhiteSpace($section.Groups['notes'].Value)) {
    throw "Add a non-empty CHANGELOG.md section for $version before releasing."
}

[pscustomobject]@{
    Tag = $Tag
    Version = $version
    PackageId = $packageId
    IsPrerelease = $isPrerelease
    Notes = $section.Groups['notes'].Value.Trim()
}

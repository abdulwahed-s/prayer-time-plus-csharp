[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Tag,
    [string]$PackageDirectory = 'artifacts/packages',
    [string]$NotesFile = 'artifacts/release-notes.md'
)

$ErrorActionPreference = 'Stop'
$metadata = & (Join-Path $PSScriptRoot 'Get-ReleaseMetadata.ps1') -Tag $Tag
$packageRoot = (Resolve-Path -LiteralPath $PackageDirectory).Path
$notesPath = (Resolve-Path -LiteralPath $NotesFile).Path
$assets = @(
    (Join-Path $packageRoot "$($metadata.PackageId).$($metadata.Version).nupkg"),
    (Join-Path $packageRoot "$($metadata.PackageId).$($metadata.Version).snupkg")
)
foreach ($asset in $assets) {
    if (-not (Test-Path -LiteralPath $asset -PathType Leaf)) {
        throw "Missing verified release asset: $asset"
    }
}
if ([string]::IsNullOrWhiteSpace((Get-Content -LiteralPath $notesPath -Raw -Encoding UTF8))) {
    throw 'Release notes must not be empty.'
}
$checksumPath = Join-Path $packageRoot 'SHA256SUMS'
$checksums = foreach ($asset in $assets) {
    $hash = (Get-FileHash -LiteralPath $asset -Algorithm SHA256).Hash.ToLowerInvariant()
    "$hash  $([IO.Path]::GetFileName($asset))"
}
$checksums | Set-Content -LiteralPath $checksumPath -Encoding ascii
$assets += $checksumPath

function Invoke-GitHubCommand {
    param([string[]]$Arguments)
    & gh @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "GitHub release command failed: $($Arguments[0]) $($Arguments[1])"
    }
}

$existingOutput = & gh release view $Tag --json isDraft,assets,url 2>$null
$existingExitCode = $LASTEXITCODE
if ($existingExitCode -eq 0) {
    $existing = $existingOutput | ConvertFrom-Json
    if (-not $existing.isDraft) {
        foreach ($asset in $assets) {
            if ($existing.assets.name -notcontains [IO.Path]::GetFileName($asset)) {
                throw 'The published release is missing an expected asset; inspect it before making changes.'
            }
        }
        Write-Output "Release already published: $($existing.url)"
        return
    }
} else {
    $createArguments = @('release', 'create', $Tag, '--verify-tag', '--draft',
        '--title', "$($metadata.PackageId) $($metadata.Version)", '--notes-file', $notesPath)
    if ($metadata.IsPrerelease) { $createArguments += '--prerelease' }
    Invoke-GitHubCommand -Arguments $createArguments
}

# Only draft assets are replaced, so reruns can finish an interrupted upload.
Invoke-GitHubCommand -Arguments (@('release', 'upload', $Tag, '--clobber') + $assets)
$publishArguments = @('release', 'edit', $Tag, '--verify-tag', '--draft=false',
    '--notes-file', $notesPath, "--prerelease=$($metadata.IsPrerelease.ToString().ToLowerInvariant())")
if (-not $metadata.IsPrerelease) { $publishArguments += '--latest' }
Invoke-GitHubCommand -Arguments $publishArguments
Invoke-GitHubCommand -Arguments @('release', 'view', $Tag, '--json', 'url', '--jq', '.url')

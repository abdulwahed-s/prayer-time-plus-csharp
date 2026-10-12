$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$metadataScript = Join-Path $repositoryRoot 'tools/Get-ReleaseMetadata.ps1'
$publisherScript = Join-Path $repositoryRoot 'tools/Publish-GitHubRelease.ps1'
$project = [xml](Get-Content -LiteralPath (Join-Path $repositoryRoot 'src/PrayerTimePlus/PrayerTimePlus.csproj') -Raw)
$version = [string]$project.Project.PropertyGroup.Version
$tag = "v$version"
$metadata = & $metadataScript -Tag $tag
if ($metadata.Version -ne $version -or $metadata.PackageId -ne 'PrayerTimePlus' -or
    $metadata.IsPrerelease -ne $version.Contains('-') -or [string]::IsNullOrWhiteSpace($metadata.Notes)) {
    throw 'Release metadata must match the project version and contain changelog notes.'
}
if ((& $metadataScript -Tag $version).Version -ne $version) {
    throw 'Unprefixed version tags must resolve to the same package version.'
}
foreach ($invalidTag in @('v999.999.999', 'v00.3.0', 'v0.3.0-alpha.01', 'v0.3.0; Write-Output invalid')) {
    $rejected = $false
    try { & $metadataScript -Tag $invalidTag | Out-Null } catch { $rejected = $true }
    if (-not $rejected) { throw "Expected the tag to be rejected: $invalidTag" }
}

$checkRoot = Join-Path $repositoryRoot ('artifacts/release-script-check-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $checkRoot -Force | Out-Null
$packageNames = @("PrayerTimePlus.$version.nupkg", "PrayerTimePlus.$version.snupkg")
foreach ($packageName in $packageNames) {
    # The publisher consumes already verified files; these stand-ins test its release lifecycle.
    [IO.File]::WriteAllText((Join-Path $checkRoot $packageName), "release fixture: $packageName")
}
$notesPath = Join-Path $checkRoot 'notes.md'
$metadata.Notes | Set-Content -LiteralPath $notesPath -Encoding UTF8
$global:ReleaseCheckCalls = [Collections.Generic.List[string]]::new()
$global:ReleaseCheckState = 'Absent'
$global:ReleaseCheckFailUpload = $false
$global:ReleaseCheckAssetNames = $packageNames + @('SHA256SUMS')
$priorNativeExitCode = 0
if ($null -ne $global:LASTEXITCODE) { $priorNativeExitCode = $global:LASTEXITCODE }

function global:gh {
    $global:LASTEXITCODE = 0
    $global:ReleaseCheckCalls.Add(($args -join ' '))
    $operation = $args[1]
    if ($operation -eq 'view') {
        if ($args -contains '--jq') { Write-Output 'https://example.invalid/release'; return }
        if ($global:ReleaseCheckState -eq 'Absent') { $global:LASTEXITCODE = 1; return }
        $assetNames = $global:ReleaseCheckAssetNames
        if ($global:ReleaseCheckState -eq 'IncompletePublished') { $assetNames = @($assetNames[0]) }
        [pscustomobject]@{
            isDraft = $global:ReleaseCheckState -eq 'Draft'
            url = 'https://example.invalid/release'
            assets = @($assetNames | ForEach-Object { [pscustomobject]@{ name = $_ } })
        } | ConvertTo-Json -Depth 4 -Compress
    } elseif ($operation -eq 'create') {
        if ($args -notcontains '--draft' -or $args -notcontains '--verify-tag') {
            throw 'Release creation must require an existing tag and start as a draft.'
        }
        $global:ReleaseCheckState = 'Draft'
    } elseif ($operation -eq 'upload') {
        if ($global:ReleaseCheckState -ne 'Draft') { throw 'Upload attempted against a published release.' }
        if ($global:ReleaseCheckFailUpload) { $global:LASTEXITCODE = 1 }
    } elseif ($operation -eq 'edit') {
        if ($args -notcontains '--draft=false') { throw 'The release did not finish publication.' }
        $global:ReleaseCheckState = 'Published'
    } else { throw 'Unexpected GitHub operation.' }
}

try {
    & $publisherScript -Tag $tag -PackageDirectory $checkRoot -NotesFile $notesPath | Out-Null
    if (($global:ReleaseCheckCalls | Where-Object { $_ -match '^release create ' }).Count -ne 1 -or
        $global:ReleaseCheckState -ne 'Published') {
        throw 'Initial release did not complete through a draft.'
    }
    $checksumLines = Get-Content -LiteralPath (Join-Path $checkRoot 'SHA256SUMS')
    foreach ($packageName in $packageNames) {
        $expectedHash = (Get-FileHash -LiteralPath (Join-Path $checkRoot $packageName)).Hash.ToLowerInvariant()
        if ($checksumLines -notcontains "$expectedHash  $packageName") { throw 'Release checksum mismatch.' }
    }

    $global:ReleaseCheckCalls.Clear()
    $global:ReleaseCheckState = 'Draft'
    & $publisherScript -Tag $tag -PackageDirectory $checkRoot -NotesFile $notesPath | Out-Null
    if (($global:ReleaseCheckCalls | Where-Object { $_ -match '^release create ' }).Count -ne 0 -or
        $global:ReleaseCheckState -ne 'Published') {
        throw 'Interrupted draft release was not resumed.'
    }

    $global:ReleaseCheckCalls.Clear()
    & $publisherScript -Tag $tag -PackageDirectory $checkRoot -NotesFile $notesPath | Out-Null
    if ($global:ReleaseCheckCalls.Count -ne 1) { throw 'Published release was changed on a rerun.' }

    $global:ReleaseCheckCalls.Clear()
    $global:ReleaseCheckState = 'IncompletePublished'
    $rejected = $false
    try { & $publisherScript -Tag $tag -PackageDirectory $checkRoot -NotesFile $notesPath | Out-Null } catch { $rejected = $true }
    if (-not $rejected -or $global:ReleaseCheckCalls.Count -ne 1) {
        throw 'Incomplete published release was changed instead of rejected.'
    }

    $global:ReleaseCheckCalls.Clear()
    $global:ReleaseCheckState = 'Draft'
    $global:ReleaseCheckFailUpload = $true
    $rejected = $false
    try { & $publisherScript -Tag $tag -PackageDirectory $checkRoot -NotesFile $notesPath | Out-Null } catch { $rejected = $true }
    if (-not $rejected -or $global:ReleaseCheckState -ne 'Draft') {
        throw 'Failed upload published an incomplete release.'
    }
    Write-Output 'Release workflow checks passed: tags, notes, checksums, draft resume, published-release preservation and upload failures.'
} finally {
    $global:LASTEXITCODE = $priorNativeExitCode
    Remove-Item -LiteralPath Function:\gh
    Remove-Variable ReleaseCheckCalls, ReleaseCheckState, ReleaseCheckFailUpload, ReleaseCheckAssetNames -Scope Global
}

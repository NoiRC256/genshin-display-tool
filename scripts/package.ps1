[CmdletBinding()]
param(
    [ValidateSet('Release', 'Debug')]
    [string] $Configuration = 'Release',
    [ValidateSet('win-x64')]
    [string] $RuntimeIdentifier = 'win-x64',
    [switch] $SelfContained,
    [string] $Version
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$project = Join-Path $repositoryRoot 'src\GenshinScreenTool\GenshinScreenTool.csproj'
$artifactRoot = Join-Path $repositoryRoot 'artifacts\package'
$publishDirectory = Join-Path $artifactRoot 'publish'
$archiveName = "GenshinDisplayTool-$RuntimeIdentifier-$Configuration.zip"
$archivePath = Join-Path $artifactRoot $archiveName

function Invoke-DotNet {
    param([string[]] $Arguments)
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) { throw "dotnet command failed with exit code $LASTEXITCODE." }
}

if (-not $Version) {
    $Version = (& git -C $repositoryRoot describe --tags --match 'v[0-9]*' --always --dirty 2>$null)
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($Version)) { $Version = '0.0.0-local' }
}
$Version = $Version.Trim() -replace '^v', ''
if ($Version -notmatch '^[0-9]+\.[0-9]+\.[0-9]+(?:-[0-9A-Za-z.-]+)?$') { throw "Version '$Version' is not SemVer-like." }

if (Test-Path $artifactRoot) { Remove-Item $artifactRoot -Recurse -Force }
New-Item $publishDirectory -ItemType Directory -Force | Out-Null

Invoke-DotNet @('restore', $project, '--runtime', $RuntimeIdentifier)
Invoke-DotNet @('publish', $project, '--configuration', $Configuration, '--runtime', $RuntimeIdentifier, '--self-contained', ([string]$SelfContained.IsPresent).ToLowerInvariant(), '--no-restore', ('-p:Version=' + $Version), ('-p:InformationalVersion=' + $Version), '--output', $publishDirectory)

$executable = Join-Path $publishDirectory 'GenshinDisplayTool.exe'
if (-not (Test-Path $executable)) { throw "Expected executable was not found: $executable" }

$files = Get-ChildItem $publishDirectory -File | Sort-Object FullName
$manifest = [ordered]@{
    product = 'GenshinDisplayTool'; version = $Version; configuration = $Configuration
    runtimeIdentifier = $RuntimeIdentifier; selfContained = $SelfContained.IsPresent
    generatedUtc = (Get-Date).ToUniversalTime().ToString('o')
    files = @($files | ForEach-Object { [ordered]@{ path = $_.Name; length = $_.Length; sha256 = (Get-FileHash $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant() } })
}
$manifest | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $publishDirectory 'build-manifest.json') -Encoding utf8NoBOM

Compress-Archive -Path (Join-Path $publishDirectory '*') -DestinationPath $archivePath -CompressionLevel Optimal
$archiveHash = (Get-FileHash $archivePath -Algorithm SHA256).Hash.ToLowerInvariant()
"$archiveHash  $archiveName" | Set-Content (Join-Path $artifactRoot 'SHA256SUMS.txt') -Encoding ascii
Write-Host "Created $archivePath"
Write-Host "SHA-256: $archiveHash"

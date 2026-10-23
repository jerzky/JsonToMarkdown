<#
.SYNOPSIS
    Builds and packs JsonToMarkdown NuGet package to a local folder or feed.

.DESCRIPTION
    Streamlines packing JsonToMarkdown with target framework selection,
    custom output directories, and optional local NuGet feed registration.

.PARAMETER Framework
    Target framework to build for: 'all', 'net10.0', 'net9.0', or 'net8.0'. Defaults to 'all'.

.PARAMETER NugetFeed
    Destination folder for generated .nupkg packages. Defaults to './artifacts'.

.PARAMETER Configuration
    Build configuration: 'Release' (default) or 'Debug'.

.PARAMETER Version
    Optional explicit package version (e.g., '0.1.0').

.PARAMETER VersionSuffix
    Optional prerelease suffix (e.g., 'preview.1').

.PARAMETER RegisterLocalFeed
    Registers the destination NuGet folder as a local source with 'dotnet nuget add source'.

.EXAMPLE
    ./publish.ps1
    Packs JsonToMarkdown for all target frameworks (net8.0, net9.0, net10.0) in Release mode.

.EXAMPLE
    ./publish.ps1 -Framework net10.0 -NugetFeed C:\MyLocalNuget
    Packs specifically for .NET 10 into C:\MyLocalNuget.

.EXAMPLE
    ./publish.ps1 -NugetFeed C:\LocalPackages -RegisterLocalFeed
    Packs to C:\LocalPackages and registers the folder as a NuGet package source.
#>

[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [ValidateSet('all', 'net10.0', 'net9.0', 'net8.0')]
    [string]$Framework = 'all',

    [Parameter(Position = 1)]
    [string]$NugetFeed = "$PSScriptRoot/artifacts",

    [Parameter()]
    [string]$Mode = 'NuGet',

    [Parameter()]
    [string]$PublishDir,

    [Parameter()]
    [ValidateSet('Release', 'Debug')]
    [string]$Configuration = 'Release',

    [Parameter()]
    [string]$Version,

    [Parameter()]
    [string]$VersionSuffix,

    [Parameter()]
    [switch]$RegisterLocalFeed
)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

$projectPath = "src/JsonToMarkdown/JsonToMarkdown.csproj"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " JsonToMarkdown - Local NuGet Pack & Publish" -ForegroundColor Cyan
Write-Host " Framework:     $Framework" -ForegroundColor Yellow
Write-Host " Configuration: $Configuration" -ForegroundColor Yellow
Write-Host " NuGet Feed:    $NugetFeed" -ForegroundColor Yellow
Write-Host "==========================================================" -ForegroundColor Cyan

if (-not (Test-Path $NugetFeed)) {
    New-Item -ItemType Directory -Path $NugetFeed -Force | Out-Null
}

$packArgs = @("pack", $projectPath, "-c", $Configuration, "-o", $NugetFeed)

if ($Framework -ne 'all') {
    $packArgs += "-p:TargetFrameworks=$Framework"
    $packArgs += "-p:TargetFramework=$Framework"
}

if (![string]::IsNullOrWhiteSpace($Version)) {
    $packArgs += "-p:PackageVersion=$Version"
}

if (![string]::IsNullOrWhiteSpace($VersionSuffix)) {
    $packArgs += "--version-suffix"
    $packArgs += $VersionSuffix
}

Write-Host "`nPacking JsonToMarkdown..." -ForegroundColor Green
& dotnet @packArgs
if ($LASTEXITCODE -ne 0) {
    throw "Failed packing JsonToMarkdown"
}

if ($RegisterLocalFeed) {
    $resolvedFeed = (Resolve-Path $NugetFeed).Path
    $existing = dotnet nuget list source | Out-String
    if ($existing -notmatch [regex]::Escape($resolvedFeed)) {
        Write-Host "`nRegistering local NuGet source 'LocalFeed' ($resolvedFeed)..." -ForegroundColor Cyan
        & dotnet nuget add source $resolvedFeed -n "LocalFeed"
    } else {
        Write-Host "`nLocal NuGet feed '$resolvedFeed' is already registered." -ForegroundColor DarkGray
    }
}

Write-Host "`n[OK] Operation completed successfully!" -ForegroundColor Green

# Builds the Mawi NuGet package (+ symbols) into ./artifacts.
# Run on Windows so the net9.0-windows target is included; requires the maui, android, ios and
# maccatalyst workloads (dotnet workload install maui).
#
#   ./pack.ps1                      # version from Mawi.csproj
#   ./pack.ps1 -Version 1.0.1       # override the version
#
# Publish with:
#   dotnet nuget push artifacts/Mawi.<version>.nupkg -k <api-key> -s https://api.nuget.org/v3/index.json

param(
    [string]$Version
)

$ErrorActionPreference = "Stop"

$project = Join-Path $PSScriptRoot "src/Mawi/Mawi.csproj"
$output = Join-Path $PSScriptRoot "artifacts"

$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:DOTNET_NOLOGO = "1"

if (-not $IsWindows -and $PSVersionTable.PSEdition -eq "Core") {
    Write-Warning "Not running on Windows: the package will be missing the net9.0-windows target."
}

# _IsPacking is what dotnet pack sets anyway; passed explicitly because Mawi.csproj keys its
# Microsoft.Maui.Controls dependency floor off it
$packArgs = @($project, "-c", "Release", "-o", $output, "-p:_IsPacking=true")
if ($Version) {
    $packArgs += "-p:Version=$Version"
}

dotnet pack @packArgs
if ($LASTEXITCODE -ne 0) {
    Write-Host "Pack failed" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "Packages written to $output" -ForegroundColor Green
Get-ChildItem $output -Filter "Mawi.*nupkg" | ForEach-Object { Write-Host "  $($_.Name)" }

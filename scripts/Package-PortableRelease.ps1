param(
    [string]$RepoRoot = (Split-Path -Parent $PSScriptRoot),
    [string]$Configuration = "Release",
    [string]$OutputRoot = "",
    [string]$Version = ""
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $RepoRoot "artifacts"
}

$releaseDir = Join-Path $RepoRoot ("WindowsFormsApplication3\\bin\\{0}\\net48" -f $Configuration)
$exePath = Join-Path $releaseDir "WindowsFormsApplication3.exe"
if (-not (Test-Path $exePath)) {
    throw "Release executable not found at '$exePath'. Build the solution first."
}

if ([string]::IsNullOrWhiteSpace($Version)) {
    $assemblyInfoPath = Join-Path $RepoRoot "WindowsFormsApplication3\\Properties\\AssemblyInfo.cs"
    $assemblyInfo = Get-Content $assemblyInfoPath
    $versionLine = $assemblyInfo |
        Where-Object { $_ -match '^\[assembly:\s*AssemblyFileVersion\("(?<version>[^"]+)"\)\]' } |
        Select-Object -First 1
    $match = if ($versionLine) {
        [regex]::Match($versionLine, 'AssemblyFileVersion\("(?<version>[^"]+)"\)')
    } else {
        [regex]::Match(($assemblyInfo -join [Environment]::NewLine), '^\[assembly:\s*AssemblyVersion\("(?<version>[^"]+)"\)\]$', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    }
    if (-not $match.Success) {
        throw "Could not determine application version from '$assemblyInfoPath'."
    }

    $Version = $match.Groups["version"].Value
    if ($Version.EndsWith(".0")) {
        $Version = $Version.Substring(0, $Version.Length - 2)
    }
}

$packageName = "PizzaExpress-{0}-portable" -f $Version
$stagingDir = Join-Path $OutputRoot $packageName
$zipPath = Join-Path $OutputRoot ($packageName + ".zip")
$checksumPath = $zipPath + ".sha256"

New-Item -ItemType Directory -Path $OutputRoot -Force | Out-Null
if (Test-Path $stagingDir) {
    Remove-Item -LiteralPath $stagingDir -Recurse -Force
}
if (Test-Path $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}
if (Test-Path $checksumPath) {
    Remove-Item -LiteralPath $checksumPath -Force
}

New-Item -ItemType Directory -Path $stagingDir -Force | Out-Null
Copy-Item -Path (Join-Path $releaseDir "*") -Destination $stagingDir -Recurse -Force

$portableReadme = @"
Pizza Express New Zealand $Version - Portable Package
======================================================

Minimum requirements
  - Windows 10 or Windows 11 (64-bit)
  - .NET Framework 4.8
    Pre-installed on Windows 11.  Windows 10 users: download free from
    https://dotnet.microsoft.com/download/dotnet-framework/net48

Quick start
  1. Extract this ZIP to any folder (e.g. Desktop\PizzaExpress)
  2. Double-click WindowsFormsApplication3.exe
  3. No installer or elevated permissions required.

Local data (created automatically - never stored inside this folder)
  Database : %APPDATA%\PizzaExpress\orders.db
  Logs     : %APPDATA%\PizzaExpress\Logs\
  Backups  : %APPDATA%\PizzaExpress\Backups\

Verifying this download
  A SHA256 checksum file (.sha256) is published alongside this ZIP on the
  GitHub releases page.  Compare it against this file to confirm integrity:

    PowerShell:  (Get-FileHash '$packageName.zip' -Algorithm SHA256).Hash
    CertUtil:    certutil -hashfile $packageName.zip SHA256

Version : $Version
"@

Set-Content -LiteralPath (Join-Path $stagingDir "PORTABLE-README.txt") -Value $portableReadme -Encoding UTF8
Compress-Archive -LiteralPath $stagingDir -DestinationPath $zipPath -Force

# Write SHA256 checksum file next to the ZIP
$hash = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash.ToLowerInvariant()
$checksumLine = "{0}  {1}" -f $hash, (Split-Path -Leaf $zipPath)
Set-Content -LiteralPath $checksumPath -Value $checksumLine -Encoding ASCII -NoNewline

Write-Host ("Portable package created: {0}" -f $zipPath)
Write-Host ("SHA256 checksum written : {0}" -f $checksumPath)
Write-Host ("PACKAGE_PATH={0}" -f $zipPath)
Write-Host ("PACKAGE_DIR={0}" -f $stagingDir)
Write-Host ("PACKAGE_SHA256={0}" -f $hash)

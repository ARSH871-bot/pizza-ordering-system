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

New-Item -ItemType Directory -Path $OutputRoot -Force | Out-Null
if (Test-Path $stagingDir) {
    Remove-Item -LiteralPath $stagingDir -Recurse -Force
}
if (Test-Path $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

New-Item -ItemType Directory -Path $stagingDir -Force | Out-Null
Copy-Item -Path (Join-Path $releaseDir "*") -Destination $stagingDir -Recurse -Force

$portableReadme = @"
Pizza Express New Zealand Portable Package
==========================================

This portable package contains the complete Release output required to run the app.

Launch:
- WindowsFormsApplication3.exe

Local data:
- %APPDATA%\PizzaExpress\orders.db
- %APPDATA%\PizzaExpress\Logs\

Notes:
- No installer is required.
- .NET Framework 4.8 must be available on the target machine.
"@

Set-Content -LiteralPath (Join-Path $stagingDir "PORTABLE-README.txt") -Value $portableReadme -Encoding UTF8
Compress-Archive -LiteralPath $stagingDir -DestinationPath $zipPath -Force

Write-Host ("Portable package created: {0}" -f $zipPath)
Write-Host ("PACKAGE_PATH={0}" -f $zipPath)
Write-Host ("PACKAGE_DIR={0}" -f $stagingDir)

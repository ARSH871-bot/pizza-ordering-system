param(
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',
    [string]$ResultsDirectory = '',
    [string]$LogFileName = 'results.trx',
    [string]$TestCaseFilter = '',
    [switch]$CollectCoverage,
    [string]$CoverageOutput = ''
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($ResultsDirectory)) {
    $ResultsDirectory = Join-Path $RepoRoot 'TestResults'
}

function Get-VsTestConsolePath {
    $vswhereCandidates = @(
        (Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'),
        (Join-Path $env:ProgramFiles 'Microsoft Visual Studio\Installer\vswhere.exe')
    ) | Where-Object { $_ -and (Test-Path $_) }

    foreach ($vswhere in $vswhereCandidates) {
        $installPath = & $vswhere -latest -products * -requires Microsoft.VisualStudio.PackageGroup.TestTools.Core -property installationPath 2>$null
        if ($LASTEXITCODE -eq 0 -and $installPath) {
            $candidate = Join-Path $installPath 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'
            if (Test-Path $candidate) {
                return $candidate
            }
        }
    }

    $fallback = Get-ChildItem (Join-Path $env:ProgramFiles 'Microsoft Visual Studio') -Recurse -Filter 'vstest.console.exe' -ErrorAction SilentlyContinue |
        Sort-Object FullName |
        Select-Object -First 1 -ExpandProperty FullName

    if ($fallback) {
        return $fallback
    }

    throw 'Could not locate vstest.console.exe.'
}

$testAssembly = Join-Path $RepoRoot "PizzaExpress.Tests\bin\$Configuration\net48\PizzaExpress.Tests.dll"
if (-not (Test-Path $testAssembly)) {
    throw "Test assembly not found: $testAssembly. Build the test project first."
}

New-Item -ItemType Directory -Path $ResultsDirectory -Force | Out-Null

$vstest = Get-VsTestConsolePath
$arguments = @(
    $testAssembly,
    "/ResultsDirectory:$ResultsDirectory",
    "/Logger:trx;LogFileName=$LogFileName"
)

if (-not [string]::IsNullOrWhiteSpace($TestCaseFilter)) {
    $arguments += "/TestCaseFilter:$TestCaseFilter"
}

Push-Location $RepoRoot
try {
    if ($CollectCoverage) {
        $dotnetCoverage = (Get-Command 'dotnet-coverage' -ErrorAction SilentlyContinue)
        if (-not $dotnetCoverage) {
            throw 'dotnet-coverage not found. Install with: dotnet tool install --global dotnet-coverage'
        }
        if ([string]::IsNullOrWhiteSpace($CoverageOutput)) {
            $CoverageOutput = Join-Path $ResultsDirectory 'coverage.xml'
        }
        & dotnet-coverage collect --output $CoverageOutput --output-format cobertura -- $vstest @arguments
        if ($LASTEXITCODE -ne 0) {
            throw "Test run with coverage collection exited with code $LASTEXITCODE."
        }
        Write-Host "Coverage report: $CoverageOutput"
    } else {
        & $vstest @arguments
        if ($LASTEXITCODE -ne 0) {
            throw "vstest.console.exe exited with code $LASTEXITCODE."
        }
    }
}
finally {
    Pop-Location
}

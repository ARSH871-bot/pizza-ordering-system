param(
    [Parameter(Mandatory)]
    [string]$CoverageXml,
    [string]$PackageFilter = 'WindowsFormsApplication3',
    [double]$MinLineRate = 0.75
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $CoverageXml)) {
    throw "Coverage file not found: $CoverageXml"
}

[xml]$doc = Get-Content $CoverageXml

$package = $doc.coverage.packages.package |
    Where-Object { $_.name -like "*$PackageFilter*" } |
    Select-Object -First 1

if (-not $package) {
    throw "No package matching '$PackageFilter' found in $CoverageXml"
}

$rate    = [double]$package.'line-rate'
$pct     = [math]::Round($rate * 100, 1)
$minPct  = [math]::Round($MinLineRate * 100, 1)

Write-Host "Coverage ($($package.name)): $pct%  (threshold: $minPct%)"

if ($rate -lt $MinLineRate) {
    Write-Error "COVERAGE GATE FAILED: $pct% is below the $minPct% threshold for '$($package.name)'."
    exit 1
}

Write-Host "Coverage gate passed."

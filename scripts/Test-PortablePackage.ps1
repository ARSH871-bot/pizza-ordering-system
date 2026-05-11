param(
    [Parameter(Mandatory = $true)]
    [string]$PackagePath,
    [int]$TimeoutSeconds = 8
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $PackagePath)) {
    throw "Portable package not found at '$PackagePath'."
}

$extractRoot = Join-Path $env:TEMP ("pizzaexpress_portable_smoke_" + [guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $extractRoot -Force | Out-Null

try {
    Expand-Archive -LiteralPath $PackagePath -DestinationPath $extractRoot -Force
    $exe = Get-ChildItem -Path $extractRoot -Recurse -Filter "WindowsFormsApplication3.exe" |
        Select-Object -First 1 -ExpandProperty FullName

    if ([string]::IsNullOrWhiteSpace($exe)) {
        throw "Packaged executable was not found after extracting '$PackagePath'."
    }

    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = $exe
    $psi.WorkingDirectory = Split-Path -Parent $exe
    $psi.UseShellExecute = $false

    $proc = [System.Diagnostics.Process]::Start($psi)
    if ($proc -eq $null) {
        throw "Failed to start packaged executable."
    }

    if ($proc.WaitForExit($TimeoutSeconds * 1000)) {
        throw ("Packaged executable exited early with code {0}." -f $proc.ExitCode)
    }

    try {
        $proc.Kill()
        $proc.WaitForExit()
    }
    catch {
    }

    Write-Host ("Portable package smoke test passed: {0}" -f $PackagePath)
}
finally {
    if (Test-Path $extractRoot) {
        Remove-Item -LiteralPath $extractRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}

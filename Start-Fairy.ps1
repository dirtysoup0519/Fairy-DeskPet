$ErrorActionPreference = 'Stop'
$petExe = Join-Path $PSScriptRoot 'FairyPet.exe'
if (-not (Test-Path -LiteralPath $petExe)) { & (Join-Path $PSScriptRoot 'build.ps1') }
Start-Process -FilePath $petExe -WorkingDirectory $PSScriptRoot

$ErrorActionPreference='Stop'
$petRoot=Split-Path $PSScriptRoot -Parent
& (Join-Path $petRoot 'build.ps1')
if($LASTEXITCODE -ne 0){throw 'Build failed.'}
$petExe=Join-Path $petRoot 'FairyPet.exe'
$petPreferences=Join-Path $env:LOCALAPPDATA 'CodexFairyPet\preferences.json'
$petBefore=if(Test-Path -LiteralPath $petPreferences){(Get-FileHash -LiteralPath $petPreferences).Hash}else{$null}
foreach($petMode in @('--self-test','--render-ui')){
    $petProcess=Start-Process -FilePath $petExe -ArgumentList $petMode -Wait -PassThru -WindowStyle Hidden
    if($petProcess.ExitCode -ne 0){throw "Test failed: $petMode (exit $($petProcess.ExitCode))"}
}
$petAfter=if(Test-Path -LiteralPath $petPreferences){(Get-FileHash -LiteralPath $petPreferences).Hash}else{$null}
if($petBefore -ne $petAfter){throw 'User preferences changed during UI tests.'}
& (Join-Path $PSScriptRoot 'check-repository.ps1')
& (Join-Path $PSScriptRoot 'check-docs.ps1')
Write-Output 'Build, self-test, UI rendering, preference preservation and repository checks passed.'

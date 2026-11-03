$ErrorActionPreference = 'Stop'
$petRoot = Split-Path $PSScriptRoot -Parent
$petData = Join-Path $env:LOCALAPPDATA 'CodexFairyPet'
New-Item -ItemType Directory -Force -Path $petData | Out-Null
function Get-FairyHealth {
    try { $petHealth=Invoke-RestMethod -Uri 'http://127.0.0.1:9880/fairy/health' -TimeoutSec 2; if($petHealth.service -eq 'dsh-fairy-tts' -and $petHealth.ready){return $petHealth} } catch {}
    return $null
}
function Resolve-PetPath([string]$value) {
    if([string]::IsNullOrWhiteSpace($value)){throw 'Missing runtime path; configure runtime.local.json first.'}
    if([IO.Path]::IsPathRooted($value)){return [IO.Path]::GetFullPath($value)}
    return [IO.Path]::GetFullPath((Join-Path $petRoot $value))
}
if(Get-FairyHealth){exit 0}
$petSettingsPath=Join-Path $petRoot 'runtime.local.json'
if(-not (Test-Path -LiteralPath $petSettingsPath)){throw 'TTS is not configured. See docs/VOICE_SETUP.md and runtime.example.json.'}
$petSettings=Get-Content -LiteralPath $petSettingsPath -Raw | ConvertFrom-Json
if($petSettings.LauncherScript){
    $petDelegate=Resolve-PetPath $petSettings.LauncherScript
    if($petDelegate -eq $PSCommandPath){throw 'LauncherScript cannot reference itself.'}
    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $petDelegate
    exit $LASTEXITCODE
}
$petPython=Resolve-PetPath $petSettings.PythonPath
$petSource=Resolve-PetPath $petSettings.SourceRoot
$petConfig=Resolve-PetPath $petSettings.TtsConfigPath
foreach($petRequired in @($petPython,$petSource,$petConfig)){if(-not(Test-Path -LiteralPath $petRequired)){throw "Required runtime path missing: $petRequired"}}
$petMutex=New-Object System.Threading.Mutex($false,'Local\DSHFairyTTS-9880')
$petLocked=$false
try {
    try {$petLocked=$petMutex.WaitOne(210000)} catch [System.Threading.AbandonedMutexException] {$petLocked=$true}
    if(-not $petLocked){throw 'Timed out waiting for TTS startup lock.'}
    if(Get-FairyHealth){exit 0}
    if(Get-NetTCPConnection -LocalPort 9880 -State Listen -ErrorAction SilentlyContinue){throw 'Port 9880 is occupied; existing process left running.'}
    $env:FAIRY_RUNTIME_SOURCE=$petSource
    $env:TORCH_FORCE_NO_WEIGHTS_ONLY_LOAD='1'
    $env:PYTHONIOENCODING='utf-8'
    $env:PYTHONPATH="$petSource;$(Join-Path $petSource 'GPT_SoVITS')"
    $env:NLTK_DATA=Join-Path $PSScriptRoot 'private\nltk_data'
    $env:OMP_NUM_THREADS='4'
    $env:version='v2'
    $petServer=Join-Path $PSScriptRoot 'serve_fairy.py'
    $petArguments=@('-u',('"'+$petServer+'"'),'-a','127.0.0.1','-p','9880','-c',('"'+$petConfig+'"'))
    $petProcess=Start-Process -FilePath $petPython -ArgumentList $petArguments -WorkingDirectory $petSource -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $petData 'tts-server.log') -RedirectStandardError (Join-Path $petData 'tts-server-error.log')
    $petDeadline=[DateTime]::UtcNow.AddSeconds(180)
    do {if(Get-FairyHealth){exit 0};$petProcess.Refresh();if($petProcess.HasExited){throw 'TTS exited; check %LOCALAPPDATA%/CodexFairyPet/tts-server-error.log.'};Start-Sleep -Seconds 2} while([DateTime]::UtcNow -lt $petDeadline)
    throw 'TTS startup timed out.'
} finally {if($petLocked){$petMutex.ReleaseMutex()};$petMutex.Dispose()}

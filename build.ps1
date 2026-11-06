param([string]$OutputName = 'FairyPet.exe', [switch]$UiCheck)
$ErrorActionPreference = 'Stop'
if ([IO.Path]::GetFileName($OutputName) -ne $OutputName -or -not $OutputName.EndsWith('.exe')) { throw 'OutputName must be an executable filename' }
$petRoot = $PSScriptRoot
foreach ($petAsset in @('assets\NOTICE','assets\fairy-persona.txt','assets\fairy.ico','assets\frames\000.png','chat-workspace\AGENTS.md')) {
    if (-not (Test-Path -LiteralPath (Join-Path $petRoot $petAsset))) { throw "Required repository asset missing: $petAsset" }
}
$petFramework = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
$petWpf = Join-Path $petFramework 'WPF'
$petReferences = @('System.dll','System.Core.dll','System.Security.dll','System.Web.Extensions.dll','System.Windows.Forms.dll','System.Drawing.dll','System.Xaml.dll') | ForEach-Object { '/reference:' + (Join-Path $petFramework $_) }
$petReferences += @('WindowsBase.dll','PresentationCore.dll','PresentationFramework.dll') | ForEach-Object { '/reference:' + (Join-Path $petWpf $_) }
$petSources = @('FairyPet.cs','PetUI.cs','QuickChat.cs','HddMotion.cs','WebChat.cs','WebUI.cs','IdleVoice.cs','ResponsiveDock.cs','RuntimePaths.cs','FirstRunChecks.cs','ScreenCapture.cs','ScreenAssist.cs','ScreenUi.cs','ScreenChecks.cs','ScreenMemory.cs','GameCompanion.cs','GameObservation.cs','GameLibrary.cs','ChatTasks.cs') | ForEach-Object { Join-Path $petRoot $_ }
$petDefines = @(); if ($UiCheck) { $petDefines += '/define:UI_CHECK' }
& (Join-Path $petFramework 'csc.exe') /nologo /target:winexe /optimize+ /platform:x64 /win32icon:"$petRoot\assets\fairy.ico" /out:"$petRoot\$OutputName" @petDefines @petReferences @petSources
if ($LASTEXITCODE -ne 0) { throw 'FairyPet compilation failed' }
Write-Output "Built $petRoot\$OutputName"

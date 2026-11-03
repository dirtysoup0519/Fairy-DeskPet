param([Parameter(Mandatory=$true)][string]$SourceDirectory)
$ErrorActionPreference='Stop'
$petRoot=Split-Path $PSScriptRoot -Parent
$petSource=[IO.Path]::GetFullPath($SourceDirectory)
$petCatalogPath=Join-Path $petSource 'catalog.json'
$petCatalog=Get-Content -LiteralPath $petCatalogPath -Raw -Encoding UTF8 | ConvertFrom-Json
if(-not $petCatalog.Clips){throw 'The source catalog must contain at least one clip.'}
$petIds=@{}
foreach($petClip in $petCatalog.Clips){
    if($petClip.Id -notmatch '^[a-z0-9][a-z0-9-]*$' -or $petIds.ContainsKey($petClip.Id)){throw 'Invalid or duplicate clip ID.'}
    $petIds[$petClip.Id]=$true
    $petPcm=Join-Path $petSource ($petClip.Id+'.pcm')
    if((Get-FileHash -LiteralPath $petPcm -Algorithm SHA256).Hash -ne $petClip.Sha256){throw "Checksum mismatch: $($petClip.Id)"}
    $petBytes=(Get-Item -LiteralPath $petPcm).Length
    if($petBytes%2 -ne 0 -or [Math]::Abs($petBytes/64000.0-$petClip.Duration) -gt .001){throw "Duration mismatch: $($petClip.Id)"}
    if(-not $petClip.Cues -or (($petClip.Cues | ForEach-Object {$_.Text}) -join '') -ne $petClip.Text){throw "Incomplete captions: $($petClip.Id)"}
}
$petTarget=Join-Path $petRoot 'assets\idle-original'
if(Test-Path -LiteralPath $petTarget){
    $petBackup=Join-Path $petRoot ('runtime\private\import-backups\'+[DateTime]::Now.ToString('yyyyMMdd-HHmmss')+'-'+[Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Force -Path $petBackup | Out-Null
    Copy-Item -Path (Join-Path $petTarget '*') -Destination $petBackup -Recurse
}
New-Item -ItemType Directory -Force -Path $petTarget | Out-Null
foreach($petClip in $petCatalog.Clips){Copy-Item -LiteralPath (Join-Path $petSource ($petClip.Id+'.pcm')) -Destination $petTarget}
Copy-Item -LiteralPath $petCatalogPath -Destination $petTarget
Write-Output "Imported $($petCatalog.Clips.Count) local-only clips. assets/idle-original/ is excluded from Git."

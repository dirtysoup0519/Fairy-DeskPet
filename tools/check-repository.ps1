param([string]$Root=(Split-Path $PSScriptRoot -Parent))
$ErrorActionPreference='Stop'
$petRoot=[IO.Path]::GetFullPath($Root)
$petPaths=@(& git -C $petRoot ls-files --cached --others --exclude-standard) | Sort-Object -Unique
if($LASTEXITCODE -ne 0){throw 'Cannot inspect Git candidate files.'}
$petErrors=New-Object System.Collections.Generic.List[string]
$petBytes=0
foreach($petRelative in $petPaths){
    $petNormalized=$petRelative.Replace('\','/')
    if($petNormalized -match '(?i)(^|/)(screen-cache|screen-captures|screenshots|captures)(/|$)|(^|/)screen-memory\.dat' -or ($petNormalized -match '(?i)\.png$' -and $petNormalized -notmatch '^assets/frames/[0-9]{3}\.png$')){$petErrors.Add("Unreviewed screenshot or private memory: $petRelative");continue}
    if($petNormalized -match '(?i)(^|/)(local|private|backups|\.vscode|\.idea|\.codex|\.dsh|\.workbuddy|automations?|scheduled-tasks?|task-scheduler|scheduler|tasks-local|tasks|task-exports|scheduler-exports|automation-state|cron\.d|plans|memory|sessions)(/|$)' -or
       $petNormalized -match '(?i)(^|/)(tasks\.json|launch\.json|crontab(?:\..*)?)$|\.(task|job|ics)$|\.local\.' -or
       $petNormalized -match '(?i)(scheduled[-_]?tasks?|task[-_]?scheduler|schtasks|计划任务|任务计划).*(\.(xml|json|toml|ya?ml|csv|txt|ps1|bat|cmd))$' -or
       $petNormalized -match '(?i)\.(crontab|lnk|url|reg|rdp|jsonl)$|(^|/)automation[^/]*\.(toml|json)$' -or
       $petNormalized -match '^docs/(COMMIT_PLAN\.md|FILES_TO_COMMIT\.txt)$|^(AGENTS|PLAN|MEMORY)\.md$'){
        $petErrors.Add("Local-only automation or planning file: $petRelative");continue
    }
    # Check tracked/staged files too: git add -f cannot bypass this check.
    if($petNormalized -match '(?i)(^|/)(models?|weights?|checkpoints?|datasets?|voice-materials|voice-training|pretrained_models|venv|\.venv|node_modules)(/|$)' -or
       $petNormalized -match '(?i)^(assets/(original|idle-original)|runtime/(private|GPT-SoVITS|venv)|preview)/' -or
       $petNormalized -match '(?i)\.(pth|pt|ckpt|safetensors|onnx|bin|th|tflite|gguf|ggml|model|pb|ot|f16|f32|h5|hdf5|pkl|pickle|joblib|npy|npz|wav|pcm|mp3|m4a|mp4|flac|ogg|zip|7z|tar|gz|exe|dll|pdb|pem|key|pfx)$' -or
       $petNormalized -match '(?i)(^|/)(\.env(?:\..*)?|auth\.json|preferences\.json)$|\.local\.(json|ya?ml)$'){
        $petErrors.Add("Forbidden candidate: $petRelative");continue
    }
    $petPath=Join-Path $petRoot $petRelative
    if(-not(Test-Path -LiteralPath $petPath -PathType Leaf)){continue}
    $petFile=Get-Item -LiteralPath $petPath
    $petBytes+=$petFile.Length
    if($petFile.Length -gt 10MB){$petErrors.Add("Unexpected large file: $petRelative")}
    if($petFile.Extension -ne '.png'){
        $petText=[IO.File]::ReadAllText($petPath)
        if($petText -match '(?:sk-[A-Za-z0-9_-]{24,}|gh[pousr]_[A-Za-z0-9]{24,}|github_pat_[A-Za-z0-9_]{24,}|hf_[A-Za-z0-9]{24,}|-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----)'){$petErrors.Add("Possible credential: $petRelative")}
        if($petText -match '(?i)(?<!file:///)\b[A-Z]:[\\/]+(?:[A-Za-z0-9_.-]+[\\/])+' -or $petText -match '/(?:h[o]me|U[s]ers)/[^\s"<>]+'){$petErrors.Add("Machine-specific absolute path: $petRelative")}
        if($petText -match '(?is)<Task\b[^>]*xmlns\s*=\s*["'']http://schemas\.microsoft\.com/windows/2004/02/mit/task' -or
           $petText -match '(?m)^\s*(?:rrule|targetThreadId)\s*=' -or
           $petText -match '(?i)"rrule"\s*:\s*"(?:RRULE:)?FREQ=' -or
           $petText -match '\bS-1-5-21-\d{8,}'){$petErrors.Add("Scheduler export or automation state: $petRelative")}
    }
}
if($petErrors.Count){$petErrors | ForEach-Object {Write-Output $_};throw 'Repository check failed. No file was changed or uploaded.'}
Write-Output ("PASS: {0} candidate files, {1:N2} MiB; no model weights, audio, credentials, local scheduler exports or personal paths." -f $petPaths.Count,($petBytes/1MB))

$ErrorActionPreference='Stop'
$petRoot=Split-Path $PSScriptRoot -Parent
$petFiles=@(& git -C $petRoot ls-files --cached --others --exclude-standard) | Sort-Object -Unique
if($LASTEXITCODE -ne 0){throw 'Cannot inspect documentation candidates.'}
$petUtf8=New-Object System.Text.UTF8Encoding($false,$true)
$petDocuments=0
$petLinks=0
foreach($petRelative in $petFiles){
    if(-not $petRelative.EndsWith('.md')){continue}
    $petPath=Join-Path $petRoot $petRelative
    $petText=[IO.File]::ReadAllText($petPath,$petUtf8)
    if($petText.Contains([char]0xfffd)){throw "Replacement character in document: $petRelative"}
    foreach($petMatch in [regex]::Matches($petText,'(?<!!)\[[^\]]+\]\((?<target>[^)\r\n]+)\)')){
        $petTarget=$petMatch.Groups['target'].Value.Trim().Trim([char[]]'<>')
        if($petTarget -match '^[a-zA-Z][a-zA-Z0-9+.-]*:' -or $petTarget.StartsWith('#')){continue}
        $petTarget=[Uri]::UnescapeDataString(($petTarget -split '#',2)[0])
        $petResolved=[IO.Path]::GetFullPath((Join-Path (Split-Path $petPath -Parent) $petTarget))
        if(-not $petResolved.StartsWith($petRoot+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw "External local path in document: $petRelative"}
        $petRepoTarget=$petResolved.Substring($petRoot.Length+1).Replace('\','/')
        if(-not(Test-Path -LiteralPath $petResolved -PathType Leaf) -or $petFiles -notcontains $petRepoTarget){throw "Missing or unpublished document target: $petRelative -> $petTarget"}
        $petLinks++
    }
    $petDocuments++
}
Write-Output "PASS: $petDocuments UTF-8 Markdown documents; $petLinks local links resolve to public candidates."

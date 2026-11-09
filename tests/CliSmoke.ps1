$ErrorActionPreference = 'Stop'
$dist = Join-Path $PSScriptRoot '..\dist'
$release = Get-ChildItem -LiteralPath $dist -Directory -Filter 'RestoryItalianPatch-*' | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $release) { throw 'Build the CLI with build.ps1 before running the CLI smoke test.' }
$exe = Join-Path $release.FullName 'RestoryItalianPatch.exe'
if (-not (Test-Path -LiteralPath $exe)) { throw 'The latest release folder does not contain the CLI executable.' }
$root = Join-Path $PSScriptRoot ('..\work\cli-' + [Guid]::NewGuid().ToString('N'))
$game = Join-Path $root 'steamapps\common\Restory'
$steamApps = Join-Path $root 'steamapps'
$payload = Join-Path $root 'payload'
New-Item -ItemType Directory -Force -Path $game, (Join-Path $payload 'BepInEx\plugins\RestoryItalianPatch') | Out-Null
try {
    Set-Content -NoNewline -Path (Join-Path $game 'Restory.exe') -Value 'exe'
    Set-Content -Path (Join-Path $steamApps 'appmanifest_999999.acf') -Value "`"AppState`"`n{`n  `"installdir`" `"Restory`"`n  `"buildid`" `"26253677`"`n}"
    Set-Content -NoNewline -Path (Join-Path $payload 'BepInEx\plugins\RestoryItalianPatch\translation.dat') -Value 'Italian patch payload'
    & $exe install $game $payload
    if ($LASTEXITCODE -ne 0) { throw "CMD install returned $LASTEXITCODE." }
    & $exe install $game $payload
    if ($LASTEXITCODE -ne 0) { throw "Repeated install returned $LASTEXITCODE." }
    if ((Get-Content -Raw (Join-Path $game 'BepInEx\plugins\RestoryItalianPatch\translation.dat')) -ne 'Italian patch payload') { throw 'Install payload is missing.' }
    & $exe uninstall $game
    if ($LASTEXITCODE -ne 0) { throw "CMD uninstall returned $LASTEXITCODE." }
    if (Test-Path -LiteralPath (Join-Path $game 'BepInEx\plugins\RestoryItalianPatch\translation.dat')) { throw 'Uninstall left the patch file behind.' }

    $unsupported = Join-Path $root 'steamapps\common\UnsupportedRestory'
    New-Item -ItemType Directory -Force -Path $unsupported | Out-Null
    Set-Content -NoNewline -Path (Join-Path $unsupported 'Restory.exe') -Value 'exe'
    Set-Content -Path (Join-Path $steamApps 'appmanifest_999998.acf') -Value "`"AppState`"`n{`n  `"installdir`" `"UnsupportedRestory`"`n  `"buildid`" `"1`"`n}"
    & $exe install $unsupported $payload
    if ($LASTEXITCODE -ne 3) { throw "Unsupported build returned $LASTEXITCODE instead of 3." }
    if (Test-Path -LiteralPath (Join-Path $unsupported '.RestoryItalianPatch')) { throw 'Unsupported build created installer state.' }
    Write-Host 'PASS: CLI install, repeat install, uninstall, and unsupported-build rejection'
}
finally {
    if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force }
}

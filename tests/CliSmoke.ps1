$ErrorActionPreference = 'Stop'
$dist = Join-Path $PSScriptRoot '..\dist'
$release = Get-ChildItem -LiteralPath $dist -Directory -Filter 'RestoryItalianPatch-*' | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $release) { throw 'Build the CLI with build.ps1 before running the CLI smoke test.' }
$exe = Join-Path $release.FullName 'RestoryItalianPatch.exe'
if (-not (Test-Path -LiteralPath $exe)) { throw 'The latest release folder does not contain the CLI executable.' }
$root = Join-Path $env:TEMP ('restory-cli-' + [Guid]::NewGuid().ToString('N'))
$steamApps = Join-Path $root 'steamapps'
$payload = Join-Path $root 'payload'
New-Item -ItemType Directory -Force -Path $steamApps, (Join-Path $payload 'BepInEx\plugins\RestoryItalianPatch') | Out-Null
try {
    Set-Content -NoNewline -Path (Join-Path $payload 'BepInEx\plugins\RestoryItalianPatch\translation.dat') -Value 'Italian patch payload'

    foreach ($buildId in @('26253677', '26622427')) {
        $game = Join-Path $root ("steamapps\common\Restory-$buildId")
        New-Item -ItemType Directory -Force -Path $game | Out-Null
        Set-Content -NoNewline -Path (Join-Path $game 'Restory.exe') -Value 'exe'
        Set-Content -Path (Join-Path $steamApps "appmanifest_$buildId.acf") -Value "`"AppState`"`n{`n  `"installdir`" `"Restory-$buildId`"`n  `"buildid`" `"$buildId`"`n}"
        & $exe install $game $payload
        if ($LASTEXITCODE -ne 0) { throw "CLI install for build $buildId returned $LASTEXITCODE." }
        & $exe install $game $payload
        if ($LASTEXITCODE -ne 0) { throw "Repeated CLI install for build $buildId returned $LASTEXITCODE." }
        if ((Get-Content -Raw (Join-Path $game 'BepInEx\plugins\RestoryItalianPatch\translation.dat')) -ne 'Italian patch payload') { throw "Install payload is missing for build $buildId." }
        & $exe uninstall $game
        if ($LASTEXITCODE -ne 0) { throw "CLI uninstall for build $buildId returned $LASTEXITCODE." }
        if (Test-Path -LiteralPath (Join-Path $game 'BepInEx\plugins\RestoryItalianPatch\translation.dat')) { throw "CLI uninstall left the patch file behind for build $buildId." }

        # Exercise the shipped CMD wrappers against the same disposable fixture only.
        $cmdRoot = Join-Path $root "cmd-$buildId"
        $cmdGame = Join-Path $cmdRoot "steamapps\common\Restory-$buildId"
        $cmdSteamApps = Join-Path $cmdRoot 'steamapps'
        New-Item -ItemType Directory -Force -Path $cmdGame, $cmdSteamApps | Out-Null
        Set-Content -NoNewline -Path (Join-Path $cmdGame 'Restory.exe') -Value 'exe'
        Set-Content -Path (Join-Path $cmdSteamApps "appmanifest_$buildId.acf") -Value "`"AppState`"`n{`n  `"installdir`" `"Restory-$buildId`"`n  `"buildid`" `"$buildId`"`n}"
        $installCmd = Join-Path $release.FullName 'install.cmd'
        $uninstallCmd = Join-Path $release.FullName 'uninstall.cmd'
        & $env:ComSpec /d /c "call `"$installCmd`" `"$cmdGame`" < NUL"
        if ($LASTEXITCODE -ne 0) { throw "CMD wrapper install for build $buildId returned $LASTEXITCODE." }
        if (-not (Test-Path -LiteralPath (Join-Path $cmdGame 'BepInEx\plugins\RestoryItalianPatch\RestoryItalianPatch.dll'))) { throw "CMD wrapper did not install the release payload for build $buildId." }
        & $env:ComSpec /d /c "call `"$uninstallCmd`" `"$cmdGame`" < NUL"
        if ($LASTEXITCODE -ne 0) { throw "CMD wrapper uninstall for build $buildId returned $LASTEXITCODE." }
        if (Test-Path -LiteralPath (Join-Path $cmdGame 'BepInEx\plugins\RestoryItalianPatch\RestoryItalianPatch.dll')) { throw "CMD wrapper uninstall left the patch file behind for build $buildId." }
    }

    $unsupported = Join-Path $root 'steamapps\common\UnsupportedRestory'
    New-Item -ItemType Directory -Force -Path $unsupported | Out-Null
    Set-Content -NoNewline -Path (Join-Path $unsupported 'Restory.exe') -Value 'exe'
    Set-Content -Path (Join-Path $steamApps 'appmanifest_999998.acf') -Value "`"AppState`"`n{`n  `"installdir`" `"UnsupportedRestory`"`n  `"buildid`" `"1`"`n}"
    & $exe install $unsupported $payload
    if ($LASTEXITCODE -ne 3) { throw "Unsupported build returned $LASTEXITCODE instead of 3." }
    if (Test-Path -LiteralPath (Join-Path $unsupported '.RestoryItalianPatch')) { throw 'Unsupported build created installer state.' }
    Write-Host 'PASS: CLI and CMD install/uninstall for builds 26253677 and 26622427; unsupported-build rejection'
}
finally {
    if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force }
}

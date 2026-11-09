$ErrorActionPreference = 'SilentlyContinue'
$steamRoots = [System.Collections.Generic.List[string]]::new()

foreach ($keyPath in @(
    'HKCU:\Software\Valve\Steam',
    'HKLM:\SOFTWARE\WOW6432Node\Valve\Steam',
    'HKLM:\SOFTWARE\Valve\Steam'
)) {
    $key = Get-ItemProperty -LiteralPath $keyPath
    foreach ($valueName in @('SteamPath', 'InstallPath')) {
        $value = $key.$valueName
        if ($value -and (Test-Path -LiteralPath $value)) { $steamRoots.Add(($value -replace '/', '\')) }
    }
}

foreach ($commonRoot in @(
    (Join-Path ${env:ProgramFiles(x86)} 'Steam'),
    (Join-Path $env:ProgramFiles 'Steam')
)) {
    if ($commonRoot -and (Test-Path -LiteralPath $commonRoot)) { $steamRoots.Add($commonRoot) }
}

foreach ($drive in (Get-PSDrive -PSProvider FileSystem)) {
    foreach ($relativeRoot in @('Steam', 'Program Files (x86)\Steam', 'Program Files\Steam')) {
        $possibleRoot = Join-Path $drive.Root $relativeRoot
        if (Test-Path -LiteralPath (Join-Path $possibleRoot 'steamapps\libraryfolders.vdf')) { $steamRoots.Add($possibleRoot) }
    }
}

$libraries = [System.Collections.Generic.List[string]]::new()
foreach ($root in ($steamRoots | Select-Object -Unique)) {
    $libraries.Add($root)
    $libraryFile = Join-Path $root 'steamapps\libraryfolders.vdf'
    if (-not (Test-Path -LiteralPath $libraryFile)) { continue }
    $contents = Get-Content -LiteralPath $libraryFile -Raw
    foreach ($match in [regex]::Matches($contents, '(?m)^\s*"path"\s+"([^"]+)"')) {
        $libraryPath = $match.Groups[1].Value.Replace('\\', '\')
        if ($libraryPath) { $libraries.Add($libraryPath) }
    }
    foreach ($match in [regex]::Matches($contents, '(?m)^\s*"\d+"\s+"([^"]+)"')) {
        $libraryPath = $match.Groups[1].Value.Replace('\\', '\')
        if ($libraryPath) { $libraries.Add($libraryPath) }
    }
}

foreach ($library in ($libraries | Select-Object -Unique)) {
    $steamApps = Join-Path $library 'steamapps'
    $gamePath = Join-Path $steamApps 'common\Restory'
    if (-not (Test-Path -LiteralPath (Join-Path $gamePath 'Restory.exe'))) { continue }
    foreach ($manifest in (Get-ChildItem -LiteralPath $steamApps -Filter 'appmanifest_*.acf' -File)) {
        $manifestText = Get-Content -LiteralPath $manifest.FullName -Raw
        $installDir = [regex]::Match($manifestText, '"installdir"\s+"([^"]+)"').Groups[1].Value
        $buildId = [regex]::Match($manifestText, '"buildid"\s+"([^"]+)"').Groups[1].Value
        if ($installDir -eq 'Restory' -and $buildId -eq '26253677') {
            Write-Output $gamePath
            exit 0
        }
    }
}

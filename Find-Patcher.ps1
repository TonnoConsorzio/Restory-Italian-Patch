param([string] $BuildFolder = (Join-Path $PSScriptRoot 'dist'))

$release = Get-ChildItem -LiteralPath $BuildFolder -Directory -Filter 'RestoryItalianPatch-*' -ErrorAction SilentlyContinue |
    Sort-Object Name -Descending |
    ForEach-Object { Join-Path $_.FullName 'RestoryItalianPatch.exe' } |
    Where-Object { Test-Path -LiteralPath $_ } |
    Select-Object -First 1

if ($release) { Write-Output $release }

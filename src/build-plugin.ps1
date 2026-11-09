param(
    [Parameter(Mandatory = $true)] [string] $ManagedGameFolder,
    [Parameter(Mandatory = $true)] [string] $BepInExCoreFolder
)

$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { throw 'The .NET Framework C# compiler was not found.' }

$references = @(
    (Join-Path $BepInExCoreFolder 'BepInEx.Core.dll'),
    (Join-Path $BepInExCoreFolder 'BepInEx.Unity.Mono.dll'),
    (Join-Path $BepInExCoreFolder '0Harmony.dll'),
    (Join-Path $ManagedGameFolder 'netstandard.dll'),
    (Join-Path $ManagedGameFolder 'UnityEngine.dll'),
    (Join-Path $ManagedGameFolder 'UnityEngine.CoreModule.dll')
)
foreach ($reference in $references) {
    if (-not (Test-Path -LiteralPath $reference)) { throw "Required build reference is missing: $reference" }
}

$projectRoot = Split-Path $PSScriptRoot -Parent
$output = Join-Path $projectRoot 'payload\BepInEx\plugins\RestoryItalianPatch\RestoryItalianPatch.dll'
New-Item -ItemType Directory -Force -Path (Split-Path $output) | Out-Null
$arguments = @('/nologo', '/target:library', '/optimize+', "/out:$output")
$arguments += $references | ForEach-Object { "/reference:$_" }
$arguments += @((Join-Path $PSScriptRoot 'TranslationCatalog.cs'), (Join-Path $PSScriptRoot 'RestoryItalianPlugin.cs'))
& $compiler @arguments
if ($LASTEXITCODE -ne 0) { throw 'Runtime plugin compilation failed.' }
Write-Output "Built $output"

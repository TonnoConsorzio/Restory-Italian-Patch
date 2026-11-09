param(
    [Parameter(Mandatory = $true)]
    [string]$BepInExCorePath,

    [Parameter(Mandatory = $true)]
    [string]$GameManagedPath
)

$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { throw 'The .NET Framework C# compiler was not found.' }

$references = @(
    (Join-Path $BepInExCorePath 'BepInEx.Core.dll'),
    (Join-Path $BepInExCorePath 'BepInEx.Unity.Mono.dll'),
    (Join-Path $BepInExCorePath '0Harmony.dll'),
    (Join-Path $GameManagedPath 'DialogueSystem.dll'),
    (Join-Path $GameManagedPath 'PixelCrushers.dll'),
    (Join-Path $GameManagedPath 'netstandard.dll'),
    (Join-Path $GameManagedPath 'UnityEngine.dll'),
    (Join-Path $GameManagedPath 'UnityEngine.CoreModule.dll')
)
foreach ($reference in $references) {
    if (-not (Test-Path -LiteralPath $reference -PathType Leaf)) { throw "Required assembly was not found: $reference" }
}

$buildDirectory = Join-Path $repo 'work\build-payload\BepInEx\plugins\RestoryItalianDialogue'
New-Item -ItemType Directory -Force -Path $buildDirectory | Out-Null
$output = Join-Path $buildDirectory 'RestoryItalianDialogue.dll'
$arguments = @('/nologo', '/target:library', '/optimize+', ('/out:' + $output))
foreach ($reference in $references) { $arguments += '/reference:' + $reference }
$arguments += (Join-Path $repo 'src\DialogueTranslationCatalog.cs')
$arguments += (Join-Path $repo 'src\DialogueTranslationRuntime.cs')
$arguments += (Join-Path $repo 'src\DialogueLocalizationPlugin.cs')
& $compiler @arguments
if ($LASTEXITCODE -ne 0) { throw 'Dialogue plugin compilation failed.' }
Write-Host "Built $output"

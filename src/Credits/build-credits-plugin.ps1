param(
    [string]$GameManaged = 'D:\Steam\steamapps\common\Restory\Restory_Data\Managed',
    [string]$BepInExCore = 'C:\Users\aless\Documents\Codex\2026-11-08\d-steam-steamapps-common-restory\work\bepinex792\BepInEx\core'
)
$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { throw 'The .NET Framework C# compiler was not found.' }
if (-not (Test-Path -LiteralPath (Join-Path $GameManaged 'Restory.Assembly.dll'))) { throw 'Restory.Assembly.dll was not found at the supported game install.' }
if (-not (Test-Path -LiteralPath (Join-Path $BepInExCore 'BepInEx.Unity.Mono.dll'))) { throw 'The BepInEx 6 Unity Mono install was not found.' }

$payload = Join-Path $PSScriptRoot '..\..\payload\BepInEx\plugins\RestoryItalianCredits'
New-Item -ItemType Directory -Force -Path $payload | Out-Null
$buildDirectory = Join-Path $PSScriptRoot '..\..\work'
New-Item -ItemType Directory -Force -Path $buildDirectory | Out-Null
$output = Join-Path $buildDirectory 'RestoryItalianCredits.dll'
$payloadAssembly = Join-Path $payload 'RestoryItalianCredits.dll'
$references = @(
    (Join-Path $BepInExCore 'BepInEx.Core.dll'),
    (Join-Path $BepInExCore 'BepInEx.Unity.Mono.dll'),
    (Join-Path $BepInExCore '0Harmony.dll'),
    (Join-Path $GameManaged 'Restory.Assembly.dll'),
    (Join-Path $GameManaged 'Zenject-usage.dll'),
    (Join-Path $GameManaged 'netstandard.dll'),
    (Join-Path $GameManaged 'UnityEngine.CoreModule.dll'),
    (Join-Path $GameManaged 'UnityEngine.IMGUIModule.dll'),
    (Join-Path $GameManaged 'UnityEngine.TextRenderingModule.dll'),
    (Join-Path $GameManaged 'UnityEngine.UI.dll'),
    (Join-Path $GameManaged 'UnityEngine.UIModule.dll'),
    (Join-Path $GameManaged 'UnityEngine.dll')
)
$referenceArgs = $references | ForEach-Object { '/reference:' + $_ }
& $compiler /nologo /target:library /nowin32manifest /optimize+ /out:$output $referenceArgs `
    (Join-Path $PSScriptRoot 'CreditOverlayDefinition.cs') `
    (Join-Path $PSScriptRoot 'RestoryItalianCreditsPlugin.cs')
if ($LASTEXITCODE -ne 0) { throw 'Credit plugin compilation failed.' }
Copy-Item -LiteralPath $output -Destination $payloadAssembly -Force
Write-Host "Built $payloadAssembly"

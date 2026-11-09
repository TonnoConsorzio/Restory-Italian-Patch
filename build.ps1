param(
    [string] $ManagedGameFolder = 'D:\Steam\steamapps\common\Restory\Restory_Data\Managed',
    [string] $BepInExCoreFolder = (Join-Path $PSScriptRoot 'payload\BepInEx\core')
)

$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { throw 'The .NET Framework C# compiler was not found.' }
$releaseName = 'RestoryItalianPatch-' + (Get-Date -Format 'yyyyMMdd-HHmmss')
$output = Join-Path $PSScriptRoot (Join-Path 'dist' $releaseName)
New-Item -ItemType Directory -Force -Path $output | Out-Null
$pluginBuilds = @(
    @{ Script = 'src\build-plugin.ps1'; Arguments = @('-ManagedGameFolder', $ManagedGameFolder, '-BepInExCoreFolder', $BepInExCoreFolder) },
    @{ Script = 'src\Credits\build-credits-plugin.ps1'; Arguments = @('-GameManaged', $ManagedGameFolder, '-BepInExCore', $BepInExCoreFolder) },
    @{ Script = 'tools\Build-DialoguePlugin.ps1'; Arguments = @('-GameManagedPath', $ManagedGameFolder, '-BepInExCorePath', $BepInExCoreFolder) }
)
foreach ($pluginBuild in $pluginBuilds) {
    & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot $pluginBuild.Script) @($pluginBuild.Arguments)
    if ($LASTEXITCODE -ne 0) { throw "Plugin build failed: $($pluginBuild.Script)" }
}
$exe = Join-Path $output 'RestoryItalianPatch.exe'
& $compiler /nologo /target:exe /optimize+ /reference:System.Xml.dll /out:$exe (Join-Path $PSScriptRoot 'src\Installer.cs') (Join-Path $PSScriptRoot 'src\Program.cs')
if ($LASTEXITCODE -ne 0) { throw 'Installer compilation failed.' }
$payload = Join-Path $PSScriptRoot 'payload'
if (Test-Path -LiteralPath $payload) {
    $packagePayload = Join-Path $output 'payload'
    New-Item -ItemType Directory -Force -Path $packagePayload | Out-Null
    Get-ChildItem -LiteralPath $payload -Force | Copy-Item -Destination $packagePayload -Recurse -Force
}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'install.cmd') -Destination $output
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'uninstall.cmd') -Destination $output
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'README-IT.txt') -Destination $output
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'THIRD-PARTY-NOTICES.md') -Destination $output
Write-Host "Built release in $output"

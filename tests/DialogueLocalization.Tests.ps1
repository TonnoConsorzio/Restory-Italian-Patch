$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$output = Join-Path $repo ('work\DialogueLocalizationTests-' + [Guid]::NewGuid().ToString('N') + '.exe')
New-Item -ItemType Directory -Force -Path (Split-Path $output) | Out-Null
& $compiler /nologo /target:exe /out:$output (Join-Path $repo 'src\DialogueTranslationCatalog.cs') (Join-Path $repo 'src\DialogueTranslationRuntime.cs') (Join-Path $repo 'tests\DialogueLocalizationTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Dialogue localization test compilation failed.' }
& $output
if ($LASTEXITCODE -ne 0) { throw 'Dialogue localization tests failed.' }
& $output (Join-Path $repo 'payload\BepInEx\plugins\RestoryItalianDialogue')
if ($LASTEXITCODE -ne 0) { throw 'Dialogue payload coverage validation failed.' }

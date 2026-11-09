$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { throw 'The .NET Framework C# compiler was not found.' }
$translationTests = Join-Path $PSScriptRoot ('work\TranslationCatalogTests-' + [Guid]::NewGuid().ToString('N') + '.exe')
New-Item -ItemType Directory -Force -Path (Split-Path $translationTests) | Out-Null
& $compiler /nologo /target:exe /reference:System.dll /out:$translationTests (Join-Path $PSScriptRoot 'src\TranslationCatalog.cs') (Join-Path $PSScriptRoot 'tests\TranslationCatalogTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Translation catalog test compilation failed.' }
& $translationTests
if ($LASTEXITCODE -ne 0) { throw 'Translation catalog tests failed.' }
$output = Join-Path $PSScriptRoot ('work\InstallerTests-' + [Guid]::NewGuid().ToString('N') + '.exe')
New-Item -ItemType Directory -Force -Path (Split-Path $output) | Out-Null
& $compiler /nologo /target:exe /reference:System.Xml.dll /out:$output (Join-Path $PSScriptRoot 'src\Installer.cs') (Join-Path $PSScriptRoot 'tests\InstallerTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Test compilation failed.' }
& $output
if ($LASTEXITCODE -ne 0) { throw 'Installer tests failed.' }
powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'tests\CreditOverlayTests.ps1')
if ($LASTEXITCODE -ne 0) { throw 'Credit overlay tests failed.' }
powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'tests\DialogueLocalization.Tests.ps1')
if ($LASTEXITCODE -ne 0) { throw 'Dialogue localization tests failed.' }
powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'tests\DialogueSourceExport.Tests.ps1')
if ($LASTEXITCODE -ne 0) { throw 'Dialogue source export tests failed.' }
powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'tests\DialogueProbeExport.Tests.ps1')
if ($LASTEXITCODE -ne 0) { throw 'Dialogue identity probe tests failed.' }
powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'build.ps1')
if ($LASTEXITCODE -ne 0) { throw 'CLI build failed.' }
powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'tests\CliSmoke.ps1')
if ($LASTEXITCODE -ne 0) { throw 'CLI smoke test failed.' }

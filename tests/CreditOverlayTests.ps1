$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { throw 'The .NET Framework C# compiler was not found.' }
$output = Join-Path $PSScriptRoot ('..\work\CreditOverlayTests-' + [Guid]::NewGuid().ToString('N') + '.exe')
New-Item -ItemType Directory -Force -Path (Split-Path $output) | Out-Null
& $compiler /nologo /target:exe /out:$output (Join-Path $PSScriptRoot '..\src\Credits\CreditOverlayDefinition.cs') (Join-Path $PSScriptRoot 'CreditOverlayTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Credit overlay test compilation failed.' }
& $output
if ($LASTEXITCODE -ne 0) { throw 'Credit overlay tests failed.' }

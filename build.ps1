$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { throw 'The .NET Framework C# compiler was not found.' }
$output = Join-Path $PSScriptRoot 'dist'
New-Item -ItemType Directory -Force -Path $output | Out-Null
$exe = Join-Path $output 'RestoryItalianPatch.exe'
& $compiler /nologo /target:exe /optimize+ /reference:System.Xml.dll /out:$exe (Join-Path $PSScriptRoot 'src\Installer.cs') (Join-Path $PSScriptRoot 'src\Program.cs')
if ($LASTEXITCODE -ne 0) { throw 'Installer compilation failed.' }
$payload = Join-Path $PSScriptRoot 'payload'
if (Test-Path -LiteralPath $payload) { Copy-Item -Recurse -Force $payload (Join-Path $output 'payload') }
Write-Host "Built $output\RestoryItalianPatch.exe"

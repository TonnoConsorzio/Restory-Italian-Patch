param(
    [Parameter(Mandatory = $true)]
    [string] $SourceCorpus,
    [string] $TranslationDirectory
)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrEmpty($TranslationDirectory)) {
    $TranslationDirectory = Join-Path $PSScriptRoot '..\payload\BepInEx\plugins\RestoryItalianPatch'
}
$corpus = Get-Content -LiteralPath $SourceCorpus -Raw -Encoding UTF8 | ConvertFrom-Json
$sourceCount = @($corpus.ui.PSObject.Properties).Count
if ($sourceCount -ne 2187) { throw "Expected 2,187 UI keys for build 26622427, found $sourceCount." }

$coverageScript = Join-Path $PSScriptRoot 'TranslationMapCoverage.ps1'
$coverageOutput = & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $coverageScript -SourceCorpus $SourceCorpus -TranslationDirectory $TranslationDirectory 2>&1 | Out-String
if ($LASTEXITCODE -ne 0) { throw "Translation map validation failed:`n$coverageOutput" }
if ($coverageOutput -notmatch "UI coverage: $sourceCount / $sourceCount; missing: 0;") {
    throw "Expected complete coverage of all $sourceCount UI keys. Coverage report:`n$coverageOutput"
}

Write-Output "Fuzzby UI coverage test passed: $sourceCount / $sourceCount keys."

$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$temp = Join-Path ([IO.Path]::GetTempPath()) ('restory-ui-newlines-' + [Guid]::NewGuid().ToString('N'))
$translations = Join-Path $temp 'translations'
New-Item -ItemType Directory -Force -Path $translations | Out-Null
try {
    $source = Join-Path $temp 'source.json'
    '{"ui":{"EMAIL_BODY":"First\nSecond"}}' | Set-Content -LiteralPath $source -Encoding UTF8
    "EMAIL_BODY`tPrima seconda" | Set-Content -LiteralPath (Join-Path $translations 'ui-004.tsv') -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $translations 'ui.tsv') -Value '' -Encoding UTF8

    try { $output = & powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'TranslationMapCoverage.ps1') -SourceCorpus $source -TranslationDirectory $translations 2>&1 | Out-String }
    catch { $output = $_.ToString() }
    if ($LASTEXITCODE -eq 0) { throw 'A new UI translation that removed a source newline was accepted.' }
    if ($output -notmatch 'newline mismatch.*EMAIL_BODY') { throw "Expected a newline mismatch for EMAIL_BODY, got:`n$output" }
    Write-Output 'PASS: new UI translations preserve source line breaks.'
}
finally { if (Test-Path -LiteralPath $temp) { Remove-Item -LiteralPath $temp -Recurse -Force } }

$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$exporter = Join-Path $repo 'tools\Export-DialogueProbe.ps1'
$temp = Join-Path ([IO.Path]::GetTempPath()) ('RestoryDialogueProbe-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temp | Out-Null
try {
    $inputPath = Join-Path $temp 'fixture.json'
    $outputPath = Join-Path $temp 'probe.json'
    @'
{
  "story": [
    { "field": "Text", "ownerType": "DialogueFragment", "shortId": "0xA1", "technicalName": "DFr_A1", "en": "<Remarks />", "locales": { "ru": "internal note" } },
    { "field": "DisplayName", "ownerType": "DialogueFragment", "shortId": "0xB2", "technicalName": "DFr_B2", "en": "Repair the radio", "locales": {} },
    { "field": "Text", "ownerType": "Instruction", "shortId": "0xC3", "technicalName": "Inst_C3", "en": "not dialogue", "locales": {} }
  ]
}
'@ | Set-Content -LiteralPath $inputPath -Encoding utf8

    & $exporter -InputPath $inputPath -OutputPath $outputPath
    if (-not $?) { throw 'Dialogue probe exporter failed.' }
    $result = Get-Content -LiteralPath $outputPath -Raw | ConvertFrom-Json
    if ($result.records.Count -ne 2) { throw "Expected 2 dialogue records, found $($result.records.Count)." }
    if (@($result.records | Where-Object { $_.shortId -eq '0xA1' }).Count -ne 1) { throw 'Text record identity was not preserved.' }
    if (@($result.records | Where-Object { $_.shortId -eq '0xB2' }).Count -ne 1) { throw 'DisplayName record identity was not preserved.' }
    if ($result.records | Where-Object { $_.PSObject.Properties.Name -contains 'translation' }) { throw 'Exporter must not add unverified translations.' }
    if ($result.records | Where-Object { $_.playerVisibilityStatus -ne 'unverified' -or $_.runtimeMappingStatus -ne 'unverified' }) { throw 'Unverified records must remain explicitly unmapped and unconfirmed.' }
    if ($result.records | Where-Object { $_.evidenceRequired -ne 'runtime-observation' }) { throw 'Exporter must state the missing evidence.' }
}
finally {
    Remove-Item -LiteralPath $temp -Recurse -Force -ErrorAction SilentlyContinue
}

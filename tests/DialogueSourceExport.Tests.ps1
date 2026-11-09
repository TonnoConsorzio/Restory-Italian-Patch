$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$exporter = Join-Path $repo 'tools\Export-DialogueSources.ps1'
$temp = Join-Path ([IO.Path]::GetTempPath()) ('RestoryDialogueSources-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temp | Out-Null
try {
    $inputPath = Join-Path $temp 'database.json'
    $outputPath = Join-Path $temp 'sources.json'
    $coveragePath = Join-Path $temp 'coverage.tsv'
    @'
[
  {
    "conversationTitle": "Fixture/Conversation",
    "conversationID": 4,
    "entryID": 5,
    "fields": { "Dialogue Text": "texto base", "Articy Id": "0x0100000000000005", "en": "Ask how to enter", "ru": "Preguntar cómo entrar" }
  },
  {
    "conversationTitle": "Fixture/Conversation",
    "conversationID": 4,
    "entryID": 6,
    "fields": { "Dialogue Text": "\u0421\u043f\u0440\u043e\u0441\u0438\u0442\u044c \u043a\u0430\u043a \u0432\u043e\u0439\u0442\u0438", "Articy Id": "0x0100000000000006", "en": "\u0421\u043f\u0440\u043e\u0441\u0438\u0442\u044c \u043a\u0430\u043a \u0432\u043e\u0439\u0442\u0438", "ru": "\u0421\u043f\u0440\u043e\u0441\u0438\u0442\u044c \u043a\u0430\u043a \u0432\u043e\u0439\u0442\u0438" }
  },
  {
    "conversationTitle": "Fixture/Conversation",
    "conversationID": 4,
    "entryID": 7,
    "fields": { "Dialogue Text": "sin id", "en": "Unmapped text" }
  },
  {
    "conversationTitle": "Fixture/Conversation",
    "conversationID": 4,
    "entryID": 8,
    "fields": { "Dialogue Text": "Fallback text", "Articy Id": "0x0100000000000008", "en": "Fallback text" }
  }
]
'@ | Set-Content -LiteralPath $inputPath -Encoding utf8

    & $exporter -InputPath $inputPath -OutputPath $outputPath -CoverageOutputPath $coveragePath
    if (-not $?) { throw 'Dialogue source exporter failed.' }
    $result = Get-Content -LiteralPath $outputPath -Raw | ConvertFrom-Json
    if ($result.records.Count -ne 3) { throw "Expected 3 keyed source records, found $($result.records.Count)." }
    $record = $result.records[0]
    if ($record.conversationId -ne 4 -or $record.entryId -ne 5 -or $record.articyId -ne '0x0100000000000005') { throw 'Stable runtime and Articy identifiers were not preserved.' }
    if ($record.sourceText -ne 'Ask how to enter' -or $record.sourceField -ne 'en') { throw 'Exporter did not preserve the source field.' }
    if ($record.PSObject.Properties.Name -contains 'italian') { throw 'Exporter must not invent an Italian translation.' }
    if ($record.playerVisibilityStatus -ne 'unverified') { throw 'Database membership alone must not claim player visibility.' }
    if ($result.records[1].sourceLanguageStatus -ne 'contains-cyrillic-review') { throw 'Non-English fallback needs a source-language review flag.' }
    if ($result.records[2].sourceLanguageStatus -ne 'matches-base-text-review') { throw 'A source field that only repeats base text needs review.' }
    $coverage = Get-Content -LiteralPath $coveragePath -Raw
    $coverageRows = @($coverage -split "`r?`n" | Where-Object { $_ })
    if ($coverageRows.Count -ne 3 -or $coverageRows[0] -ne "4`t5`t0x0100000000000005") { throw 'Coverage export did not preserve the runtime and Articy keys.' }

    $singleInputPath = Join-Path $temp 'single.json'
    $singleOutputPath = Join-Path $temp 'single-sources.json'
    '[{"conversationID":4,"entryID":5,"fields":{"Articy Id":"0x0100000000000005","en":"One source"}}]' | Set-Content -LiteralPath $singleInputPath -Encoding utf8
    & $exporter -InputPath $singleInputPath -OutputPath $singleOutputPath
    if (-not $?) { throw 'Dialogue source exporter failed for a one-entry extract.' }
    $singleResult = Get-Content -LiteralPath $singleOutputPath -Raw | ConvertFrom-Json
    if ($singleResult.records.Count -ne 1) { throw 'Exporter must handle a one-entry JSON array.' }
}
finally {
    Remove-Item -LiteralPath $temp -Recurse -Force -ErrorAction SilentlyContinue
}

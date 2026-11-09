param(
    [Parameter(Mandatory = $true)]
    [string]$InputPath,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath,

    [Parameter(Mandatory = $false)]
    [string]$CoverageOutputPath
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $InputPath -PathType Leaf)) {
    throw "Input dialogue database extract was not found: $InputPath"
}

$database = Get-Content -LiteralPath $InputPath -Raw | ConvertFrom-Json
$entries = if ($database -is [Array]) { @($database) } elseif ($null -ne $database.entries) { @($database.entries) } elseif ($null -ne $database.records) { @($database.records) } elseif ($null -ne $database.fields) { @($database) } else { @() }

$records = @(
    $entries |
        Where-Object {
            $null -ne $_.fields -and
            $_.fields.'Articy Id' -and
            ([string]$_.fields.en).Trim().Length -gt 0 -and
            $null -ne $_.conversationID -and
            $null -ne $_.entryID
        } |
        Sort-Object conversationID, entryID |
        ForEach-Object {
            $sourceText = [string]$_.fields.en
            $sourceLanguageStatus = if ($sourceText -match '[\u0400-\u04FF]') {
                'contains-cyrillic-review'
            }
            elseif ($sourceText -ceq [string]$_.fields.'Dialogue Text') {
                'matches-base-text-review'
            }
            else {
                'non-cyrillic-review'
            }
            [ordered]@{
                conversationTitle = [string]$_.conversationTitle
                conversationId = [int]$_.conversationID
                entryId = [int]$_.entryID
                articyId = [string]$_.fields.'Articy Id'
                sourceField = 'en'
                sourceText = $sourceText
                sourceLanguageStatus = $sourceLanguageStatus
                playerVisibilityStatus = 'unverified'
                runtimeMappingStatus = 'keyed'
            }
        }
)

$result = [ordered]@{
    schemaVersion = 1
    source = 'Pixel Crushers DialogueDatabase en fields; language requires review'
    records = $records
}

$outputDirectory = Split-Path -Parent $OutputPath
if ($outputDirectory -and -not (Test-Path -LiteralPath $outputDirectory -PathType Container)) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}
$result | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $OutputPath -Encoding utf8
if ($CoverageOutputPath) {
    $coverageDirectory = Split-Path -Parent $CoverageOutputPath
    if ($coverageDirectory -and -not (Test-Path -LiteralPath $coverageDirectory -PathType Container)) {
        New-Item -ItemType Directory -Path $coverageDirectory -Force | Out-Null
    }
    $coverage = @($records | ForEach-Object { "$($_.conversationId)`t$($_.entryId)`t$($_.articyId)" })
    Set-Content -LiteralPath $CoverageOutputPath -Value $coverage -Encoding utf8
}
Write-Output "Wrote $($records.Count) keyed source records from the en field to $OutputPath"

param(
    [Parameter(Mandatory = $true)]
    [string]$InputPath,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $InputPath -PathType Leaf)) {
    throw "Input corpus was not found: $InputPath"
}

$corpus = Get-Content -LiteralPath $InputPath -Raw | ConvertFrom-Json
if ($null -eq $corpus.story) {
    throw 'Input corpus must contain a story array.'
}

$records = @(
    $corpus.story |
        Where-Object {
            $_.ownerType -eq 'DialogueFragment' -and
            $_.field -in @('Text', 'DisplayName') -and
            ($_.shortId -or $_.externalId)
        } |
        Sort-Object ownerType, shortId, field |
        ForEach-Object {
            [ordered]@{
                ownerType = [string]$_.ownerType
                field = [string]$_.field
                shortId = [string]$_.shortId
                externalId = [string]$_.externalId
                technicalName = [string]$_.technicalName
                playerVisibilityStatus = 'unverified'
                runtimeMappingStatus = 'unverified'
                evidenceRequired = 'runtime-observation'
            }
        }
)

$result = [ordered]@{
    schemaVersion = 1
    source = 'Articy story extraction; review probe only'
    records = $records
}

$outputDirectory = Split-Path -Parent $OutputPath
if ($outputDirectory -and -not (Test-Path -LiteralPath $outputDirectory -PathType Container)) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}
$result | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $OutputPath -Encoding utf8
Write-Output "Wrote $($records.Count) unverified dialogue records to $OutputPath"

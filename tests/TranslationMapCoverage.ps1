param(
    [Parameter(Mandatory = $true)]
    [string] $SourceCorpus,
    [string] $TranslationMap
)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrEmpty($TranslationMap)) {
    $TranslationMap = Join-Path $PSScriptRoot '..\payload\BepInEx\plugins\RestoryItalianPatch\ui.tsv'
}

function Unescape-MapValue([string] $Value) {
    $result = [System.Text.StringBuilder]::new()
    for ($i = 0; $i -lt $Value.Length; $i++) {
        if ($Value[$i] -ne '\' -or $i + 1 -ge $Value.Length) {
            [void] $result.Append($Value[$i])
            continue
        }
        $i++
        switch ($Value[$i]) {
            'n' { [void] $result.Append("`n") }
            'r' { [void] $result.Append("`r") }
            't' { [void] $result.Append("`t") }
            '\' { [void] $result.Append('\') }
            default { throw "Unsupported escape '\$($Value[$i])' in $TranslationMap." }
        }
    }
    return $result.ToString()
}

$corpus = Get-Content -LiteralPath $SourceCorpus -Raw -Encoding UTF8 | ConvertFrom-Json
$source = @{}
foreach ($property in $corpus.ui.PSObject.Properties) { $source[$property.Name] = [string]$property.Value }
$map = @{}
$lineNumber = 0
foreach ($line in Get-Content -LiteralPath $TranslationMap -Encoding UTF8) {
    $lineNumber++
    if ([string]::IsNullOrWhiteSpace($line) -or $line.StartsWith('#')) { continue }
    $separator = $line.IndexOf("`t")
    if ($separator -lt 1) { throw "Invalid map row at line $lineNumber." }
    $key = Unescape-MapValue $line.Substring(0, $separator)
    $value = Unescape-MapValue $line.Substring($separator + 1)
    if ($map.ContainsKey($key)) { throw "Duplicate map key '$key'." }
    if (-not $source.ContainsKey($key)) { throw "Map key '$key' is absent from the source corpus." }
    if ([string]::IsNullOrWhiteSpace($value)) { throw "Empty translation for '$key'." }
    $tokenPattern = '<[^>]+>|\{[^{}]+\}|%[0-9]*[a-zA-Z]'
    $sourceTokens = [regex]::Matches($source[$key], $tokenPattern).Value -join "`0"
    $targetTokens = [regex]::Matches($value, $tokenPattern).Value -join "`0"
    if ($sourceTokens -cne $targetTokens) { throw "Placeholder/rich-text token mismatch for '$key'." }
    $map[$key] = $value
}

$achievementNames = @($source.Keys | Where-Object { $_ -match '^ACH_.+_NAME$' })
$achievementDescriptions = @($source.Keys | Where-Object { $_ -match '^ACH_.+_DESC$' })
$translatedNames = @($achievementNames | Where-Object { $map.ContainsKey($_) }).Count
$translatedDescriptions = @($achievementDescriptions | Where-Object { $map.ContainsKey($_) }).Count
$unchanged = @($map.Keys | Where-Object { $map[$_] -ceq $source[$_] }).Count
$missing = $source.Count - $map.Count

Write-Output "UI coverage: $($map.Count) / $($source.Count); missing: $missing; unchanged English: $unchanged"
Write-Output "Achievements: names $translatedNames / $($achievementNames.Count); descriptions $translatedDescriptions / $($achievementDescriptions.Count)"
Write-Output "Token mismatches: 0; unknown keys: 0"

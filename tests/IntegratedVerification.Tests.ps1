$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$translationDir = Join-Path $repo 'payload\BepInEx\plugins\RestoryItalianPatch'
$expectedKeys = @(
    'APP_TANKS_DESC', 'APP_TANKS_NAME', 'DEVICE_NAME_COMMODOREKEYBOARD', 'DEVICE_NAME_FUZZBY', 'DEVICE_NAME_NONY_PLAYMACHINE_CONTROLLER',
    'ELEMENT_COMMODOREKEYBOARD_BACK_CORPUS_NAME', 'ELEMENT_COMMODOREKEYBOARD_CHIP_BIG_01_NAME', 'ELEMENT_COMMODOREKEYBOARD_CHIP_SMALL_01_NAME', 'ELEMENT_COMMODOREKEYBOARD_CHIP_VIC_NAME', 'ELEMENT_COMMODOREKEYBOARD_FUSE_NAME', 'ELEMENT_COMMODOREKEYBOARD_KEYBOARD_PW_NAME', 'ELEMENT_COMMODOREKEYBOARD_MOTHERBOARD_NAME', 'ELEMENT_COMMODOREKEYBOARD_PLATE_VIC_NAME', 'ELEMENT_COMMODOREKEYBOARD_SHIFT_LOCK_BUTTON_NAME', 'ELEMENT_COMMODOREKEYBOARD_TOP_CORPUS_NAME',
    'ELEMENT_FUZZBY_BATTERY_1_NAME', 'ELEMENT_FUZZBY_BATTERY_COVER_NAME', 'ELEMENT_FUZZBY_BOARD_NAME', 'ELEMENT_FUZZBY_CASE_BOT_PW_NAME', 'ELEMENT_FUZZBY_CASE_L_NAME', 'ELEMENT_FUZZBY_CASE_R_NAME', 'ELEMENT_FUZZBY_FACE_NAME', 'ELEMENT_FUZZBY_FUR_NAME', 'ELEMENT_FUZZBY_LEFT_PANEL_PW_NAME', 'ELEMENT_FUZZBY_MECHANISM_BLOCK_NAME', 'ELEMENT_FUZZBY_MICRO_PW_NAME', 'ELEMENT_FUZZBY_MOTOR_PW_NAME', 'ELEMENT_FUZZBY_RIGHT_PANEL_PW_NAME', 'ELEMENT_FUZZBY_SPEAKER_PW_NAME', 'ELEMENT_FUZZBY_TOP_PANEL_PW_NAME',
    'ELEMENT_NONY_PLAYMACHINE_CONTROLLER_BUMPER_DOWN_L_NAME', 'ELEMENT_NONY_PLAYMACHINE_CONTROLLER_BUMPER_TOP_L_NAME', 'ELEMENT_NONY_PLAYMACHINE_CONTROLLER_BUMPER_TOP_R_NAME',
    'Email_Name_HinodeExpressCustomerService', 'Email_Narrative_Body_C64', 'Email_Narrative_Topic_C64',
    'TOOL_DEVICE_SCANNER_DESCRIPTION', 'TOOL_DEVICE_SCANNER_NAME', 'UI_SETTING_CUSTOMIZATION_BUTTON_BRUSH', 'UI_SETTING_CUSTOMIZATION_BUTTON_STICKERS', 'UI_WARNING_SCANNING_NO_DEVICE'
)
$rows = Get-Content -LiteralPath (Join-Path $translationDir 'ui-004.tsv') -Encoding UTF8 | Where-Object { $_ -and -not $_.StartsWith('#') }
$actualKeys = @($rows | ForEach-Object { $_.Substring(0, $_.IndexOf("`t")) })
if ($actualKeys.Count -ne 41) { throw "Expected exactly 41 Fuzzby UI translations, found $($actualKeys.Count)." }
if ((Compare-Object $expectedKeys $actualKeys -CaseSensitive).Count) { throw 'Fuzzby UI shard keys differ from the reviewed 41-key set.' }
foreach ($row in $rows) {
    $parts = $row.Split("`t", 2)
    if ($parts.Count -ne 2 -or [string]::IsNullOrWhiteSpace($parts[1])) { throw "Missing Italian translation for $($parts[0])." }
}

$readme = Get-Content -LiteralPath (Join-Path $repo 'README.md') -Raw -Encoding UTF8
$italian = Get-Content -LiteralPath (Join-Path $repo 'README-IT.txt') -Raw -Encoding UTF8
$installer = Get-Content -LiteralPath (Join-Path $repo 'docs\installer.md') -Raw -Encoding UTF8
foreach ($document in @(@{ Name = 'README.md'; Text = $readme }, @{ Name = 'README-IT.txt'; Text = $italian }, @{ Name = 'docs/installer.md'; Text = $installer })) {
    if ($document.Text -notmatch '26622427' -or $document.Text -notmatch '1\.0\.035r') { throw "$($document.Name) must identify stable build 1.0.035r / 26622427." }
    if ($document.Text -notmatch '2,187|2\.187') { throw "$($document.Name) must document 2,187 UI keys." }
    if ($document.Text -notmatch '2,146|2\.146') { throw "$($document.Name) should retain the legacy build compatibility coverage details." }
    if ($document.Text -notmatch '26253677') { throw "$($document.Name) must retain legacy build 26253677 compatibility." }
}
if ($installer -notmatch 'corpus' -or $installer -notmatch 'manual|facoltativ|optional') { throw 'Installer docs must distinguish optional corpus coverage from routine tests.' }
if ($installer -notmatch 'beta_version' -or $installer -notmatch 'fuori ambito|out of scope') { throw 'Installer docs must state beta branch changes remain out of scope.' }

$smoke = Get-Content -LiteralPath (Join-Path $repo 'tests\CliSmoke.ps1') -Raw -Encoding UTF8
foreach ($buildId in @('26253677', '26622427')) {
    if ($smoke -notmatch [regex]::Escape($buildId)) { throw "CLI smoke test must cover supported BuildID $buildId." }
}
Write-Output 'PASS: integrated Fuzzby shard, documentation, and compatibility checks.'

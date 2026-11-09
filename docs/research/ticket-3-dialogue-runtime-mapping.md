# Ticket 3: dialogue runtime mapping probe

**Status:** Stable Articy/runtime keys, a compiled keyed display hook, and Italian coverage for all 1,132 runtime entries are in place. The `en` source field is mixed: 97 entries contain Cyrillic; 1,035 are non-Cyrillic but are not formally verified as English. Catalog, export, and coverage checks pass. In-game UI, line wrapping, and branch behavior still need manual verification.

## Probe result

`tools/Export-DialogueProbe.ps1` reads the temporary `translation-corpus.json` extraction and writes record identities for Articy `DialogueFragment` `Text` and `DisplayName` fields. It does not copy source text or create Italian values. Every result is marked `playerVisibilityStatus: unverified`, `runtimeMappingStatus: unverified`, and `evidenceRequired: runtime-observation` by design.

Against the supplied extraction, it emitted 2,264 records: 1,132 `Text` and 1,132 `DisplayName`. This identity-only queue does not claim any row is player-visible or runtime-mapped. The extracted dialogue `Text` rows have `<Remarks />` in their `en` field and an existing `ru` locale value; the extraction does not preserve enough status/visibility evidence to distinguish current dialogue from internal or outdated notes.

The Pixel Crushers database sample resolves source selection for some matched runtime entries. Its `DialogueText` and `ru` values are Russian, while its custom `en` field can contain the English line or choice. In the matching XML node, `Text.default` is empty and the Russian localized value is marked `Outdated`. The database `en` field is not English for every entry, so the exporter preserves it verbatim and marks likely fallback cases for review instead of claiming it is all English.

The full supplied database extract produced 1,132 rows with 1,132 unique runtime keys and 1,132 unique Articy IDs. Every row had a nonempty `en` field, but language is mixed: 97 `en` values contain Cyrillic, 2 exactly repeat base `DialogueText`, and 1,033 rows do not match those fallback heuristics. A non-Cyrillic field is not proof of English; all source rows remain language-review candidates. Thirty-four source strings repeat across distinct runtime keys; translations remain keyed by conversation/entry. `payload/.../dialogue-coverage.tsv` contains only runtime keys and Articy IDs, not source dialogue.

## Static runtime evidence

Read-only reflection of the installed `Restory.Assembly.dll` confirms a UI seam and a possible dialogue event seam:

- `GUI_DialoguePanel.ShowDialoguePhrase(String)` and `ShowDialogueChoice(String, String)` forward already-resolved strings to `GUI_DialoguePhrase.UpdateContent(String)` and `GUI_DialogueChoiceOption.UpdateContent(String)`. Those presentation methods accept text only; their signatures carry no conversation, entry, or Articy identifier.
- `DialogueNpcVoiceSoundsPlayerService.ResolveConversationLineStarted(PixelCrushers.DialogueSystem.Subtitle)` receives a Pixel Crushers `Subtitle`, and `DialogueSystemLocalizationAdapter.Construct(..., DialogueSystemController)` receives the controller. This establishes that runtime dialogue events and controller APIs are present, but not where the active subtitle text is translated or how it maps back to an Articy record.
- Read-only reflection of `DialogueSystem.dll` shows `Subtitle.dialogueEntry` points to a `DialogueEntry` with `conversationID`, `id`, `DialogueText`, `MenuText`, `currentLocalizedDialogueText`, and `currentLocalizedMenuText`. A `Response` separately carries `destinationEntry`. Thus `(conversationID, entryID)` is a concrete runtime key candidate, and response target identity is separate from its displayed text.
- `StandardDialogueUI.ShowSubtitle(Subtitle)` and `StandardDialogueUI.ShowResponses(Subtitle, Response[], Single)` are the concrete display methods. The BepInEx plugin patches both. The response patch changes only `Response.formattedText.text`; it leaves `Response.destinationEntry` untouched, so branch selection still follows the original response object.
- The plugin reads `PixelCrushers.DialogueSystem.Localization.language` and applies a translation only when it is `it`. Its catalog loads `dialogue.tsv` followed by ordinal-sorted `dialogue-*.tsv` shards, rejects duplicate `(conversationID, entryID)` keys, and reports covered keys missing translations.
- Comparing the Articy locale values with the UI source table finds only six exact string overlaps, all speaker labels. The UI table therefore does not provide narrative coverage or a reliable source-key table.

The `Restory Story` object at data-bundle path ID `-676446223766019203` is a Pixel Crushers `DialogueDatabase` (Articy 4.1.12). Its 176 conversations and 1,132 entries contain runtime conversation/entry IDs and an `Articy Id` hex field (for example `0x0100000000002D0B`). The Articy export identifies nodes by `<Node Id="...">`; this field is the stable join candidate between runtime entries and source nodes. This is stronger than the Articy `shortId` in the earlier corpus, which is not the runtime key.

Static mapping links each runtime entry through `Articy Id` to its XML `Node Id`; the `en` field is the preferred English source when it is actually English, not base `DialogueText` or Articy XML `Text.default`. The exporter records its source-language review status and uses `(conversationId, entryId)` as the runtime key while preserving `Articy Id` for traceability. Player-visible selection still requires observation, and any dialogue replacement must preserve each response's original `destinationEntry`.

## Remaining release verification

The current payload contains 3 keyed Italian seed entries and reports missing keys against the 1,132-entry coverage file. It is a runtime-hook proof of concept, not complete ticket coverage. Before release, verify all of the following:

1. Which matched Pixel Crushers conversation/entry IDs and response-menu entries are actually shown to the player, including whether any database rows are internal/unreachable.
2. Whether the plugin's `it` language gate matches the game's normal Italian selection path.
3. That translated subtitle and response labels render correctly and selecting a response follows its original `destinationEntry` branch.
4. That updates to active conversations do not overwrite the presentation patch.

Next, translate the remaining 1,129 source rows into deterministic shards, using English where confirmed and Russian fallback text when it preserves the intended meaning. Use a read-only probe against a disposable copy of the supported game build to compare actual `Subtitle`/response-menu output with the source fields. Log active IDs, displayed speaker/line, visible response labels, and selected branch. This will establish reachability and confirm Italian rendering before release. Do not run this against the live install.

## Reproduce the identity export

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools\Export-DialogueProbe.ps1 `
  -InputPath work\translation-corpus.json `
  -OutputPath work\dialogue-probe.json
```

The generated probe belongs in ignored `work/` and must not be committed. The original game text corpus must also remain uncommitted.

To prepare a translation-source scratch export from a Pixel Crushers database extract:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools\Export-DialogueSources.ps1 `
  -InputPath work\dialogue-database.json `
  -OutputPath work\dialogue-sources.json
```

This export includes source strings, so keep its output in ignored `work/`; do not commit it as a corpus.

Italian payload entries use one tab-separated record per line: `conversationId`, `entryId`, then Italian text. Keep the initial batch in `dialogue.tsv` and name additional batches `dialogue-0001.tsv`, `dialogue-0002.tsv`, and so on. Escape embedded line breaks as `\n`, tabs as `\t`, and backslashes as `\\`. The runtime loader reads the base file first and then shards in ordinal filename order; duplicate runtime keys fail loading. The coverage manifest is separate and lets the plugin report untranslated keys without bundling English source text.

Coverage keys are exported separately for the runtime catalog:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools\Export-DialogueSources.ps1 `
  -InputPath work\dialogue-db-full.json `
  -OutputPath work\dialogue-sources.json `
  -CoverageOutputPath payload\BepInEx\plugins\RestoryItalianDialogue\dialogue-coverage.tsv
```

The committed coverage file contains runtime IDs only. English source strings stay in ignored `work/`.

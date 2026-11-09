# Restory Italian patch feasibility research

**Date:** 2026-11-08  
**Target:** `D:\Steam\steamapps\common\Restory`  
**Purpose:** Find a practical, separate command-line Italian patch route. No game files were modified.

## Direct observations

- `Restory.exe`, `UnityPlayer.dll`, `Restory_Data/Managed/Restory.Assembly.dll`, `mscorlib.dll`, and `MonoBleedingEdge` indicate a Unity Mono game. `Restory.exe` reports Unity `6000.3.10f1`.
- The game uses Addressables. The catalog lists a `Localization.json` asset and a `RestoryStory` asset in `restory.data_assets_all_23cea2296714af619bf9880cfa9dd77a.bundle`.
- Read-only parsing of that bundle with temporary UnityPy 1.25.4 found `Localization` (2,245,376 characters), `RestoryStory` (3,567,068 characters), and `GeneratedNoiseSeedsCatalog` TextAssets.
- `Localization` is JSON with 2,149 English entries and dictionaries for English, EnglishDraft, Russian, Spanish, French, German, ChineseSimplified, Japanese, Korean, and Portuguese. It has no Italian dictionary.
- The installed build's `AchievementsList` data references 50 achievements; their `nameKey` and `descriptionKey` fields resolve to 100 English localization entries (`ACH_*_NAME` and `ACH_*_DESC`). The linked older mod describes 66 achievement texts, so the current build's own data is the coverage source.
- `GameConfig - Main` supports nine Unity languages: English, Russian, Spanish, French, German, ChineseSimplified, Japanese, Korean, and Portuguese. The installed Unity enum also defines Italian, but the game configuration does not include it.
- `RestoryStory` is an articy:draft XML export. It has 2,105 `DisplayName` elements, 1,167 default `Text` elements, and 1,436 `LocalizedString` elements. All 1,436 localized entries are tagged `ru`; there are no Italian entries in this export.
- The 1,436 XML locale strings break down into 1,166 `Text`, 245 `DisplayName`, and 25 `Description` entries. Some `Text` values are English project/planning notes even though they are stored under `Lang="ru"` and marked `TextState="Outdated"`; they cannot all be assumed to be live dialogue. Keep this XML as a discovery source and map entries to runtime/player-visible content before translating them.
- An exact-value comparison between those XML locale strings and the 2,146 UI localization values found only six overlaps, all speaker labels (`Bucket`, `Hashimoto`, `Gabe`, `Tourist`, `Unknown`). The UI table is not a dialogue-body source. A runtime identity mapping for story text and choices is still required.
- Managed metadata exposes `LocalizationSystem.Initialize`, `GetTranslation`, `AddModifier`, and `RemoveModifier`. `LocalizationModifierBase` is an abstract `ScriptableObject` with an `Execute(string)` method. `GameSettingsManager.Localization` can read/write `UnityEngine.SystemLanguage`. `DialogueSystemLocalizationAdapter` connects game language changes to PixelCrushers Dialogue System.
- Read-only .NET reflection confirmed `PixelCrushers.DialogueSystem.Localization.GetLanguage(SystemLanguage)` returns `en` for English, `it` for Italian, and `ru` for Russian. `DialogueSystemController.SetLanguage(string)` is public. This confirms the language-code mapping and an API entry point, but not that an injected plugin can safely initialize the complete story locale at the right time.
- Additional metadata inspection found `Restory.Data.GameConfigs.GameConfig.SupportedLocalizations` is an instance property backed by a `SystemLanguage[]`, and `LocalizationDataBase.Initialize` accepts both the language dictionaries and supported-language collection. `LocalizationSystem` owns the runtime database and config references. These provide concrete candidates for adding Italian at runtime; invocation order and the dialogue content keys still require confirmation.
- `Master.strings.bank` is an FMOD audio lookup asset; its presence does not locate dialogue translations. FMOD's [official manual](https://www.fmod.com/docs/2.04/studio/advanced-topics.html) documents strings-bank lookup for event names and paths.

## Inferences and recommendation

- A reversible command-line EXE can install/uninstall a BepInEx plugin and external Italian data. The plugin can likely use the game's localization modifier API and add Italian to the runtime language list. These are code-level leads, not a confirmed runtime hook.
- Dialogue translation needs a separate verified route through the Articy/PixelCrushers integration. The XML export is a discovery source but is not proven to be the runtime dialogue database or entirely player-facing; it includes planning notes alongside localized fields.
- Prefer runtime translation over rewriting Addressables bundles until catalog integrity and asset rewrite behavior are understood. The translation patch should keep source strings and translated values separate from the game's original install and back up every installed file.

Official ecosystem sources: [BepInEx overview](https://docs.bepinex.dev/), [BepInEx runtime patching](https://docs.bepinex.dev/v5.4.11/articles/dev_guide/runtime_patching.html), [BepInEx plugin guide](https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/2_plugin_start.html), [Unity Addressables API](https://docs.unity3d.com/Packages/com.unity.addressables@1.20/api/UnityEngine.AddressableAssets.Addressables.html), [articy Unity integration](https://www.articy.com/en/downloads/unity/), and [articy importer documentation](https://www.articy.com/articy-importer/unity/adx/html/welcome.htm).

## Unresolved

1. Confirm exact game methods and timing for the runtime UI translation modifier.
2. Confirm a safe runtime hook and data shape for translated dialogue/choices; the language-code mapping is now known (`it`).
3. Confirm BepInEx version compatibility with this Unity Mono build.
4. Confirm Italian font coverage, placeholders, rich text, and UI fit in game.
5. Build a complete translation corpus and CLI installer with hash/version checks, backups, and uninstall.

REA tools were unavailable. This note uses local filesystem inspection, .NET reflection over metadata, and read-only UnityPy asset parsing; it does not claim runtime observation or verified patch compatibility.

## Loader version check (2026-11-08)

BepInEx's official repository identifies Unity Mono as its only runtime with stable releases. Its release page lists BepInEx 5.4.23.6 as the latest release and records a Unity 6 logging fix in 5.4.23.5. BepInEx 6 remains pre-release and its release notes state that BepInEx 5 plugins are not yet supported there. This supports choosing BepInEx 5.4.23.6 as the initial candidate; it still needs a Restory launch check before compatibility can be claimed ([official releases](https://github.com/BepInEx/BepInEx/releases), [official installation guide](https://docs.bepinex.dev/articles/user_guide/installation/unity_mono.html)).

## User-provided community-mod reference

The user linked an existing Italian mod page for scope inspiration. Its description covers menu/tutorial/email/review/component/description/objective/finale text, story nodes and choices, plus achievement text. It reports testing on ReStory `1.0.015r`, Steam build `24885009`, Unity `6000.3.10f1` Mono, with BepInEx 6 Mono x64 build `6.0.0-be.785+6abdba4` ([mod description](https://www.nexusmods.com/restorychillelectronicrepairs/mods/60)). This is evidence of a prior approach and expected coverage only; no mod file or translation text was downloaded or reused.

The selected local install reports Steam build ID `26253677`. Unity `PlayerSettings.bundleVersion` is `1.0.033r`. The local build therefore differs from the mod's stated tested game build. Its implementation and compatibility cannot be assumed for this install.

The official BepInEx Bleeding Edge page lists build `6.0.0-be.792+3448ef7` for Unity Mono on Windows x64, built on 2026-11-05 ([official build page](https://builds.bepinex.dev/projects/bepinex_be)). This is the current packaging candidate. Exact compatibility with ReStory `1.0.033r` remains to be verified.

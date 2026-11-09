# Restory Italian Patch

**Status:** Draft for GitHub publication after a remote is connected.

## Problem Statement

Restory does not offer Italian. Italian-speaking players cannot follow the interface, dialogue, choices, instructions, and other in-game text in their language.

## Solution

Provide a standalone command-line patcher that adds Italian to Restory through a reversible runtime patch. It translates the full player-facing game text while preserving the original meaning, tone, formatting, placeholders, and practical text length. It adds a credit in the lower-right corner of the home screen: “Tradotto con ❤️ e 🍕 da Alessio | IG alessio.bellan”.

The patcher installs any required loader and translation files, checks that the game version is supported, creates backups, and can restore the original installation on uninstall.

## User Stories

1. As an Italian-speaking player, I want to select Italiano in the game's language settings, so that the patch feels like a built-in language option.
2. As a player, I want the main menu and settings translated, so that I can navigate the game in Italian.
3. As a player, I want tutorials, prompts, and help text translated, so that I understand how to play.
4. As a player, I want dialogue translated, so that I can follow the story.
5. As a player, I want dialogue choices translated, so that I understand what each response means before selecting it.
6. As a player, I want speaker names and player-facing narrative labels translated where appropriate, so that dialogue screens read naturally.
7. As a player, I want tooltips, item names, descriptions, and interaction messages translated, so that I can understand gameplay details.
8. As a player, I want quest, journal, notification, and objective text translated, so that I can track progress.
9. As a player, I want emails, in-game reviews, component text, endings, achievements, subtitles, and other player-facing text translated, so that all written content remains understandable.
10. As a player, I want translations to preserve the original meaning and tone, so that characters and instructions still feel consistent with the English source.
11. As a player, I want names, variables, placeholders, rich-text tags, and control tokens preserved, so that translated text does not break formatting or game behavior.
12. As a player, I want Italian text to fit the available UI where possible, so that labels and dialogue remain readable.
13. As a player, I want accented Italian characters to render correctly, so that the text is legible.
14. As a player, I want missing or unsupported text to fall back safely, so that the game remains playable.
15. As Alessio, I want the home screen to show “Tradotto con ❤️ e 🍕 da Alessio | IG alessio.bellan” in the lower-right corner, so that the translation is credited.
16. As a player, I want to install the patch from Command Prompt, so that I can apply it without manually copying plugin files.
17. As a player, I want the patcher to find the Steam installation or accept its path, so that it can install to the correct game folder.
18. As a player, I want the patcher to check compatibility before changing files, so that it does not damage an unsupported game version.
19. As a player, I want the patcher to create recoverable backups, so that I can restore the original installation.
20. As a player, I want repeated installation to avoid duplicate or corrupt files, so that I can safely reapply an update.
21. As a player, I want uninstall to remove patch files and restore backed-up originals, so that I can return the game to its previous state.
22. As a player, I want clear command-line success and error messages, so that I know what the patcher did and what action is needed.
23. As a player, I want the patch to work offline after I receive it, so that installation does not depend on a translation service or network access.

## Implementation Decisions

- Build a standalone command-line patcher and runtime translation plugin for installed ReStory `1.0.033r`. The patcher may install a Unity Mono loader automatically, as approved by the user.
- Use the game's existing localization API for interface text when the runtime hook is confirmed. Add Italian to the existing language selection path.
- Translate dialogue and responses through a verified hook in the game's Articy/PixelCrushers dialogue path. The exact hook remains an implementation discovery.
- Use the user-linked Nexus mod as a coverage checklist: menus, tutorials, emails, reviews, components, descriptions, objectives, endings, story nodes, choices, and achievement text. Create fresh Italian translations from the installed game's English source; do not reuse the mod's files or translation wording.
- Keep translation data separate from original game assets where the runtime architecture allows it.
- Preserve English source meaning, tone, placeholders, tags, and text-length constraints. Use English as the primary source and compare other existing languages for tone where useful.
- Support safe installation, compatibility checks, backups, repeat installation, and full restoration.
- Add the requested credit overlay only on the home screen, anchored at the lower-right, and display the exact approved wording and emojis.
- Do not modify the installed game while developing the patch. Changes occur only when the user runs the patcher.

## Testing Decisions

- Tests should validate player-visible behavior, not private method names or plugin internals.
- Test the patcher from Command Prompt against a disposable copy of the target installation: install, repeat install, unsupported-version rejection, and uninstall/restore.
- Test the in-game language-selection flow: Italiano appears, can be selected, and remains selected after restarting the game.
- Test user-visible translation coverage in menus, settings, tutorials, prompts, emails, reviews, components, item descriptions, dialogue, dialogue responses, subtitles, objectives, notifications, endings, and achievements.
- Test placeholders, formatting tags, punctuation, accents, text wrapping, and safe fallback for untranslated entries.
- Test the home-screen credit placement and exact text, including both emoji glyphs.
- No existing test suite or source-code testing seam was found in the project at setup time. The highest-value seam is end-to-end behavior in the game and the command-line installer.

## Approved Ticket Breakdown

These four issue drafts are approved for publication after the GitHub remote is connected. Tickets 2–4 depend on ticket 1.

1. **Italiano installabile da CMD** — Deliver a standalone EXE, automatic reversible loader setup, supported-build check, backups, repeat-install handling, and uninstall/restore. Acceptance: install and uninstall work from CMD against a disposable game copy; unsupported versions are rejected before file changes.
2. **Traduzione completa dell’interfaccia** — Translate all player-visible localization entries, including menus, tutorials, emails, reviews, components, descriptions, objectives, notifications, endings, and achievements. Acceptance: Italiano is selectable; sampled text across every category displays in Italian with formatting and placeholders intact.
3. **Dialoghi e risposte in italiano** — Translate player-visible story dialogue, speaker labels, and choices. Acceptance: conversations and selectable responses display in Italian, with the expected branch still chosen after selection.
4. **Crediti in home** — Add the exact approved credit to the lower-right of the home screen. Acceptance: it reads `Tradotto con ❤️ e 🍕 da Alessio | IG alessio.bellan`, both emoji render, and it appears on the home screen only.

## Out of Scope

- Changing Restory's official distribution or publishing the patch through the game developer.
- Translating source code, developer-only comments, or internal authoring notes that players never see.
- Modifying save data or gameplay rules.
- Uploading game assets or the original text corpus to a translation service.

## Further Notes

- Local inspection found ReStory `1.0.033r` (Steam build `26253677`), a Unity Mono build using Addressables. Its localization data contains 2,146 English UI keys and nine existing language dictionaries, but no Italian dictionary.
- The Articy story export contains 1,132 dialogue-fragment text entries and 1,436 Russian localized strings. It has no Italian localized entries. The export's relationship to the runtime dialogue database still needs confirmation.
- The game's localization classes expose a modifier API, and the game configuration already uses Unity's language enum. This makes a runtime Italian option plausible, but does not yet prove the exact UI or dialogue hook.
- The user-provided Nexus mod is a coverage reference for reviews and other in-game text (emails, components, descriptions, objectives, endings, and achievements); its page reports testing an older ReStory build (`1.0.015r`, Steam build `24885009`). This project will translate the current installed build from its own English sources and will not reuse the mod's files or translation wording.
- The approved end-to-end seam is: install from Command Prompt, select Italian, inspect UI and dialogue behavior, then uninstall and confirm restoration.
- GitHub Issues is the configured tracker, but no remote exists yet. Publish this draft after the user connects the repository.
- See the feasibility research note for evidence and unresolved compatibility questions.


# ADR 0001: Reversible CMD installer

**Status:** Accepted for implementation

## Decision

The patcher validates the game build before creating or changing files. It prepares payloads and a backup manifest before activation, and records enough journal data to recover an interrupted install. Installation is idempotent. Uninstall removes only files owned by this patch and restores any files it replaced.

If BepInEx already exists, the patch leaves its loader files and unrelated plugins in place. If the patch installed BepInEx, uninstall removes those loader files only when they are still the patch-owned versions and no unrelated plugin files rely on the loader. Otherwise it removes the translation plugin and retains BepInEx.

## Context

The installer must work from CMD, install a loader when needed, and be reversible. The supported game build is pinned during development; Steam updates can replace files. Players may already use BepInEx for other mods, so uninstalling the loader indiscriminately could break them.

## Evidence

The interactive state-model prototype is preserved on branch `prototype/installer-state` at commit `5271034`. It demonstrates first install/uninstall, repeated install, pre-existing loader, unsupported build rejection, and recovery after an interrupted preparation.

## Consequences

- The installer needs a versioned manifest of every file it writes, plus hashes for owned loader files.
- Compatibility rejection happens before backup creation or loader installation.
- The patch must preserve pre-existing files at every destination it replaces.
- An old backup or mismatched installed file requires a clear recovery error; the installer must not overwrite it silently.
- Runtime compatibility and rollback behavior still require end-to-end verification against a disposable game copy.

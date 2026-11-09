# BepInEx-free Italian localization options for Restory

**Date:** 2026-11-09
**Target:** Restory, Unity Mono build (installation inventory documented in [the feasibility note](restory-italian-patch-feasibility.md))
**Scope:** Compare Addressables catalog/bundle edits, offline managed-assembly edits, and runtime plugins without BepInEx. This is research only; no game files were modified.

## Recommendation

There is no universally cleanest route until the actual strings and their consumers are located. For a BepInEx-free patch, first inventory the text. If the target strings live in `Restory.Assembly.dll`, a reversible offline patch of that DLL is the most direct option: it needs no runtime loader, but is tied to the exact game build and must be reapplied after updates. If strings are serialized in Addressables bundles, edit the relevant assets/bundles only after confirming they can be rebuilt for the game's exact Unity version and Windows target. A new Addressables catalog is not a translation hook by itself: the running game must load that catalog and request its assets. A runtime plugin that does not use BepInEx still needs some other mechanism to load managed plugin code, so it is not loader-free.

## Comparison

| Route | What it can change | Practical fit for Restory | Main cost/risk |
|---|---|---|---|
| Edit catalog and bundles | Serialized asset data that the game already loads | Appropriate only if inspection proves the text is in editable Addressable assets. A catalog can describe new or changed asset locations, but the game still needs to load it and use those addresses. | Unity says bundles are platform-specific; bundle compatibility also depends on Unity version. Catalog entries, provider/dependency metadata, bundle IDs, and hashes must stay coherent. |
| Patch `Restory.Assembly.dll` offline | Literal strings, lookup tables, and code paths implemented in the managed game assembly | The cleanest no-loader route if text or a suitable shared display seam exists there. Package a patcher/backup/restore operation and target a verified assembly version. | Replaces an original binary; game updates invalidate offsets/assumptions, and a poor patch can break startup or game logic. It cannot translate text stored only in assets or generated externally. |
| Runtime plugin without BepInEx | Runtime behavior, potentially via a method hook or string replacement | Technically possible only if another loader/injector is supplied or already present. No evidence found that Restory has such a loader or a public mod API. | Must solve loading/bootstrap, runtime compatibility, and game-specific hook discovery; avoids modifying the game's assembly on disk but adds infrastructure. |

## Evidence and implications

### Addressables catalogs and bundles

Unity documents `Addressables.LoadContentCatalogAsync` as an API to additively load catalogs from runtime data. This makes catalog addition a runtime action taken by code; placing a catalog file alongside the game does not, by itself, cause the game to consume it ([Unity Addressables `LoadContentCatalogAsync`, v1.20](https://docs.unity3d.com/ja/Packages/com.unity.addressables%401.20/api/UnityEngine.AddressableAssets.Addressables.LoadContentCatalogAsync.html)). Addressables catalogs describe resource locations; the Addressables API resolves keys and loads assets through those locations ([Unity Addressables API, v1.20](https://docs.unity3d.com/ja/Packages/com.unity.addressables%401.20/api/UnityEngine.AddressableAssets.Addressables.html)).

Addressables' own group schema shows that catalogs may contain addresses, GUIDs, and labels, with those fields needed for corresponding runtime lookup modes. Bundle groups also carry load paths and bundle naming/compression settings ([Unity `BundledAssetGroupSchema`, v1.21](https://docs.unity3d.com/kr/Packages/com.unity.addressables%401.21/api/UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema.html)). Therefore direct catalog text substitution is viable only if the target data is actually represented there; a catalog generally describes where/how to load content rather than containing arbitrary dialogue itself.

Unity documents that AssetBundles are platform-specific and that forward compatibility between Unity versions is not supported; Addressables is the higher-level package that builds and loads AssetBundles ([Unity AssetBundle introduction](https://docs.unity3d.com/cn/2023.2/Manual/AssetBundlesIntro.html), [platform considerations](https://docs.unity3d.com/ja/current/Manual/assetbundles-platforms.html)). Rebuilding bundles safely therefore requires matching the game's platform and compatible Unity build version. The game installation's catalog/bundle presence alone does not establish that all dialogue or UI text is there.

Unity's Localization package supports String Tables and Locale-specific strings, but it is a package integrated by a game's project and runtime code, not a generic overlay for an already-built game ([Unity Localization package overview](https://docs.unity3d.com/ja/current/Manual/com.unity.localization.html), [runtime localized string database API](https://docs.unity3d.com/ja/Packages/com.unity.localization%401.4/api/UnityEngine.Localization.Settings.LocalizedStringDatabase.html)). No project evidence currently shows Restory uses that package.

### Offline managed-assembly patch

This is a technique assessment rather than a Unity-sanctioned patch mechanism. A managed assembly edit can alter only content/behavior represented in that assembly. It is self-contained at launch and does not depend on an in-game loader, but the game binary must be restored or patched again when Steam replaces it. Before choosing it, inspect whether translations are string literals, resource lookups, or embedded tables and whether the UI/dialogue routes through a shared method. If the assembly only requests Addressable assets, changing its strings may not reach those assets.

### Runtime plugins without BepInEx

BepInEx's official documentation describes it as a Mono Unity plugin framework, and its plugin guide places compiled plugin DLLs in `BepInEx/plugins` for the framework to load ([BepInEx overview](https://docs.bepinex.dev/), [plugin guide](https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/index.html)). Its runtime patching guide says BepInEx ships HarmonyX and MonoMod.RuntimeDetour to alter methods while the game runs ([BepInEx runtime patching](https://docs.bepinex.dev/v5.4.11/articles/dev_guide/runtime_patching.html)). Harmony's own docs describe runtime method patching as modifying methods in memory rather than changing the DLL on disk, and document Mono support ([Harmony 2 overview](https://github.com/pardeike/Harmony/blob/master/Documentation/index.md), [patching concepts](https://github.com/pardeike/Harmony/blob/master/Documentation/articles/patching.md)).

Those sources establish what the loader and patching framework provide; they do not establish a BepInEx-free loading route for this game. Removing BepInEx means separately solving how plugin code enters the process and runs. Harmony alone is a patching library, not a plugin bootstrapper. Also, method hooks depend on finding stable game methods, and Harmony's advanced transpilers work against the method's IL, so they can be brittle across game updates ([Harmony transpiler guide](https://github.com/pardeike/Harmony/blob/master/Documentation/articles/patching-transpiler.md)).

## Practical decision sequence

1. Inventory strings and their sources in the managed assembly and Addressables bundles. Do not infer dialogue storage from the existence of either.
2. If the relevant corpus and shared formatting/display code are in the managed assembly, prototype an offline assembly patch on a disposable copy; retain pristine backup and identify a reliable game-version fingerprint.
3. If the strings are serialized in bundles, determine the exact Unity version and asset serialization types first. Prefer a bundle-level content edit only if references, dependencies, catalog data, and target-platform build compatibility can be preserved.
4. Choose a runtime plugin only if a loader is acceptable or a separate loader is already available. If the requirement is strictly BepInEx-free and no other bootstrapper is allowed, an offline patch is the viable code-based route.

## Evidence limits

The prior installation inventory identifies a Mono Unity layout and Addressables files but does not identify the location of any Restory text, the exact Unity version, any string API, or whether a mod loader is installed. This comparison therefore ranks routes conditionally; it does not claim a working Restory-specific patch point. No game files were changed.

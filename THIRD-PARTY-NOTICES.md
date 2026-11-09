# Third-party components

The installer payload includes BepInEx 6.0.0-be.792 (commit `3448ef7`) and Unity Mono runtime dependencies from the upstream BepInEx release archive. The bundled upstream files are unmodified. Source links below point to the respective upstream projects and license files.

- BepInEx Core and Unity Mono: LGPL-2.1. [Pinned upstream source](https://github.com/BepInEx/BepInEx/tree/3448ef7) and [license text](https://github.com/BepInEx/BepInEx/blob/3448ef7/LICENSE).
- Unity Doorstop (`winhttp.dll`): LGPL-2.1. [Upstream source and license](https://github.com/NeighTools/UnityDoorstop).
- HarmonyX (`0Harmony.dll`): MIT. [Upstream source and license](https://github.com/BepInEx/HarmonyX).
- MonoMod (`MonoMod.RuntimeDetour.dll`, `MonoMod.Utils.dll`): MIT. [Upstream source and license](https://github.com/MonoMod/MonoMod).
- Mono.Cecil (`Mono.Cecil*.dll`): MIT. [Upstream source and license](https://github.com/jbevain/cecil).
- AssetRipper.Primitives: MIT. [Upstream source and license](https://github.com/AssetRipper/AssetRipper.Primitives).
- SemanticVersioning (`SemanticVersioning.dll`): MIT. [Upstream source and license](https://github.com/adamreeve/semver.net).

The patch's installer and translation plugins are separate works and are not part of BepInEx. Refer to the linked upstream repositories for the corresponding source code and complete license texts. The project license in `LICENSE` applies only to this repository's original code and build tooling; it does not relicense the bundled dependencies or translation TSV files.

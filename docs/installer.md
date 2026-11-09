# CMD installer core

## Build and verify

The source targets the Windows .NET Framework console runtime. On a Windows machine with its C# compiler installed:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\test.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
```

The generated executable is `dist\RestoryItalianPatch.exe`. It accepts:

```cmd
RestoryItalianPatch.exe install "C:\path\to\steamapps\common\Restory" [payload-folder]
RestoryItalianPatch.exe uninstall "C:\path\to\steamapps\common\Restory"
```

Without an explicit payload folder, install reads `payload` beside the executable. The compatibility check finds the adjacent Steam `appmanifest_*.acf`, matches its `installdir` to the selected folder, and accepts only build ID `26253677`. A missing marker or any other build exits before creating installer state or writing to the game.

## Release payload seam

The payload now includes BepInEx 6.0.0-be.792 for Unity Mono, the runtime plugin, and an Italian UI map at `BepInEx\plugins\RestoryItalianPatch\ui.tsv`. The plugin also loads disjoint `ui-*.tsv` shards in deterministic filename order and rejects duplicate keys. Build the plugin with `src\build-plugin.ps1`, passing `-ManagedGameFolder` and `-BepInExCoreFolder` for the supported game and loader assemblies. The installer still builds through the existing `build.ps1` entry point.

Ticket 2 remains partial and unresolved: the current map translates 229 of 2,146 UI keys, including all 50 achievement names and all 50 descriptions. The other 1,917 keys fall back to the English source until translated shards are added. Run `tests\TranslationMapCoverage.ps1 -SourceCorpus <path-to-read-only-corpus>` to report coverage, achievement counts, unknown IDs, and token mismatches. Do not add the source corpus to the repository.

The installer hashes every payload file, stages it, records preexisting destinations under `.RestoryItalianPatch\backups`, and writes an XML manifest and journal before activation. Repeat installation with the same payload is a no-op. Uninstall restores replaced files and deletes patch-created files. It refuses to overwrite a file modified after installation. It only treats `winhttp.dll` and known BepInEx core assemblies as evidence that the loader already exists. If another plugin is present at uninstall time, loader files installed by this patch are kept.

The smoke tests use temporary Steam manifests and toy payload files. They do not verify the actual BepInEx release, Restory launch behavior, runtime translation plugin, or compatibility with a live game installation.

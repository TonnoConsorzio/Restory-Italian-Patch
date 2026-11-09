# CMD installer core

## Build and verify

The source targets the Windows .NET Framework console runtime. On a Windows machine with its C# compiler installed:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\test.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
```

Each build creates a clean, timestamped release folder under `dist\RestoryItalianPatch-<timestamp>`. It contains `RestoryItalianPatch.exe`, its payload, `install.cmd`, `uninstall.cmd`, and the Italian quick-start guide. It accepts:

```cmd
RestoryItalianPatch.exe install "C:\path\to\steamapps\common\Restory" [payload-folder]
RestoryItalianPatch.exe uninstall "C:\path\to\steamapps\common\Restory"
```

Without an explicit payload folder, install reads `payload` beside the executable. The `install.cmd` and `uninstall.cmd` launch against `D:\Steam\steamapps\common\Restory` by default; passing a folder path as an argument selects a different location. The compatibility check finds the adjacent Steam `appmanifest_*.acf`, matches its `installdir` to the selected folder, and accepts only build ID `26253677`. A missing marker or any other build exits before creating installer state or writing to the game.

## Release payload seam

The release payload includes BepInEx 6.0.0-be.792 for Unity Mono, the UI and dialogue runtime plugins, the credits overlay, and the Italian TSV maps. `build.ps1` compiles all three plugins from source under ignored `work\build-payload`, then stages them into a timestamped release folder with the EXE, loader payload, CMD wrappers, and quick-start guide. No generated plugin DLL is tracked in `payload/`.

The UI map covers 2,146/2,146 source keys, including all achievement names and descriptions. Dialogue coverage is 1,132/1,132 runtime keys. Maps load numbered shards in deterministic filename order and reject duplicate IDs. Run `tests\TranslationMapCoverage.ps1 -SourceCorpus <path-to-read-only-corpus>` to report UI coverage, achievement counts, unknown IDs, and token mismatches. Do not add the source corpus to the repository.

The installer hashes every payload file, stages it, records preexisting destinations under `.RestoryItalianPatch\backups`, and writes an XML manifest and journal before activation. Repeat installation with the same payload is a no-op. Uninstall restores replaced files and deletes patch-created files. It refuses to overwrite a file modified after installation. It only treats `winhttp.dll` and known BepInEx core assemblies as evidence that the loader already exists. If another plugin is present at uninstall time, loader files installed by this patch are kept.

`test.ps1` checks the installer with temporary Steam manifests and a toy payload, checks translation catalogs/exporters, compiles the plugins, and exercises the CLI. A separate disposable copy of build 26253677 was installed and launched; its BepInEx log confirmed that all three plugins loaded, 2,146 UI strings were loaded, and all 1,132 dialogue keys were covered. The CMD wrappers then installed and removed the patch from that copy. The live Steam installation was not changed. Visual layout, the home-screen credit appearance, and selecting a dialogue choice in a full playthrough still need manual review.

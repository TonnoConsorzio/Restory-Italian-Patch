# CMD installer core

## Supported builds

The verified stable target is Restory **1.0.035r**, Steam BuildID **26622427**. The previous Restory **1.0.033r**, BuildID **26253677**, remains supported. The 1.0.035r UI catalog contains **2,187** translated keys, including the 41 Fuzzby update keys; the previous catalog covered 2,146 keys. The runtime dialogue map contains 1,132 keys, including the new conversation 156, entry 13. The optional Steam `[beta_version]` branch and its story progression changes are out of scope.

The CMD wrappers search Steam libraries for either supported BuildID and otherwise prompt for the game folder. A path may also be passed as the first argument. The compatibility check independently finds the adjacent Steam `appmanifest_*.acf`, matches its `installdir` to the selected folder, and accepts only BuildID `26253677` or `26622427`. A missing marker or any other build exits before creating installer state or writing to the game.

## Build and verify

The source targets the Windows .NET Framework console runtime. On Windows with the C# compiler installed and access to the game's managed references, run:

```powershell
$managed = 'C:\path\to\steamapps\common\Restory\Restory_Data\Managed'
powershell -NoProfile -ExecutionPolicy Bypass -File .\test.ps1 -ManagedGameFolder $managed
powershell -NoProfile -ExecutionPolicy Bypass -File .\build.ps1 -ManagedGameFolder $managed
```

Each build creates a clean, timestamped release folder under `dist\RestoryItalianPatch-<timestamp>`. It contains `RestoryItalianPatch.exe`, its payload, `install.cmd`, `uninstall.cmd`, and the Italian quick-start guide. The CLI accepts:

```cmd
RestoryItalianPatch.exe install "C:\path\to\steamapps\common\Restory" [payload-folder]
RestoryItalianPatch.exe uninstall "C:\path\to\steamapps\common\Restory"
```

Without an explicit payload folder, install reads `payload` beside the executable. The wrappers use the executable beside them or locate the newest release under the source folder's `dist`.

Pass the game's `Restory_Data\Managed` folder with `-ManagedGameFolder`, or set `RESTORY_MANAGED_FOLDER` to that directory. The default is the standard Steam library under `Program Files (x86)`.

`test.ps1` runs installer fixtures with temporary Steam manifests and toy payloads, validates translation catalogs and exporters, compiles plugins, and runs the CLI plus shipped CMD wrappers against disposable game fixtures for both supported BuildIDs. It checks repeated installation, uninstall, and rejection of an unsupported BuildID. These automated steps do not launch or modify the installed game. Do not commit the original game text corpus.

The integrated regression check verifies the exact 41 Fuzzby UI IDs and their non-empty translations from the patch-owned `ui-004.tsv`, without requiring a source corpus. Full UI coverage against a source export is an optional manual check; provide a local read-only JSON path and keep that export out of the repository:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tests\FuzzbyUiCoverage.Tests.ps1 -SourceCorpus 'C:\local-path\ui-source.json'
```

The exporter and coverage tools also accept explicit local source files. All original game extracts and exports must remain outside version control.

## Release payload seam

The release payload includes BepInEx 6.0.0-be.792 for Unity Mono, the UI and dialogue runtime plugins, the credits overlay, and the Italian TSV maps. `build.ps1` compiles all three plugins from source under ignored `work\build-payload`, then stages them into a timestamped release folder with the EXE, loader payload, CMD wrappers, and quick-start guide. No generated plugin DLL is tracked in `payload/`.

The installer hashes every payload file, stages it, records preexisting destinations under `.RestoryItalianPatch\backups`, and writes an XML manifest and journal before activation. Repeat installation with the same payload is a no-op. Uninstall restores replaced files and deletes patch-created files. It refuses to overwrite a file modified after installation. If another plugin is present at uninstall time, loader files installed by this patch are kept.

Visual layout, the home-screen credit appearance, and selecting a dialogue choice in a playthrough still need manual review. The game must not be launched during automated verification.

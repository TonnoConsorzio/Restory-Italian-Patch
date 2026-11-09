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

The runtime/plugin project and BepInEx release files are not present yet. The current executable is therefore a buildable installer core and is not a distributable Italian patch. Place the verified plugin and loader files under a `payload` directory, preserving their paths relative to the game root, before packaging a release. The plugin should live in `BepInEx\plugins\RestoryItalianPatch\`. The loader payload should include the Windows Mono bootstrap files and `BepInEx\core` assemblies needed by the selected BepInEx build.

The installer hashes every payload file, stages it, records preexisting destinations under `.RestoryItalianPatch\backups`, and writes an XML manifest and journal before activation. Repeat installation with the same payload is a no-op. Uninstall restores replaced files and deletes patch-created files. It refuses to overwrite a file modified after installation. It only treats `winhttp.dll` and known BepInEx core assemblies as evidence that the loader already exists. If another plugin is present at uninstall time, loader files installed by this patch are kept.

The smoke tests use temporary Steam manifests and toy payload files. They do not verify the actual BepInEx release, Restory launch behavior, runtime translation plugin, or compatibility with a live game installation.

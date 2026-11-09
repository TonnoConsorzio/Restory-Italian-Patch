using System;
using System.IO;
using RestoryItalianPatch;

internal static class InstallerTests
{
    private static int _assertions;

    private static void Main()
    {
        UnsupportedBuildIsRejectedBeforeWrites();
        UnownedStateDirectoryIsNeverDeleted();
        InstallIsReversibleAndRepeatInstallIsIdempotent();
        ExistingLoaderAndOtherPluginsArePreserved();
        InterruptedInstallIsRecoveredBeforeRetry();
        PatchOwnedLoaderIsKeptWhenAnotherPluginAppears();
        PartialBepInExFolderDoesNotCountAsInstalledLoader();
        InterruptedUninstallRestoresMissingOriginal();
        Console.WriteLine("PASS: " + _assertions + " assertions");
    }

    private static void UnsupportedBuildIsRejectedBeforeWrites()
    {
        WithFixture((root, game, payload) =>
        {
            WriteGame(game, "123");
            WritePayload(payload);
            var before = Directory.GetFileSystemEntries(game, "*", SearchOption.AllDirectories).Length;
            Throws<UnsupportedGameException>(() => Engine(game, payload).Install());
            Equal(before, Directory.GetFileSystemEntries(game, "*", SearchOption.AllDirectories).Length, "unsupported install leaves game tree unchanged");
            False(Directory.Exists(Path.Combine(game, InstallerEngine.StateDirectoryName)), "unsupported install creates no state directory");
        });
    }

    private static void UnownedStateDirectoryIsNeverDeleted()
    {
        WithFixture((root, game, payload) =>
        {
            WriteGame(game, InstallerEngine.SupportedBuildId);
            WritePayload(payload);
            var state = Path.Combine(game, InstallerEngine.StateDirectoryName);
            Directory.CreateDirectory(state);
            var userFile = Path.Combine(state, "keep-me.txt");
            File.WriteAllText(userFile, "user data");

            Throws<InstallerException>(() => Engine(game, payload).Install());

            Equal("user data", File.ReadAllText(userFile), "unknown installer state is preserved");
            False(File.Exists(Path.Combine(game, "BepInEx", "plugins", "RestoryItalianPatch", "sample.txt")), "unknown state prevents activation");
        });
    }

    private static void InstallIsReversibleAndRepeatInstallIsIdempotent()
    {
        WithFixture((root, game, payload) =>
        {
            WriteGame(game, InstallerEngine.SupportedBuildId);
            WritePayload(payload);
            var overwritten = Path.Combine(game, "BepInEx", "plugins", "RestoryItalianPatch", "sample.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(overwritten));
            File.WriteAllText(overwritten, "original");

            Engine(game, payload).Install();
            Equal("patch", File.ReadAllText(overwritten), "patch payload installed");
            Equal("original", File.ReadAllText(Path.Combine(game, InstallerEngine.StateDirectoryName, "backups", "BepInEx", "plugins", "RestoryItalianPatch", "sample.txt")), "preexisting destination backed up");
            var manifest = Path.Combine(game, InstallerEngine.StateDirectoryName, "manifest.xml");
            True(File.Exists(manifest), "manifest written");
            True(File.Exists(Path.Combine(game, InstallerEngine.StateDirectoryName, "journal.xml")), "journal written");

            Engine(game, payload).Install();
            Equal("original", File.ReadAllText(Path.Combine(game, InstallerEngine.StateDirectoryName, "backups", "BepInEx", "plugins", "RestoryItalianPatch", "sample.txt")), "repeat install preserves first backup");
            Equal("patch", File.ReadAllText(overwritten), "repeat install keeps one payload copy");

            Engine(game, payload).Uninstall();
            Equal("original", File.ReadAllText(overwritten), "uninstall restores overwritten original");
            False(File.Exists(Path.Combine(game, "BepInEx", "plugins", "RestoryItalianPatch", "new.txt")), "uninstall removes new patch file");
            False(File.Exists(Path.Combine(game, ".doorstop_version")), "uninstall removes patch-owned Doorstop version marker when no other plugins remain");
            False(File.Exists(Path.Combine(game, "changelog.txt")), "uninstall removes patch-owned loader changelog when no other plugins remain");
            False(Directory.Exists(Path.Combine(game, InstallerEngine.StateDirectoryName)), "uninstall removes patch state");
        });
    }

    private static void ExistingLoaderAndOtherPluginsArePreserved()
    {
        WithFixture((root, game, payload) =>
        {
            WriteGame(game, InstallerEngine.SupportedBuildId);
            WritePayload(payload);
            var loader = Path.Combine(game, "BepInEx", "core", "BepInEx.dll");
            var otherPlugin = Path.Combine(game, "BepInEx", "plugins", "OtherMod.dll");
            Directory.CreateDirectory(Path.GetDirectoryName(loader));
            Directory.CreateDirectory(Path.GetDirectoryName(otherPlugin));
            File.WriteAllText(loader, "user loader");
            File.WriteAllText(otherPlugin, "other plugin");

            Engine(game, payload).Install();
            Equal("user loader", File.ReadAllText(loader), "preexisting loader file is untouched");
            Engine(game, payload).Uninstall();
            Equal("user loader", File.ReadAllText(loader), "uninstall keeps preexisting loader");
            Equal("other plugin", File.ReadAllText(otherPlugin), "uninstall keeps unrelated plugin");
        });
    }

    private static void InterruptedInstallIsRecoveredBeforeRetry()
    {
        WithFixture((root, game, payload) =>
        {
            WriteGame(game, InstallerEngine.SupportedBuildId);
            WritePayload(payload);
            var original = Path.Combine(game, "BepInEx", "plugins", "RestoryItalianPatch", "sample.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(original));
            File.WriteAllText(original, "original");
            Engine(game, payload).Install();
            var state = Path.Combine(game, InstallerEngine.StateDirectoryName);
            File.WriteAllText(Path.Combine(state, "manifest.xml"), File.ReadAllText(Path.Combine(state, "manifest.xml")).Replace("status=\"installed\"", "status=\"preparing\""));
            File.WriteAllText(Path.Combine(state, "journal.xml"), File.ReadAllText(Path.Combine(state, "journal.xml")).Replace("status=\"installed\"", "status=\"activating\""));
            File.Delete(Path.Combine(game, "BepInEx", "plugins", "RestoryItalianPatch", "new.txt"));

            Engine(game, payload).Install();
            Equal("new patch file", File.ReadAllText(Path.Combine(game, "BepInEx", "plugins", "RestoryItalianPatch", "new.txt")), "retry recovers and reapplies incomplete patch file");
            Equal("original", File.ReadAllText(Path.Combine(state, "backups", "BepInEx", "plugins", "RestoryItalianPatch", "sample.txt")), "recovery preserves the original backup");
        });
    }

    private static void PatchOwnedLoaderIsKeptWhenAnotherPluginAppears()
    {
        WithFixture((root, game, payload) =>
        {
            WriteGame(game, InstallerEngine.SupportedBuildId);
            WritePayload(payload);
            Engine(game, payload).Install();
            var otherPlugin = Path.Combine(game, "BepInEx", "plugins", "OtherMod.dll");
            File.WriteAllText(otherPlugin, "other plugin");

            Engine(game, payload).Uninstall();
            Equal("bundled loader", File.ReadAllText(Path.Combine(game, "BepInEx", "core", "BepInEx.dll")), "patch-owned loader retained when another plugin relies on it");
            Equal("Doorstop 6", File.ReadAllText(Path.Combine(game, ".doorstop_version")), "patch-owned Doorstop marker retained with loader");
            Equal("BepInEx changelog", File.ReadAllText(Path.Combine(game, "changelog.txt")), "patch-owned loader changelog retained with loader");
            Equal("other plugin", File.ReadAllText(otherPlugin), "unrelated plugin remains installed");
            False(File.Exists(Path.Combine(game, "BepInEx", "plugins", "RestoryItalianPatch", "new.txt")), "patch plugin removed while retaining loader");
        });
    }

    private static void InterruptedUninstallRestoresMissingOriginal()
    {
        WithFixture((root, game, payload) =>
        {
            WriteGame(game, InstallerEngine.SupportedBuildId);
            WritePayload(payload);
            var original = Path.Combine(game, "BepInEx", "plugins", "RestoryItalianPatch", "sample.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(original));
            File.WriteAllText(original, "original");
            Engine(game, payload).Install();
            var state = Path.Combine(game, InstallerEngine.StateDirectoryName);
            File.WriteAllText(Path.Combine(state, "manifest.xml"), File.ReadAllText(Path.Combine(state, "manifest.xml")).Replace("status=\"installed\"", "status=\"preparing\""));
            File.WriteAllText(Path.Combine(state, "journal.xml"), File.ReadAllText(Path.Combine(state, "journal.xml")).Replace("status=\"installed\"", "status=\"activating\""));
            File.Delete(original);

            Engine(game, payload).Uninstall();
            Equal("original", File.ReadAllText(original), "interrupted uninstall restores a missing preexisting file from backup");
            False(Directory.Exists(state), "recovered uninstall clears installer state");
        });
    }

    private static void PartialBepInExFolderDoesNotCountAsInstalledLoader()
    {
        WithFixture((root, game, payload) =>
        {
            WriteGame(game, InstallerEngine.SupportedBuildId);
            WritePayload(payload);
            var unrelated = Path.Combine(game, "BepInEx", "plugins", "OtherMod.dll");
            Directory.CreateDirectory(Path.GetDirectoryName(unrelated));
            File.WriteAllText(unrelated, "other plugin");

            Engine(game, payload).Install();
            Equal("bundled loader", File.ReadAllText(Path.Combine(game, "BepInEx", "core", "BepInEx.dll")), "partial BepInEx directory receives required loader payload");
            Engine(game, payload).Uninstall();
            Equal("bundled loader", File.ReadAllText(Path.Combine(game, "BepInEx", "core", "BepInEx.dll")), "loader remains because an unrelated plugin uses it");
        });
    }

    private static InstallerEngine Engine(string game, string payload)
    {
        return new InstallerEngine(game, payload);
    }

    private static void WriteGame(string game, string buildId)
    {
        Directory.CreateDirectory(game);
        File.WriteAllText(Path.Combine(game, "Restory.exe"), "test executable");
        var steamApps = Directory.GetParent(Directory.GetParent(game).FullName).FullName;
        Directory.CreateDirectory(steamApps);
        File.WriteAllText(Path.Combine(steamApps, "appmanifest_999999.acf"),
            "\"AppState\"\r\n{\r\n  \"installdir\" \"Restory\"\r\n  \"buildid\" \"" + buildId + "\"\r\n}\r\n");
    }

    private static void WritePayload(string payload)
    {
        Directory.CreateDirectory(Path.Combine(payload, "BepInEx", "plugins", "RestoryItalianPatch"));
        Directory.CreateDirectory(Path.Combine(payload, "BepInEx", "core"));
        File.WriteAllText(Path.Combine(payload, "BepInEx", "plugins", "RestoryItalianPatch", "sample.txt"), "patch");
        File.WriteAllText(Path.Combine(payload, "BepInEx", "plugins", "RestoryItalianPatch", "new.txt"), "new patch file");
        File.WriteAllText(Path.Combine(payload, "BepInEx", "core", "BepInEx.dll"), "bundled loader");
        File.WriteAllText(Path.Combine(payload, ".doorstop_version"), "Doorstop 6");
        File.WriteAllText(Path.Combine(payload, "changelog.txt"), "BepInEx changelog");
    }

    private static void WithFixture(Action<string, string, string> test)
    {
        var root = Path.Combine(Path.GetTempPath(), "restory-installer-test-" + Guid.NewGuid().ToString("N"));
        var game = Path.Combine(root, "steamapps", "common", "Restory");
        var payload = Path.Combine(root, "payload");
        try { test(root, game, payload); }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { _assertions++; return; }
        throw new Exception("Expected " + typeof(T).Name);
    }

    private static void Equal<T>(T expected, T actual, string message)
    {
        if (!Object.Equals(expected, actual)) throw new Exception(message + ": expected " + expected + ", got " + actual);
        _assertions++;
    }

    private static void True(bool condition, string message) { Equal(true, condition, message); }
    private static void False(bool condition, string message) { Equal(false, condition, message); }
}

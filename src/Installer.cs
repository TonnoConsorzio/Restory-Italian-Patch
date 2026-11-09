using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace RestoryItalianPatch
{
    public sealed class UnsupportedGameException : Exception
    {
        public UnsupportedGameException(string message) : base(message) { }
    }

    public sealed class InstallerException : Exception
    {
        public InstallerException(string message) : base(message) { }
    }

    internal sealed class OwnedFile
    {
        public string RelativePath;
        public string InstalledHash;
        public string OriginalHash;
        public bool HadOriginal;
        public bool IsLoaderFile;
        public bool WasInstalled;
    }

    public sealed class InstallerEngine
    {
        public const string SupportedBuildId = "26253677";
        public const string StateDirectoryName = ".RestoryItalianPatch";
        private readonly string _game;
        private readonly string _payload;
        private readonly string _state;
        private readonly string _journal;
        private readonly string _manifest;

        public InstallerEngine(string gameDirectory, string payloadDirectory)
        {
            _game = Path.GetFullPath(gameDirectory);
            _payload = Path.GetFullPath(payloadDirectory);
            _state = Path.Combine(_game, StateDirectoryName);
            _journal = Path.Combine(_state, "journal.xml");
            _manifest = Path.Combine(_state, "manifest.xml");
        }

        public void Install()
        {
            ValidateCompatibility(); // Must remain the first operation that can gate installation.
            if (!Directory.Exists(_payload)) throw new InstallerException("Payload folder is missing: " + _payload);

            var sources = Directory.GetFiles(_payload, "*", SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToList();
            if (sources.Count == 0) throw new InstallerException("The release payload is empty.");
            if (sources.Any(path => IsWithin(path, _game))) throw new InstallerException("The payload cannot be inside the game folder.");

            if (Directory.Exists(_state))
            {
                if (File.Exists(_manifest))
                {
                    var installed = ReadFiles(_manifest);
                    if (ReadStatus(_manifest) != "installed")
                    {
                        RecoverPreparation(ReadFiles(File.Exists(_journal) ? _journal : _manifest));
                    }
                    else if (IsSamePayload(installed, sources))
                    {
                        EnsureInstalledFilesStillOwned(installed);
                        return;
                    }
                    else throw new InstallerException("A different patch version is installed. Uninstall it before installing this payload.");
                }
                else if (File.Exists(_journal)) RecoverPreparation(ReadFiles(_journal));
                else throw new InstallerException("The patch state folder exists without a valid manifest or journal; refusing to delete or overwrite it: " + _state);
            }

            Directory.CreateDirectory(Path.Combine(_state, "backups"));
            Directory.CreateDirectory(Path.Combine(_state, "staging"));
            var loaderPresent = HasInstalledLoader();
            var files = new List<OwnedFile>();
            foreach (var source in sources)
            {
                var relative = RelativeTo(_payload, source);
                if (IsLoaderFile(relative) && loaderPresent) continue;
                var destination = Destination(relative);
                var record = new OwnedFile { RelativePath = relative, InstalledHash = Hash(source), IsLoaderFile = IsLoaderFile(relative) };
                record.HadOriginal = File.Exists(destination);
                if (record.HadOriginal)
                {
                    record.OriginalHash = Hash(destination);
                    var backup = BackupPath(relative);
                    Directory.CreateDirectory(Path.GetDirectoryName(backup));
                    File.Copy(destination, backup, false);
                }
                var staged = StagedPath(relative);
                Directory.CreateDirectory(Path.GetDirectoryName(staged));
                File.Copy(source, staged, false);
                files.Add(record);
            }
            if (!files.Any(file => !file.IsLoaderFile))
                throw new InstallerException("The payload has no translation plugin files.");

            WriteFiles(_journal, "preparing", files);
            WriteFiles(_manifest, "preparing", files);
            WriteFiles(_journal, "activating", files);
            try
            {
                foreach (var file in files)
                {
                    var destination = Destination(file.RelativePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(destination));
                    var staged = StagedPath(file.RelativePath);
                    if (File.Exists(destination))
                    {
                        var swap = destination + ".restory-new";
                        if (File.Exists(swap)) File.Delete(swap);
                        File.Copy(staged, swap, false);
                        File.Replace(swap, destination, null);
                    }
                    else File.Move(staged, destination);
                    file.WasInstalled = true;
                    WriteFiles(_journal, "activating", files);
                }
                WriteFiles(_manifest, "installed", files);
                WriteFiles(_journal, "installed", files);
                Directory.Delete(Path.Combine(_state, "staging"), true);
            }
            catch
            {
                Rollback(files, false);
                throw;
            }
        }

        public void Uninstall()
        {
            if (!File.Exists(_manifest)) return;
            if (ReadStatus(_manifest) != "installed")
            {
                RecoverPreparation(ReadFiles(File.Exists(_journal) ? _journal : _manifest));
                return;
            }
            var files = ReadFiles(_manifest);
            EnsureInstalledFilesStillOwned(files);
            // Validate every restoration source before making the first change.
            foreach (var file in files)
                if (file.HadOriginal && (!File.Exists(BackupPath(file.RelativePath)) || Hash(BackupPath(file.RelativePath)) != file.OriginalHash))
                    throw new InstallerException("Backup is missing or changed: " + file.RelativePath);

            Rollback(files, HasOtherPlugins());
            Directory.Delete(_state, true);
        }

        private void ValidateCompatibility()
        {
            if (!File.Exists(Path.Combine(_game, "Restory.exe")))
                throw new UnsupportedGameException("Restory.exe was not found in the selected folder.");
            var steamApps = FindSteamApps(_game);
            if (steamApps == null) throw new UnsupportedGameException("Steam build metadata was not found; no files were changed.");
            var manifestFiles = Directory.GetFiles(steamApps, "appmanifest_*.acf", SearchOption.TopDirectoryOnly);
            foreach (var path in manifestFiles)
            {
                var content = File.ReadAllText(path);
                var installDir = MatchValue(content, "installdir");
                if (!String.Equals(installDir, new DirectoryInfo(_game).Name, StringComparison.OrdinalIgnoreCase)) continue;
                var build = MatchValue(content, "buildid");
                if (build == SupportedBuildId) return;
                throw new UnsupportedGameException("Unsupported Restory Steam build '" + (build ?? "unknown") + "'; supported build is " + SupportedBuildId + ". No files were changed.");
            }
            throw new UnsupportedGameException("Steam metadata does not identify this folder as Restory; no files were changed.");
        }

        private static string FindSteamApps(string game)
        {
            var current = new DirectoryInfo(game);
            while (current != null)
            {
                if (String.Equals(current.Name, "common", StringComparison.OrdinalIgnoreCase) && current.Parent != null &&
                    String.Equals(current.Parent.Name, "steamapps", StringComparison.OrdinalIgnoreCase)) return current.Parent.FullName;
                current = current.Parent;
            }
            return null;
        }

        private bool HasInstalledLoader()
        {
            if (File.Exists(Path.Combine(_game, "winhttp.dll"))) return true;
            var core = Path.Combine(_game, "BepInEx", "core");
            return File.Exists(Path.Combine(core, "BepInEx.dll")) ||
                File.Exists(Path.Combine(core, "BepInEx.Preloader.dll")) ||
                File.Exists(Path.Combine(core, "BepInEx.Core.dll"));
        }

        private static string MatchValue(string content, string key)
        {
            var match = Regex.Match(content, "(?m)^\\s*\\\"" + Regex.Escape(key) + "\\\"\\s+\\\"([^\\\"]*)\\\"");
            return match.Success ? match.Groups[1].Value : null;
        }

        private bool IsSamePayload(List<OwnedFile> installed, List<string> sources)
        {
            var current = sources.Select(path => new { Relative = RelativeTo(_payload, path), Hash = Hash(path) })
                .Where(item => !(IsLoaderFile(item.Relative) && LoaderWasPreexisting(installed)))
                .ToList();
            return installed.Count == current.Count && installed.All(file => current.Any(item =>
                String.Equals(item.Relative, file.RelativePath, StringComparison.OrdinalIgnoreCase) && item.Hash == file.InstalledHash));
        }

        private bool LoaderWasPreexisting(List<OwnedFile> files)
        {
            // Loader files absent from the manifest were deliberately skipped because BepInEx already existed.
            return files.All(file => !file.IsLoaderFile) && HasInstalledLoader();
        }

        private void EnsureInstalledFilesStillOwned(List<OwnedFile> files)
        {
            foreach (var file in files)
            {
                var destination = Destination(file.RelativePath);
                if (!File.Exists(destination) || Hash(destination) != file.InstalledHash)
                    throw new InstallerException("Installed file changed since patching; refusing to overwrite it: " + file.RelativePath);
            }
        }

        private void RecoverPreparation(List<OwnedFile> files)
        {
            // A journal without an installed manifest is a recoverable interrupted install.
            Rollback(files, false);
            Directory.Delete(_state, true);
        }

        private bool HasOtherPlugins()
        {
            var plugins = Path.Combine(_game, "BepInEx", "plugins");
            if (!Directory.Exists(plugins)) return false;
            foreach (var path in Directory.GetFiles(plugins, "*", SearchOption.AllDirectories))
            {
                var relative = RelativeTo(plugins, path).Replace('\\', '/');
                if (!relative.StartsWith("RestoryItalianPatch/", StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private void Rollback(List<OwnedFile> files, bool retainLoader)
        {
            // Check ownership for all files before changing any of them.
            foreach (var file in files)
            {
                var destination = Destination(file.RelativePath);
                if (!File.Exists(destination)) continue;
                var current = Hash(destination);
                if (current != file.InstalledHash && (!file.HadOriginal || current != file.OriginalHash))
                    throw new InstallerException("File changed during installation; refusing to remove or replace it: " + file.RelativePath);
            }
            foreach (var file in files.AsEnumerable().Reverse())
            {
                if (retainLoader && file.IsLoaderFile) continue;
                var destination = Destination(file.RelativePath);
                if (!File.Exists(destination))
                {
                    if (file.HadOriginal)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(destination));
                        File.Copy(BackupPath(file.RelativePath), destination, false);
                    }
                    continue;
                }
                if (file.HadOriginal && Hash(destination) == file.OriginalHash) continue;
                if (file.HadOriginal)
                {
                    var temp = destination + ".restory-restore";
                    if (File.Exists(temp)) File.Delete(temp);
                    File.Copy(BackupPath(file.RelativePath), temp, false);
                    if (File.Exists(destination)) File.Replace(temp, destination, null);
                    else File.Move(temp, destination);
                }
                else File.Delete(destination);
            }
        }

        private static bool IsLoaderFile(string relative)
        {
            var normalized = relative.Replace('\\', '/');
            return String.Equals(normalized, "winhttp.dll", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(normalized, "doorstop_config.ini", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(normalized, ".doorstop_version", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(normalized, "changelog.txt", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("BepInEx/core/", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("BepInEx/patchers/", StringComparison.OrdinalIgnoreCase);
        }

        private string Destination(string relative) { return SafeCombine(_game, relative); }
        private string BackupPath(string relative) { return SafeCombine(Path.Combine(_state, "backups"), relative); }
        private string StagedPath(string relative) { return SafeCombine(Path.Combine(_state, "staging"), relative); }

        private static string SafeCombine(string root, string relative)
        {
            var path = Path.GetFullPath(Path.Combine(root, relative));
            if (!IsWithin(path, root)) throw new InstallerException("Unsafe payload path: " + relative);
            return path;
        }

        private static bool IsWithin(string path, string root)
        {
            var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return Path.GetFullPath(path).StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase);
        }

        private static string RelativeTo(string root, string path)
        {
            return Path.GetFullPath(path).Substring(Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar).Length + 1);
        }

        private static string Hash(string path)
        {
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(path)) return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }

        private static void WriteFiles(string path, string status, List<OwnedFile> files)
        {
            var doc = new XmlDocument();
            var root = doc.CreateElement("installer");
            root.SetAttribute("version", "1");
            root.SetAttribute("status", status);
            doc.AppendChild(root);
            foreach (var file in files)
            {
                var item = doc.CreateElement("file");
                item.SetAttribute("path", file.RelativePath);
                item.SetAttribute("installedHash", file.InstalledHash);
                item.SetAttribute("hadOriginal", file.HadOriginal ? "true" : "false");
                item.SetAttribute("originalHash", file.OriginalHash ?? "");
                item.SetAttribute("loader", file.IsLoaderFile ? "true" : "false");
                item.SetAttribute("wasInstalled", file.WasInstalled ? "true" : "false");
                root.AppendChild(item);
            }
            var temp = path + ".new";
            doc.Save(temp);
            if (File.Exists(path)) File.Replace(temp, path, null); else File.Move(temp, path);
        }

        private static List<OwnedFile> ReadFiles(string path)
        {
            var doc = new XmlDocument();
            doc.Load(path);
            var root = doc.DocumentElement;
            if (root == null || root.Name != "installer" || root.GetAttribute("version") != "1")
                throw new InstallerException("Installer state is invalid or from an unsupported version.");
            var result = new List<OwnedFile>();
            foreach (XmlElement item in root.SelectNodes("file"))
            {
                result.Add(new OwnedFile {
                    RelativePath = item.GetAttribute("path"),
                    InstalledHash = item.GetAttribute("installedHash"),
                    HadOriginal = item.GetAttribute("hadOriginal") == "true",
                    OriginalHash = item.GetAttribute("originalHash"),
                    IsLoaderFile = item.GetAttribute("loader") == "true",
                    WasInstalled = item.GetAttribute("wasInstalled") == "true"
                });
            }
            return result;
        }

        private static string ReadStatus(string path)
        {
            var doc = new XmlDocument();
            doc.Load(path);
            var root = doc.DocumentElement;
            if (root == null || root.Name != "installer" || root.GetAttribute("version") != "1")
                throw new InstallerException("Installer state is invalid or from an unsupported version.");
            return root.GetAttribute("status");
        }
    }
}

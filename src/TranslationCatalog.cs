using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RestoryItalianPatch
{
    public sealed class TranslationCatalog
    {
        private static readonly Regex Tokens = new Regex("<[^>]+>|\\{[^{}]+\\}|%[0-9]*[a-zA-Z]", RegexOptions.Compiled);
        private readonly Dictionary<string, string> _translations;

        private TranslationCatalog(Dictionary<string, string> translations)
        {
            _translations = translations;
        }

        public int Count { get { return _translations.Count; } }

        public static TranslationCatalog Load(string path)
        {
            var translations = new Dictionary<string, string>(StringComparer.Ordinal);
            LoadFile(path, translations);
            return new TranslationCatalog(translations);
        }

        public static TranslationCatalog LoadDirectory(string directory)
        {
            if (!Directory.Exists(directory)) throw new DirectoryNotFoundException("Translation folder was not found: " + directory);
            var files = Directory.GetFiles(directory, "ui*.tsv", SearchOption.TopDirectoryOnly)
                .Where(path => String.Equals(Path.GetFileName(path), "ui.tsv", StringComparison.OrdinalIgnoreCase) ||
                    (Path.GetFileName(path).StartsWith("ui-", StringComparison.OrdinalIgnoreCase) &&
                     String.Equals(Path.GetExtension(path), ".tsv", StringComparison.OrdinalIgnoreCase)))
                .OrderBy(path => String.Equals(Path.GetFileName(path), "ui.tsv", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(path => Path.GetFileName(path), StringComparer.Ordinal)
                .ToList();
            if (files.Count == 0) throw new FileNotFoundException("No ui.tsv or ui-*.tsv translation files were found in " + directory + ".");

            var translations = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var path in files) LoadFile(path, translations);
            return new TranslationCatalog(translations);
        }

        private static void LoadFile(string path, Dictionary<string, string> translations)
        {
            var lineNumber = 0;
            foreach (var line in File.ReadAllLines(path, Encoding.UTF8))
            {
                lineNumber++;
                if (line.Length == 0 || line[0] == '#') continue;
                var separator = line.IndexOf('\t');
                if (separator <= 0) throw new InvalidDataException("Invalid translation row at line " + lineNumber + ".");
                var key = Unescape(line.Substring(0, separator));
                var value = Unescape(line.Substring(separator + 1));
                if (key.Length == 0 || value.Length == 0) throw new InvalidDataException("Empty key or translation at line " + lineNumber + ".");
                if (translations.ContainsKey(key)) throw new InvalidDataException("Duplicate translation key in " + Path.GetFileName(path) + " at line " + lineNumber + ": " + key);
                translations.Add(key, value);
            }
        }

        public string Translate(string key, string source)
        {
            string translated;
            if (key == null || source == null || !_translations.TryGetValue(key, out translated)) return source;
            return HasSameTokens(source, translated) ? translated : source;
        }

        public bool Contains(string key) { return _translations.ContainsKey(key); }

        public static bool HasSameTokens(string source, string translated)
        {
            if (source == null || translated == null) return false;
            var sourceTokens = Tokens.Matches(source);
            var translatedTokens = Tokens.Matches(translated);
            if (sourceTokens.Count != translatedTokens.Count) return false;
            for (var i = 0; i < sourceTokens.Count; i++)
                if (!String.Equals(sourceTokens[i].Value, translatedTokens[i].Value, StringComparison.Ordinal)) return false;
            return true;
        }

        private static string Unescape(string value)
        {
            var result = new StringBuilder(value.Length);
            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] != '\\' || i + 1 >= value.Length)
                {
                    result.Append(value[i]);
                    continue;
                }
                var next = value[++i];
                if (next == 'n') result.Append('\n');
                else if (next == 'r') result.Append('\r');
                else if (next == 't') result.Append('\t');
                else if (next == '\\') result.Append('\\');
                else throw new InvalidDataException("Unsupported escape sequence: \\" + next);
            }
            return result.ToString();
        }
    }
}

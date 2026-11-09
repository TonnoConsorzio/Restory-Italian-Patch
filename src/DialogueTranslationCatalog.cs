using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

internal sealed class DialogueTranslationCatalog
{
    private readonly Dictionary<long, string> _translations;
    private readonly List<string> _missingKeys;
    private readonly int _coverageCount;

    private DialogueTranslationCatalog(Dictionary<long, string> translations, HashSet<long> coverage)
    {
        _translations = translations;
        _coverageCount = coverage.Count;
        _missingKeys = coverage.Where(key => !translations.ContainsKey(key))
            .OrderBy(key => (int)(key >> 32))
            .ThenBy(key => (int)key)
            .Select(key => ((int)(key >> 32)).ToString(CultureInfo.InvariantCulture) + ":" + ((int)key).ToString(CultureInfo.InvariantCulture))
            .ToList();
    }

    public int Count { get { return _translations.Count; } }
    public int CoverageCount { get { return _coverageCount; } }
    public int MissingCount { get { return _missingKeys.Count; } }
    public IList<string> MissingKeys { get { return _missingKeys.AsReadOnly(); } }

    public static DialogueTranslationCatalog FromTsv(string contents)
    {
        if (contents == null) throw new ArgumentNullException("contents");

        var translations = new Dictionary<long, string>();
        AddTranslations(translations, contents, "dialogue.tsv");
        return new DialogueTranslationCatalog(translations, new HashSet<long>());
    }

    public static DialogueTranslationCatalog FromDirectory(string directory)
    {
        if (directory == null) throw new ArgumentNullException("directory");
        if (!Directory.Exists(directory)) throw new DirectoryNotFoundException("Dialogue translation directory was not found: " + directory);

        var translations = new Dictionary<long, string>();
        var baseFile = Path.Combine(directory, "dialogue.tsv");
        if (File.Exists(baseFile)) AddTranslations(translations, File.ReadAllText(baseFile, Encoding.UTF8), baseFile);

        var shardFiles = Directory.GetFiles(directory, "dialogue-*.tsv", SearchOption.TopDirectoryOnly)
            .Where(path => !String.Equals(Path.GetFileName(path), "dialogue-coverage.tsv", StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => Path.GetFileName(path), StringComparer.Ordinal)
            .ToList();
        foreach (var path in shardFiles) AddTranslations(translations, File.ReadAllText(path, Encoding.UTF8), path);

        var coverage = new HashSet<long>();
        var coveragePath = Path.Combine(directory, "dialogue-coverage.tsv");
        if (File.Exists(coveragePath)) AddCoverage(coverage, File.ReadAllText(coveragePath, Encoding.UTF8), coveragePath);
        return new DialogueTranslationCatalog(translations, coverage);
    }

    private static void AddTranslations(Dictionary<long, string> translations, string contents, string sourceName)
    {
        using (var reader = new StringReader(contents))
        {
            string line;
            var lineNumber = 0;
            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;
                if (String.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal)) continue;

                var columns = line.Split(new[] { '\t' }, 3);
                if (columns.Length != 3) throw new FormatException("Expected conversation ID, entry ID, and Italian text in " + sourceName + " line " + lineNumber + ".");

                int conversationId;
                int entryId;
                if (!Int32.TryParse(columns[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out conversationId) ||
                    !Int32.TryParse(columns[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out entryId))
                {
                    throw new FormatException("Invalid runtime key in " + sourceName + " line " + lineNumber + ".");
                }

                var italian = Unescape(columns[2]);
                if (String.IsNullOrWhiteSpace(italian)) continue;

                var key = MakeKey(conversationId, entryId);
                if (translations.ContainsKey(key)) throw new InvalidOperationException("Duplicate dialogue translation key in " + sourceName + " line " + lineNumber + ".");
                translations.Add(key, italian);
            }
        }
    }

    private static void AddCoverage(HashSet<long> coverage, string contents, string sourceName)
    {
        using (var reader = new StringReader(contents))
        {
            string line;
            var lineNumber = 0;
            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;
                if (String.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal)) continue;
                var columns = line.Split('\t');
                int conversationId;
                int entryId;
                if (columns.Length < 2 ||
                    !Int32.TryParse(columns[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out conversationId) ||
                    !Int32.TryParse(columns[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out entryId))
                {
                    throw new FormatException("Invalid coverage key in " + sourceName + " line " + lineNumber + ".");
                }
                if (!coverage.Add(MakeKey(conversationId, entryId)))
                    throw new InvalidOperationException("Duplicate dialogue coverage key in " + sourceName + " line " + lineNumber + ".");
            }
        }
    }

    public bool TryGet(int conversationId, int entryId, out string italian)
    {
        return _translations.TryGetValue(MakeKey(conversationId, entryId), out italian);
    }

    private static long MakeKey(int conversationId, int entryId)
    {
        return ((long)conversationId << 32) | (uint)entryId;
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

            switch (value[++i])
            {
                case 'n': result.Append('\n'); break;
                case 'r': result.Append('\r'); break;
                case 't': result.Append('\t'); break;
                case '\\': result.Append('\\'); break;
                default:
                    result.Append('\\');
                    result.Append(value[i]);
                    break;
            }
        }
        return result.ToString();
    }
}

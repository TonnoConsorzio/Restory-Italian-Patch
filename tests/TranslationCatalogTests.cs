using System;
using System.IO;
using RestoryItalianPatch;

internal static class TranslationCatalogTests
{
    private static int _assertions;

    private static void Main()
    {
        LoadsEscapedTranslationsAndFallsBackForUnknownKeys();
        PreservesTranslationTokens();
        LoadsSortedTranslationShards();
        RejectsDuplicateKeysAcrossTranslationShards();
        Console.WriteLine("PASS: " + _assertions + " translation assertions");
    }

    private static void LoadsEscapedTranslationsAndFallsBackForUnknownKeys()
    {
        WithMap("MENU\tMenu\nEMAIL\t\u00c8 arrivata una nuova email\\nApri\\tora\n",
            catalog =>
            {
                Equal("Menu", catalog.Translate("MENU", "Menu"), "known key translated");
                Equal("È arrivata una nuova email\nApri\tora", catalog.Translate("EMAIL", "English"), "escaped newline and tab decoded");
                Equal("English", catalog.Translate("MISSING", "English"), "unknown key falls back to source");
            });
    }

    private static void PreservesTranslationTokens()
    {
        WithMap("TEMPLATE\tRipara {0} <b>{device}</b> %s\n", catalog =>
        {
            var source = "Repair {0} <b>{device}</b> %s";
            var translated = catalog.Translate("TEMPLATE", source);
            Equal("Ripara {0} <b>{device}</b> %s", translated, "known translation returned");
            Equal(true, TranslationCatalog.HasSameTokens(source, translated), "format and rich text tokens retained");
            Equal(false, TranslationCatalog.HasSameTokens("Repair {0} <b>item</b>", "Ripara <b>oggetto</b>"), "missing format token is detected");
        });
    }

    private static void LoadsSortedTranslationShards()
    {
        WithDirectory(directory =>
        {
            File.WriteAllText(Path.Combine(directory, "ui.tsv"), "BASE\tBase\n");
            File.WriteAllText(Path.Combine(directory, "ui-z.tsv"), "LAST\tUltimo\n");
            File.WriteAllText(Path.Combine(directory, "ui-a.tsv"), "FIRST\tPrimo\n");
            var catalog = TranslationCatalog.LoadDirectory(directory);
            Equal(3, catalog.Count, "base map and sorted shards are loaded");
            Equal("Primo", catalog.Translate("FIRST", "First"), "first shard is loaded");
            Equal("Ultimo", catalog.Translate("LAST", "Last"), "last shard is loaded");
        });
    }

    private static void RejectsDuplicateKeysAcrossTranslationShards()
    {
        WithDirectory(directory =>
        {
            File.WriteAllText(Path.Combine(directory, "ui.tsv"), "DUPLICATE\tPrima\n");
            File.WriteAllText(Path.Combine(directory, "ui-extra.tsv"), "DUPLICATE\tSeconda\n");
            Throws<InvalidDataException>(() => TranslationCatalog.LoadDirectory(directory));
        });
    }

    private static void WithDirectory(Action<string> test)
    {
        var path = Path.Combine(Path.GetTempPath(), "restory-it-shards-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        try { test(path); }
        finally { if (Directory.Exists(path)) Directory.Delete(path, true); }
    }

    private static void WithMap(string contents, Action<TranslationCatalog> test)
    {
        var path = Path.Combine(Path.GetTempPath(), "restory-it-map-" + Guid.NewGuid().ToString("N") + ".tsv");
        try
        {
            File.WriteAllText(path, contents);
            test(TranslationCatalog.Load(path));
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    private static void Equal(string expected, string actual, string message)
    {
        if (!String.Equals(expected, actual, StringComparison.Ordinal)) throw new Exception(message + ": expected [" + expected + "], got [" + actual + "]");
        _assertions++;
    }

    private static void Equal(bool expected, bool actual, string message)
    {
        if (expected != actual) throw new Exception(message + ": expected " + expected + ", got " + actual);
        _assertions++;
    }

    private static void Equal(int expected, int actual, string message)
    {
        if (expected != actual) throw new Exception(message + ": expected " + expected + ", got " + actual);
        _assertions++;
    }

    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { _assertions++; return; }
        throw new Exception("Expected " + typeof(T).Name);
    }
}

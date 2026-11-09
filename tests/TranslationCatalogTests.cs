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
}

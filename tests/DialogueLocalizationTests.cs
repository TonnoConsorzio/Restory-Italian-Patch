using System;
using System.IO;

internal static class DialogueLocalizationTests
{
    private static int _assertions;

    private static void Main(string[] args)
    {
        LoadsItalianTranslationsByConversationAndEntry();
        MissingEntriesFallBack();
        DuplicateRuntimeKeysAreRejected();
        TranslationAppliesOnlyForItalianLanguage();
        DirectoryLoaderReportsMissingCoverageAndRejectsCrossShardDuplicates();
        if (args.Length > 0) ValidatesPayloadCoverage(args[0]);
        Console.WriteLine("PASS: " + _assertions + " dialogue localization assertions");
    }

    private static void LoadsItalianTranslationsByConversationAndEntry()
    {
        var catalog = DialogueTranslationCatalog.FromTsv(
            "4\t5\tChiedere come iscriversi alla gara\n4\t6\tPrima riga\\nSeconda riga");
        Assert(catalog.Count == 2, "catalog count was not reported");
        string translation;
        Assert(catalog.TryGet(4, 5, out translation), "mapped entry was not found");
        Assert(translation == "Chiedere come iscriversi alla gara", "wrong Italian value returned");
        Assert(catalog.TryGet(4, 6, out translation), "second entry was not loaded");
        Assert(translation == "Prima riga\nSeconda riga", "escaped line break was not restored");
        Assert(!catalog.TryGet(4, 7, out translation), "unknown entry received another entry's translation");
    }

    private static void MissingEntriesFallBack()
    {
        var catalog = DialogueTranslationCatalog.FromTsv("");
        string translation;
        Assert(!catalog.TryGet(1, 1, out translation), "empty catalog should use the game's fallback");
        Assert(translation == null, "missing translation should not synthesize text");
    }

    private static void DuplicateRuntimeKeysAreRejected()
    {
        const string tsv = "4\t5\tUno\n4\t5\tDue";
        AssertThrows<InvalidOperationException>(() => DialogueTranslationCatalog.FromTsv(tsv));
    }

    private static void TranslationAppliesOnlyForItalianLanguage()
    {
        var catalog = DialogueTranslationCatalog.FromTsv("4\t5\tTesto italiano");
        string translation;
        Assert(DialogueTranslationRuntime.TryResolve(catalog, "it", 4, 5, out translation), "Italian dialogue was not resolved");
        Assert(translation == "Testo italiano", "runtime returned the wrong value");
        Assert(!DialogueTranslationRuntime.TryResolve(catalog, "en", 4, 5, out translation), "English dialogue was replaced");
        Assert(!DialogueTranslationRuntime.TryResolve(catalog, "it", 4, 6, out translation), "unmapped dialogue should fall back");
    }

    private static void DirectoryLoaderReportsMissingCoverageAndRejectsCrossShardDuplicates()
    {
        var directory = Path.Combine(Path.GetTempPath(), "RestoryCatalog-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllText(Path.Combine(directory, "dialogue.tsv"), "4\t5\tUno");
            File.WriteAllText(Path.Combine(directory, "dialogue-0002.tsv"), "4\t6\tDue");
            File.WriteAllText(Path.Combine(directory, "dialogue-coverage.tsv"), "4\t5\t0x5\n4\t6\t0x6\n4\t7\t0x7");

            var catalog = DialogueTranslationCatalog.FromDirectory(directory);
            Assert(catalog.Count == 2, "base file and shards were not loaded");
            Assert(catalog.CoverageCount == 3, "coverage key count was not loaded");
            Assert(catalog.MissingCount == 1, "missing translation coverage was not reported");
            Assert(catalog.MissingKeys[0] == "4:7", "missing runtime key was not reported");

            File.WriteAllText(Path.Combine(directory, "dialogue-0001.tsv"), "4\t5\tDuplicate");
            AssertThrows<InvalidOperationException>(() => DialogueTranslationCatalog.FromDirectory(directory));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    private static void ValidatesPayloadCoverage(string directory)
    {
        var catalog = DialogueTranslationCatalog.FromDirectory(directory);
        Assert(catalog.Count > 0, "payload has no Italian dialogue seed entries");
        Assert(catalog.CoverageCount == 1132, "payload coverage does not match the current runtime key set");
        Assert(catalog.Count <= catalog.CoverageCount, "translation count exceeds the coverage set");
        Assert(catalog.MissingCount == catalog.CoverageCount - catalog.Count, "missing-key report does not match coverage");
        Assert(catalog.MissingCount == 0, "release payload is missing Italian dialogue translations");
        Assert(catalog.MissingKeys.Count == catalog.MissingCount, "missing-key list is incomplete");

        string translation;
        Assert(catalog.TryGet(156, 13, out translation), "new Fuzzby dialogue key 156/13 was not translated");
        Assert(translation == "Giochi per PC, direttamente in tasca. Su un dispositivo portatile che usa Linux! Riesci a immaginarlo? Lo so, sembra assurdo.", "new Fuzzby dialogue translation changed");

        var coverage = File.ReadAllLines(Path.Combine(directory, "dialogue-coverage.tsv"));
        Assert(Array.Exists(coverage, line => line == "156\t13\t0x0100000000006FEC"), "new Fuzzby dialogue Articy ID was not mapped to runtime key 156/13");
    }

    private static void Assert(bool condition, string message)
    {
        _assertions++;
        if (!condition) throw new Exception(message);
    }

    private static void AssertThrows<T>(Action action) where T : Exception
    {
        _assertions++;
        try { action(); }
        catch (T) { return; }
        throw new Exception("Expected " + typeof(T).Name + ".");
    }
}

using System;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Unity.Mono;
using HarmonyLib;
using PixelCrushers.DialogueSystem;

[BepInPlugin("restory.italian.dialogue", "Restory Italian Dialogue", "0.1.0")]
public sealed class DialogueLocalizationPlugin : BaseUnityPlugin
{
    private Harmony _harmony;

    internal static DialogueTranslationCatalog Catalog { get; private set; }

    private void Awake()
    {
        var directory = Path.Combine(Paths.PluginPath, "RestoryItalianDialogue");
        try
        {
            Catalog = DialogueTranslationCatalog.FromDirectory(directory);
            Logger.LogInfo("Loaded " + Catalog.Count + " keyed Italian dialogue translations; " +
                Catalog.MissingCount + " of " + Catalog.CoverageCount + " runtime entries are untranslated.");
            if (Catalog.MissingCount > 0)
            {
                var sample = String.Join(", ", Catalog.MissingKeys.Take(10).ToArray());
                Logger.LogWarning("Missing dialogue keys (first 10): " + sample);
            }
        }
        catch (Exception ex)
        {
            Catalog = DialogueTranslationCatalog.FromTsv(String.Empty);
            Logger.LogError("Could not load dialogue translations: " + ex.Message);
        }

        _harmony = new Harmony("restory.italian.dialogue");
        _harmony.PatchAll(typeof(DialogueLocalizationPlugin).Assembly);
    }

    private void OnDestroy()
    {
        if (_harmony != null) _harmony.UnpatchSelf();
    }
}

[HarmonyPatch(typeof(StandardDialogueUI), "ShowSubtitle", new Type[] { typeof(Subtitle) })]
internal static class ItalianSubtitlePatch
{
    private static void Prefix(Subtitle subtitle)
    {
        if (subtitle == null || subtitle.dialogueEntry == null || subtitle.formattedText == null) return;

        string italian;
        var entry = subtitle.dialogueEntry;
        if (DialogueTranslationRuntime.TryResolve(DialogueLocalizationPlugin.Catalog, Localization.language,
            entry.conversationID, entry.id, out italian))
        {
            subtitle.formattedText.text = italian;
        }
    }
}

[HarmonyPatch(typeof(StandardDialogueUI), "ShowResponses", new Type[] { typeof(Subtitle), typeof(Response[]), typeof(float) })]
internal static class ItalianResponsesPatch
{
    private static void Prefix(Response[] responses)
    {
        if (responses == null) return;

        var language = Localization.language;
        var catalog = DialogueLocalizationPlugin.Catalog;
        for (var i = 0; i < responses.Length; i++)
        {
            var response = responses[i];
            if (response == null || response.destinationEntry == null || response.formattedText == null) continue;

            string italian;
            var target = response.destinationEntry;
            if (DialogueTranslationRuntime.TryResolve(catalog, language, target.conversationID, target.id, out italian))
            {
                response.formattedText.text = italian;
            }
        }
    }
}

using System;

internal static class DialogueTranslationRuntime
{
    public static bool TryResolve(DialogueTranslationCatalog catalog, string language, int conversationId, int entryId, out string italian)
    {
        italian = null;
        return catalog != null && String.Equals(language, "it", StringComparison.OrdinalIgnoreCase) &&
            catalog.TryGet(conversationId, entryId, out italian);
    }
}

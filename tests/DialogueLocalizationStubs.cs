using System;

namespace BepInEx
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class BepInPlugin : Attribute
    {
        public BepInPlugin(string guid, string name, string version) { }
    }
}

namespace BepInEx.Unity.Mono
{
    public class BaseUnityPlugin
    {
        protected static class Paths { public static string PluginPath { get { return ""; } } }
        protected class Log { public void LogInfo(string message) { } public void LogWarning(string message) { } public void LogError(string message) { } }
        protected Log Logger = new Log();
    }
}

namespace HarmonyLib
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class HarmonyPatch : Attribute
    {
        public HarmonyPatch(Type type, string method, Type[] arguments) { }
    }
    public sealed class Harmony
    {
        public Harmony(string id) { }
        public void PatchAll(System.Reflection.Assembly assembly) { }
        public void UnpatchSelf() { }
    }
}

namespace PixelCrushers.DialogueSystem
{
    public sealed class StandardDialogueUI { }
    public sealed class Subtitle { public DialogueEntry dialogueEntry; public FormattedText formattedText; }
    public sealed class DialogueEntry { public int conversationID; public int id; }
    public sealed class FormattedText { public string text; }
    public sealed class Response { public DialogueEntry destinationEntry; public FormattedText formattedText; }
    public static class Localization { public static string language; }
}

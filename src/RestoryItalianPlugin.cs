using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Unity.Mono;
using HarmonyLib;
using UnityEngine;

namespace RestoryItalianPatch
{
    [BepInPlugin("it.alessio.restory.italian", "Restory Italian Patch", "0.2.0")]
    public sealed class RestoryItalianPlugin : BaseUnityPlugin
    {
        private static TranslationCatalog _catalog;
        private static bool _italianActive;
        private Harmony _harmony;

        private void Awake()
        {
            try
            {
                var translationDirectory = Path.GetDirectoryName(Info.Location);
                _catalog = TranslationCatalog.LoadDirectory(translationDirectory);
                _italianActive = Application.systemLanguage == SystemLanguage.Italian;
                _harmony = new Harmony("it.alessio.restory.italian");

                PatchFirst("Restory.Data.GameConfigs.GameConfig", "get_SupportedLocalizations", postfix: "SupportedLocalizationsPostfix");
                PatchFirst("Restory.Gameplay.GameSettings.GameSettingsManager", "set_Localization", postfix: "LocalizationSetterPostfix");
                PatchFirst("Restory.Data.Localization.LocalizationSystem", "GetTranslation", postfix: "GetTranslationPostfix");
                Logger.LogInfo("Loaded " + _catalog.Count + " Italian UI translations.");
            }
            catch (Exception ex)
            {
                Logger.LogError("Italian localization could not be initialized: " + ex);
            }
        }

        private void PatchFirst(string typeName, string methodName, string prefix = null, string postfix = null)
        {
            var target = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(typeName, false))
                .FirstOrDefault(type => type != null);
            if (target == null)
            {
                Logger.LogWarning("Could not find game type " + typeName + ".");
                return;
            }

            var methods = target.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(candidate => candidate.Name == methodName);
            if (methodName == "GetTranslation")
                methods = methods.Where(candidate => candidate.ReturnType == typeof(string) && candidate.GetParameters().Length > 0 && candidate.GetParameters()[0].ParameterType == typeof(string));
            var method = methods.OrderBy(item => item.GetParameters().Length).FirstOrDefault();
            if (method == null)
            {
                Logger.LogWarning("Could not find " + typeName + "." + methodName + ".");
                return;
            }

            var prefixMethod = prefix == null ? null : typeof(RestoryItalianPlugin).GetMethod(prefix, BindingFlags.Static | BindingFlags.NonPublic);
            var postfixMethod = postfix == null ? null : typeof(RestoryItalianPlugin).GetMethod(postfix, BindingFlags.Static | BindingFlags.NonPublic);
            _harmony.Patch(method,
                prefixMethod == null ? null : new HarmonyMethod(prefixMethod),
                postfixMethod == null ? null : new HarmonyMethod(postfixMethod));
            Logger.LogInfo("Patched " + typeName + "." + methodName + ".");
        }

        private static void SupportedLocalizationsPostfix(ref IReadOnlyCollection<SystemLanguage> __result)
        {
            if (__result == null) return;
            if (__result.Contains(SystemLanguage.Italian)) return;
            __result = __result.Concat(new[] { SystemLanguage.Italian }).ToArray();
        }

        private static void LocalizationSetterPostfix(object[] __args)
        {
            if (__args == null || __args.Length == 0 || __args[0] == null) return;
            _italianActive = String.Equals(__args[0].ToString(), "Italian", StringComparison.OrdinalIgnoreCase);
        }

        private static void GetTranslationPostfix(object[] __args, ref string __result)
        {
            if (!_italianActive || _catalog == null || __args == null || __args.Length == 0 || __args[0] == null) return;
            __result = _catalog.Translate(__args[0].ToString(), __result);
        }
    }
}

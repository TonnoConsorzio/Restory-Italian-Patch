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
                _italianActive = false;
                _harmony = new Harmony("it.alessio.restory.italian");

                PatchFirst("Restory.Data.GameConfigs.GameConfig", "get_SupportedLocalizations", postfix: "SupportedLocalizationsPostfix");
                PatchFirst("Restory.UI.Presenters.SettingsMenu.GUI_SettingsMenu", "ResolveOnLanguageChanged", postfix: "LanguageOptionsPostfix");
                PatchFirst("Restory.UI.Views.SettingsMenu.GUI_SettingsMenuMainView", "SetLanguageOptions", postfix: "LanguageOptionsValuesPostfix");
                PatchFirst("Restory.Gameplay.GameSettings.GameSettingsManager", "get_Localization", postfix: "LocalizationGetterPostfix");
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
            if (__result.Contains(SystemLanguage.Italian))
                return;
            var languages = __result.ToList();
            var englishIndex = languages.IndexOf(SystemLanguage.English);
            languages.Insert(englishIndex < 0 ? 0 : englishIndex + 1, SystemLanguage.Italian);
            __result = languages.ToArray();
        }

        private static void LanguageOptionsPostfix(object __instance)
        {
            if (__instance == null) return;
            var field = __instance.GetType().GetField("availableLanguages", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var languages = field == null ? null : field.GetValue(__instance) as IEnumerable<SystemLanguage>;
            if (languages != null && !languages.Contains(SystemLanguage.Italian))
            {
                var list = languages.ToList();
                var englishIndex = list.IndexOf(SystemLanguage.English);
                list.Insert(englishIndex < 0 ? 0 : englishIndex + 1, SystemLanguage.Italian);
                field.SetValue(__instance, list);
            }
        }

        private static void LanguageOptionsValuesPostfix(object __instance, List<string> __0)
        {
            if (__0 == null) return;
            var italianIndex = __0.FindIndex(language => String.Equals(language, "Italian", StringComparison.OrdinalIgnoreCase));
            if (italianIndex < 0) return;
            __0[italianIndex] = "Italiano";
            var viewType = __instance == null ? null : __instance.GetType();
            var fields = viewType == null ? new FieldInfo[0] : viewType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var dropdownType = AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetType("TMPro.TMP_Dropdown", false)).FirstOrDefault(type => type != null);
            var dropdownField = dropdownType == null ? null : fields.FirstOrDefault(field => dropdownType.IsAssignableFrom(field.FieldType));
            var dropdown = dropdownField == null ? null : dropdownField.GetValue(__instance);
            var optionsProperty = dropdownType == null ? null : dropdownType.GetProperty("options", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var actualOptions = dropdown == null || optionsProperty == null ? null : optionsProperty.GetValue(dropdown, null) as System.Collections.IEnumerable;
            if (actualOptions != null)
            {
                foreach (var option in actualOptions.Cast<object>())
                {
                    var textProperty = option.GetType().GetProperty("text", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (textProperty != null && String.Equals(Convert.ToString(textProperty.GetValue(option, null)), "Italian", StringComparison.OrdinalIgnoreCase))
                        textProperty.SetValue(option, "Italiano", null);
                }
            }
            var refresh = dropdownType == null ? null : dropdownType.GetMethod("RefreshShownValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (refresh != null && dropdown != null) refresh.Invoke(dropdown, null);
        }

        private static void LocalizationSetterPostfix(object[] __args)
        {
            if (__args == null || __args.Length == 0 || __args[0] == null) return;
            _italianActive = String.Equals(__args[0].ToString(), "Italian", StringComparison.OrdinalIgnoreCase);
        }

        private static void LocalizationGetterPostfix(ref SystemLanguage __result)
        {
            _italianActive = __result == SystemLanguage.Italian;
        }

        private static void GetTranslationPostfix(object[] __args, ref string __result)
        {
            if (!_italianActive || _catalog == null || __args == null || __args.Length == 0 || __args[0] == null) return;
            __result = _catalog.Translate(__args[0].ToString(), __result);
        }
    }
}

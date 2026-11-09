using BepInEx;
using BepInEx.Unity.Mono;
using HarmonyLib;
using Restory.UI.Presenters.MainMenu;
using Restory.UI.Presenters.SaveSlots;
using Restory.UI.Presenters.SettingsMenu;
using UnityEngine;
using UnityEngine.UI;

namespace RestoryItalianCredits
{
    [BepInPlugin("alessio.bellan.restoryitaliancredits", "Restory Italian Credits", "1.0.0")]
    public sealed class RestoryItalianCreditsPlugin : BaseUnityPlugin
    {
        private static CreditOverlayBehaviour _overlay;

        private void Awake()
        {
            var overlayObject = new GameObject("RestoryItalianCredits");
            DontDestroyOnLoad(overlayObject);
            _overlay = overlayObject.AddComponent<CreditOverlayBehaviour>();
            new Harmony("alessio.bellan.restoryitaliancredits").PatchAll();
        }

        internal static void SetHomeScreenActive(bool active)
        {
            if (_overlay != null) _overlay.SetMainMenuActive(active);
        }

        internal static void SetSettingsVisible(bool visible)
        {
            if (_overlay != null) _overlay.SetSettingsVisible(visible);
        }

        internal static void SetSaveSlotsVisible(bool visible)
        {
            if (_overlay != null) _overlay.SetSaveSlotsVisible(visible);
        }
    }

    [HarmonyPatch(typeof(GUI_MainMenu), "OnEnable")]
    internal static class MainMenuEnabledPatch
    {
        private static void Postfix()
        {
            RestoryItalianCreditsPlugin.SetHomeScreenActive(true);
        }
    }

    [HarmonyPatch(typeof(GUI_MainMenu), "OnDisable")]
    internal static class MainMenuDisabledPatch
    {
        private static void Postfix()
        {
            RestoryItalianCreditsPlugin.SetHomeScreenActive(false);
        }
    }

    [HarmonyPatch(typeof(GUI_SettingsMenu), "Show")]
    internal static class SettingsMenuShownPatch
    {
        private static void Postfix()
        {
            RestoryItalianCreditsPlugin.SetSettingsVisible(true);
        }
    }

    [HarmonyPatch(typeof(GUI_SettingsMenu), "Hide")]
    internal static class SettingsMenuHiddenPatch
    {
        private static void Postfix()
        {
            RestoryItalianCreditsPlugin.SetSettingsVisible(false);
        }
    }

    [HarmonyPatch(typeof(GUI_SaveSlotsPanel), "Show")]
    internal static class SaveSlotsShownPatch
    {
        private static void Postfix()
        {
            RestoryItalianCreditsPlugin.SetSaveSlotsVisible(true);
        }
    }

    [HarmonyPatch(typeof(GUI_SaveSlotsPanel), "Hide")]
    internal static class SaveSlotsHiddenPatch
    {
        private static void Postfix()
        {
            RestoryItalianCreditsPlugin.SetSaveSlotsVisible(false);
        }
    }

    internal sealed class CreditOverlayBehaviour : MonoBehaviour
    {
        private Canvas _canvas;
        private bool _mainMenuActive;
        private bool _settingsVisible;
        private bool _saveSlotsVisible;

        private void Awake()
        {
            var canvasObject = new GameObject("CreditCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(transform, false);
            _canvas = canvasObject.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var labelObject = new GameObject("CreditText", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(canvasObject.transform, false);
            var rect = (RectTransform)labelObject.transform;
            rect.anchorMin = new Vector2(CreditOverlayDefinition.HorizontalAnchor, CreditOverlayDefinition.VerticalAnchor);
            rect.anchorMax = new Vector2(CreditOverlayDefinition.HorizontalAnchor, CreditOverlayDefinition.VerticalAnchor);
            rect.pivot = new Vector2(CreditOverlayDefinition.PivotX, CreditOverlayDefinition.PivotY);
            rect.sizeDelta = new Vector2(CreditOverlayDefinition.Width, CreditOverlayDefinition.Height);
            rect.anchoredPosition = new Vector2(-CreditOverlayDefinition.RightMargin, CreditOverlayDefinition.BottomMargin);

            var label = labelObject.GetComponent<Text>();
            label.text = CreditOverlayDefinition.Text;
            label.font = Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI Emoji", "Arial" }, 22);
            label.fontSize = 18;
            label.color = Color.white;
            label.alignment = TextAnchor.LowerRight;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.supportRichText = false;
            label.raycastTarget = false;

            ApplyVisibility();
        }

        internal void SetMainMenuActive(bool active)
        {
            _mainMenuActive = active;
            ApplyVisibility();
        }

        internal void SetSettingsVisible(bool visible)
        {
            _settingsVisible = visible;
            ApplyVisibility();
        }

        internal void SetSaveSlotsVisible(bool visible)
        {
            _saveSlotsVisible = visible;
            ApplyVisibility();
        }

        private void ApplyVisibility()
        {
            if (_canvas != null) _canvas.enabled = CreditOverlayDefinition.ShouldBeVisible(_mainMenuActive, _settingsVisible, _saveSlotsVisible);
        }
    }
}

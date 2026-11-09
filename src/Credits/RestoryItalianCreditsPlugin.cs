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

            var cardObject = new GameObject("CreditCard", typeof(RectTransform), typeof(Image));
            cardObject.transform.SetParent(canvasObject.transform, false);
            var rect = (RectTransform)cardObject.transform;
            rect.anchorMin = new Vector2(CreditOverlayDefinition.HorizontalAnchor, CreditOverlayDefinition.VerticalAnchor);
            rect.anchorMax = new Vector2(CreditOverlayDefinition.HorizontalAnchor, CreditOverlayDefinition.VerticalAnchor);
            rect.pivot = new Vector2(CreditOverlayDefinition.PivotX, CreditOverlayDefinition.PivotY);
            rect.sizeDelta = new Vector2(CreditOverlayDefinition.Width, CreditOverlayDefinition.Height);
            rect.anchoredPosition = new Vector2(-CreditOverlayDefinition.RightMargin, CreditOverlayDefinition.BottomMargin);

            var background = cardObject.GetComponent<Image>();
            background.color = new Color(0.035f, 0.055f, 0.085f, 0.82f);
            background.raycastTarget = false;

            var accentObject = new GameObject("CreditAccent", typeof(RectTransform), typeof(Image));
            accentObject.transform.SetParent(cardObject.transform, false);
            var accentRect = (RectTransform)accentObject.transform;
            accentRect.anchorMin = new Vector2(0f, 0f);
            accentRect.anchorMax = new Vector2(0f, 1f);
            accentRect.pivot = new Vector2(0f, 0.5f);
            accentRect.sizeDelta = new Vector2(3f, 0f);
            accentRect.anchoredPosition = Vector2.zero;
            var accent = accentObject.GetComponent<Image>();
            accent.color = new Color(0.91f, 0.69f, 0.38f, 1f);
            accent.raycastTarget = false;

            CreateAttributionLine(cardObject.transform);
            CreateCreditLabel(cardObject.transform, "CreditSocial", CreditOverlayDefinition.Social,
                CreditOverlayDefinition.SocialFontSize, FontStyle.Normal, TextAnchor.MiddleLeft,
                new Vector2(0f, 0f), new Vector2(1f, 0.5f), new Vector2(16f, 5f), new Vector2(-12f, 0f));

            ApplyVisibility();
        }

        private static void CreateAttributionLine(Transform parent)
        {
            var lineObject = new GameObject("CreditAttribution", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            lineObject.transform.SetParent(parent, false);
            var rect = (RectTransform)lineObject.transform;
            rect.anchorMin = new Vector2(0f, 0.46f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = new Vector2(0f, -5f);

            var layout = lineObject.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(16, 12, 0, 0);
            layout.spacing = 5f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            CreateInlineLabel(lineObject.transform, "CreditAttributionPrefix", CreditOverlayDefinition.AttributionPrefix);
            CreateEmojiIcon(lineObject.transform, "CreditHeart", CreateHeartSprite());
            CreateInlineLabel(lineObject.transform, "CreditAttributionConnector", CreditOverlayDefinition.AttributionConnector);
            CreateEmojiIcon(lineObject.transform, "CreditPizza", CreatePizzaSprite());
            CreateInlineLabel(lineObject.transform, "CreditAttributionSuffix", CreditOverlayDefinition.AttributionSuffix);
        }

        private static void CreateInlineLabel(Transform parent, string name, string text)
        {
            var labelObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(parent, false);
            var label = labelObject.GetComponent<Text>();
            label.text = text;
            label.font = Font.CreateDynamicFontFromOSFont("Arial", CreditOverlayDefinition.AttributionFontSize + 2);
            label.fontSize = CreditOverlayDefinition.AttributionFontSize;
            label.fontStyle = FontStyle.Bold;
            label.color = new Color(1f, 0.97f, 0.91f, 1f);
            label.alignment = TextAnchor.MiddleLeft;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
        }

        private static void CreateEmojiIcon(Transform parent, string name, Sprite sprite)
        {
            var iconObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            iconObject.transform.SetParent(parent, false);
            var image = iconObject.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            var layout = iconObject.GetComponent<LayoutElement>();
            layout.preferredWidth = 22f;
            layout.preferredHeight = 22f;
            layout.flexibleWidth = 0f;
            layout.flexibleHeight = 0f;
        }

        private static Sprite CreateHeartSprite()
        {
            const int size = 96;
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            {
                var v = 1f - y / (size - 1f);
                for (var x = 0; x < size; x++)
                {
                    var u = x / (size - 1f);
                    var inLobe = IsNear(u, v, 0.31f, 0.31f, 0.29f) || IsNear(u, v, 0.69f, 0.31f, 0.29f);
                    var triangleLeft = 0.03f + (v - 0.36f) * 0.47f / 0.59f;
                    var triangleRight = 0.97f - (v - 0.36f) * 0.47f / 0.59f;
                    var inPoint = v >= 0.36f && v <= 0.95f && u >= triangleLeft && u <= triangleRight;
                    if (!inLobe && !inPoint) continue;
                    var highlight = u < 0.35f && v < 0.34f && IsNear(u, v, 0.27f, 0.2f, 0.09f);
                    pixels[y * size + x] = highlight ? new Color32(255, 171, 165, 255) : new Color32(232, 56, 69, 255);
                }
            }
            return CreateSprite(size, pixels);
        }

        private static Sprite CreatePizzaSprite()
        {
            const int size = 96;
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            {
                var v = y / (size - 1f);
                for (var x = 0; x < size; x++)
                {
                    var u = x / (size - 1f);
                    var leftEdge = 0.5f - (v - 0.08f) * 0.52f;
                    var rightEdge = 0.5f + (v - 0.08f) * 0.52f;
                    if (v < 0.08f || v > 0.88f || u < leftEdge || u > rightEdge) continue;

                    var color = v > 0.72f
                        ? new Color32(177, 104, 53, 255)
                        : new Color32(255, 197, 72, 255);
                    if (v <= 0.72f && (IsNear(u, v, 0.40f, 0.36f, 0.075f) ||
                        IsNear(u, v, 0.62f, 0.55f, 0.075f) || IsNear(u, v, 0.36f, 0.62f, 0.07f)))
                        color = new Color32(217, 61, 52, 255);
                    else if (v <= 0.72f && (IsNear(u, v, 0.53f, 0.4f, 0.027f) || IsNear(u, v, 0.48f, 0.62f, 0.025f)))
                        color = new Color32(70, 133, 70, 255);
                    pixels[y * size + x] = color;
                }
            }
            return CreateSprite(size, pixels);
        }

        private static bool IsNear(float x, float y, float centerX, float centerY, float radius)
        {
            var dx = x - centerX;
            var dy = y - centerY;
            return dx * dx + dy * dy <= radius * radius;
        }

        private static Sprite CreateSprite(int size, Color32[] pixels)
        {
            var texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
            texture.SetPixels32(pixels);
            texture.filterMode = FilterMode.Bilinear;
            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static void CreateCreditLabel(Transform parent, string name, string text, int fontSize,
            FontStyle style, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var labelObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(parent, false);
            var rect = (RectTransform)labelObject.transform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            var label = labelObject.GetComponent<Text>();
            label.text = text;
            label.font = Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI Emoji", "Arial" }, fontSize + 2);
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.color = new Color(1f, 0.97f, 0.91f, 1f);
            label.alignment = alignment;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.supportRichText = false;
            label.raycastTarget = false;
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

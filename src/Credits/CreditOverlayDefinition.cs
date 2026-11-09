namespace RestoryItalianCredits
{
    public static class CreditOverlayDefinition
    {
        public const string Text = "Tradotto con ❤️ e 🍕 da Alessio | IG alessio.bellan";
        public const float HorizontalAnchor = 1f;
        public const float VerticalAnchor = 0f;
        public const float PivotX = 1f;
        public const float PivotY = 0f;
        public const float RightMargin = 24f;
        public const float BottomMargin = 18f;
        public const float Width = 660f;
        public const float Height = 36f;

        public static bool ShouldBeVisible(bool homeScreenActive, bool settingsVisible, bool saveSlotsVisible)
        {
            return homeScreenActive && !settingsVisible && !saveSlotsVisible;
        }
    }
}

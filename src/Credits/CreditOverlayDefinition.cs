namespace RestoryItalianCredits
{
    public static class CreditOverlayDefinition
    {
        public const string AttributionPrefix = "Tradotto con";
        public const string AttributionConnector = "e";
        public const string AttributionSuffix = "da Alessio";
        public const string Attribution = AttributionPrefix + " ❤️ " + AttributionConnector + " 🍕 " + AttributionSuffix;
        public const string Social = "IG alessio.bellan";
        public const string Text = Attribution + " | " + Social;
        public const float HorizontalAnchor = 1f;
        public const float VerticalAnchor = 0f;
        public const float PivotX = 1f;
        public const float PivotY = 0f;
        public const float RightMargin = 28f;
        public const float BottomMargin = 190f;
        public const float Width = 500f;
        public const float Height = 64f;
        public const int AttributionFontSize = 15;
        public const int SocialFontSize = 12;

        public static bool ShouldBeVisible(bool homeScreenActive, bool settingsVisible, bool saveSlotsVisible)
        {
            return homeScreenActive && !settingsVisible && !saveSlotsVisible;
        }
    }
}

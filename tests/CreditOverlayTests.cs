using System;
using RestoryItalianCredits;

internal static class CreditOverlayTests
{
    private const string ExpectedCredit = "Tradotto con ❤️ e 🍕 da Alessio | IG alessio.bellan";

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    private static void Main()
    {
        Assert(CreditOverlayDefinition.Text == ExpectedCredit, "Credit wording or emoji differ from the approved text.");
        Assert(CreditOverlayDefinition.Text.Contains("❤️") && CreditOverlayDefinition.Text.Contains("🍕"), "Both approved emoji must be present.");
        Assert(CreditOverlayDefinition.ShouldBeVisible(true, false, false), "Credit should appear on the unobstructed home screen.");
        Assert(!CreditOverlayDefinition.ShouldBeVisible(false, false, false), "Credit should be hidden when the main menu is inactive.");
        Assert(!CreditOverlayDefinition.ShouldBeVisible(true, true, false), "Credit should be hidden while settings are open.");
        Assert(!CreditOverlayDefinition.ShouldBeVisible(true, false, true), "Credit should be hidden while save slots are open.");
        Assert(CreditOverlayDefinition.HorizontalAnchor == 1f && CreditOverlayDefinition.VerticalAnchor == 0f,
            "Credit must anchor to the lower-right corner.");
        Assert(CreditOverlayDefinition.PivotX == 1f && CreditOverlayDefinition.PivotY == 0f,
            "Credit must use the lower-right pivot.");

        Console.WriteLine("PASS: exact home-screen credit, emoji, visibility, and lower-right anchor");
    }
}

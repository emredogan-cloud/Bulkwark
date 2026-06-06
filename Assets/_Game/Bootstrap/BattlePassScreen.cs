// BULWARK — BATTLE PASS (UI Implementation · WP-07). Presentation-only, REMOVABLE.
//
// Landscape Battle Pass on the UiRouter shell (design/BattlePassDesign.png): season header, current tier + XP
// bar, a horizontal Free/Premium tier track (sampled around the current tier), and an UNLOCK PREMIUM CTA.
// Binds to UiStub (display-only; real BattlePassService binding is GATE-3). FROZEN: premium is cosmetic/
// convenience, never power. NO ECS/gameplay/backend.

using UnityEngine;

namespace Bulwark.Bootstrap
{
    /// <summary>WP-07 landscape Battle Pass. Presentation-only.</summary>
    public sealed class BattlePassScreen : UiScreen
    {
        protected override void Build()
        {
            UiWidgets.Stretch("Bg", Rect, UiWidgets.Dark, "bg_menu");
            UiWidgets.BackButton(SafeContent, () => Router.Pop());
            UiWidgets.CurrencyChip(SafeContent, UiWidgets.Gold, UiStub.Gold, 0, out _);
            UiWidgets.CurrencyChip(SafeContent, UiWidgets.Gem, UiStub.Gems, 1, out _);

            UiWidgets.LabelAt(SafeContent, UiStub.SeasonName, 60, new Vector2(0.5f, 0.90f), new Vector2(1100, 90), TextAnchor.MiddleCenter, UiWidgets.Gold);

            // Tier + XP bar.
            UiWidgets.LabelAt(SafeContent, "TIER " + UiStub.PassTier + "   ·   " + UiStub.PassXp + " / " + UiStub.PassXpPerTier + " XP", 34,
                new Vector2(0.5f, 0.79f), new Vector2(900, 50), TextAnchor.MiddleCenter, Color.white);
            UiWidgets.ProgressBar(SafeContent, new Vector2(0.5f, 0.73f), new Vector2(0.5f, 0.73f), Vector2.zero, new Vector2(1300, 30), UiWidgets.Purple,
                (float)UiStub.PassXp / UiStub.PassXpPerTier);

            // Tier track (Free row + Premium row) sampled around the current tier.
            int start = UiStub.PassTier - 2;
            for (int i = 0; i < 6; i++)
            {
                int tier = start + i;
                float fx = (i + 0.5f) / 6f;
                bool past = tier <= UiStub.PassTier;
                UiWidgets.LabelAt(SafeContent, tier.ToString(), 28, new Vector2(fx, 0.58f), new Vector2(120, 36), TextAnchor.MiddleCenter, Color.white);
                UiWidgets.Panel(SafeContent, new Vector2(fx, 0.48f), new Vector2(fx, 0.48f), Vector2.zero, new Vector2(190, 120),
                    past ? new Color(0.2f, 0.5f, 0.25f, 0.9f) : new Color(0.15f, 0.16f, 0.22f, 0.9f)); // FREE node
                UiWidgets.Panel(SafeContent, new Vector2(fx, 0.30f), new Vector2(fx, 0.30f), Vector2.zero, new Vector2(190, 120),
                    UiStub.PassPremiumOwned && past ? new Color(0.5f, 0.3f, 0.6f, 0.95f) : new Color(0.30f, 0.16f, 0.40f, 0.6f)); // PREMIUM node
            }
            UiWidgets.LabelAt(SafeContent, "FREE", 26, new Vector2(0.04f, 0.48f), new Vector2(150, 36), TextAnchor.MiddleLeft, Color.white);
            UiWidgets.LabelAt(SafeContent, "PREMIUM", 26, new Vector2(0.06f, 0.30f), new Vector2(220, 36), TextAnchor.MiddleLeft, UiWidgets.Gold);

            if (!UiStub.PassPremiumOwned)
                UiWidgets.Button(SafeContent, "UNLOCK PREMIUM — " + UiStub.PassPremiumPriceGems + " GEMS", new Vector2(0.5f, 0.10f), new Vector2(0.5f, 0.10f), Vector2.zero, new Vector2(900, 96), UiWidgets.Purple, UnlockPremium, 36);
            else
                UiWidgets.LabelAt(SafeContent, "PREMIUM ACTIVE — +20% XP, +20% Gold, Exclusive Rewards", 30, new Vector2(0.5f, 0.10f), new Vector2(1100, 50), TextAnchor.MiddleCenter, UiWidgets.Gold);
        }

        public override void OnShow() => AudioManager.Instance?.PlayMenuMusic();

        private void UnlockPremium()
        {
            // Display-only stub spend (real purchase is a GATE-3 server-authoritative flow).
            if (UiStub.TrySpendGems(UiStub.PassPremiumPriceGems))
            {
                UiStub.PassPremiumOwned = true;
                Router.Replace<BattlePassScreen>();
            }
            else UiModals.Insufficient(UiStub.PassPremiumPriceGems - UiStub.Gems);
        }
    }
}

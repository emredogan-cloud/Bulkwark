// BULWARK — STORE (UI Implementation · WP-06). Presentation-only, REMOVABLE.
//
// Landscape Store on the UiRouter shell (design/StoreScreenDesign.png): shared shop tab bar (SPELLS/SKINS/
// CHESTS/STORE — only STORE is built in this sequence; the others are ADR-gated/not-in-sequence → "coming
// soon"), currency chips, a Starter Bundle banner, a Battle Pass promo (→ BattlePassScreen), the gem-pack row
// (UiStub), and Featured/Gems/Resources/Offers/Daily-Deals sub-tabs (cosmetic). Real IAP/receipts are a GATE-3
// integration → purchase surfaces a notice (no real charge). FROZEN: gems never buy power; Gold+Gems only.
// NO ECS/gameplay/backend.

using UnityEngine;

namespace Bulwark.Bootstrap
{
    /// <summary>WP-06 landscape Store. Presentation-only (stub catalog; live IAP deferred to GATE-3).</summary>
    public sealed class StoreScreen : UiScreen
    {
        protected override void Build()
        {
            UiWidgets.Stretch("Bg", Rect, UiWidgets.Dark, "bg_menu");
            UiWidgets.BackButton(SafeContent, () => Router.Pop());
            UiWidgets.CurrencyChip(SafeContent, UiWidgets.Gold, UiStub.Gold, 0, out _);
            UiWidgets.CurrencyChip(SafeContent, UiWidgets.Gem, UiStub.Gems, 1, out _);

            // Shared shop tab bar (top-center).
            string[] tabs = { "SPELLS", "SKINS", "CHESTS", "STORE" };
            for (int i = 0; i < tabs.Length; i++)
            {
                float fx = 0.34f + i * 0.11f;
                bool active = tabs[i] == "STORE";
                string tab = tabs[i];
                UiWidgets.Button(SafeContent, tab, new Vector2(fx, 0.93f), new Vector2(fx, 0.93f), Vector2.zero, new Vector2(220, 80),
                    active ? UiWidgets.Gold : UiWidgets.Grey, () => { if (!active) Router.Toast(tab + " — not in this build sequence"); }, 30);
            }

            // Starter bundle banner (left) + Battle Pass promo (right).
            var bundle = UiWidgets.Panel(SafeContent, new Vector2(0.30f, 0.74f), new Vector2(0.30f, 0.74f), Vector2.zero, new Vector2(760, 220), new Color(0.22f, 0.16f, 0.30f, 0.95f));
            UiWidgets.LabelAt(bundle.transform, "LEGENDARY STARTER BUNDLE", 34, new Vector2(0.5f, 0.74f), new Vector2(720, 50), TextAnchor.MiddleCenter, UiWidgets.Gold);
            UiWidgets.LabelAt(bundle.transform, "Best value! 2000 Gems + 50000 Gold", 26, new Vector2(0.5f, 0.45f), new Vector2(720, 40), TextAnchor.MiddleCenter, Color.white);
            UiWidgets.Button(bundle.transform, "$9.99", new Vector2(0.5f, 0.16f), new Vector2(0.5f, 0.16f), Vector2.zero, new Vector2(240, 70), UiWidgets.Gold, () => BuyMoney("Starter Bundle"), 30);

            UiWidgets.Button(SafeContent, "BATTLE PASS", new Vector2(0.85f, 0.74f), new Vector2(0.85f, 0.74f), Vector2.zero, new Vector2(320, 200), UiWidgets.Purple, () => Router.Show<BattlePassScreen>(), 36);

            // Gem-pack row.
            for (int i = 0; i < UiStub.GemPacks.Length; i++)
            {
                var gp = UiStub.GemPacks[i];
                float fx = (i + 0.5f) / UiStub.GemPacks.Length;
                var card = UiWidgets.Panel(SafeContent, new Vector2(fx, 0.34f), new Vector2(fx, 0.34f), Vector2.zero, new Vector2(280, 360), new Color(0.12f, 0.13f, 0.20f, 0.96f));
                if (gp.Best) UiWidgets.LabelAt(card.transform, "BEST SELLER", 22, new Vector2(0.5f, 0.92f), new Vector2(260, 34), TextAnchor.MiddleCenter, UiWidgets.Gold);
                UiWidgets.LabelAt(card.transform, gp.Name, 26, new Vector2(0.5f, 0.74f), new Vector2(270, 60), TextAnchor.MiddleCenter, Color.white);
                UiWidgets.LabelAt(card.transform, gp.Gems.ToString("N0") + "  Gems", 30, new Vector2(0.5f, 0.42f), new Vector2(270, 50), TextAnchor.MiddleCenter, UiWidgets.Gem);
                string name = gp.Name;
                UiWidgets.Button(card.transform, gp.Price, new Vector2(0.5f, 0.12f), new Vector2(0.5f, 0.12f), Vector2.zero, new Vector2(220, 72), UiWidgets.IronBlue, () => BuyMoney(name), 30);
            }

            // Cosmetic sub-tabs.
            string[] sub = { "FEATURED", "GEMS", "RESOURCES", "OFFERS", "DAILY DEALS" };
            for (int i = 0; i < sub.Length; i++)
            {
                float fx = (i + 0.5f) / sub.Length;
                UiWidgets.LabelAt(SafeContent, sub[i], 24, new Vector2(fx, 0.04f), new Vector2(260, 34), TextAnchor.MiddleCenter, new Color(1, 1, 1, i == 0 ? 1f : 0.5f));
            }
        }

        public override void OnShow() => AudioManager.Instance?.PlayMenuMusic();

        private void BuyMoney(string sku)
            => Router.Toast("Purchase '" + sku + "' — real-money store pending (GATE-3)");
    }
}

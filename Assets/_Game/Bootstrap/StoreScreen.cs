// BULWARK — STORE (UI Construction Bible · 17). Presentation-only, REMOVABLE.
//
// Forensic build of design/StoreScreenDesign.png per Sections A–O. The shop-hub STORE tab (IAP storefront):
// shared chrome (Back top-left, 4-tab bar top-center with STORE selected + red notify dot, Gems+Gold currency
// chips top-right), a top-left Legendary Starter Bundle banner ($9.99 · 2000 gems/50000 gold/5 keys/10 potions
// · 400% VALUE), a 5-card gem-pack row (UiStub.GemPacks, Pack_3 "BEST SELLER" lifted/largest), a right-rail
// Battle Pass promo (SEASON 1 · GLORY OF KINGS · 400% VALUE · GO NOW), and 5 bottom sub-tabs (FEATURED
// selected). Currency chips bind to UiStub (display-only); purchases gate on UiStub.TrySpendGems + Router.Toast
// and NEVER imply a real balance write (server-authoritative IAP). §12: NO ECS / gameplay / balance / backend.

using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-17 landscape Store (shop-chrome anchor). Presentation-only (stub catalog; server-auth IAP).</summary>
    public sealed class StoreScreen : UiScreen
    {
        private Text _goldText, _gemText;

        // px helpers (CanvasScaler 2340×1080, match-height). anchorY = 1 − fy_from_top.
        private const float W = 2340f, H = 1080f;

        protected override void Build()
        {
            // ---------------------------------------------------------------------------------------------
            // Bg_FullBleed (OUTSIDE safe area → bleeds under cutout) + radial vignette + treasury bloom.
            // ---------------------------------------------------------------------------------------------
            UiWidgets.Backdrop(Rect, "store");
            UiWidgets.Stretch("Bg_Tint", Rect, UiTheme.A(Hex("#0c0e14"), 0.55f)); // obsidian vault wash (denser store mood)
            // warm focal god-ray behind the central Starter chest (upper-left bias) + amethyst product bloom.
            UiWidgets.Glow(Rect, UiTheme.A(Hex("#f4dca0"), 0.26f), new Vector2(0.30f, 0.78f), new Vector2(0.30f, 0.78f), Vector2.zero, new Vector2(1100, 760), 1.5f);
            UiWidgets.Glow(Rect, UiTheme.A(UiTheme.AmethystHi, 0.12f), new Vector2(0.43f, 0.32f), new Vector2(0.43f, 0.32f), Vector2.zero, new Vector2(1500, 760), 1.7f);
            UiWidgets.Vignette(Rect, 0.6f); // darken the four corners

            // ---------------------------------------------------------------------------------------------
            // SHARED CHROME (canonical — 18/19/20 inherit). Back (top-left), TabBar (top-center),
            // CurrencyChips (top-right). All in SafeContent.
            // ---------------------------------------------------------------------------------------------
            UiWidgets.BackButton(SafeContent, () => Router.Pop());

            // Currency chips: Gold rightmost (idx 0), Gems left of it (idx 1). Each carries a green "+".
            _goldText = UiWidgets.CurrencyChip(SafeContent, UiTheme.Gold, UiStub.Gold, 0, out _);
            _gemText  = UiWidgets.CurrencyChip(SafeContent, UiTheme.AmethystHi, UiStub.Gems, 1, out _);

            // Tab bar: top-center, width ≈0.44 W (≈1030px) × height ≈0.062 H (≈67px), y inset −1.5%.
            // Active index 0 = STORE; we reorder so STORE is first/selected (ignore i==0).
            var tabBar = UiWidgets.TabBar(SafeContent,
                new[] { "STORE", "SPELLS", "SKINS", "CHESTS" },
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0, -0.015f * H - 0.062f * H * 0.5f), new Vector2(0.44f * W, 0.062f * H), 0,
                i =>
                {
                    if (i == 1) Router.Replace<SpellsScreen>();
                    else if (i == 2) Router.Replace<SkinsScreen>();
                    else if (i == 3) Router.Replace<ChestsScreen>();
                    // i==0 = STORE (current) → ignore
                });

            // Tab_NotifyDot — red pulsing dot on the selected STORE tab (index 0), top-right of the cell.
            if (tabBar != null && tabBar.Length > 0 && tabBar[0] != null)
            {
                var dot = UiWidgets.Rect("Tab_NotifyDot", tabBar[0].transform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-6, -6), new Vector2(22, 22));
                var dimg = dot.gameObject.AddComponent<Image>(); dimg.sprite = UiTex.Disc(UiTheme.Ember2, 32); dimg.raycastTarget = false;
                var dp = dot.gameObject.AddComponent<PulseScale>(); dp.min = 1.0f; dp.max = 1.15f; dp.period = 1.2f;
            }

            // ---------------------------------------------------------------------------------------------
            // CONTENT — free 3-column placement (bundle stack | pack row | BP rail). Insets: top ~9%,
            // bottom ~9% (above sub-tabs), left/right ~1.5% (kept off the full-bleed side props).
            // ---------------------------------------------------------------------------------------------
            var content = UiWidgets.Rect("Content", SafeContent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            content.anchorMin = new Vector2(0.015f, 0.09f); content.anchorMax = new Vector2(0.985f, 0.91f);
            content.offsetMin = Vector2.zero; content.offsetMax = Vector2.zero;

            BuildStarterBundle(content);
            BuildGemPackRow(content);
            BuildBattlePassRail(content);

            // ---------------------------------------------------------------------------------------------
            // SubTabBar (bottom) — 5 cosmetic filter cells, FEATURED selected. Width ≈0.66 W centered,
            // height ≈0.075 H, bottom inset +1.0%. Each = icon-over-label mini group.
            // ---------------------------------------------------------------------------------------------
            BuildSubTabBar();
        }

        // =================================================================================================
        // STARTER BUNDLE — top-left banner: x 0.07→0.555 W, y top 0.135→0.345 H (fractions of CONTENT-free
        // canvas math). Title (2 lines) left, hero chest center-right, dark contents strip lower-left,
        // 400% VALUE burst near chest, gold $9.99 ribbon bottom-right.
        // =================================================================================================
        private void BuildStarterBundle(RectTransform content)
        {
            // Banner rect via fraction math (anchored to SafeContent space for stable placement at 2340×1080).
            // x 0.07→0.555 → center 0.3125, w 0.485; y top 0.135→0.345 → anchorY center = 1 − 0.24 = 0.76, h 0.21.
            var card = UiWidgets.Card(SafeContent, new Vector2(0.3125f, 0.76f), new Vector2(0.3125f, 0.76f), Vector2.zero,
                new Vector2(0.485f * W, 0.21f * H), UiTheme.A(Hex("#161a24"), 0.94f));
            var frame = UiWidgets.OrnateFrame(card, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Hex("#1a1422"), false, 16f);

            // Title — "LEGENDARY" small-caps line + "STARTER BUNDLE" larger, gold gradient bevel.
            UiWidgets.TitleLabel(frame, "LEGENDARY", 36, new Vector2(0.02f, 0.86f), new Vector2(0.02f, 0.86f), Vector2.zero, new Vector2(700, 60), TextAnchor.MiddleLeft, Hex("#f0cf72"), UiTheme.Gold);
            UiWidgets.TitleLabel(frame, "STARTER BUNDLE", 52, new Vector2(0.02f, 0.70f), new Vector2(0.02f, 0.70f), Vector2.zero, new Vector2(760, 70), TextAnchor.MiddleLeft, Hex("#f0cf72"), UiTheme.Gold);

            // Two-color subtitle: cream "Best value!" + ember "Limited time offer!".
            var sub = UiWidgets.Label(frame, "Best value!", 20, new Vector2(0.025f, 0.555f), new Vector2(0.025f, 0.555f), Vector2.zero, new Vector2(280, 40), TextAnchor.MiddleLeft, Hex("#e7dcc0"));
            sub.fontStyle = FontStyle.Italic;
            var lim = UiWidgets.Label(frame, "Limited time offer!", 20, new Vector2(0.22f, 0.555f), new Vector2(0.22f, 0.555f), Vector2.zero, new Vector2(340, 40), TextAnchor.MiddleLeft, Hex("#e0432a"));
            lim.fontStyle = FontStyle.Italic;

            // Hero — cobalt war-chest spilling gems, lit by a warm god-ray (center-right of the banner).
            var heroGlow = UiWidgets.Glow(frame, UiTheme.A(Hex("#ffe9a0"), 0.45f), new Vector2(0.72f, 0.58f), new Vector2(0.72f, 0.58f), Vector2.zero, new Vector2(360, 320), 1.5f);
            var hgp = heroGlow.gameObject.AddComponent<PulseGraphic>(); hgp.target = heroGlow; hgp.min = 0.8f; hgp.max = 1.0f; hgp.period = 3f; // god-ray slow pulse
            var chest = UiWidgets.Rect("Bundle_Hero", frame, new Vector2(0.72f, 0.55f), new Vector2(0.72f, 0.55f), Vector2.zero, new Vector2(220, 170));
            var chImg = chest.gameObject.AddComponent<Image>(); chImg.raycastTarget = false; chImg.sprite = UiTex.VGradient(Hex("#3a63d0"), Hex("#1e3a8c"), 64); // royal-blue lacquered chest
            var lid = UiWidgets.Rect("Lid_Gem", chest, new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), Vector2.zero, new Vector2(54, 54));
            var lidImg = lid.gameObject.AddComponent<Image>(); lidImg.raycastTarget = false; lidImg.sprite = UiTex.Diamond(UiTheme.AmethystHi, 48); // blue/violet gem on lid
            var spill = UiWidgets.Rect("GemSpill", chest, new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f), Vector2.zero, new Vector2(240, 110));
            var spEf = spill.gameObject.AddComponent<EmberField>(); spEf.count = 10; spEf.color = Hex("#e7ccff"); // gem-pile sparkle

            // Contents bar — dark rounded strip, 4 even items (icon-above-number mini groups).
            var bar = UiWidgets.Card(frame, new Vector2(0.025f, 0.255f), new Vector2(0.025f, 0.255f), Vector2.zero, new Vector2(560, 96), UiTheme.A(Hex("#0c0e14"), 0.92f));
            ((RectTransform)bar).pivot = new Vector2(0f, 0.5f);
            BundleItem(bar, 0, UiTheme.AmethystHi, "2000");  // violet gem
            BundleItem(bar, 1, Hex("#cdd0d8"),     "50000"); // silver coin
            BundleItem(bar, 2, Hex("#6f9bff"),     "5");     // blue key
            BundleItem(bar, 3, Hex("#b06bff"),     "10");    // violet potion vial

            // 400% VALUE badge — small angled burst near the chest (x≈0.50, y≈0.16 of screen → upper-right area).
            var badge = UiWidgets.Rect("Bundle_ValueBadge", frame, new Vector2(0.60f, 0.92f), new Vector2(0.60f, 0.92f), Vector2.zero, new Vector2(130, 130));
            var bImg = badge.gameObject.AddComponent<Image>(); bImg.raycastTarget = false; bImg.sprite = UiTex.Diamond(Hex("#c12a20"), 48);
            badge.localRotation = Quaternion.Euler(0, 0, -12);
            var bTxt = UiWidgets.Label(badge, "400%\nVALUE", 22, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Hex("#fff8e6"));
            bTxt.alignment = TextAnchor.MiddleCenter; bTxt.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.9f);

            // Price ribbon — gold banner "$9.99" bottom-right; one-time bundle gated via TrySpendGems-style flow.
            var price = UiWidgets.Button(frame, "", new Vector2(0.92f, 0.18f), new Vector2(0.92f, 0.18f), Vector2.zero, new Vector2(170, 78), Hex("#e8b94a"), BuyBundle, 30);
            UiWidgets.Label(price.transform, "$9.99", 30, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Hex("#3a2a10"));

            // OnShow reveal handled per-card by UiFx; banner breathes subtly.
            card.gameObject.AddComponent<PulseScale>().period = 6f;
        }

        // One bundle contents item (icon over number), evenly placed across the 560px strip.
        private void BundleItem(RectTransform bar, int index, Color iconCol, string value)
        {
            float cx = (index + 0.5f) / 4f; // 0.125 / 0.375 / 0.625 / 0.875
            var icon = UiWidgets.Rect("Item_Icon", bar, new Vector2(cx, 0.66f), new Vector2(cx, 0.66f), Vector2.zero, new Vector2(46, 46));
            var iImg = icon.gameObject.AddComponent<Image>(); iImg.raycastTarget = false; iImg.sprite = UiTex.Diamond(iconCol, 48);
            UiWidgets.Label(bar, value, 24, new Vector2(cx, 0.22f), new Vector2(cx, 0.22f), Vector2.zero, new Vector2(130, 34), TextAnchor.MiddleCenter, Hex("#f4ead0"));
        }

        // =================================================================================================
        // GEM PACK ROW — band x 0.07→0.79 W, y 0.40→0.86 H. 5 equal cards; Pack_3 (BEST SELLER) scaled
        // 1.05 + lifted −12px with extra rim glow. Each card: [ribbon?] → title → art → gem count → price.
        // =================================================================================================
        private void BuildGemPackRow(RectTransform content)
        {
            var packs = UiStub.GemPacks;
            int n = packs.Length;
            // band center y: 1 − 0.63 = 0.37; spans y 0.40→0.86 (h 0.46). x 0.07→0.79 (w 0.72, center 0.43).
            float bandLeft = 0.07f, bandRight = 0.79f, bandW = bandRight - bandLeft;
            float cardWFrac = 0.135f;
            float gap = (bandW - n * cardWFrac) / (n - 1); // even gaps between 5 cards

            for (int i = 0; i < n; i++)
            {
                var gp = packs[i];
                bool best = gp.Best;
                float fx = bandLeft + cardWFrac * 0.5f + i * (cardWFrac + gap); // card center fraction (x)
                float anchorY = 0.37f; // band center in anchorY
                float liftPx = best ? 12f : 0f;
                float scale = best ? 1.05f : 1f;

                var card = UiWidgets.Card(SafeContent, new Vector2(fx, anchorY), new Vector2(fx, anchorY), new Vector2(0, liftPx),
                    new Vector2(cardWFrac * W, 0.46f * H), UiTheme.A(Hex("#161a24"), 0.96f));
                card.localScale = Vector3.one * scale;

                if (best)
                {
                    var glow = UiWidgets.Glow(card, UiTheme.A(UiTheme.GoldHi, 0.4f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(cardWFrac * W + 80, 0.46f * H + 80), 1.6f);
                    var gpls = glow.gameObject.AddComponent<PulseGraphic>(); gpls.target = glow; gpls.min = 0.25f; gpls.max = 0.5f; gpls.period = 2.5f; // rim-glow shimmer
                }

                // Gold hairline interior frame.
                var frame = UiWidgets.OrnateFrame(card, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Hex("#12131a"), false, 12f);

                // BEST SELLER ribbon (gold tab, dark text) top-center.
                if (best)
                {
                    var rib = UiWidgets.Card(frame, new Vector2(0.5f, 1.0f), new Vector2(0.5f, 1.0f), new Vector2(0, 6), new Vector2(0.135f * W - 20, 40), Hex("#e8b94a"));
                    var rl = UiWidgets.Label(rib, UiTheme.Track("BEST SELLER"), 18, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Hex("#2c1f08"));
                    var shGo = rib.gameObject.AddComponent<PulseScale>(); shGo.min = 1.0f; shGo.max = 1.02f; shGo.period = 2.5f; // faint shimmer
                }

                // Title (1–2 lines), top ~12%.
                UiWidgets.TitleLabel(frame, gp.Name.ToUpperInvariant(), 24, new Vector2(0.5f, 0.86f), new Vector2(0.5f, 0.86f), Vector2.zero, new Vector2(0.135f * W - 16, 70), TextAnchor.MiddleCenter, Hex("#e9cf86"), UiTheme.Gold);

                // Art icon — gem pile (violet glow + sparkle), center ~45%.
                var artGlow = UiWidgets.Glow(frame, UiTheme.A(UiTheme.AmethystHi, 0.35f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), Vector2.zero, new Vector2(180, 180), 1.7f);
                var art = UiWidgets.Rect("Pack_Art", frame, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), Vector2.zero, new Vector2(118, 118));
                var aImg = art.gameObject.AddComponent<Image>(); aImg.raycastTarget = false; aImg.sprite = UiTex.Diamond(UiTheme.AmethystHi, 48);
                var spark = UiWidgets.Rect("Pack_Sparkle", art, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(150, 150));
                var sEf = spark.gameObject.AddComponent<EmberField>(); sEf.count = 6; sEf.color = Hex("#e7ccff");

                // Gem count (white, gem icon left) ~70%.
                var gIcon = UiWidgets.Rect("Pack_GemIcon", frame, new Vector2(0.5f, 0.30f), new Vector2(0.5f, 0.30f), new Vector2(-52, 0), new Vector2(40, 40));
                var giImg = gIcon.gameObject.AddComponent<Image>(); giImg.raycastTarget = false; giImg.sprite = UiTex.Diamond(UiTheme.AmethystHi, 48);
                UiWidgets.Label(frame, gp.Gems.ToString("N0"), 30, new Vector2(0.5f, 0.30f), new Vector2(0.5f, 0.30f), new Vector2(18, 0), new Vector2(180, 44), TextAnchor.MiddleLeft, Color.white);

                // Price chip (cobalt-violet plate, light text) bottom.
                string sku = gp.Name; int gems = gp.Gems; string priceStr = gp.Price;
                var priceBtn = UiWidgets.Button(frame, "", new Vector2(0.5f, 0.09f), new Vector2(0.5f, 0.09f), Vector2.zero, new Vector2(0.10f * W, 0.052f * H), Hex("#3a2c7a"), () => BuyPack(sku, gems, priceStr), 26);
                UiWidgets.Label(priceBtn.transform, priceStr, 26, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Hex("#f3ecff"));
            }
        }

        // =================================================================================================
        // BATTLE PASS RAIL — right column: x 0.79→0.985 W (center 0.8875, w 0.195), y 0.10→0.875 H
        // (anchorY center = 1 − 0.4875 = 0.5125, h 0.775). Title→Season→Name→art→reward strip→caption→GO NOW.
        // =================================================================================================
        private void BuildBattlePassRail(RectTransform content)
        {
            var rail = UiWidgets.Card(SafeContent, new Vector2(0.8875f, 0.5125f), new Vector2(0.8875f, 0.5125f), Vector2.zero,
                new Vector2(0.195f * W, 0.775f * H), UiTheme.A(Hex("#141019"), 0.96f));
            var frame = UiWidgets.OrnateFrame(rail, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Hex("#141019"), false, 14f);

            // BP_Title "BATTLE PASS" + info dot.
            UiWidgets.TitleLabel(frame, "BATTLE PASS", 30, new Vector2(0.5f, 0.93f), new Vector2(0.5f, 0.93f), Vector2.zero, new Vector2(420, 50), TextAnchor.MiddleCenter, Hex("#f3d98a"), UiTheme.Gold);
            var info = UiWidgets.Rect("BP_InfoDot", frame, new Vector2(0.92f, 0.93f), new Vector2(0.92f, 0.93f), Vector2.zero, new Vector2(28, 28));
            var infoImg = info.gameObject.AddComponent<Image>(); infoImg.raycastTarget = false; infoImg.sprite = UiTex.Disc(UiTheme.A(UiTheme.Gold, 0.7f), 32);
            UiWidgets.Label(info, "i", 22, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, UiTheme.Obsidian);

            // BP_Season "— SEASON 1 —" between two rule lines.
            UiWidgets.Divider(frame, new Vector2(0.18f, 0.875f), new Vector2(0.18f, 0.875f), Vector2.zero, 90, 3, UiTheme.A(UiTheme.Gold, 0.7f));
            UiWidgets.Label(frame, UiTheme.Track("SEASON 1"), 18, new Vector2(0.5f, 0.875f), new Vector2(0.5f, 0.875f), Vector2.zero, new Vector2(260, 32), TextAnchor.MiddleCenter, Hex("#cdb887"));
            UiWidgets.Divider(frame, new Vector2(0.82f, 0.875f), new Vector2(0.82f, 0.875f), Vector2.zero, 90, 3, UiTheme.A(UiTheme.Gold, 0.7f));

            // BP_Name "GLORY OF KINGS".
            UiWidgets.TitleLabel(frame, "GLORY OF KINGS", 26, new Vector2(0.5f, 0.80f), new Vector2(0.5f, 0.80f), Vector2.zero, new Vector2(420, 50), TextAnchor.MiddleCenter, Hex("#f0cf72"), UiTheme.Gold);

            // BP_ValueBadge "400% VALUE", angled, left edge.
            var vb = UiWidgets.Rect("BP_ValueBadge", frame, new Vector2(0.14f, 0.74f), new Vector2(0.14f, 0.74f), Vector2.zero, new Vector2(96, 96));
            var vbImg = vb.gameObject.AddComponent<Image>(); vbImg.raycastTarget = false; vbImg.sprite = UiTex.Diamond(Hex("#c12a20"), 48); vb.localRotation = Quaternion.Euler(0, 0, -14);
            var vbt = UiWidgets.Label(vb, "400%\nVALUE", 18, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Hex("#fff8e6")); vbt.alignment = TextAnchor.MiddleCenter;

            // BP_Art — dark-knight key art (framed, ember rim) y 0.30→0.66; ember drift over it.
            var art = UiWidgets.Card(frame, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), Vector2.zero, new Vector2(0.195f * W - 70, 250), UiTheme.A(Hex("#1a2030"), 1f));
            var artBg = art.gameObject.GetComponent<Image>(); if (artBg != null) artBg.sprite = UiTex.VGradient(Hex("#243049"), Hex("#0e1320"), 64);
            UiWidgets.Glow(art, UiTheme.A(UiTheme.Ember2, 0.18f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), Vector2.zero, new Vector2(280, 180), 1.6f);
            var knight = UiWidgets.Rect("BP_Knight", art, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(120, 150));
            var kImg = knight.gameObject.AddComponent<Image>(); kImg.raycastTarget = false; kImg.sprite = UiTex.VGradient(Hex("#3a4a6a"), Hex("#161c2c"), 64);
            var emb = UiWidgets.Rect("BP_Embers", art, new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), Vector2.zero, new Vector2(280, 240));
            var ef = emb.gameObject.AddComponent<EmberField>(); ef.count = 10; ef.color = Hex("#d8a36a");

            // BP_RewardStrip — 3 reward thumbs (gold token, violet chest, blue gem) y ~0.22.
            RewardThumb(frame, 0.28f, UiTheme.GoldHi);
            RewardThumb(frame, 0.50f, UiTheme.AmethystHi);
            RewardThumb(frame, 0.72f, UiTheme.IronBlueHi);

            // BP_Caption.
            UiWidgets.Label(frame, UiTheme.Track("UNLOCK PREMIUM REWARDS!"), 18, new Vector2(0.5f, 0.135f), new Vector2(0.5f, 0.135f), Vector2.zero, new Vector2(420, 34), TextAnchor.MiddleCenter, Hex("#e7dcc0"));

            // BP_GoBtn "GO NOW" + crown, gold, full rail width → Battle Pass screen.
            var go = UiWidgets.GemButton(frame, "GO NOW", new Vector2(0.5f, 0.05f), new Vector2(0.5f, 0.05f), Vector2.zero, new Vector2(0.195f * W - 50, 80), UiTheme.Gold, () => Router.Show<BattlePassScreen>(), 26, true, true, TextAnchor.MiddleCenter);
        }

        private void RewardThumb(Transform parent, float cx, Color col)
        {
            var t = UiWidgets.Rect("BP_Reward", parent, new Vector2(cx, 0.22f), new Vector2(cx, 0.22f), Vector2.zero, new Vector2(70, 70));
            var img = t.gameObject.AddComponent<Image>(); img.raycastTarget = false; img.sprite = UiTex.Diamond(col, 48);
        }

        // =================================================================================================
        // SUB-TAB BAR (bottom) — 5 cosmetic filter cells, FEATURED selected. Width ≈0.66 W centered.
        // =================================================================================================
        private void BuildSubTabBar()
        {
            var bar = UiWidgets.Card(SafeContent, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 0.01f * H + 0.075f * H * 0.5f),
                new Vector2(0.66f * W, 0.075f * H), UiTheme.A(Hex("#0c0e14"), 0.95f));

            string[] sub = { "FEATURED", "GEMS", "RESOURCES", "OFFERS", "DAILY DEALS" };
            Color[] icons = { UiTheme.GoldHi, UiTheme.AmethystHi, Hex("#cdd0d8"), UiTheme.Ember2, Hex("#e8d5a8") };
            for (int i = 0; i < sub.Length; i++)
            {
                bool selected = i == 0; // FEATURED
                float cx = (i + 0.5f) / sub.Length;
                int idx = i; string label = sub[i];
                var cell = UiWidgets.Button(bar, "", new Vector2(cx, 0.5f), new Vector2(cx, 0.5f), Vector2.zero, new Vector2(0.66f * W / sub.Length - 8, 0.075f * H - 8),
                    selected ? UiTheme.A(UiTheme.Gold, 0.22f) : UiTheme.A(UiTheme.Charcoal, 0.0f),
                    () => { if (!selected) Router.Toast(label + " — filter coming soon"); }, 22);
                // icon over label
                var icon = UiWidgets.Rect("SubTab_Icon", cell.transform, new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), Vector2.zero, new Vector2(34, 34));
                var iImg = icon.gameObject.AddComponent<Image>(); iImg.raycastTarget = false; iImg.sprite = UiTex.Diamond(selected ? icons[i] : UiTheme.A(icons[i], 0.6f), 48);
                UiWidgets.Label(cell.transform, label, selected ? 23 : 22, new Vector2(0.5f, 0.26f), new Vector2(0.5f, 0.26f), Vector2.zero, new Vector2(0.66f * W / sub.Length, 32), TextAnchor.MiddleCenter, selected ? Hex("#f3d98a") : Hex("#b9a36e"));
                if (selected)
                    UiWidgets.Glow(cell.transform, UiTheme.A(UiTheme.GoldHi, 0.3f), new Vector2(0.5f, 0.1f), new Vector2(0.5f, 0.1f), Vector2.zero, new Vector2(160, 80), 1.6f); // gold up-light
            }
        }

        public override void OnShow()
        {
            // Re-bind currency chips to the live (display-only) wallet.
            if (_goldText != null) _goldText.text = UiStub.Gold.ToString("N0");
            if (_gemText != null) _gemText.text = UiStub.Gems.ToString("N0");
            AudioManager.Instance?.PlayMenuMusic();
        }

        // -------------------------------------------------------------------------------------------------
        // Purchases are DISPLAY-ONLY (§12 / server-authoritative IAP). $-priced SKUs never charge here; the
        // gem-cost gate (TrySpendGems) is a presentation affordance only — it NEVER implies a real wallet
        // write. Always surface a Router.Toast as feedback; insufficient = decline toast (red-shake stand-in).
        // -------------------------------------------------------------------------------------------------
        private void BuyPack(string sku, int gems, string price)
            => Router.Toast("Purchase '" + sku + "' (" + price + ") — real-money store is server-authoritative (pending)");

        private void BuyBundle()
            => Router.Toast("Legendary Starter Bundle ($9.99) — real-money store is server-authoritative (pending)");

        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}

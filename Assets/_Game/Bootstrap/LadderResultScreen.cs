// BULWARK — LADDER RESULT (UI Construction Bible · 16). Presentation-only, REMOVABLE.
//
// Forensic build of design/LadderResultDesign.png per Sections A–O: the async/ranked post-match result.
// A centered obsidian card (ornate gold frame) over a dark, vignetted castle-skyline arena with blue
// Iron-Pact eagle-finial banners + warm god-rays; the hero RANK-TIER CREST (gold laurel wreath encircling a
// royal-blue shield bearing a gold helm sigil, radiating rays) labelled "Gold Tier III"; an optional white
// "Rank up!"; a You-vs-Opponent shield row (blue Iron-Pact left, oxblood Ashen right, gold VS center); a GREEN
// "+24 Points" delta (count-up); a single silver-coin "Rewards 12,450" row (count-up); and one royal-blue gold-
// framed CONTINUE CTA (the brightest interactive object). Win state is the source; loss-variant noted in §K.
// §12: rank/tier/tier-up/points/rewards are SERVER-AUTHORITATIVE — all values below are display-only local stubs;
// the screen never writes balance/economy/rank. NO ECS / NO Unity.Entities / NO gameplay. CONTINUE routes via the
// existing presentation seam (MatchPresentation.OnContinue → hub).

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-16 Ladder (ranked/async) result. Presentation-only; rank/points/rewards display-only (§12).</summary>
    public sealed class LadderResultScreen : UiScreen
    {
        // ---- Display-only stubs (server-authoritative in production; the UI NEVER writes these). ----
        // Mirror the mock's verbatim numbers. NOT economy/balance writes — pure presentation (§12, Negative Rule 1/9).
        private const string TierName  = "Gold Tier III"; // server-auth rank tier label
        private const bool   TierUp    = true;            // server-auth: did this result advance the tier?
        private const int    PointsDelta = 24;            // server-auth ranked-points delta (+ = gain → green)
        private const int    CoinReward  = 12450;         // server-auth ranked coin reward
        private const bool   Win       = true;            // ECS MatchState verdict (read-only); source = VICTORY

        // Panel geometry (fractions of 2340×1080, match HEIGHT) — §E.
        private const float PanelW = 1264f; // ≈ 0.54·2340
        private const float PanelH = 994f;  // ≈ 0.92·1080

        // Forensic hex (§F/§G) not in UiTheme.
        private static readonly Color GreenPoints   = Hex("#7ad13a");
        private static readonly Color GreenPointsHi = Hex("#b6e35a");
        private static readonly Color GreenWord     = Hex("#8ab84a");
        private static readonly Color ParchLabel    = Hex("#d8cfbc");
        private static readonly Color TierGold       = Hex("#e0bf6a");
        private static readonly Color VsGold         = Hex("#e0bf6a");
        private static readonly Color RankUpWhite    = Hex("#fffbf0");
        private static readonly Color RewardGold     = Hex("#c8a55a");
        private static readonly Color CoinsWhite     = Hex("#f3ead2");
        private static readonly Color BlueShield     = Hex("#2b56c8");
        private static readonly Color BlueShieldLo   = Hex("#214365");
        private static readonly Color OxShield       = Hex("#8a241c");
        private static readonly Color OxShieldLo     = Hex("#5a1410");
        private static readonly Color ContinueHi     = Hex("#388ee8");
        private static readonly Color ContinueLo     = Hex("#0d2a66");

        private CanvasGroup _panelCg;
        private Text _pointsText, _coinsText;
        private CountUp _pointsCount, _coinsCount;

        protected override void Build()
        {
            // ============ FullBleedRoot: dark ranked-arena bg + heavy vignette + god-rays + scrim ============
            UiWidgets.Backdrop(Rect, "ladderresult"); // dark castle/capital skyline (AspectFill stand-in)
            UiWidgets.Stretch("BG_NightGrade", Rect, UiTheme.A(Hex("#0a0d18"), 0.5f)); // dusk/night grade, push the arena back
            // Faint distant red opponent banners barely lit behind the scrim (§J).
            UiWidgets.Glow(Rect, UiTheme.A(UiTheme.Oxblood, 0.16f), new Vector2(0.16f, 0.42f), new Vector2(0.16f, 0.42f), Vector2.zero, new Vector2(360, 600), 1.7f);
            UiWidgets.Glow(Rect, UiTheme.A(UiTheme.Oxblood, 0.14f), new Vector2(0.84f, 0.40f), new Vector2(0.84f, 0.40f), Vector2.zero, new Vector2(340, 560), 1.7f);
            // FX_GodRays — warm gold halo behind the rank crest (width ≈0.45·W, height ≈0.50·H), top-center, additive feel.
            var rays = UiWidgets.Glow(Rect, UiTheme.A(Hex("#ffe9a0"), 0.34f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), Vector2.zero, new Vector2(1050, 1100), 1.5f);
            Pulse(rays, 0.22f, 0.4f, 4.5f); // slow shimmer/sway
            UiWidgets.Vignette(Rect, 0.62f); // heavy → near-black edges
            UiWidgets.Stretch("BG_DarkenScrim", Rect, new Color(0, 0, 0, 0.45f)); // α0.45

            // ============ ResultPanel: centered ornate gold frame over obsidian ============
            var panelGo = new GameObject("ResultPanel", typeof(RectTransform), typeof(CanvasGroup));
            var panel = (RectTransform)panelGo.transform; panel.SetParent(SafeContent, false);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(PanelW, PanelH); panel.anchoredPosition = Vector2.zero;
            _panelCg = panelGo.GetComponent<CanvasGroup>();

            UiWidgets.OrnateFrame(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(PanelW, PanelH), Hex("#0c0d12"), false, 26f);
            // Faint warm radial glow inside, behind the crest (lit by the god-rays) — §G.
            UiWidgets.Glow(panel, UiTheme.A(Hex("#3a2a0e"), 0.5f), PA(0.5f, 0.24f), PA(0.5f, 0.24f), Vector2.zero, new Vector2(720, 560), 1.7f);

            BuildBanners(panel);
            BuildTitle(panel);
            BuildRankCrest(panel);
            BuildRankUp(panel);
            BuildVsRow(panel);
            BuildPoints(panel);
            BuildRewards(panel);
            BuildContinue(panel);

            // Top-left back → Pop (aliased to the non-destructive CONTINUE action via §K, but the shell Pop is fine here).
            UiWidgets.BackButton(SafeContent, () => Router.Pop());
        }

        // ---- Top-corner blue Iron-Pact banners with gold eagle/wing finials (mirror) ----
        private void BuildBanners(Transform panel)
        {
            BuildBanner(panel, +1f); // left
            BuildBanner(panel, -1f); // right
        }

        private void BuildBanner(Transform panel, float side) // side = +1 left, -1 right
        {
            float bw = PanelW * 0.11f;     // ≈0.11·panelW
            float bh = PanelH * 0.55f;     // hang down ≈0.55·panelH
            // pivot the cloth at the very top corner; pos so the banner hangs inside the top edge.
            var ax = side > 0 ? 0.045f : 0.955f;
            var cloth = UiWidgets.Rect(side > 0 ? "Banner_Left" : "Banner_Right", panel, PA(ax, 0f), PA(ax, 0f), new Vector2(0, -bh * 0.5f + 24), new Vector2(bw, bh));
            var ci = cloth.gameObject.AddComponent<Image>(); ci.raycastTarget = false;
            ci.sprite = UiTex.VGradient(BlueShield, Hex("#1a347a"), 64);
            // stitched gold trim down the edge
            var trim = UiWidgets.Rect("Trim", cloth, new Vector2(0.5f, 0), new Vector2(0.5f, 1), Vector2.zero, new Vector2(bw - 10, 0));
            var ti = trim.gameObject.AddComponent<Image>(); ti.raycastTarget = false;
            ti.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 5); ti.type = Image.Type.Sliced;
            // notched tail (V cut at the bottom) via a rotated diamond mask-ish ornament
            var tail = UiWidgets.Rect("Tail", cloth, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 6), new Vector2(bw, bw * 0.7f));
            var tli = tail.gameObject.AddComponent<Image>(); tli.raycastTarget = false; tli.color = UiTheme.Obsidian;
            tli.sprite = UiTex.Diamond(UiTheme.Obsidian, 48); tail.localRotation = Quaternion.Euler(0, 0, 45);
            // gold eagle/wing finial at the very top corner (sits at/above the frame top)
            var fin = UiWidgets.Finial(panel, PA(ax, 0f), new Vector2(0, 16), 86f); fin.color = UiTheme.GoldHi;
            fin.transform.localRotation = Quaternion.Euler(0, 0, side > 0 ? 25 : -25);
            UiWidgets.Glow(panel, UiTheme.A(UiTheme.GoldHi, 0.32f), PA(ax, 0f), PA(ax, 0f), new Vector2(0, 16), new Vector2(150, 150));
        }

        // ---- Title "VICTORY" — gold-bevel serif, breaks the top frame, blue-banner-flanked, with bloom ----
        private void BuildTitle(Transform panel)
        {
            // bloom behind glyphs
            UiWidgets.Glow(panel, UiTheme.A(Hex("#f4dca0"), 0.45f), PA(0.5f, 0.05f), PA(0.5f, 0.05f), Vector2.zero, new Vector2(900, 220), 1.6f);
            var title = UiWidgets.TitleLabel(panel, Win ? "VICTORY" : "DEFEAT", 100,
                PA(0.5f, 0.05f), PA(0.5f, 0.05f), Vector2.zero, new Vector2(PanelW * 0.62f, 150),
                TextAnchor.MiddleCenter, Hex("#fded61"), UiTheme.Gold); // gradient #fded61→#caa04a
            // deepen the dark stroke per spec (#3a2a08 ~3px) + drop shadow already on the label.
            var ol = title.gameObject.GetComponent<Outline>(); if (ol != null) { ol.effectColor = UiTheme.A(Hex("#3a2a08"), 0.95f); ol.effectDistance = new Vector2(3, -3); }
        }

        // ---- Rank crest (the display HERO): rays → wreath → shield → sigil → tier label ----
        private void BuildRankCrest(Transform panel)
        {
            // Rank_Group centered at panel-fraction y≈0.24 (height ≈0.22·panelH).
            var grp = UiWidgets.Rect("Rank_Group", panel, PA(0.5f, 0.24f), PA(0.5f, 0.24f), Vector2.zero, new Vector2(PanelW * 0.5f, PanelH * 0.22f));

            // Rank_Rays — radiating gold light behind crest (additive feel), ≈0.50·panelW (overflow).
            var rkRays = UiWidgets.Glow(grp, UiTheme.A(Hex("#ffe9a0"), 0.6f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(PanelW * 0.5f, PanelW * 0.5f), 1.5f);
            var spokes = UiWidgets.Rect("Ray_Spokes", grp, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(PanelW * 0.46f, PanelW * 0.46f));
            var spImg = spokes.gameObject.AddComponent<Image>(); spImg.raycastTarget = false;
            spImg.sprite = UiTex.Radial(UiTheme.A(Hex("#f0b840"), 0.5f), UiTheme.A(Hex("#f0b840"), 0f), 128, 2.2f);
            spokes.gameObject.AddComponent<Spin>().degPerSec = 6f; // very slow shimmer/sway
            Pulse(rkRays, 0.4f, 0.62f, 2.5f);

            float wreathD = PanelW * 0.30f; // wreath diameter ≈0.30·panelW
            float shieldW = PanelW * 0.16f; // shield ≈0.16·panelW

            // Rank_Wreath — gold laurel ring (a gold disc rim around a transparent center).
            var wreath = UiWidgets.Rect("Rank_Wreath", grp, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(wreathD, wreathD));
            var wImg = wreath.gameObject.AddComponent<Image>(); wImg.raycastTarget = false;
            wImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 96, 16); wImg.type = Image.Type.Sliced;
            // laurel leaf hint: a soft gold ring glow
            UiWidgets.Glow(wreath, UiTheme.A(UiTheme.GoldHi, 0.3f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(wreathD * 1.15f, wreathD * 1.15f), 1.9f);

            // Rank_Shield — royal-blue heraldic shield (rounded-top shape stand-in: blue disc + gold rim).
            var shield = UiWidgets.Rect("Rank_Shield", grp, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(shieldW, shieldW * 1.15f));
            var shImg = shield.gameObject.AddComponent<Image>(); shImg.raycastTarget = false;
            shImg.sprite = UiTex.VGradient(BlueShield, BlueShieldLo, 64);
            var shRim = UiWidgets.Rect("ShieldRim", shield, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var shRimImg = shRim.gameObject.AddComponent<Image>(); shRimImg.raycastTarget = false;
            shRimImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 8); shRimImg.type = Image.Type.Sliced;

            // Rank_Sigil — gold gladiator/helm sigil centered on the shield (top z), with a glint.
            var sigil = UiWidgets.Rect("Rank_Sigil", shield, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(shieldW * 0.56f, shieldW * 0.56f));
            var sgImg = sigil.gameObject.AddComponent<Image>(); sgImg.raycastTarget = false;
            sgImg.sprite = UiTex.Diamond(Hex("#e8c46a"), 48);
            Pulse(sgImg, 0.8f, 1f, 2.5f); // occasional gold glint on the helm sigil

            // Rank_TierLabel — "Gold Tier III" beneath the crest (panel-fraction y≈0.35).
            var tier = UiWidgets.Label(panel, TierName, 30, PA(0.5f, 0.35f), PA(0.5f, 0.35f), Vector2.zero, new Vector2(PanelW * 0.62f, 50), TextAnchor.MiddleCenter, TierGold);
            tier.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(Hex("#3a2a08"), 0.85f);
        }

        // ---- "Rank up!" (white serif) — ONLY shown on tier-up (Negative Rule 2) ----
        private void BuildRankUp(Transform panel)
        {
            if (!TierUp) return; // omit entirely when tierUp == false
            var t = UiWidgets.Label(panel, "Rank up!", 50, PA(0.5f, 0.43f), PA(0.5f, 0.43f), Vector2.zero, new Vector2(PanelW * 0.6f, 70), TextAnchor.MiddleCenter, RankUpWhite);
            t.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(Hex("#1a140a"), 0.85f);
            UiWidgets.Glow(panel, UiTheme.A(UiTheme.GoldHi, 0.3f), PA(0.5f, 0.43f), PA(0.5f, 0.43f), Vector2.zero, new Vector2(420, 120), 1.7f);
        }

        // ---- You vs Opponent row (the ladder's competitive signature — Negative Rule 4) ----
        private void BuildVsRow(Transform panel)
        {
            // VS row centered at panel-fraction y≈0.56, width ≈0.72·panelW; each side ≈0.32·panelW.
            float sideDx = PanelW * 0.28f;
            // You — blue Iron-Pact shield + gold cross (left); winner sits slightly brighter/forward (§H).
            BuildFactionSide(panel, "Side_You", -sideDx, BlueShield, BlueShieldLo, "You", true, isWinner: Win);
            // Opponent — oxblood Ashen shield + jagged sigil (right); slightly dimmer/recessed.
            BuildFactionSide(panel, "Side_Opponent", +sideDx, OxShield, OxShieldLo, "Opponent", false, isWinner: !Win);
            // VS_Mark — gold beveled monogram, centered between the shields (≈0.08·panelW).
            UiWidgets.Glow(panel, UiTheme.A(UiTheme.GoldHi, 0.32f), PA(0.5f, 0.56f), PA(0.5f, 0.56f), Vector2.zero, new Vector2(180, 180));
            var vs = UiWidgets.TitleLabel(panel, "VS", 44, PA(0.5f, 0.56f), PA(0.5f, 0.56f), Vector2.zero, new Vector2(PanelW * 0.10f, 80), TextAnchor.MiddleCenter, Hex("#fff0c0"), VsGold, false);
            var vol = vs.gameObject.GetComponent<Outline>(); if (vol != null) vol.effectColor = UiTheme.A(Hex("#3a2a08"), 0.9f);
        }

        private void BuildFactionSide(Transform panel, string name, float dx, Color top, Color bottom, string label, bool cross, bool isWinner)
        {
            float shieldW = PanelW * 0.16f;
            float a = isWinner ? 1f : 0.82f; // winner brighter, loser dimmer/recessed
            var side = UiWidgets.Rect(name, panel, PA(0.5f, 0.56f), PA(0.5f, 0.56f), new Vector2(dx, 0), new Vector2(PanelW * 0.32f, PanelH * 0.16f));
            // shield banner
            var shield = UiWidgets.Rect("Shield", side, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -shieldW * 0.55f), new Vector2(shieldW, shieldW * 1.1f));
            var shImg = shield.gameObject.AddComponent<Image>(); shImg.raycastTarget = false;
            shImg.sprite = UiTex.VGradient(UiTheme.A(top, a), UiTheme.A(bottom, a), 64);
            var rim = UiWidgets.Rect("Rim", shield, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rimImg = rim.gameObject.AddComponent<Image>(); rimImg.raycastTarget = false;
            rimImg.sprite = UiTex.Frame(UiTheme.A(UiTheme.GoldHi, a), UiTheme.A(UiTheme.Gold, a), UiTheme.GoldShadow, 48, 6); rimImg.type = Image.Type.Sliced;
            // emblem: gold cross (You) or jagged dark sigil (Opponent)
            var emblem = UiWidgets.Rect("Emblem", shield, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(shieldW * 0.5f, shieldW * 0.5f));
            var emImg = emblem.gameObject.AddComponent<Image>(); emImg.raycastTarget = false;
            if (cross) { emImg.sprite = UiTex.Solid(UiTheme.A(UiTheme.GoldHi, a)); BuildCrossBar(shield, shieldW, a); }
            else { emImg.sprite = UiTex.Diamond(UiTheme.A(Hex("#1a0d0b"), a), 48); emblem.localRotation = Quaternion.Euler(0, 0, 45); }
            // label beneath (small caps)
            var lbl = UiWidgets.Label(side, label, 26, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 8), new Vector2(PanelW * 0.30f, 40), TextAnchor.MiddleCenter, ParchLabel);
            var c = lbl.color; c.a = a; lbl.color = c;
        }

        private void BuildCrossBar(Transform shield, float shieldW, float a)
        {
            // horizontal arm of the gold cross (the vertical arm is the Emblem Solid above, scaled tall)
            var bar = UiWidgets.Rect("CrossBar", shield, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(shieldW * 0.5f, shieldW * 0.16f));
            var bImg = bar.gameObject.AddComponent<Image>(); bImg.raycastTarget = false; bImg.color = UiTheme.A(UiTheme.GoldHi, a);
            // make the emblem the vertical bar (narrow + tall) by overlaying a slim vertical bar
            var vbar = UiWidgets.Rect("CrossBarV", shield, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(shieldW * 0.16f, shieldW * 0.5f));
            var vImg = vbar.gameObject.AddComponent<Image>(); vImg.raycastTarget = false; vImg.color = UiTheme.A(UiTheme.GoldHi, a);
        }

        // ---- "+24 Points" GREEN delta with a small green up-indicator (Negative Rule 3: green = gain only) ----
        private void BuildPoints(Transform panel)
        {
            bool gain = PointsDelta >= 0;
            // green for gain; a loss variant would be red #d8452b with a downward indicator (§K). Source = gain.
            Color num   = gain ? GreenPointsHi : Hex("#e8866a");
            Color word  = gain ? GreenWord     : Hex("#c87a5a");
            Color arrow = gain ? GreenPoints   : UiTheme.Ember2;

            UiWidgets.Glow(panel, UiTheme.A(arrow, 0.32f), PA(0.5f, 0.70f), PA(0.5f, 0.70f), Vector2.zero, new Vector2(420, 110), 1.7f);
            // small green up-indicator left of the value (≈0.04·panelW).
            var ind = UiWidgets.Rect("Points_Icon", panel, PA(0.5f, 0.70f), PA(0.5f, 0.70f), new Vector2(-PanelW * 0.16f, 0), new Vector2(PanelW * 0.04f, PanelW * 0.04f));
            var indImg = ind.gameObject.AddComponent<Image>(); indImg.raycastTarget = false;
            indImg.sprite = UiTex.Diamond(arrow, 48); ind.localRotation = Quaternion.Euler(0, 0, gain ? 45 : -135); // ▲ up (gain) / ▼ down (loss)

            // value "+24 Points" — count-up the number (§H/§I). Number color = bright; word = dimmer green.
            _pointsText = UiWidgets.Label(panel, "", 38, PA(0.5f, 0.70f), PA(0.5f, 0.70f), new Vector2(PanelW * 0.02f, 0), new Vector2(PanelW * 0.5f, 60), TextAnchor.MiddleCenter, num);
            _pointsText.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(Hex("#2c4a0c"), 0.9f);
            string sign = gain ? "+" : "-";
            _pointsCount = gameObject.AddComponent<CountUp>();
            _pointsCount.Bind(_pointsText, v => sign + Mathf.Abs(v) + " Points", 0f);
        }

        // ---- "Rewards" + silver coin stack + "12,450" (single reward row — Negative Rule 5) ----
        private void BuildRewards(Transform panel)
        {
            // "Rewards" small-caps label (panel-fraction y≈0.76).
            var lbl = UiWidgets.Label(panel, UiTheme.Track("REWARDS"), 24, PA(0.5f, 0.76f), PA(0.5f, 0.76f), Vector2.zero, new Vector2(PanelW * 0.5f, 40), TextAnchor.MiddleCenter, RewardGold);
            lbl.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.GoldShadow, 0.7f);

            // coin icon (silver stack) left of the value (≈0.06·panelW), row centered at y≈0.82.
            var coin = UiWidgets.Rect("Icon_Coins", panel, PA(0.5f, 0.82f), PA(0.5f, 0.82f), new Vector2(-PanelW * 0.12f, 0), new Vector2(PanelW * 0.06f, PanelW * 0.06f));
            var ci = coin.gameObject.AddComponent<Image>(); ci.raycastTarget = false; ci.sprite = UiTex.Disc(Hex("#9aa0a8"), 48);
            var coinFace = UiWidgets.Rect("CoinFace", coin, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(PanelW * 0.045f, PanelW * 0.045f));
            var cf = coinFace.gameObject.AddComponent<Image>(); cf.raycastTarget = false; cf.sprite = UiTex.Disc(Hex("#5a6068"), 48);

            // value "12,450" — count-up (§H/§I), tabular warm-white.
            _coinsText = UiWidgets.Label(panel, "", 36, PA(0.5f, 0.82f), PA(0.5f, 0.82f), new Vector2(PanelW * 0.04f, 0), new Vector2(PanelW * 0.4f, 56), TextAnchor.MiddleCenter, CoinsWhite);
            _coinsCount = gameObject.AddComponent<CountUp>();
            _coinsCount.Bind(_coinsText, v => v.ToString("N0"), 0f);
        }

        // ---- Single CONTINUE CTA — royal-blue, gold frame, inner cobalt glow (the brightest interactive) ----
        // Negative Rule 6: NO RETRY on a ranked/async result. Negative Rule 7: CONTINUE is the brightest button.
        private void BuildContinue(Transform panel)
        {
            float cw = PanelW * 0.74f;   // width ≈0.74·panelW
            float ch = PanelH * 0.075f;  // height ≈0.075·panelH
            // breathing cobalt rim-glow (persistent, §I/§J).
            var glow = UiWidgets.Glow(panel, UiTheme.A(Hex("#5aa0ff"), 0.45f), PA(0.5f, 0.92f), PA(0.5f, 0.92f), Vector2.zero, new Vector2(cw * 1.1f, ch * 2.4f), 1.7f);
            Pulse(glow, 0.3f, 0.6f, 1.8f);

            var btn = UiWidgets.GemButton(panel, "CONTINUE", PA(0.5f, 0.92f), PA(0.5f, 0.92f), Vector2.zero, new Vector2(cw, ch),
                UiTheme.IronBlue, MatchPresentation.OnContinue, 44, false, false, TextAnchor.MiddleCenter);
            // re-skin the body to the spec's royal-blue gradient #0d2a66→#388ee8 + cobalt rim sheen.
            var body = btn.GetComponent<Image>(); if (body != null) body.sprite = UiTex.VGradient(ContinueHi, ContinueLo, 32);
            // brighten the CONTINUE label toward warm white with a dark-blue stroke (§F).
            var lbl = btn.GetComponentInChildren<Text>();
            if (lbl != null)
            {
                lbl.color = RankUpWhite;
                var ol = lbl.gameObject.GetComponent<Outline>(); if (ol != null) ol.effectColor = UiTheme.A(Hex("#0a1f44"), 0.95f);
            }
        }

        public override void OnShow()
        {
            AudioManager.Instance?.PlayEndStinger(Win); // ranked verdict sting (guarded single fire)
            StartCoroutine(Reveal());
        }

        // Prestige rank reveal (§I): panel → title → crest → tier label → Rank up! → VS → points count-up →
        // reward count-up → CONTINUE. Display-only; never resolves/writes rank or balance. Tap-to-skip snaps to end.
        private IEnumerator Reveal()
        {
            if (_panelCg == null) yield break;
            _panelCg.alpha = 0f;
            float t = 0f; const float d = 0.4f;
            while (t < d) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d); _panelCg.alpha = k; _panelCg.transform.localScale = Vector3.one * Mathf.Lerp(0.9f, 1f, k); yield return null; }
            _panelCg.alpha = 1f; _panelCg.transform.localScale = Vector3.one;

            yield return Wait(1.85f); // crest assembly + tier label + Rank up! + VS slide-in have elapsed (passive FX)
            _pointsCount?.To(PointsDelta, 0.6f);   // +0 → +24 (green)
            yield return Wait(0.55f);
            _coinsCount?.To(CoinReward, 0.7f);     // 0 → 12,450
        }

        private IEnumerator Wait(float seconds)
        {
            float t = 0f;
            while (t < seconds) { t += Time.unscaledDeltaTime; yield return null; }
        }

        // ---- helpers ----
        // Point-anchor inside the PANEL using the spec's vertical rhythm: fy = fraction from panel TOP → anchorY = 1 − fy.
        private static Vector2 PA(float fx, float fyFromTop) => new Vector2(fx, 1f - fyFromTop);

        // Attach a documented PulseGraphic (sine alpha breathe, unscaled time) to a Graphic.
        private static void Pulse(Graphic g, float min, float max, float period)
        {
            var p = g.gameObject.AddComponent<PulseGraphic>();
            p.target = g; p.min = min; p.max = max; p.period = period;
        }

        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}

// BULWARK — CAMPAIGN RESULT (UI Construction Bible · 14). Presentation-only, REMOVABLE.
//
// Forensic build of design/CampaignResultDesign.png (Bible §14 A–O): the post-campaign-level "Level Cleared"
// result — a warm PARCHMENT QUEST-SCROLL (cream #d6bd86–#e2c98a, gold 9-slice frame) flanked by blue Iron-Pact
// war-banners over a dimmed, heavily-vignetted campaign map, with the hero 3-STAR ARC breaking the top frame
// (center largest+highest, sides smaller+lower; fills driven by starsEarned — never hard-coded, §L-1), a GREEN
// clear-time stat (#93bf37), two gold-framed dark reward slots (silver coins / purple gems), and the NEXT LEVEL
// (royal-blue primary, breathing glow) + REPLAY (dark-stone secondary) CTAs split by a gold sword+shield emblem.
// Runs the §I reveal ceremony (panel→banners→title→ribbon→L/C/R star pops→time count→reward count-ups→CTAs)
// with tap-to-skip that still resolves the correct star end-state. ALL level/reward values are display-only local
// stubs matching the mock — NO ECS, NO gameplay/economy/progress mutation; CTAs route via MatchPresentation (§12).

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-14 Campaign "Level Cleared" result. Presentation-only (values display-only stubs, §12).</summary>
    public sealed class CampaignResultScreen : UiScreen
    {
        // ---- DISPLAY-ONLY stub payload (the mock's numbers). Per §K these arrive read-only from ECS level
        // evaluation + server reward grant; here they are inert local constants. NO balance/progress is written. ----
        private const int   LevelNumber       = 7;       // → "LEVEL 7 CLEARED"
        private const int   StarsEarned       = 2;       // 0–3; drives star fills (source shows 2 of 3)
        private const int   StarsTotal        = 3;
        private const int   ClearTimeSeconds  = 165;     // 02:45
        private const int   RewardCoins       = 12450;   // "12,450"
        private const int   RewardGems        = 60;      // "60"
        private const bool  HasNextLevel      = true;    // §H: if false → NEXT LEVEL desaturates/disables

        // Panel geometry (fractions of 2340×1080, match-height): width ≈ 0.58·W, height ≈ 0.84·H, ≈1.50:1 scroll.
        private const float PanelW = 1357f; // 0.58 * 2340
        private const float PanelH = 907f;  // 0.84 * 1080

        // Forensic hexes not in UiTheme.
        private static readonly Color Parch       = Hex("#d6bd86"); // parchment base
        private static readonly Color ParchHi     = Hex("#e2c98a"); // lighter center
        private static readonly Color ParchEdge   = Hex("#916330"); // aged dark edge/stain
        private static readonly Color InkBrown     = Hex("#3a2a12"); // title engraved dark
        private static readonly Color InkBrownHi   = Hex("#f0e2b8"); // letterpress light highlight
        private static readonly Color LabelBrown   = Hex("#5a4424"); // muted-brown small-caps labels
        private static readonly Color GreenTime    = Hex("#93bf37"); // vivid lime-green clear-time
        private static readonly Color GreenTimeHi  = Hex("#b6e35a"); // brighter green gradient top
        private static readonly Color GreenStroke  = Hex("#2c4a0c"); // dark-green stroke/glow
        private static readonly Color TileDark      = Hex("#15110b"); // recessed slot interior
        private static readonly Color TileFrame     = Hex("#af874e"); // slot gold bevel
        private static readonly Color RewardWhite   = Hex("#f3ead2"); // warm-white reward numbers
        private static readonly Color StarFillHi    = Hex("#ffe27a"); // filled-star gradient top
        private static readonly Color StarFillLo    = Hex("#e0a83a"); // filled-star gradient bottom
        private static readonly Color StarHollow    = Hex("#23180f"); // empty-star dark interior
        private static readonly Color StarRim       = Hex("#a07a3a"); // empty-star gold rim
        private static readonly Color BannerBlue    = Hex("#2b56c8"); // Iron-Pact cloth (top)
        private static readonly Color BannerBlueLo  = Hex("#1f3e78"); // Iron-Pact cloth (bottom)
        private static readonly Color CoinHi        = Hex("#9aa0a8"); // silver coin highlight
        private static readonly Color CoinLo        = Hex("#5a6068"); // silver coin shadow
        private static readonly Color GemHi         = Hex("#c89bff"); // gem highlight
        private static readonly Color GemBody       = Hex("#7a3fd0"); // gem body
        private static readonly Color GemLo         = Hex("#5a2db0"); // gem deep
        private static readonly Color NextBlueHi    = Hex("#2f6fd6"); // NEXT fill top
        private static readonly Color NextBlueLo    = Hex("#0d2a66"); // NEXT fill bottom
        private static readonly Color NextRim       = Hex("#5aa0ff"); // cobalt rim-glow
        private static readonly Color StoneHi       = Hex("#3a342a"); // REPLAY stone top
        private static readonly Color StoneLo       = Hex("#2a2620"); // REPLAY stone bottom

        // Ceremony handles (driven by Reveal / snapped by skip).
        private CanvasGroup _panelCg, _bannerCg, _titleCg, _ribbonCg, _timeCg, _rewLblCg, _coinTileCg, _gemTileCg, _nextCg, _replayCg;
        private RectTransform _starL, _starC, _starR;
        private Text _timeValue, _coinValue, _gemValue;
        private CountUp _coinCounter, _gemCounter;
        private bool _revealed; // skip-guard

        protected override void Build()
        {
            // ============================ FULL-BLEED BACKGROUND (Rect) ============================
            // BG_CampaignMap (dimmed battlefield) → heavy Vignette (→near-black #010101 corners) → dark scrim α0.45.
            UiWidgets.Stretch("BG_CampaignMap", Rect, UiTheme.Charcoal, "bg_battle");
            UiWidgets.Stretch("BG_MapDim", Rect, UiTheme.A(Hex("#010101"), 0.30f)); // push the map far back
            UiWidgets.Vignette(Rect, 0.72f); // heavy → near-black edges
            UiWidgets.Stretch("BG_DarkenScrim", Rect, new Color(0f, 0f, 0f, 0.45f));
            // Faint warm dust motes drifting in front of the parchment (campaign ambiance, very low opacity — §J).
            var dust = UiWidgets.Rect("Dust", Rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(PanelW, PanelH));
            var dustFx = dust.gameObject.AddComponent<EmberField>(); dustFx.count = 10; dustFx.color = UiTheme.A(Hex("#e8d9a8"), 0.5f);

            // ============================ RESULT PANEL (SafeContent — interactive/important) ============================
            var panelGo = new GameObject("ResultPanel", typeof(RectTransform), typeof(CanvasGroup));
            var panel = (RectTransform)panelGo.transform; panel.SetParent(SafeContent, false);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(PanelW, PanelH);
            panel.anchoredPosition = new Vector2(0f, -24f); // sits a touch low to leave room for the star arc above
            _panelCg = panelGo.GetComponent<CanvasGroup>();

            // Panel_Frame (ornate gold 9-slice) → inner field reused as the parchment fill.
            var field = UiWidgets.OrnateFrame(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(PanelW, PanelH), Parch, false, 24f);
            // Panel_Parchment: paint the field as aged cream paper (lighter center → darker aged edges) over the gradient.
            var parchImg = field.gameObject.GetComponent<Image>();
            if (parchImg != null) parchImg.sprite = UiTex.VGradient(ParchHi, ParchEdge, 64);
            // soft inner shadow from the frame (top-darkened band), + uneven aged stain at the lower edge.
            UiWidgets.Glow(field, UiTheme.A(Hex("#6b4a1e"), 0.22f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 20), new Vector2(PanelW, 220), 1.4f);

            BuildBanners(panel);     // §C Banner_Left / Banner_Right (draped blue Iron-Pact war-banners)
            BuildStars(panel);       // §C Stars_Group (ribbon + 3-star arc breaking the top frame)
            BuildTitle(field);       // §C Title_Cleared
            BuildClearTime(field);   // §C ClearTime_Group (label + gold clock + GREEN value)
            BuildRewards(field);     // §C Rewards_Group (label + two gold-framed dark slot tiles)
            BuildButtons(field);     // §C Buttons_Group (NEXT LEVEL + emblem + REPLAY)

            // Top-left system back → Campaign Map (non-destructive). Destination not built → toast (§K OnBack).
            UiWidgets.BackButton(SafeContent, () => Router.Toast("Campaign Map — coming soon"));
        }

        // ---- Banners: blue Iron-Pact war-banners draped over the top-left/right corners, hanging down, extending
        //      slightly OUTSIDE the parchment (§E). Gold stitched trim + a gold heraldic crest; soft drop shadow. ----
        private void BuildBanners(Transform panel)
        {
            var grpGo = new GameObject("Banners_Group", typeof(RectTransform), typeof(CanvasGroup));
            var grp = (RectTransform)grpGo.transform; grp.SetParent(panel, false);
            grp.anchorMin = Vector2.zero; grp.anchorMax = Vector2.one; grp.offsetMin = Vector2.zero; grp.offsetMax = Vector2.zero;
            _bannerCg = grpGo.GetComponent<CanvasGroup>();

            Banner(grp, new Vector2(0f, 1f), +1f); // Banner_Left  (pivot top-left corner)
            Banner(grp, new Vector2(1f, 1f), -1f); // Banner_Right (mirrored top-right)
        }

        private void Banner(Transform grp, Vector2 corner, float dir)
        {
            float bw = PanelW * 0.10f, bh = PanelH * 0.55f;
            float xOff = dir * (bw * 0.18f); // drape slightly outside the parchment to the left/right
            // cloth (royal-blue vertical gradient), hangs DOWN from the corner.
            var cloth = UiWidgets.Rect(dir > 0 ? "Banner_Left" : "Banner_Right", grp, corner, corner,
                new Vector2(xOff, bh * -0.5f + 14f), new Vector2(bw, bh));
            var ci = cloth.gameObject.AddComponent<Image>(); ci.raycastTarget = false;
            ci.sprite = UiTex.VGradient(BannerBlue, BannerBlueLo, 64);
            // drop shadow onto the parchment.
            var sh = cloth.gameObject.AddComponent<Shadow>(); sh.effectColor = new Color(0, 0, 0, 0.45f); sh.effectDistance = new Vector2(dir * -4f, -6f);
            // stitched gold trim down each long edge (thin gold bar).
            UiWidgets.Divider(cloth, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, bh * 0.5f), bw - 8f, 4f, UiTheme.Gold);
            // gold heraldic crest (lion/sigil) near the top of the cloth.
            var crest = UiWidgets.Rect("Crest", cloth, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -bw * 0.55f), new Vector2(bw * 0.72f, bw * 0.72f));
            var cri = crest.gameObject.AddComponent<Image>(); cri.raycastTarget = false; cri.sprite = UiTex.Diamond(UiTheme.GoldHi, 48);
            // forked banner tail (small diamond at the bottom).
            var tail = UiWidgets.Rect("Tail", cloth, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, -bw * 0.35f), new Vector2(bw, bw * 0.7f));
            var ti = tail.gameObject.AddComponent<Image>(); ti.raycastTarget = false; ti.sprite = UiTex.Diamond(BannerBlueLo, 48);
        }

        // ---- Stars: gold ribbon + 3-star arc breaking the top frame. Center largest+highest; sides smaller+lower.
        //      Fills DRIVEN by StarsEarned (§L-1): filled = hot-gold gradient + bloom; empty = dark hollow + gold rim. ----
        private void BuildStars(Transform panel)
        {
            // Stars_Group anchored top-center of the panel (y=1), breaking upward.
            var grp = UiWidgets.Rect("Stars_Group", panel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(PanelW * 0.6f, PanelH * 0.22f));

            // Star_Ribbon: curved gold banner the stars sit on (≈0.55·panelW), behind the stars.
            var ribGo = new GameObject("Star_Ribbon", typeof(RectTransform), typeof(CanvasGroup));
            var rib = (RectTransform)ribGo.transform; rib.SetParent(grp, false);
            rib.anchorMin = rib.anchorMax = new Vector2(0.5f, 0.5f); rib.anchoredPosition = new Vector2(0, -6f); rib.sizeDelta = new Vector2(PanelW * 0.55f, PanelH * 0.10f);
            _ribbonCg = ribGo.GetComponent<CanvasGroup>();
            var ribImg = rib.gameObject.AddComponent<Image>(); ribImg.raycastTarget = false; ribImg.sprite = UiTex.VGradient(UiTheme.GoldHi, UiTheme.Gold, 32);
            // subtle gold shimmer traveling along the banner (§J).
            var shGo = UiWidgets.Rect("RibbonShimmer", rib, new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, new Vector2(80f, 0f));
            var shImg = shGo.gameObject.AddComponent<Image>(); shImg.raycastTarget = false; shImg.sprite = UiTex.HGradient(UiTheme.A(Color.white, 0f), UiTheme.A(Color.white, 0.45f), 32);
            var sheen = rib.gameObject.AddComponent<Sheen>(); sheen.target = shGo; sheen.minX = -PanelW * 0.27f; sheen.maxX = PanelW * 0.27f; sheen.period = 2.2f;
            UiWidgets.Finial(rib, new Vector2(0, 0.5f), new Vector2(-8, 0), PanelH * 0.10f * 1.4f);
            UiWidgets.Finial(rib, new Vector2(1, 0.5f), new Vector2(8, 0), PanelH * 0.10f * 1.4f);

            // Side stars at x≈0.38/0.62·panelW, centerY≈0; center star at x=0.5, centerY≈ -0.06·panelH (higher), bigger.
            float sideD = PanelH * 0.13f, ctrD = PanelH * 0.16f;
            float sideX = PanelW * 0.12f; // 0.62 − 0.5 of panelW, relative to the centered group
            _starL = MakeStar(grp, "Star_L", new Vector2(-sideX, 0f), sideD, StarsEarned >= 1);
            _starR = MakeStar(grp, "Star_R", new Vector2(+sideX, 0f), sideD, StarsEarned >= 3);
            _starC = MakeStar(grp, "Star_C", new Vector2(0f, PanelH * 0.06f), ctrD, StarsEarned >= 2); // highest + largest + top z
        }

        private RectTransform MakeStar(Transform grp, string name, Vector2 pos, float diameter, bool earned)
        {
            var star = UiWidgets.Rect(name, grp, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(diameter, diameter));
            if (earned)
            {
                // bloom halo (filled stars + the green time + gem + NEXT carry the bloom budget — §G).
                UiWidgets.Glow(star, UiTheme.A(StarFillHi, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(diameter * 1.7f, diameter * 1.7f), 1.6f);
            }
            // diamond stand-in for the 5-point star pip (filled hot-gold OR dark hollow with a thin gold rim).
            var pip = UiWidgets.Rect("Pip", star, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(diameter, diameter));
            var pimg = pip.gameObject.AddComponent<Image>(); pimg.raycastTarget = false;
            if (earned) pimg.sprite = UiTex.VGradient(StarFillHi, StarFillLo, 32); // emboss via vertical gradient
            else        pimg.sprite = UiTex.Diamond(StarHollow, 48);
            if (!earned)
            {
                // thin gold rim on the hollow star (clearly "not earned").
                var rim = UiWidgets.Rect("Rim", star, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(diameter * 1.08f, diameter * 1.08f));
                var ri = rim.gameObject.AddComponent<Image>(); ri.raycastTarget = false; ri.sprite = UiTex.Diamond(StarRim, 48); rim.SetSiblingIndex(0);
            }
            else
            {
                // gentle persistent glint on earned stars (loop, §I 3.05).
                var p = pip.gameObject.AddComponent<PulseGraphic>(); p.target = pimg; p.min = 0.85f; p.max = 1f; p.period = 3f;
            }
            // initial scale is set in Reveal() (earned stars pop from small; unearned settle in); built look is final.
            return star;
        }

        // ---- Title_Cleared: dark engraved serif on parchment (NOT gold-on-dark, §L-2). centerY 0.16, height 0.12. ----
        private void BuildTitle(Transform field)
        {
            var go = new GameObject("Title_Cleared", typeof(RectTransform), typeof(CanvasGroup));
            var rt = (RectTransform)go.transform; rt.SetParent(field, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.84f); rt.anchoredPosition = Vector2.zero; rt.sizeDelta = new Vector2(PanelW * 0.60f, PanelH * 0.12f);
            _titleCg = go.GetComponent<CanvasGroup>();
            // letterpress: a light bottom-edge highlight (debossed) UNDER the dark engraved fill.
            UiWidgets.Label(rt, UiTheme.Track("LEVEL " + LevelNumber + " CLEARED"), 60, Vector2.zero, Vector2.one, new Vector2(0, -2), Vector2.zero, TextAnchor.MiddleCenter, UiTheme.A(InkBrownHi, 0.5f));
            UiWidgets.Label(rt, UiTheme.Track("LEVEL " + LevelNumber + " CLEARED"), 60, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, InkBrown);
        }

        // ---- ClearTime: "Clear Time" small-caps + gold stopwatch + "02:45" in vivid GREEN with a green glow (§L-3). ----
        private void BuildClearTime(Transform field)
        {
            // label (centerY 0.30 of panel = anchorY 0.70).
            UiWidgets.Label(field, UiTheme.Track("CLEAR TIME"), 27, new Vector2(0.5f, 0.70f), new Vector2(0.5f, 0.70f), Vector2.zero, new Vector2(PanelW * 0.5f, PanelH * 0.05f), TextAnchor.MiddleCenter, LabelBrown);
            // thin gold filigree rule under the label.
            UiWidgets.Divider(field, new Vector2(0.5f, 0.665f), new Vector2(0.5f, 0.665f), Vector2.zero, PanelW * 0.22f, 2f, UiTheme.A(UiTheme.Gold, 0.7f));

            var rowGo = new GameObject("ClearTime_Row", typeof(RectTransform), typeof(CanvasGroup));
            var row = (RectTransform)rowGo.transform; row.SetParent(field, false);
            row.anchorMin = row.anchorMax = new Vector2(0.5f, 0.60f); row.anchoredPosition = Vector2.zero; row.sizeDelta = new Vector2(PanelW * 0.5f, PanelH * 0.09f); // centerY 0.40
            _timeCg = rowGo.GetComponent<CanvasGroup>();

            // gold stopwatch icon (ring + pale face) just left of the value (≈0.06·panelW square).
            float clk = PanelW * 0.06f;
            var ring = UiWidgets.Rect("Icon_Clock", row, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-120f, 0), new Vector2(clk, clk));
            var rImg = ring.gameObject.AddComponent<Image>(); rImg.raycastTarget = false; rImg.sprite = UiTex.Disc(UiTheme.Gold, 48);
            var face = UiWidgets.Rect("Face", ring, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(clk * 0.62f, clk * 0.62f));
            var fImg = face.gameObject.AddComponent<Image>(); fImg.raycastTarget = false; fImg.sprite = UiTex.Disc(Hex("#e8dcc0"), 48);
            var hand = UiWidgets.Rect("Hand", face, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, clk * 0.12f), new Vector2(3f, clk * 0.30f));
            var hImg = hand.gameObject.AddComponent<Image>(); hImg.raycastTarget = false; hImg.color = Hex("#3a2a12");

            // green-glow bloom behind the value, then the value itself.
            UiWidgets.Glow(row, UiTheme.A(GreenTime, 0.4f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(40f, 0), new Vector2(360f, 140f), 1.6f);
            _timeValue = UiWidgets.Label(row, Mmss(ClearTimeSeconds), 50, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(40f, 0), new Vector2(360f, PanelH * 0.09f), TextAnchor.MiddleCenter, GreenTime);
            var tg = _timeValue.gameObject.AddComponent<UiGradientText>(); tg.top = GreenTimeHi; tg.bottom = GreenTime; // slight emboss
            var to = _timeValue.gameObject.AddComponent<Outline>(); to.effectColor = UiTheme.A(GreenStroke, 0.9f); to.effectDistance = new Vector2(1.5f, -1.5f);
        }

        // ---- Rewards: "Rewards" small-caps + two gold-framed dark slot tiles: LEFT silver coins, RIGHT purple gems
        //      (in that order — §L-4). Each tile ≈0.30·panelW × 0.20·panelH, gap ≈0.04. Numbers count-up (§I). ----
        private void BuildRewards(Transform field)
        {
            // label (centerY 0.52 → anchorY 0.48).
            var lblGo = new GameObject("Rewards_Label", typeof(RectTransform), typeof(CanvasGroup));
            var lbl = (RectTransform)lblGo.transform; lbl.SetParent(field, false);
            lbl.anchorMin = lbl.anchorMax = new Vector2(0.5f, 0.48f); lbl.anchoredPosition = Vector2.zero; lbl.sizeDelta = new Vector2(PanelW * 0.5f, PanelH * 0.05f);
            _rewLblCg = lblGo.GetComponent<CanvasGroup>();
            UiWidgets.Label(lbl, UiTheme.Track("REWARDS"), 27, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, LabelBrown);

            float tileW = PanelW * 0.30f, tileH = PanelH * 0.20f, gapHalf = PanelW * 0.02f;
            // LEFT = coins, RIGHT = gems (centerY 0.66 → anchorY 0.34).
            _coinTileCg = MakeRewardTile(field, "Reward_Slot_Coins", new Vector2(-(tileW * 0.5f + gapHalf), 0f), tileW, tileH, true,  RewardCoins, out _coinValue, out _coinCounter);
            _gemTileCg  = MakeRewardTile(field, "Reward_Slot_Gems",  new Vector2(+(tileW * 0.5f + gapHalf), 0f), tileW, tileH, false, RewardGems,  out _gemValue,  out _gemCounter);
        }

        private CanvasGroup MakeRewardTile(Transform field, string name, Vector2 pos, float tileW, float tileH, bool coins, int value, out Text valueText, out CountUp counter)
        {
            var tileGo = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            var tile = (RectTransform)tileGo.transform; tile.SetParent(field, false);
            tile.anchorMin = tile.anchorMax = new Vector2(0.5f, 0.34f); tile.anchoredPosition = pos; tile.sizeDelta = new Vector2(tileW, tileH);
            var cg = tileGo.GetComponent<CanvasGroup>();

            // dark recessed interior so light reward art/numbers pop.
            var bg = tile.gameObject.AddComponent<Image>(); bg.sprite = UiTex.VGradient(Hex("#1c160e"), TileDark, 32); bg.raycastTarget = false;
            // gold bevel frame (9-slice).
            var frame = UiWidgets.Rect("Slot_Frame", tile, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var fImg = frame.gameObject.AddComponent<Image>(); fImg.sprite = UiTex.Frame(UiTheme.GoldHi, TileFrame, UiTheme.GoldShadow, 48, 7); fImg.type = Image.Type.Sliced; fImg.raycastTarget = false;

            // icon in the upper area of the tile.
            var icon = UiWidgets.Rect(coins ? "Icon_Coins" : "Icon_Gems", tile, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -tileH * 0.34f), new Vector2(tileH * 0.46f, tileH * 0.46f));
            var iImg = icon.gameObject.AddComponent<Image>(); iImg.raycastTarget = false;
            if (coins)
            {
                // stacked silver/steel coins (occasional silver glint sweep — §J).
                iImg.sprite = UiTex.VGradient(CoinHi, CoinLo, 32);
                var c2 = UiWidgets.Rect("Coin2", icon, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, -10), new Vector2(tileH * 0.5f, tileH * 0.18f));
                var c2i = c2.gameObject.AddComponent<Image>(); c2i.raycastTarget = false; c2i.sprite = UiTex.VGradient(UiWidgets.Lighten(CoinHi, 0.1f), CoinLo, 32);
                var glintGo = UiWidgets.Rect("Glint", icon, new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, new Vector2(24f, 0f));
                var glImg = glintGo.gameObject.AddComponent<Image>(); glImg.raycastTarget = false; glImg.sprite = UiTex.HGradient(UiTheme.A(Color.white, 0f), UiTheme.A(Color.white, 0.5f), 32);
                var glSheen = icon.gameObject.AddComponent<Sheen>(); glSheen.target = glintGo; glSheen.minX = -tileH * 0.25f; glSheen.maxX = tileH * 0.25f; glSheen.period = 3.5f;
            }
            else
            {
                // clustered violet crystals + magical inner glow / occasional spec twinkle (§J).
                UiWidgets.Glow(icon, UiTheme.A(GemHi, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(tileH * 0.7f, tileH * 0.7f), 1.6f);
                iImg.sprite = UiTex.Diamond(GemBody, 48);
                var g2 = UiWidgets.Rect("Gem2", icon, new Vector2(0.32f, 0.4f), new Vector2(0.32f, 0.4f), Vector2.zero, new Vector2(tileH * 0.24f, tileH * 0.24f));
                var g2i = g2.gameObject.AddComponent<Image>(); g2i.raycastTarget = false; g2i.sprite = UiTex.Diamond(GemHi, 48);
                var g3 = UiWidgets.Rect("Gem3", icon, new Vector2(0.7f, 0.42f), new Vector2(0.7f, 0.42f), Vector2.zero, new Vector2(tileH * 0.2f, tileH * 0.2f));
                var g3i = g3.gameObject.AddComponent<Image>(); g3i.raycastTarget = false; g3i.sprite = UiTex.Diamond(GemLo, 48);
                var tw = g2i.gameObject.AddComponent<PulseGraphic>(); tw.target = g2i; tw.min = 0.6f; tw.max = 1f; tw.period = 2.4f;
            }

            // bottom row: small currency glyph + number (warm white on the dark tile).
            var glyph = UiWidgets.Rect("MiniGlyph", tile, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-tileW * 0.26f, tileH * 0.20f), new Vector2(tileH * 0.16f, tileH * 0.16f));
            var glImg2 = glyph.gameObject.AddComponent<Image>(); glImg2.raycastTarget = false; glImg2.sprite = coins ? UiTex.Disc(CoinHi, 48) : UiTex.Diamond(GemHi, 48);
            valueText = UiWidgets.Label(tile, value.ToString("N0"), 36, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(tileW * 0.06f, tileH * 0.20f), new Vector2(tileW * 0.7f, tileH * 0.3f), TextAnchor.MiddleCenter, RewardWhite);
            counter = valueText.gameObject.AddComponent<CountUp>();
            counter.Bind(valueText, v => v.ToString("N0"), 0f);
            return cg;
        }

        // ---- Buttons: NEXT LEVEL (royal-blue primary, glowing) + gold sword+shield emblem + REPLAY (dark-stone
        //      secondary). NEXT routes forward; REPLAY re-enters the level. REPLAY must NOT compete (§L-5). ----
        private void BuildButtons(Transform field)
        {
            float btnW = PanelW * 0.40f, btnH = PanelH * 0.10f;
            float halfGap = PanelW * 0.04f; // emblem ≈0.06 centered → buttons sit ±(btnW/2 + a touch)
            float bx = btnW * 0.5f + halfGap;

            // CTA_NextLevel (left) — bright blue primary. centerY 0.89 → anchorY 0.11.
            _nextCg = MakeCta(field, "Next", "NEXT LEVEL", new Vector2(-bx, 0f), btnW, btnH, true, () =>
            {
                if (!HasNextLevel) { Router.Toast("World Map — coming soon"); return; }
                MatchPresentation.OnContinue(); // → progression (next level / hub via the existing presentation seam)
            });

            // gold sword+shield heraldic emblem divider (≈0.06·panelW), bridging the two buttons.
            var emb = UiWidgets.Rect("Buttons_Divider", field, new Vector2(0.5f, 0.11f), new Vector2(0.5f, 0.11f), Vector2.zero, new Vector2(PanelW * 0.06f, btnH));
            UiWidgets.Glow(emb, UiTheme.A(UiTheme.GoldHi, 0.4f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(PanelW * 0.07f, btnH * 0.9f), 1.6f);
            var shield = UiWidgets.Rect("Shield", emb, new Vector2(0.5f, 0.45f), new Vector2(0.5f, 0.45f), Vector2.zero, new Vector2(PanelW * 0.045f, PanelW * 0.045f));
            var sImg = shield.gameObject.AddComponent<Image>(); sImg.raycastTarget = false; sImg.sprite = UiTex.Diamond(UiTheme.Gold, 48);
            var sword = UiWidgets.Rect("Sword", emb, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(6f, btnH * 0.85f));
            var swImg = sword.gameObject.AddComponent<Image>(); swImg.raycastTarget = false; swImg.sprite = UiTex.VGradient(UiTheme.GoldHi, UiTheme.GoldShadow, 32);

            // CTA_Replay (right) — dark-stone secondary, matte (no glow pulse).
            _replayCg = MakeCta(field, "Replay", "REPLAY", new Vector2(+bx, 0f), btnW, btnH, false, MatchPresentation.OnRetry);
        }

        private CanvasGroup MakeCta(Transform field, string id, string label, Vector2 pos, float w, float h, bool primary, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("CTA_" + id, typeof(RectTransform), typeof(CanvasGroup));
            var rt = (RectTransform)go.transform; rt.SetParent(field, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.11f); rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(w, h);
            var cg = go.GetComponent<CanvasGroup>();

            bool unavailable = primary && !HasNextLevel; // §H: no next level → desaturate/disable NEXT LEVEL.

            if (primary && !unavailable)
            {
                // persistent breathing cobalt rim-glow (it is the encouraged progression).
                var gl = UiWidgets.Glow(rt, UiTheme.A(NextRim, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(w * 1.18f, h * 1.7f), 1.6f);
                var pg = gl.gameObject.AddComponent<PulseGraphic>(); pg.target = gl; pg.min = 0.3f; pg.max = 0.6f; pg.period = 1.8f;
            }

            // fill (Next_Fill / Replay_Fill).
            var fill = rt.gameObject.AddComponent<Image>();
            if (unavailable)      fill.sprite = UiTex.VGradient(Hex("#4a443a"), StoneLo, 32);
            else if (primary)     fill.sprite = UiTex.VGradient(NextBlueHi, NextBlueLo, 32);
            else                  fill.sprite = UiTex.VGradient(StoneHi, StoneLo, 32);

            var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = fill; btn.interactable = !unavailable;
            var cbz = btn.colors; cbz.normalColor = Color.white; cbz.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f); cbz.pressedColor = new Color(0.9f, 0.9f, 0.92f, 1f); cbz.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.7f); cbz.fadeDuration = 0.08f; btn.colors = cbz;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); onClick?.Invoke(); });

            // inner blue rim-glow (primary only).
            if (primary && !unavailable)
            {
                var inner = UiWidgets.Rect("InnerRim", rt, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-6, -6));
                var inImg = inner.gameObject.AddComponent<Image>(); inImg.raycastTarget = false; inImg.sprite = UiTex.Frame(UiTheme.A(NextRim, 0.45f), UiTheme.A(NextRim, 0.2f), UiTheme.A(NextBlueLo, 0f), 48, 6); inImg.type = Image.Type.Sliced;
            }

            // cast-gold beveled frame over the fill edge (Next_Frame / Replay_Frame).
            var frame = UiWidgets.Rect("Frame", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var frImg = frame.gameObject.AddComponent<Image>(); frImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 8); frImg.type = Image.Type.Sliced; frImg.raycastTarget = false;

            // label.
            Color labelCol = unavailable ? Hex("#8a8278") : (primary ? Hex("#fffbf0") : Hex("#f3ecdc"));
            var lbl = UiWidgets.Label(rt, UiTheme.Track(label), primary ? 38 : 38, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, labelCol);
            var ol = lbl.gameObject.AddComponent<Outline>(); ol.effectColor = primary ? UiTheme.A(Hex("#0a1f44"), 0.85f) : UiTheme.A(Hex("#1a140a"), 0.85f); ol.effectDistance = new Vector2(2f, -3f);
            return cg;
        }

        // ============================ LIFECYCLE + CEREMONY ============================
        public override void OnShow()
        {
            AudioManager.Instance?.PlayMenuMusic();
            StartCoroutine(Reveal());
        }

        // §I rich campaign reveal. Tap anywhere during the reveal snaps all tweens to their end-state (and the star
        // ceremony still resolves to the correct earned/unearned end-state — §L-10).
        private IEnumerator Reveal()
        {
            // start everything hidden.
            SetCg(_panelCg, 0f); SetCg(_bannerCg, 0f); SetCg(_titleCg, 0f); SetCg(_ribbonCg, 0f);
            SetCg(_timeCg, 0f); SetCg(_rewLblCg, 0f); SetCg(_coinTileCg, 0f); SetCg(_gemTileCg, 0f);
            SetCg(_nextCg, 0f); SetCg(_replayCg, 0f);
            if (_starL != null) _starL.localScale = Vector3.zero;
            if (_starC != null) _starC.localScale = Vector3.zero;
            if (_starR != null) _starR.localScale = Vector3.zero;
            if (_coinValue != null) _coinValue.text = "0";
            if (_gemValue != null) _gemValue.text = "0";

            // panel scale-in (back-ease overshoot).
            yield return Wait(0.10f);
            yield return Tween(0.40f, k => { float e = Back(k); SetCg(_panelCg, k); if (_panelCg != null) _panelCg.transform.localScale = Vector3.one * Mathf.Lerp(0.9f, 1f, e); });
            // banners drop.
            yield return FadeUp(_bannerCg, 0.30f);
            // title.
            yield return FadeUp(_titleCg, 0.30f);
            // star ribbon.
            yield return Wait(0.05f);
            yield return Tween(0.30f, k => { SetCg(_ribbonCg, k); if (_ribbonCg != null) _ribbonCg.transform.localScale = Vector3.one * Mathf.Lerp(0.8f, 1f, k); });
            // star pops in arc order: L, C (bigger/brighter), R (R unearned in the mock → just settles).
            yield return PopStar(_starL, StarsEarned >= 1);
            yield return PopStar(_starC, StarsEarned >= 2);
            yield return PopStar(_starR, StarsEarned >= 3);
            // clear-time label/icon, then count toward 02:45 with a green settle.
            yield return FadeUp(_timeCg, 0.30f);
            // rewards label + tile pops + count-ups.
            yield return FadeUp(_rewLblCg, 0.25f);
            yield return PopTile(_coinTileCg);
            _coinCounter?.To(RewardCoins, 0.8f);
            yield return PopTile(_gemTileCg);
            _gemCounter?.To(RewardGems, 0.6f);
            // CTAs ignite.
            yield return Tween(0.30f, k => { float e = Back(k); SetCg(_nextCg, k); if (_nextCg != null) _nextCg.transform.localScale = Vector3.one * Mathf.Lerp(0.94f, 1f, e); });
            yield return Tween(0.30f, k => { SetCg(_replayCg, k); if (_replayCg != null) _replayCg.transform.localScale = Vector3.one * Mathf.Lerp(0.94f, 1f, k); });

            SnapAll();
        }

        // Snap the whole reveal to its end-state (called when the coroutine completes OR on tap-skip).
        private void SnapAll()
        {
            if (_revealed) return;
            _revealed = true;
            StopAllCoroutines();
            SetCg(_panelCg, 1f); SetCg(_bannerCg, 1f); SetCg(_titleCg, 1f); SetCg(_ribbonCg, 1f);
            SetCg(_timeCg, 1f); SetCg(_rewLblCg, 1f); SetCg(_coinTileCg, 1f); SetCg(_gemTileCg, 1f);
            SetCg(_nextCg, 1f); SetCg(_replayCg, 1f);
            if (_panelCg != null) _panelCg.transform.localScale = Vector3.one;
            if (_ribbonCg != null) _ribbonCg.transform.localScale = Vector3.one;
            if (_nextCg != null) _nextCg.transform.localScale = Vector3.one;
            if (_replayCg != null) _replayCg.transform.localScale = Vector3.one;
            // star ceremony resolves to the CORRECT earned/unearned end-state regardless of when skipped (§L-10):
            // every star is fully visible at full scale; the earned/unearned LOOK (filled-gold vs dark-hollow) was
            // fixed at build time from StarsEarned, so the snapped end-state is always the correct fill state.
            if (_starL != null) _starL.localScale = Vector3.one;
            if (_starC != null) _starC.localScale = Vector3.one;
            if (_starR != null) _starR.localScale = Vector3.one;
            _coinCounter?.Snap(RewardCoins);
            _gemCounter?.Snap(RewardGems);
            if (_timeValue != null) _timeValue.text = Mmss(ClearTimeSeconds);
        }

        // A full-bleed transparent catcher so a tap anywhere snaps the reveal (added lazily on first Update).
        private void Update()
        {
            if (_revealed) return;
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0) SnapAll();
        }

        // ---- tiny unscaled tween helpers (menus run at timeScale 0) ----
        private static void SetCg(CanvasGroup cg, float a) { if (cg != null) cg.alpha = a; }
        private static float Back(float k) { const float s = 1.70158f; k -= 1f; return k * k * ((s + 1f) * k + s) + 1f; } // back-ease-out
        private IEnumerator Wait(float d) { float t = 0f; while (t < d) { t += Time.unscaledDeltaTime; yield return null; } }
        private IEnumerator Tween(float d, System.Action<float> step) { float t = 0f; while (t < d) { t += Time.unscaledDeltaTime; step(Mathf.Clamp01(t / d)); yield return null; } step(1f); }
        private IEnumerator FadeUp(CanvasGroup cg, float d)
        {
            if (cg == null) yield break;
            var rt = cg.transform as RectTransform; Vector2 baseP = rt != null ? rt.anchoredPosition : Vector2.zero;
            yield return Tween(d, k => { cg.alpha = k; if (rt != null) rt.anchoredPosition = baseP + new Vector2(0, Mathf.Lerp(10f, 0f, k)); });
        }
        private IEnumerator PopStar(RectTransform star, bool earned)
        {
            if (star == null) yield break;
            if (!earned) { // unearned: settle as a dark hollow outline, no pop/sound (§I 1.35).
                yield return Tween(0.30f, k => star.localScale = Vector3.one * k);
                yield break;
            }
            AudioManager.Instance?.Click(); // chime stand-in on earned pop
            yield return Tween(0.30f, k => star.localScale = Vector3.one * Mathf.Lerp(0.2f, 1f, Back(k)));
        }
        private IEnumerator PopTile(CanvasGroup cg)
        {
            if (cg == null) yield break;
            yield return Tween(0.30f, k => { cg.alpha = k; cg.transform.localScale = Vector3.one * Mathf.Lerp(0.85f, 1f, Back(k)); });
            cg.transform.localScale = Vector3.one;
        }

        private static string Mmss(int s) { int m = s / 60, ss = s % 60; return m.ToString("00") + ":" + ss.ToString("00"); }
        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}

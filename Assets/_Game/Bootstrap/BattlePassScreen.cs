// BULWARK — BATTLE PASS (UI Construction Bible · 25). Presentation-only, REMOVABLE.
//
// §12 boundary: this is pure presentation. NO ECS / Unity.Entities, NO gameplay / balance / AI / economy /
// backend. Season data + reward states are read DISPLAY-ONLY from UiStub; claim and unlock-premium are
// display-only affordances (Router.Toast / UiStub.TrySpendGems) standing in for server-authoritative requests
// that never write a real balance. Deleting this file removes the screen 100%.
//
// Forensic build of design/BattlePassDesign.png per Construction-Bible 25 (Sections A–O). The "SEASON OF GLORY"
// seasonal live-ops track on the UiRouter shell: (A/E) an ornate gold "SEASON OF GLORY" banner on hanging
// amethyst drapes with an "Ends in: 28d 14h" timer + Gems/Gold chips (Header, y 0.86→1.00); a left progress
// strip — level "23" medallion, "Battle Pass XP", a 650/1,000 (~65%) XP bar → next "24", a "Missions" pill
// (y 0.79→0.855); a horizontally-scrolling two-row tier track (FREE top / PREMIUM bottom) built on a standard
// UnityEngine.UI.ScrollRect with sticky rail labels, numbered tier columns 19–25 (reward cells in
// claimed/claimable/locked/mystery states) and a tall tier-30 "BATTLE PASS" showcase pinned right (region
// y 0.230→0.770); the tier-progress (XP) bar lives in the progress strip. A bottom CTA bar — "UNLOCK PREMIUM" title/sub, three perk chips,
// and the "UNLOCK PREMIUM 999" gem CTA (y 0.020→0.110). (F) typography + (G) materials honored via
// UiTheme/UiTex/UiFx; (I/J) animations: BG/drape settle, XP fill, tier stagger, claim ✓ pop, premium-unlock
// cascade. (L) exact drawn copy/values; two rows only; two chips only; ambiguous cells rendered as icon +
// legible number. (K) all claims/purchases are display-only requests — no local/ECS mutation of balance.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-25 landscape Battle Pass ("SEASON OF GLORY"). Presentation-only (stub season; server-auth meta).</summary>
    public sealed class BattlePassScreen : UiScreen
    {
        // CanvasScaler 2340×1080, match-height. Spec Section-E y values are bottom-origin anchorY (1.0 = top);
        // any value the kit gives as fy-from-top is converted inline via (1f − fy). px = frac × W / × H.
        private const float W = 2340f, H = 1080f;

        // ---- Screen-local display-only palette (Section B/G hex not in UiTheme). Clearly cosmetic; no economy. ----
        private static readonly Color CBgTop      = Hex("#0a0b0f"); // dusk battlefield top
        private static readonly Color CBgBot      = Hex("#1a1320"); // violet-shifted base
        private static readonly Color CDrape      = Hex("#5a2db0"); // amethyst velvet (drape top)
        private static readonly Color CDrapeHi    = Hex("#7e3fd6"); // drape highlight
        private static readonly Color CViolet     = Hex("#7e3fd6"); // premium violet
        private static readonly Color CVioletHi   = Hex("#b07cf0"); // premium highlight
        private static readonly Color CVioletGlow = Hex("#c79bff"); // premium glow / "?" mystery
        private static readonly Color CSteel      = Hex("#2b3a5a"); // free steel-blue (frame base)
        private static readonly Color CSteelHi    = Hex("#3d6fb0"); // free steel-blue highlight
        private static readonly Color CSteelLbl   = Hex("#9fb6d8"); // FREE rail text
        private static readonly Color CGem        = Hex("#9e6bf0"); // gem crystal icon
        private static readonly Color CCoin       = Hex("#f0c14a"); // gold coin icon
        private static readonly Color CGreen      = Hex("#5fd35a"); // claimed check
        private static readonly Color CTimer      = Hex("#cbb98a"); // timer / sub text
        private static readonly Color CCurrency   = Hex("#f4e9c8"); // currency / reward qty
        private static readonly Color CLevel      = Hex("#fff4d6"); // level / showcase tier number
        private static readonly Color CXpTag      = Hex("#c9b27a"); // "Battle Pass XP"
        private static readonly Color CXpVal      = Hex("#f0e8cf"); // "650 / 1,000 XP"
        private static readonly Color CTierCur    = Hex("#f0d27a"); // current tier number
        private static readonly Color CTierOff    = Hex("#d9c79a"); // other tier numbers

        // Section-E drawn track contents (L→R, tiers 19..25). Transcribed exactly; ambiguous cells = icon + qty.
        private static readonly int[]    Tiers       = { 19, 20, 21, 22, 23, 24, 25 };
        private static readonly string[] FreeQty     = { "10,000", "25", "✓", "50", "x1", "15,000", "50" };
        private static readonly int[]    FreeIcon    = { 1, 0, 2, 0, 3, 1, 0 }; // 0=gem 1=coin 2=claimed 3=chest
        private static readonly string[] PremiumQty  = { "5,000", "100", "✓", "?", "x1", "200", "x1" };
        private static readonly int[]    PremiumIcon = { 1, 0, 2, 4, 3, 1, 3 }; // 4=mystery
        // Per spec H: tier 21 shows ✓ on both rows; tier 22 premium is "?" mystery.

        private CanvasGroup _headerCg, _stripCg, _ctaCg;
        private RectTransform _trackContent;
        private UiWidgets.BarParts _xpBar;
        private float _xpTarget;
        private System.Collections.Generic.List<RectTransform> _columns =
            new System.Collections.Generic.List<RectTransform>(8);
        private System.Collections.Generic.List<Image> _premiumLocks =
            new System.Collections.Generic.List<Image>(8);
        private RectTransform _showcase;
        private GameObject _ctaButton;

        // =====================================================================================================
        protected override void Build()
        {
            BuildBackground();
            BuildHeader();
            BuildProgressStrip();
            BuildTierTrack();
            BuildBottomCtaBar();
        }

        // --- BG_FullBleed (Section G): dusk battlefield, violet-shifted, faint siege silhouettes + vignette. ---
        private void BuildBackground()
        {
            var bg = UiWidgets.Backdrop(Rect, "battlepass");
            bg.color = Hex("#2a2030"); // violet-tint the placeholder key art
            // Violet→obsidian wash so the dusk palette reads even if the placeholder is missing.
            var washRt = UiWidgets.Rect("BG_Wash", Rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var wash = washRt.gameObject.AddComponent<Image>(); wash.raycastTarget = false;
            wash.sprite = UiTex.VGradient(UiTheme.A(CBgTop, 0.86f), UiTheme.A(CBgBot, 0.94f), 64);
            // Faint ruined-siege vista silhouette band along the horizon.
            var vista = UiWidgets.Rect("BG_Vista", Rect, new Vector2(0f, 0.28f), new Vector2(1f, 0.46f), Vector2.zero, Vector2.zero);
            var vi = vista.gameObject.AddComponent<Image>(); vi.raycastTarget = false;
            vi.sprite = UiTex.VGradient(UiTheme.A(Hex("#0a0810"), 0.55f), UiTheme.A(Hex("#0a0810"), 0f), 32);
            // Premium amethyst bloom (upper-right, behind the showcase) + vignette.
            UiWidgets.Glow(Rect, UiTheme.A(CVioletGlow, 0.16f), new Vector2(0.9f, 0.55f), new Vector2(0.9f, 0.55f), Vector2.zero, new Vector2(1200, 1000), 1.7f);
            UiWidgets.Glow(Rect, UiTheme.A(CVioletGlow, 0.10f), new Vector2(0.5f, 0.95f), new Vector2(0.5f, 0.95f), Vector2.zero, new Vector2(1600, 520), 1.6f); // banner up-light
            UiWidgets.Vignette(Rect, 0.6f);
        }

        // =====================================================================================================
        // HEADER (y 0.86→1.00): Back (TL) · "SEASON OF GLORY" on drapes · timer · Gems/Gold chips (TR).
        // =====================================================================================================
        private void BuildHeader()
        {
            var hGo = new GameObject("Header", typeof(RectTransform), typeof(CanvasGroup));
            var header = (RectTransform)hGo.transform; header.SetParent(SafeContent, false);
            header.anchorMin = new Vector2(0f, 0.86f); header.anchorMax = Vector2.one;
            header.offsetMin = Vector2.zero; header.offsetMax = Vector2.zero;
            _headerCg = hGo.GetComponent<CanvasGroup>();

            // Shared chrome stays on SafeContent (does not fade-stagger): Btn_Back (TL) + the two CurrencyChips
            // (TR; Gold rightmost idx 0, Gems idx 1 — only two chips, Section L). The fading header content
            // (drapes/title/timer) parents under `header` so _headerCg actually animates the Section-I reveal.
            UiWidgets.BackButton(SafeContent, () => Router.Pop());
            UiWidgets.CurrencyChip(SafeContent, UiTheme.Gold, UiStub.Gold, 0, out _);        // 98,760 in the mock
            UiWidgets.CurrencyChip(SafeContent, UiTheme.AmethystHi, UiStub.Gems, 1, out _);  // 2,450 in the mock

            // header-local anchorY = (screenY − 0.86) / 0.14. Title screenY 0.945 → 0.607; timer 0.875 → 0.107.
            const float titleAy = 0.607f, timerAy = 0.107f;

            // TitleBanner: hanging amethyst drapes span x 0.30→0.70 from the top of the header; title centered.
            var drapes = UiWidgets.Rect("Drapes", header, new Vector2(0.30f, 1f), new Vector2(0.70f, 1f), new Vector2(0, 6), Vector2.zero);
            drapes.pivot = new Vector2(0.5f, 1f); drapes.sizeDelta = new Vector2(0f, 0.16f * H); // hang ≈173px below the top
            var di = drapes.gameObject.AddComponent<Image>(); di.raycastTarget = false;
            di.sprite = UiTex.VGradient(CDrapeHi, UiTheme.A(CDrape, 0.0f), 64); // velvet fades downward
            di.color = UiTheme.A(Color.white, 0.92f);
            // Gold trim rail under the banner cap + two tassels.
            UiWidgets.Divider(drapes, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -6), 0.40f * W, 4f, UiTheme.GoldHi);
            Tassel(drapes, 0.06f); Tassel(drapes, 0.94f);
            var sway = drapes.gameObject.AddComponent<PulseScale>(); sway.min = 0.997f; sway.max = 1.006f; sway.period = 5f; // idle drape sway

            // Lbl_Title "SEASON OF GLORY" — serif gold-bevel UPPERCASE, ≈65px@1080, gold gradient + bloom.
            UiWidgets.Glow(header, UiTheme.A(UiTheme.GoldHi, 0.28f), new Vector2(0.5f, titleAy), new Vector2(0.5f, titleAy), Vector2.zero, new Vector2(1200, 200), 1.6f);
            var title = UiWidgets.TitleLabel(header, UiStub.SeasonName, 65, new Vector2(0.5f, titleAy), new Vector2(0.5f, titleAy), Vector2.zero, new Vector2(1400, 110), TextAnchor.MiddleCenter, UiTheme.GoldHi, UiTheme.Gold);
            title.fontStyle = FontStyle.Bold;

            // Lbl_Timer "Ends in: 28d 14h" — centered, ≈22px, dim, with an hourglass disc icon.
            var clock = UiWidgets.Rect("Timer_Icon", header, new Vector2(0.5f, timerAy), new Vector2(0.5f, timerAy), new Vector2(-150, 0), new Vector2(26, 26));
            var ci = clock.gameObject.AddComponent<Image>(); ci.raycastTarget = false; ci.sprite = UiTex.Disc(UiTheme.A(CTimer, 0.9f), 32);
            UiWidgets.Label(header, "Ends in: 28d 14h", 22, new Vector2(0.5f, timerAy), new Vector2(0.5f, timerAy), new Vector2(20, 0), new Vector2(520, 36), TextAnchor.MiddleCenter, CTimer);
        }

        private void Tassel(Transform parent, float fx)
        {
            var t = UiWidgets.Rect("Tassel", parent, new Vector2(fx, 0f), new Vector2(fx, 0f), Vector2.zero, new Vector2(10, 34));
            var img = t.gameObject.AddComponent<Image>(); img.raycastTarget = false; img.sprite = UiTex.VGradient(UiTheme.GoldHi, UiTheme.GoldShadow, 32);
        }

        // =====================================================================================================
        // PROGRESS STRIP (band y 0.790→0.855, x 0.030→0.760): level badge "23", XP tag, XP bar 650/1,000,
        // next-level "24" node, "Missions" pill.
        // =====================================================================================================
        private void BuildProgressStrip()
        {
            var sGo = new GameObject("ProgressStrip", typeof(RectTransform), typeof(CanvasGroup));
            var strip = (RectTransform)sGo.transform; strip.SetParent(SafeContent, false);
            strip.anchorMin = new Vector2(0.030f, 0.790f); strip.anchorMax = new Vector2(0.760f, 0.855f);
            strip.offsetMin = Vector2.zero; strip.offsetMax = Vector2.zero;
            _stripCg = sGo.GetComponent<CanvasGroup>();

            // Badge_SeasonLevel "23" — gold shield/medallion Ø≈140px at the left, with glow + stroke.
            UiWidgets.Glow(strip, UiTheme.A(UiTheme.GoldHi, 0.35f), new Vector2(0.04f, 0.5f), new Vector2(0.04f, 0.5f), Vector2.zero, new Vector2(220, 220), 1.7f);
            var badge = UiWidgets.Rect("Badge_SeasonLevel", strip, new Vector2(0.04f, 0.5f), new Vector2(0.04f, 0.5f), Vector2.zero, new Vector2(140, 140));
            var bImg = badge.gameObject.AddComponent<Image>(); bImg.raycastTarget = false; bImg.sprite = UiTex.Disc(UiTheme.Gold, 64);
            var bRing = UiWidgets.Rect("Badge_Ring", badge, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var brImg = bRing.gameObject.AddComponent<Image>(); brImg.raycastTarget = false; brImg.type = Image.Type.Sliced;
            brImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 64, 10);
            var lvl = UiWidgets.Label(badge, UiStub.PassTier.ToString(), 44, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, CLevel);
            lvl.fontStyle = FontStyle.Bold; lvl.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.9f);
            badge.gameObject.AddComponent<PulseScale>().period = 3.2f; // level-badge pop/breathe
            // Supplemental "/ 50" season-length context (display-only; PassTierCount). The drawn medallion stays "23".
            UiWidgets.Label(badge, "/ " + UiStub.PassTierCount, 20, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, -18), new Vector2(140, 30), TextAnchor.MiddleCenter, CTierOff);

            // Lbl_XPTag "Battle Pass XP" — above the bar, left, ≈22px dim gold.
            UiWidgets.Label(strip, "Battle Pass XP", 22, new Vector2(0.11f, 1f), new Vector2(0.11f, 1f), new Vector2(0, 4), new Vector2(420, 34), TextAnchor.LowerLeft, CXpTag);

            // Remaining strip widgets parent under `strip` so _stripCg fades the whole strip (Section I).
            // strip-local anchor = ((screenX − 0.030)/0.730 , (screenY − 0.790)/0.065). Band center y → ay 0.2308.
            const float sBarY = 0.2308f;

            // XPBar (no handle): screen x 0.110→0.560, violet/gold glassy fill at 650/1,000 (~65%).
            float barLeft = 0.110f, barRight = 0.560f;
            float barCx = (barLeft + barRight) * 0.5f, barWFrac = barRight - barLeft;
            float barAx = (barCx - 0.030f) / 0.730f;
            _xpTarget = UiStub.PassXpPerTier > 0 ? Mathf.Clamp01((float)UiStub.PassXp / UiStub.PassXpPerTier) : 0f;
            _xpBar = UiWidgets.GoldBar(strip, new Vector2(barAx, sBarY), new Vector2(barAx, sBarY), Vector2.zero, new Vector2(barWFrac * W, 0.030f * H), 0f, true, true);
            // Violet under-glow so the bar reads premium (Section G "violet/gold glassy fill").
            UiWidgets.Glow(_xpBar.Group, UiTheme.A(CVioletGlow, 0.20f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(barWFrac * W, 90), 1.6f).transform.SetAsFirstSibling();
            UiWidgets.Label(_xpBar.Group, UiStub.PassXp.ToString("N0") + " / " + UiStub.PassXpPerTier.ToString("N0") + " XP", 20, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(barWFrac * W, 36), TextAnchor.MiddleCenter, CXpVal);

            // Lbl_NextLevel "24" with ➜ arrow — small gold node just right of the bar (screen x≈0.575).
            float arrowAx = (0.568f - 0.030f) / 0.730f, nodeAx = (0.585f - 0.030f) / 0.730f;
            UiWidgets.Label(strip, "➜", 28, new Vector2(arrowAx, sBarY), new Vector2(arrowAx, sBarY), Vector2.zero, new Vector2(40, 40), TextAnchor.MiddleCenter, UiTheme.GoldHi);
            var node = UiWidgets.Rect("NextLevel_Node", strip, new Vector2(nodeAx, sBarY), new Vector2(nodeAx, sBarY), Vector2.zero, new Vector2(0.038f * W, 0.038f * W));
            var nImg = node.gameObject.AddComponent<Image>(); nImg.raycastTarget = false; nImg.sprite = UiTex.Disc(UiTheme.A(UiTheme.Charcoal, 0.95f), 64);
            var nRing = UiWidgets.Rect("Node_Ring", node, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var nrImg = nRing.gameObject.AddComponent<Image>(); nrImg.raycastTarget = false; nrImg.type = Image.Type.Sliced; nrImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 6);
            UiWidgets.Label(node, (UiStub.PassTier + 1).ToString(), 26, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, CTierCur);

            // Btn_Missions — gold pill (screen x 0.640→0.760), scroll icon + "Missions".
            float mLeft = 0.640f, mRight = 0.760f, mCx = (mLeft + mRight) * 0.5f;
            float mAx = (mCx - 0.030f) / 0.730f;
            UiWidgets.GemButton(strip, "Missions", new Vector2(mAx, sBarY), new Vector2(mAx, sBarY), Vector2.zero, new Vector2((mRight - mLeft) * W, 0.052f * H), UiTheme.Gold, () => Router.Show<QuestsScreen>(), 26, false, true, TextAnchor.MiddleCenter);
        }

        // =====================================================================================================
        // TIER TRACK (region y 0.230→0.770, x 0.020→0.840): horizontal ScrollRect with sticky rail labels,
        // tier columns 19–25, and a tier-30 showcase pinned right (x 0.850→0.982).
        // =====================================================================================================
        private void BuildTierTrack()
        {
            // --- ScrollRect viewport (masked) over the track region, inside the scrollable x band. ---
            float regTop = 0.770f, regBot = 0.230f, regCx = (regTop + regBot) * 0.5f, regH = (regTop - regBot) * H;
            float vpLeft = 0.020f, vpRight = 0.840f, vpCx = (vpLeft + vpRight) * 0.5f, vpW = (vpRight - vpLeft) * W;

            // Self-viewport ScrollRect: Image+Mask define the masked viewport; the ScrollRect references it +
            // the content. (Standard uGUI; configured fully so it actually scrolls horizontally only.)
            var viewportGo = new GameObject("TrackScroll", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
            var viewport = (RectTransform)viewportGo.transform; viewport.SetParent(SafeContent, false);
            viewport.anchorMin = viewport.anchorMax = new Vector2(vpCx, regCx);
            viewport.sizeDelta = new Vector2(vpW, regH); viewport.anchoredPosition = Vector2.zero;
            var vpImg = viewportGo.GetComponent<Image>(); vpImg.color = new Color(0, 0, 0, 0.001f); // mask needs a graphic; near-invisible
            var mask = viewportGo.GetComponent<Mask>(); mask.showMaskGraphic = false;

            // --- TrackContent: left-anchored, width grows with the tier columns + the showcase. ---
            int n = Tiers.Length;
            float colW = 0.090f * W;     // ≈211px@1080
            float colGap = 0.006f * W;   // ≈14px
            float railW = 0.075f * W;     // sticky rail label column
            float startPad = 0.090f * W;  // gap after the rail before column 19
            float showcaseW = (0.982f - 0.850f) * W;
            float showcasePad = 0.030f * W;
            float contentW = railW + startPad + n * colW + (n - 1) * colGap + showcasePad + showcaseW + 60f;

            var contentGo = new GameObject("TrackContent", typeof(RectTransform));
            _trackContent = (RectTransform)contentGo.transform; _trackContent.SetParent(viewport, false);
            // Left-edge anchored & pivoted (horizontal ScrollRect convention): content slides along x.
            _trackContent.anchorMin = new Vector2(0f, 0f); _trackContent.anchorMax = new Vector2(0f, 1f);
            _trackContent.pivot = new Vector2(0f, 0.5f);
            _trackContent.sizeDelta = new Vector2(contentW, 0f);
            _trackContent.anchoredPosition = Vector2.zero;

            var scroll = viewportGo.GetComponent<ScrollRect>();
            scroll.viewport = viewport; scroll.content = _trackContent;
            scroll.horizontal = true; scroll.vertical = false;
            scroll.movementType = ScrollRect.MovementType.Elastic; scroll.elasticity = 0.1f;
            scroll.inertia = true; scroll.decelerationRate = 0.135f; scroll.scrollSensitivity = 24f;

            // Track frame backdrop (brushed-gold framed channel behind both rows) — non-scrolling, in SafeContent.
            var frameBg = UiWidgets.Rect("Track_Frame", SafeContent, new Vector2(vpCx, regCx), new Vector2(vpCx, regCx), Vector2.zero, new Vector2(vpW + 24, regH + 24));
            var fbImg = frameBg.gameObject.AddComponent<Image>(); fbImg.raycastTarget = false; fbImg.type = Image.Type.Sliced;
            fbImg.sprite = UiTex.Frame(UiTheme.A(UiTheme.GoldHi, 0.7f), UiTheme.A(UiTheme.Gold, 0.6f), UiTheme.A(UiTheme.GoldShadow, 0.6f), 64, 10);
            frameBg.SetAsFirstSibling();

            // Row vertical centers within the content: top (FREE) y≈0.640, bottom (PREMIUM) y≈0.380 of the SCREEN.
            // Map those screen fractions to the content's local 0..1 across the region [regBot..regTop].
            float freeRowFrac = RowFrac(0.640f, regBot, regTop);
            float premRowFrac = RowFrac(0.380f, regBot, regTop);
            float tierNumFrac = RowFrac(0.730f, regBot, regTop);

            // --- TierColumns ---
            for (int i = 0; i < n; i++)
            {
                float colLeftPx = railW + startPad + i * (colW + colGap);
                bool current = Tiers[i] == UiStub.PassTier;

                var col = new GameObject("TierColumn_" + Tiers[i], typeof(RectTransform), typeof(CanvasGroup));
                var crt = (RectTransform)col.transform; crt.SetParent(_trackContent, false);
                crt.anchorMin = new Vector2(0f, 0f); crt.anchorMax = new Vector2(0f, 1f); crt.pivot = new Vector2(0f, 0.5f);
                crt.sizeDelta = new Vector2(colW, 0f); crt.anchoredPosition = new Vector2(colLeftPx, 0f);
                _columns.Add(crt);

                // Tier number node at the top of the column (current = larger + gold glow).
                if (current)
                    UiWidgets.Glow(crt, UiTheme.A(UiTheme.GoldHi, 0.45f), new Vector2(0.5f, tierNumFrac), new Vector2(0.5f, tierNumFrac), Vector2.zero, new Vector2(120, 120), 1.7f);
                var tn = UiWidgets.Rect("Tier_Node", crt, new Vector2(0.5f, tierNumFrac), new Vector2(0.5f, tierNumFrac), Vector2.zero, new Vector2(current ? 78 : 64, current ? 78 : 64));
                var tnImg = tn.gameObject.AddComponent<Image>(); tnImg.raycastTarget = false; tnImg.sprite = UiTex.Disc(UiTheme.A(UiTheme.Charcoal, 0.95f), 64);
                var tnRing = UiWidgets.Rect("Tier_Ring", tn, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var tnrImg = tnRing.gameObject.AddComponent<Image>(); tnrImg.raycastTarget = false; tnrImg.type = Image.Type.Sliced;
                tnrImg.sprite = UiTex.Frame(UiTheme.GoldHi, current ? UiTheme.GoldHi : UiTheme.Gold, UiTheme.GoldShadow, 48, 6);
                UiWidgets.Label(tn, Tiers[i].ToString(), current ? 30 : 26, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, current ? CTierCur : CTierOff);

                // Reward cells: FREE (top row), PREMIUM (bottom row). Cell ≈0.080w × 0.150h.
                bool claimableTier = Tiers[i] <= UiStub.PassTier; // tier ≤ current (23) → earned (Section H)
                BuildCell(crt, freeRowFrac, false, i, claimableTier);
                BuildCell(crt, premRowFrac, true, i, claimableTier);
            }

            // --- RailLabels (sticky-left, NOT inside the scrolling columns; sit over the viewport edge). ---
            BuildRailLabel(false, 0.640f); // "FREE" at the top row center
            BuildRailLabel(true, 0.380f);  // "PREMIUM" + 🔒 at the bottom row center

            // --- Showcase_Tier30 (right end, taller card spanning both rows). Scrolls with content. ---
            BuildShowcase(railW + startPad + n * (colW + colGap) + showcasePad, showcaseW, regH);
        }

        // Map a screen-space anchorY (bottom-origin) onto the track content's local 0..1 across [bot..top].
        private static float RowFrac(float screenY, float regBot, float regTop) => Mathf.Clamp01((screenY - regBot) / (regTop - regBot));

        // One reward cell. free=steel-blue, premium=violet (🔒 padlock when the pass isn't owned).
        private void BuildCell(RectTransform col, float rowFrac, bool premium, int i, bool claimableTier)
        {
            string qty = premium ? PremiumQty[i] : FreeQty[i];
            int iconKind = premium ? PremiumIcon[i] : FreeIcon[i];
            bool mystery = iconKind == 4;
            bool isClaimedCell = qty == "✓";
            bool premiumLocked = premium && !UiStub.PassPremiumOwned;
            bool dimLocked = (!claimableTier) || (premium && !UiStub.PassPremiumOwned && !isClaimedCell);

            var cell = UiWidgets.Card(col, new Vector2(0.5f, rowFrac), new Vector2(0.5f, rowFrac), Vector2.zero,
                new Vector2(0.080f * W, 0.150f * H),
                premium ? UiTheme.A(Hex("#1c1230"), 0.96f) : UiTheme.A(Hex("#10141f"), 0.96f));

            // Beveled frame: violet (premium) vs steel-blue (free).
            var frame = UiWidgets.Rect("Cell_Frame", cell, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var fImg = frame.gameObject.AddComponent<Image>(); fImg.raycastTarget = false; fImg.type = Image.Type.Sliced;
            if (premium) fImg.sprite = UiTex.Frame(CVioletHi, CViolet, Hex("#2e1556"), 48, 7);
            else fImg.sprite = UiTex.Frame(CSteelHi, CSteel, Hex("#16213a"), 48, 7);

            if (premium && !dimLocked)
            {
                var ig = UiWidgets.Glow(cell, UiTheme.A(CVioletGlow, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0.080f * W + 40, 0.150f * H + 40), 1.6f);
                ig.transform.SetAsFirstSibling();
            }

            // Reward icon (recessed well centered).
            if (mystery)
            {
                UiWidgets.Glow(cell, UiTheme.A(CVioletGlow, 0.30f), new Vector2(0.5f, 0.58f), new Vector2(0.5f, 0.58f), Vector2.zero, new Vector2(120, 120), 1.6f);
                var q = UiWidgets.Label(cell, "?", 30, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), Vector2.zero, new Vector2(90, 90), TextAnchor.MiddleCenter, CVioletGlow);
                q.fontStyle = FontStyle.Bold;
            }
            else if (!isClaimedCell)
            {
                var icon = UiWidgets.Rect("Reward_Icon", cell, new Vector2(0.5f, 0.58f), new Vector2(0.5f, 0.58f), Vector2.zero, new Vector2(72, 72));
                var iImg = icon.gameObject.AddComponent<Image>(); iImg.raycastTarget = false;
                switch (iconKind)
                {
                    case 0: iImg.sprite = UiTex.Diamond(CGem, 48); break;                          // gem
                    case 1: iImg.sprite = UiTex.Disc(CCoin, 48); break;                            // gold coin
                    case 3: iImg.sprite = UiTex.VGradient(Hex("#a05b25"), Hex("#7a4a20"), 64); break; // chest
                    default: iImg.sprite = UiTex.Diamond(CGem, 48); break;
                }
                // Reward_Qty (cell bottom, e.g. "10,000").
                UiWidgets.Label(cell, qty, 20, new Vector2(0.5f, 0.16f), new Vector2(0.5f, 0.16f), Vector2.zero, new Vector2(0.080f * W, 32), TextAnchor.MiddleCenter, CCurrency)
                    .gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.85f);
            }

            // Claimed ✓ medallion overlay (tier 21 both rows).
            if (isClaimedCell)
            {
                var medGlow = UiWidgets.Glow(cell, UiTheme.A(CGreen, 0.35f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(120, 120), 1.6f);
                medGlow.transform.SetAsFirstSibling();
                var med = UiWidgets.Rect("Claimed_Check", cell, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(76, 76));
                var mImg = med.gameObject.AddComponent<Image>(); mImg.raycastTarget = false; mImg.sprite = UiTex.Disc(CGreen, 48);
                var chk = UiWidgets.Label(med, "✓", 40, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Color.white);
                chk.fontStyle = FontStyle.Bold;
            }

            // Locked overlay: dim + padlock for not-yet-reached tiers and pass-locked premium cells.
            if (dimLocked)
            {
                var scrim = UiWidgets.Rect("Cell_LockScrim", cell, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var sImg = scrim.gameObject.AddComponent<Image>(); sImg.raycastTarget = false; sImg.color = new Color(0, 0, 0, 0.42f);
                var lk = UiWidgets.Rect("Cell_Lock", cell, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(46, 46));
                var lkImg = lk.gameObject.AddComponent<Image>(); lkImg.raycastTarget = false; lkImg.sprite = UiTex.Diamond(UiTheme.A(premium ? CVioletGlow : CSteelLbl, 0.95f), 48);
                if (premiumLocked) _premiumLocks.Add(lkImg); // animated open on unlock cascade
            }
            else if (claimableTier && !isClaimedCell && !mystery && !(premium && !UiStub.PassPremiumOwned))
            {
                // Claimable: lit frame + gentle pulse; tap → claim flow (display-only request).
                var pulse = cell.gameObject.AddComponent<PulseScale>(); pulse.min = 0.99f; pulse.max = 1.02f; pulse.period = 1.6f;
                int tier = Tiers[i]; bool prem = premium; string label = qty;
                AttachClaim(cell, tier, prem, label);
            }
        }

        // Make a cell tappable as a (display-only) claim request.
        private void AttachClaim(RectTransform cell, int tier, bool premium, string qty)
        {
            var img = cell.gameObject.GetComponent<Image>(); if (img != null) img.raycastTarget = true;
            var btn = cell.gameObject.AddComponent<Button>(); btn.targetGraphic = img;
            var cb = btn.colors; cb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); cb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); ClaimReward(tier, premium, qty); });
        }

        private void BuildRailLabel(bool premium, float screenY)
        {
            // Sticky-left label group anchored to the SAFE area (does not scroll with the track).
            var grp = UiWidgets.Rect(premium ? "Lbl_Premium" : "Lbl_Free", SafeContent, new Vector2(0.022f, screenY), new Vector2(0.022f, screenY), Vector2.zero, new Vector2(0.075f * W, 90));
            grp.pivot = new Vector2(0f, 0.5f);
            // Crest disc.
            var crest = UiWidgets.Rect("Crest", grp, new Vector2(0f, 0.62f), new Vector2(0f, 0.62f), new Vector2(34, 0), new Vector2(48, 48));
            var crImg = crest.gameObject.AddComponent<Image>(); crImg.raycastTarget = false; crImg.sprite = UiTex.Diamond(premium ? CVioletHi : CSteelHi, 48);
            UiWidgets.Label(grp, premium ? "PREMIUM" : "FREE", 24, new Vector2(0f, 0.18f), new Vector2(0f, 0.18f), new Vector2(4, 0), new Vector2(0.075f * W + 60, 40), TextAnchor.MiddleLeft, premium ? CVioletGlow : CSteelLbl)
                .fontStyle = FontStyle.Bold;
            if (premium)
            {
                var lk = UiWidgets.Rect("Premium_Lock", grp, new Vector2(0f, 0.62f), new Vector2(0f, 0.62f), new Vector2(96, 0), new Vector2(34, 34));
                var lkImg = lk.gameObject.AddComponent<Image>(); lkImg.raycastTarget = false; lkImg.sprite = UiTex.Diamond(UiTheme.A(CVioletGlow, 0.95f), 48);
                if (!UiStub.PassPremiumOwned) _premiumLocks.Add(lkImg);
            }
        }

        // Tier-30 "BATTLE PASS" showcase card (violet ornate, legendary reward, pinned right end).
        private void BuildShowcase(float leftPx, float wPx, float hPx)
        {
            var card = new GameObject("Showcase_Tier30", typeof(RectTransform), typeof(CanvasGroup));
            _showcase = (RectTransform)card.transform; _showcase.SetParent(_trackContent, false);
            _showcase.anchorMin = new Vector2(0f, 0.5f); _showcase.anchorMax = new Vector2(0f, 0.5f); _showcase.pivot = new Vector2(0f, 0.5f);
            _showcase.sizeDelta = new Vector2(wPx, hPx); _showcase.anchoredPosition = new Vector2(leftPx, 0f);

            UiWidgets.Glow(_showcase, UiTheme.A(CVioletGlow, 0.40f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(wPx + 90, hPx + 90), 1.6f);
            var field = UiWidgets.OrnateFrame(_showcase, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Hex("#1a1030"), true, 18f);

            // "BATTLE PASS" stacked at the top (serif gold-bevel + violet bloom).
            UiWidgets.TitleLabel(field, "BATTLE", 30, new Vector2(0.5f, 0.92f), new Vector2(0.5f, 0.92f), Vector2.zero, new Vector2(wPx, 44), TextAnchor.MiddleCenter, UiTheme.GoldHi, UiTheme.Gold);
            UiWidgets.TitleLabel(field, "PASS", 30, new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.85f), Vector2.zero, new Vector2(wPx, 44), TextAnchor.MiddleCenter, UiTheme.GoldHi, UiTheme.Gold);

            // Tier "30".
            var tn = UiWidgets.Rect("Showcase_TierNode", field, new Vector2(0.5f, 0.74f), new Vector2(0.5f, 0.74f), Vector2.zero, new Vector2(96, 96));
            var tnImg = tn.gameObject.AddComponent<Image>(); tnImg.raycastTarget = false; tnImg.sprite = UiTex.Disc(UiTheme.A(UiTheme.Charcoal, 0.95f), 64);
            var tnRing = UiWidgets.Rect("Ring", tn, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var tnrImg = tnRing.gameObject.AddComponent<Image>(); tnrImg.raycastTarget = false; tnrImg.type = Image.Type.Sliced; tnrImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 7);
            var t30 = UiWidgets.Label(tn, "30", 48, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, CLevel);
            t30.fontStyle = FontStyle.Bold; t30.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.9f);

            // Legendary armored-king reward render (placeholder lit silhouette) + rising amethyst motes.
            var art = UiWidgets.Rect("Reward_LegendaryArmor", field, new Vector2(0.5f, 0.40f), new Vector2(0.5f, 0.40f), Vector2.zero, new Vector2(wPx * 0.62f, hPx * 0.40f));
            var aImg = art.gameObject.AddComponent<Image>(); aImg.raycastTarget = false; aImg.sprite = UiTex.VGradient(CVioletHi, Hex("#241040"), 64);
            var motes = UiWidgets.Rect("Showcase_Motes", field, new Vector2(0.5f, 0.40f), new Vector2(0.5f, 0.40f), Vector2.zero, new Vector2(wPx * 0.8f, hPx * 0.6f));
            var ef = motes.gameObject.AddComponent<EmberField>(); ef.count = 12; ef.color = CVioletGlow;

            // 🔒 / featured caption + slow specular shimmer.
            UiWidgets.Label(field, UiTheme.Track("LEGENDARY"), 18, new Vector2(0.5f, 0.08f), new Vector2(0.5f, 0.08f), Vector2.zero, new Vector2(wPx, 32), TextAnchor.MiddleCenter, CVioletGlow);
            var shim = field.gameObject.AddComponent<PulseScale>(); shim.min = 0.996f; shim.max = 1.008f; shim.period = 4.5f;

            // Tap → tier-30 detail (display-only).
            var hit = UiWidgets.Rect("Showcase_Hit", _showcase, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var hImg = hit.gameObject.AddComponent<Image>(); hImg.color = new Color(0, 0, 0, 0.001f);
            var btn = hit.gameObject.AddComponent<Button>(); btn.targetGraphic = hImg;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); Router.Toast("Tier 30 — Legendary premium reward (featured)"); });
        }

        // =====================================================================================================
        // BOTTOM CTA BAR (band y 0.020→0.110, x 0.020→0.982): UNLOCK PREMIUM title/sub · 3 perk chips · CTA.
        // =====================================================================================================
        private void BuildBottomCtaBar()
        {
            float top = 0.110f, bot = 0.020f, cy = (top + bot) * 0.5f, h = (top - bot) * H;
            float left = 0.020f, right = 0.982f, cx = (left + right) * 0.5f, w = (right - left) * W;

            var barGo = new GameObject("BottomCTABar", typeof(RectTransform), typeof(CanvasGroup));
            var bar = (RectTransform)barGo.transform; bar.SetParent(SafeContent, false);
            bar.anchorMin = bar.anchorMax = new Vector2(cx, cy); bar.sizeDelta = new Vector2(w, h); bar.anchoredPosition = Vector2.zero;
            _ctaCg = barGo.GetComponent<CanvasGroup>();
            var field = UiWidgets.OrnateFrame(bar, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Hex("#181020"), false, 14f);

            // Lbl_UnlockTitle "UNLOCK PREMIUM" (left, ≈34px serif gold) + sub.
            UiWidgets.TitleLabel(field, "UNLOCK PREMIUM", 34, new Vector2(0.020f, 0.68f), new Vector2(0.020f, 0.68f), Vector2.zero, new Vector2(620, 50), TextAnchor.MiddleLeft, UiTheme.GoldHi, UiTheme.Gold);
            UiWidgets.Label(field, "Get premium rewards and exclusive perks!", 20, new Vector2(0.020f, 0.30f), new Vector2(0.020f, 0.30f), Vector2.zero, new Vector2(640, 36), TextAnchor.MiddleLeft, CTimer);

            // PerkChips: centered cluster x 0.470→0.760, three ≈0.090w chips.
            PerkChip(field, 0.500f, CXpTag,  "+20% XP Boost");          // book/star
            PerkChip(field, 0.615f, CCoin,   "+20% Gold Bonus");        // coin
            PerkChip(field, 0.730f, Hex("#a05b25"), "Exclusive Rewards"); // chest

            // Btn_UnlockPremium / PREMIUM ACTIVE badge (right).
            float bLeft = 0.790f, bRight = 0.972f, bCx = (bLeft + bRight) * 0.5f;
            if (!UiStub.PassPremiumOwned)
            {
                _ctaButton = UiWidgets.GemButton(field, "UNLOCK PREMIUM   " + UiStub.PassPremiumPriceGems, new Vector2(bCx, 0.5f), new Vector2(bCx, 0.5f), Vector2.zero,
                    new Vector2((bRight - bLeft) * W, 0.075f * H), UiTheme.Amethyst, UnlockPremium, 30, true, true, TextAnchor.MiddleCenter).gameObject;
            }
            else
            {
                BuildPremiumActiveBadge(field, bCx);
            }
        }

        private void PerkChip(Transform parent, float cx, Color iconCol, string label)
        {
            var chip = UiWidgets.Card(parent, new Vector2(cx, 0.5f), new Vector2(cx, 0.5f), Vector2.zero, new Vector2(0.090f * W, 0.060f * H), UiTheme.A(Hex("#1c1430"), 0.92f));
            var icon = UiWidgets.Rect("Perk_Icon", chip, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(30, 0), new Vector2(36, 36));
            var iImg = icon.gameObject.AddComponent<Image>(); iImg.raycastTarget = false; iImg.sprite = UiTex.Diamond(iconCol, 48);
            UiWidgets.Label(chip, label, 19, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(28, 0), new Vector2(-20, 40), TextAnchor.MiddleLeft, Hex("#e8e2cf"));
        }

        private void BuildPremiumActiveBadge(Transform parent, float cx)
        {
            var badge = UiWidgets.Card(parent, new Vector2(cx, 0.5f), new Vector2(cx, 0.5f), Vector2.zero, new Vector2(0.182f * W, 0.075f * H), UiTheme.A(CViolet, 0.95f));
            UiWidgets.Glow(badge, UiTheme.A(CVioletGlow, 0.4f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0.182f * W + 60, 0.075f * H + 60), 1.6f);
            UiWidgets.Label(badge, UiTheme.Track("PREMIUM ACTIVE"), 26, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, CLevel)
                .gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.9f);
        }

        // =====================================================================================================
        // Lifecycle + animation (Section I). All on UNSCALED time (menus run at timeScale = 0).
        // =====================================================================================================
        public override void OnShow()
        {
            AudioManager.Instance?.PlayMenuMusic();
            StartCoroutine(EnterTimeline());
        }

        private IEnumerator EnterTimeline()
        {
            // 0.00s header/strip/cta start hidden for the staged reveal.
            if (_headerCg != null) _headerCg.alpha = 0f;
            if (_stripCg != null) _stripCg.alpha = 0f;
            if (_ctaCg != null) _ctaCg.alpha = 0f;
            for (int i = 0; i < _columns.Count; i++)
                if (_columns[i] != null) { var cg = _columns[i].GetComponent<CanvasGroup>(); if (cg != null) cg.alpha = 0f; _columns[i].localScale = Vector3.one * 0.94f; }
            if (_showcase != null) { var sc = _showcase.GetComponent<CanvasGroup>(); if (sc != null) sc.alpha = 0f; _showcase.localScale = Vector3.one * 0.96f; }

            // 0.05s Header fades + title settles.
            yield return FadeCg(_headerCg, 0.05f, 0.25f);
            // 0.12s Progress strip + XP bar fills left→right to 650/1,000.
            StartCoroutine(FadeCg(_stripCg, 0f, 0.25f));
            yield return FillXp(0.45f);
            // 0.20s Tier columns stagger in from the left (0.03s stagger each).
            for (int i = 0; i < _columns.Count; i++)
            {
                StartCoroutine(PopColumn(_columns[i], 0.20f));
                yield return Wait(0.03f);
            }
            // 0.40s Showcase scales up + bloom.
            StartCoroutine(PopColumn(_showcase, 0f));
            // 0.50s Bottom CTA bar slides up + fade.
            yield return FadeCg(_ctaCg, 0.0f, 0.22f);
        }

        private IEnumerator FillXp(float dur)
        {
            if (_xpBar.Fill == null) yield break;
            float t = 0f;
            while (t < dur) { t += Time.unscaledDeltaTime; float k = 1f - (1f - Mathf.Clamp01(t / dur)) * (1f - Mathf.Clamp01(t / dur)); _xpBar.Fill.fillAmount = _xpTarget * k; UiWidgets.UpdateBarTip(_xpBar); yield return null; }
            _xpBar.Fill.fillAmount = _xpTarget; UiWidgets.UpdateBarTip(_xpBar);
        }

        private IEnumerator PopColumn(RectTransform rt, float delay)
        {
            if (delay > 0f) yield return Wait(delay);
            if (rt == null) yield break;
            var cg = rt.GetComponent<CanvasGroup>();
            float t = 0f; const float d = 0.18f; float from = rt.localScale.x;
            while (t < d) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d); if (cg != null) cg.alpha = k; rt.localScale = Vector3.one * Mathf.Lerp(from, 1f, k); yield return null; }
            if (cg != null) cg.alpha = 1f; rt.localScale = Vector3.one;
        }

        private IEnumerator FadeCg(CanvasGroup cg, float delay, float dur)
        {
            if (delay > 0f) yield return Wait(delay);
            if (cg == null) yield break;
            float t = 0f;
            while (t < dur) { t += Time.unscaledDeltaTime; cg.alpha = Mathf.Clamp01(t / dur); yield return null; }
            cg.alpha = 1f;
        }

        private static IEnumerator Wait(float seconds)
        {
            float t = 0f; while (t < seconds) { t += Time.unscaledDeltaTime; yield return null; }
        }

        // =====================================================================================================
        // Display-only actions (Section K / §12): claims + purchases are server-authoritative REQUESTS. The UI
        // never writes a real balance; these surface feedback + (premium) toggle the display-only stub flag.
        // =====================================================================================================
        private void ClaimReward(int tier, bool premium, string qty)
        {
            if (premium && !UiStub.PassPremiumOwned) { Router.Toast("Premium reward — unlock the Battle Pass to claim"); return; }
            // Server-authoritative grant in production; here just acknowledge (no local/ECS balance mutation).
            Router.Toast("Claim tier " + tier + " " + (premium ? "premium" : "free") + " reward (" + qty + ") — sent (server-authoritative)");
        }

        private void UnlockPremium()
        {
            // Display-only spend (real purchase is a server-authoritative flow); on success run the unlock cascade.
            if (UiStub.TrySpendGems(UiStub.PassPremiumPriceGems))
            {
                UiStub.PassPremiumOwned = true;
                if (_ctaButton != null) _ctaButton.SetActive(false);
                StartCoroutine(UnlockCascade());
            }
            else
            {
                UiModals.Insufficient(UiStub.PassPremiumPriceGems - UiStub.Gems);
            }
        }

        // OnUnlockPremium (Section I/J): premium padlocks open left→right (0.04s each) with a violet sparkle.
        private IEnumerator UnlockCascade()
        {
            for (int i = 0; i < _premiumLocks.Count; i++)
            {
                var lk = _premiumLocks[i];
                if (lk != null)
                {
                    lk.color = CVioletGlow;
                    var spark = UiWidgets.Glow(lk.transform, UiTheme.A(CVioletGlow, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(80, 80), 1.6f);
                    var pg = spark.gameObject.AddComponent<PulseGraphic>(); pg.target = spark; pg.min = 0f; pg.max = 0.7f; pg.period = 0.5f;
                    var scrim = lk.transform.parent != null ? lk.transform.parent.Find("Cell_LockScrim") : null;
                    if (scrim != null) scrim.gameObject.SetActive(false);
                    lk.gameObject.SetActive(false);
                }
                yield return Wait(0.04f);
            }
            Router.Toast("Premium unlocked — rewards available");
            Router.Replace<BattlePassScreen>(); // rebuild with premium-owned state (PREMIUM ACTIVE badge + unlocked cells)
        }

        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}

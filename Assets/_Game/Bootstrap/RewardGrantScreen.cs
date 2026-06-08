// BULWARK — REWARD GRANT (UI Construction Bible · 38). Presentation-only, REMOVABLE.
//
// Forensic build of design/RewardGrantDesign.png per Sections A–O: the generic "REWARD!" grant popup — a
// reusable celebratory modal floating over a dim, input-BLOCKING scrim above a darkened dusk-battlefield
// ambience. A centered near-square antique-gold ornate panel (top blue/gold gem finial + four corner
// flourishes) announces the grant ("You have received the following:"), shows 1–4 reward items inside a
// rotating warm gold sunburst (violet gem cluster "+40", silver star-stamped coin stack "+500" — each on a
// dark amount chip with a gold "+N" that counts up on entry), a thin gold divider, then one cobalt COLLECT
// CTA (the brightest, only-exit element). Entry timeline (Section I): scrim/panel pop → burst bloom+spin →
// title flare → staggered item pops + count-up → divider wipe → COLLECT raise+glow. Back key = COLLECT.
// §12 — PRESENTATION ONLY: rewards are server-authoritative DISPLAY-ONLY stub data; the client NEVER mints or
// writes a balance (no UiStub.Grant* call), there is NO scrim-tap dismiss, and COLLECT only acknowledges →
// Router.Pop(). NO ECS / Unity.Entities / gameplay / balance / AI / economy / backend.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-38 generic "REWARD!" grant popup. Presentation-only (server-auth display; §12).</summary>
    public sealed class RewardGrantScreen : UiScreen
    {
        // Reference canvas (CanvasScaler 2340×1080, match height). Layout math is fractions of these (Section E).
        private const float W = 2340f, H = 1080f;

        // ---------------------------------------------------------------------------------------------------
        // DISPLAY-ONLY GRANT CONFIG (Section K). A reward source sets these BEFORE Router.Show<RewardGrantScreen>().
        // Server-authoritative — these strings are inert layout text; the client never mints currency (Section L).
        // Each entry is rendered "+N MATERIAL" (e.g. "+40 GEMS"); GEM/COIN/SILVER/GOLD pick the icon art.
        // ---------------------------------------------------------------------------------------------------
        public static string Title = "REWARD!";
        public static string[] Rewards = { "+40 GEMS", "+500 COINS" };

        // ---- Reward material → icon art family (display-only; carries NO economy meaning, §12). ----
        private enum Material { Gems, Coins, Generic }

        private struct Item { public Material Mat; public int Amount; public string Label; }

        // Reveal/idle handles.
        private CanvasGroup _panelCg;     // panel pop (scale + alpha)
        private RectTransform _burst;     // rotating god-ray sunburst
        private CanvasGroup _titleCg;     // title flare-in
        private RectTransform[] _cards;   // per-item columns (staggered pop)
        private CountUp[] _counts;        // per-item "+N" count-up
        private RectTransform _divider;   // left→right wipe-in
        private CanvasGroup _collectCg;   // COLLECT raise+fade-in
        private RectTransform _collectRt;
        private bool _collected;          // single-fire acknowledgement guard

        protected override void Build()
        {
            var items = ParseRewards();
            _cards  = new RectTransform[items.Length];
            _counts = new CountUp[items.Length];
            _itemAmounts = new int[items.Length];
            for (int i = 0; i < items.Length; i++) _itemAmounts[i] = items[i].Amount; // count-up targets (display-only)

            // ============================================================================================
            // FULL-BLEED SCRIM + DUSK-BATTLEFIELD AMBIENCE (Rect; OUTSIDE safe area → dims the notch too).
            // Section D/E/G: dim battlefield at dusk with deep-blue faction banners + braziers, heavily
            // darkened so the panel pops, under a ~58% black input-BLOCKING scrim.
            // ============================================================================================
            var amb = UiWidgets.Backdrop(Rect, "rewardgrant");
            amb.raycastTarget = false;
            // Cold dusk grade over the ambience (deep blue-black), keeps the battlefield read but subdued.
            var grade = UiWidgets.Rect("BG_Grade", Rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var gradImg = grade.gameObject.AddComponent<Image>(); gradImg.raycastTarget = false;
            gradImg.sprite = UiTex.VGradient(Hex("#161a2a"), Hex("#070810"), 64); gradImg.color = UiTheme.A(Color.white, 0.62f);
            // Deep-blue faction banners at the sides (Section B/G "banners at the sides"), heavily darkened.
            SideBanner(-0.42f); SideBanner(0.42f);
            // Scattered warm braziers (faint focal warmth low on the field).
            UiWidgets.Glow(Rect, UiTheme.A(Hex("#ff9a3c"), 0.10f), new Vector2(0.22f, 0.16f), new Vector2(0.22f, 0.16f), Vector2.zero, new Vector2(360, 300), 1.7f);
            UiWidgets.Glow(Rect, UiTheme.A(Hex("#ff9a3c"), 0.10f), new Vector2(0.78f, 0.16f), new Vector2(0.78f, 0.16f), Vector2.zero, new Vector2(360, 300), 1.7f);
            // Strong vignette → near-black corners (focus on the panel).
            UiWidgets.Vignette(Rect, 0.7f);
            // Scrim: solid black @ ~58% alpha, raycastTarget=TRUE → BLOCKS input beneath. NO button/onClick
            // attached ⇒ a scrim tap does nothing (Section L: no scrim-tap close — must press COLLECT).
            var scrim = UiWidgets.Stretch("Scrim", Rect, new Color(0f, 0f, 0f, 0.58f));
            scrim.raycastTarget = true;

            // ============================================================================================
            // REWARD PANEL (SafeContent; centered, near-square, FIXED px — never stretched on ultrawide, §L).
            // Section E: ≈0.345 W × 0.62 H (~807×670), centered, clamped inside the safe area; 9-slice gold
            // ornate frame (~26 px molding) over an obsidian field; gem finial overhangs the top edge.
            // ============================================================================================
            float panelW = 0.345f * W, panelH = 0.62f * H;     // ≈807 × 670
            var panelGo = new GameObject("RewardPanel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            var panel = (RectTransform)panelGo.transform; panel.SetParent(SafeContent, false);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f); panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(panelW, panelH); panel.anchoredPosition = Vector2.zero;
            _panelCg = panelGo.GetComponent<CanvasGroup>();
            var panelHit = panelGo.GetComponent<Image>(); panelHit.color = new Color(0, 0, 0, 0); panelHit.raycastTarget = true; // catch taps over the panel (Section D)

            // Ornate gold frame → returns the inner obsidian field (parent content inside it). inset = molding ≈26.
            var field = UiWidgets.OrnateFrame(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(panelW, panelH), Hex("#16131e"), false, 26f);

            BuildCornerFlourishes(panel);
            BuildTopGemFinial(panel);

            // ---- Title "REWARD!" (Section F: serif gold bevel, UPPER, ~72, strong bloom). baseline ~0.16 from
            //      panel top → anchorY = 1 − 0.16 = 0.84. ----
            UiWidgets.Glow(field, UiTheme.A(Hex("#ffdca0"), 0.32f), new Vector2(0.5f, 0.84f), new Vector2(0.5f, 0.84f), Vector2.zero, new Vector2(0.85f * panelW, 0.30f * panelH), 1.7f);
            var titleGo = new GameObject("Title_Group", typeof(RectTransform), typeof(CanvasGroup));
            var titleRt = (RectTransform)titleGo.transform; titleRt.SetParent(field, false);
            titleRt.anchorMin = titleRt.anchorMax = new Vector2(0.5f, 0.84f); titleRt.pivot = new Vector2(0.5f, 0.5f);
            titleRt.sizeDelta = new Vector2(0.92f * panelW, 0.20f * panelH); titleRt.anchoredPosition = Vector2.zero;
            _titleCg = titleGo.GetComponent<CanvasGroup>();
            var title = UiWidgets.TitleLabel(titleRt, Title, 72, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Hex("#f2d885"), UiTheme.Gold);
            title.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(Hex("#3a2c0e"), 0.95f); // dark gold stroke (Section F)

            // ---- Subtitle "You have received the following:" (Section F: regular, sentence, ~28, cream).
            //      ~0.27 from top → anchorY = 0.73. ----
            UiWidgets.Label(field, "You have received the following:", 28, new Vector2(0.5f, 0.73f), new Vector2(0.5f, 0.73f),
                Vector2.zero, new Vector2(0.92f * panelW, 0.10f * panelH), TextAnchor.MiddleCenter, Hex("#d9d2c2"));

            // ============================================================================================
            // REWARD BURST + ROW (Section E: burst centered on the row at ~0.50 panel height, radius ~0.42 of
            // panel width; row vertical center ~0.50 from top, items ~64 gap, icon Ø ~0.24 W (≈190), chip
            // ~0.20 W × 56). anchorY for fy 0.50 = 0.50. Burst drawn BEFORE the row (earlier sibling).
            // ============================================================================================
            float rowAnchorY = 0.50f;                 // fy 0.50 from top → anchorY 0.50
            // RewardBurst: warm radiant gold sunburst behind the items, additive look, slow rotation + pulse.
            float burstR = 0.84f * panelW;            // ~0.42 W radius → ~0.84 W diameter (spans behind both icons)
            var burstGo = UiWidgets.Glow(field, UiTheme.A(Hex("#ffdca0"), 0.55f), new Vector2(0.5f, rowAnchorY), new Vector2(0.5f, rowAnchorY), Vector2.zero, new Vector2(burstR, burstR), 1.6f);
            _burst = (RectTransform)burstGo.transform;
            _burst.gameObject.AddComponent<Spin>().degPerSec = 30f;   // ~12 s/rev (Section I idle)
            var bp = _burst.gameObject.AddComponent<PulseGraphic>(); bp.target = burstGo; bp.min = 0.42f; bp.max = 0.62f; bp.period = 2.4f; // ±6% breathe
            // Warm gold ray-spoke layer (#caa04a) counter-rotating for a richer god-ray read (Section G/J).
            var spokesGo = UiWidgets.Glow(field, UiTheme.A(Hex("#caa04a"), 0.34f), new Vector2(0.5f, rowAnchorY), new Vector2(0.5f, rowAnchorY), Vector2.zero, new Vector2(0.66f * panelW, 0.66f * panelW), 1.9f);
            spokesGo.gameObject.AddComponent<Spin>().degPerSec = -18f;
            // Drifting gold dust motes inside the burst (Section J low-rate).
            var moteHost = UiWidgets.Rect("Burst_Motes", field, new Vector2(0.5f, rowAnchorY), new Vector2(0.5f, rowAnchorY), Vector2.zero, new Vector2(0.7f * panelW, 0.34f * panelH));
            var mef = moteHost.gameObject.AddComponent<EmberField>(); mef.count = 12; mef.color = Hex("#ffe9a0");

            // ---- RewardRow: code-built horizontal layout (auto-centers 1–4 items; Section E adaptivity). ----
            float iconD = Mathf.Max(0.18f, 0.24f) * panelW;       // icon Ø ≈190 (clamp ≥0.18 W, Section L)
            float colW  = 0.30f * panelW;                          // each item column ≈242
            float gap   = 64f;                                     // ~64 px gap (Section E)
            // Tighter gaps as count grows (4 items still fit the panel width).
            if (items.Length >= 3) gap = 40f;
            if (items.Length >= 4) { gap = 24f; colW = 0.26f * panelW; iconD = Mathf.Max(0.18f, 0.21f) * panelW; }
            float rowW = colW * items.Length + gap * (items.Length - 1);
            float x = -rowW * 0.5f + colW * 0.5f;                  // left-most column center (row centered on panel)
            for (int i = 0; i < items.Length; i++)
            {
                BuildRewardItem(field, i, items[i], new Vector2(0.5f, rowAnchorY), new Vector2(x, 0f), colW, iconD);
                x += colW + gap;
            }

            // ---- Divider: thin gold rule below the row (Section E: ~0.74 from top → anchorY 0.26, width ~0.72 W).
            //      Pivot at the LEFT edge so the entry wipe (scaleX 0→1) grows left→right while staying centered
            //      on the panel (anchoredPosition compensates the pivot shift). ----
            float divW = 0.72f * panelW;
            _divider = UiWidgets.Rect("Divider", field, new Vector2(0.5f, 0.26f), new Vector2(0.5f, 0.26f), new Vector2(-divW * 0.5f, 0f), new Vector2(divW, 3f));
            _divider.pivot = new Vector2(0f, 0.5f);
            var divImg = _divider.gameObject.AddComponent<Image>(); divImg.raycastTarget = false;
            divImg.sprite = UiTex.HGradient(UiTheme.A(UiTheme.Gold, 0f), UiTheme.A(Hex("#f0d27a"), 0.95f), 64); // gold→fade rule (Section G)

            // ---- Btn_COLLECT: cobalt gold-edged primary, brightest element, ≥72 px, centered (Section E/H).
            //      ~0.87 from top → anchorY 0.13, ≈0.62 W × 72. ----
            BuildCollect(field, new Vector2(0.5f, 0.13f), new Vector2(0.62f * panelW, 84f));

            // NEGATIVE RULES enforced (Section L): no client currency mint (no UiStub.Grant*), no scrim-tap
            // close (scrim has no onClick), panel is fixed-px (never stretched), icons ≥0.18 W, COLLECT-only exit.
        }

        // ====================================================================================================
        // PANEL CHROME
        // ====================================================================================================

        // Blue/gold faceted gem finial overhanging the top edge (Section D/G), with a faint steady bloom.
        private void BuildTopGemFinial(Transform panel)
        {
            var grp = UiWidgets.Rect("TopGemFinial", panel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, 30f), new Vector2(96, 96)); // ~30 px overhang (Section E)
            var bloom = UiWidgets.Glow(grp, UiTheme.A(Hex("#4f8bff"), 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(150, 150), 1.6f);
            bloom.gameObject.AddComponent<PulseGraphic>().target = bloom; // faint steady bloom (Section J)
            var gold = UiWidgets.Rect("Gem_GoldSetting", grp, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(96, 96));
            var gi = gold.gameObject.AddComponent<Image>(); gi.raycastTarget = false; gi.sprite = UiTex.Diamond(UiTheme.GoldHi, 48);
            var gem = UiWidgets.Rect("Gem_Blue", grp, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(60, 60));
            var bi = gem.gameObject.AddComponent<Image>(); bi.raycastTarget = false; bi.sprite = UiTex.Diamond(Hex("#4f8bff"), 48);
        }

        // Four engraved gold corner flourishes (Section D), with a faint corner bloom each (Section J).
        private void BuildCornerFlourishes(Transform panel)
        {
            CornerFlourish(panel, new Vector2(0f, 1f), new Vector2( 30f, -30f), 35f);
            CornerFlourish(panel, new Vector2(1f, 1f), new Vector2(-30f, -30f), -35f);
            CornerFlourish(panel, new Vector2(0f, 0f), new Vector2( 30f,  30f), -35f);
            CornerFlourish(panel, new Vector2(1f, 0f), new Vector2(-30f,  30f), 35f);
        }

        private void CornerFlourish(Transform panel, Vector2 anchor, Vector2 pos, float rot)
        {
            var f = UiWidgets.Finial(panel, anchor, pos, 46f); f.color = UiTheme.GoldHi;
            f.transform.localRotation = Quaternion.Euler(0, 0, rot);
            UiWidgets.Glow(panel, UiTheme.A(Hex("#f0d27a"), 0.18f), anchor, anchor, pos, new Vector2(90, 90), 1.7f);
        }

        // A darkened deep-blue faction banner hanging at a side of the field (full-bleed ambience, Section G).
        private void SideBanner(float anchorX)
        {
            var ax = 0.5f + anchorX;
            var rt = UiWidgets.Rect("FactionBanner", Rect, new Vector2(ax, 0.5f), new Vector2(ax, 0.5f), new Vector2(0, 40f), new Vector2(240, 760));
            var img = rt.gameObject.AddComponent<Image>(); img.raycastTarget = false;
            img.sprite = UiTex.VGradient(UiTheme.IronBanner, Hex("#101830"), 64);
            img.color = UiTheme.A(Color.white, 0.5f); // heavily darkened (Section G)
        }

        // ====================================================================================================
        // REWARD ITEM (icon over a dark amount chip with a count-up "+N")
        // ====================================================================================================
        private void BuildRewardItem(Transform field, int i, Item it, Vector2 anchor, Vector2 pos, float colW, float iconD)
        {
            // Column container centered on the row band (icon stacked over the amount chip — VerticalLayout read).
            var col = UiWidgets.Rect("RewardItem_" + i, field, anchor, anchor, pos, new Vector2(colW, 0.40f * (0.62f * H)));
            _cards[i] = col;

            // Icon (top of the column). Bespoke material art stand-ins (Section G/N).
            var iconAnchor = new Vector2(0.5f, 0.72f); // icon sits above the chip
            var icon = UiWidgets.Rect("Icon", col, iconAnchor, iconAnchor, Vector2.zero, new Vector2(iconD, iconD));
            if (it.Mat == Material.Gems)      BuildGemCluster(icon, iconD);
            else if (it.Mat == Material.Coins) BuildCoinStack(icon, iconD);
            else                               BuildGenericGem(icon, iconD);

            // AmountChip (below the icon): rounded near-black capsule + gold "+N" (Section D/F/G).
            float chipW = 0.20f * (0.345f * W);  // ≈0.20 W ≈161
            var chip = UiWidgets.Rect("AmountChip", col, new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f), Vector2.zero, new Vector2(chipW, 56f));
            var chipBg = chip.gameObject.AddComponent<Image>(); chipBg.raycastTarget = false;
            chipBg.sprite = UiTex.VGradient(Hex("#141821"), Hex("#10131a"), 32); // near-black capsule #10131a
            var chipRim = UiWidgets.Rect("ChipRim", chip, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var cri = chipRim.gameObject.AddComponent<Image>(); cri.raycastTarget = false; cri.sprite = UiTex.Frame(UiTheme.A(UiTheme.GoldHi, 0.85f), UiTheme.A(UiTheme.Gold, 0.8f), UiTheme.A(UiTheme.GoldShadow, 0.7f), 48, 4); cri.type = Image.Type.Sliced; // thin gold rim

            // "+N" gold text on the capsule (~36, gold, dark shadow) — counts up on entry (Section F/I).
            var amt = UiWidgets.Label(chip, "+0", 36, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Hex("#f0d27a"));
            var cu = chip.gameObject.AddComponent<CountUp>(); cu.Bind(amt, v => "+" + v.ToString("N0"), 0f);
            _counts[i] = cu;

            col.localScale = Vector3.zero; // hidden pre-reveal (Section I) — staggered pop scales it up.
        }

        // Violet amethyst crystal cluster with inner glow + sparkle (Section G/J).
        private void BuildGemCluster(Transform icon, float d)
        {
            var glow = UiWidgets.Glow(icon, UiTheme.A(Hex("#9e6bf0"), 0.55f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(d * 1.4f, d * 1.4f), 1.7f);
            glow.transform.SetAsFirstSibling();
            float[] dx = { -0.24f, 0.24f, 0f }; float[] dy = { -0.12f, -0.12f, 0.2f };
            for (int k = 0; k < 3; k++)
            {
                var gem = UiWidgets.Rect("Gem", icon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(dx[k] * d, dy[k] * d), new Vector2(d * 0.55f, d * 0.55f));
                var gi = gem.gameObject.AddComponent<Image>(); gi.raycastTarget = false;
                gi.sprite = UiTex.Diamond(k == 2 ? Hex("#a06ff2") : Hex("#5a2db0"), 48); // core bright, base deep violet
            }
            // White specular sparkle (Section G "white specular" + J intermittent sparkle).
            var spark = UiWidgets.Rect("Sparkle", icon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(d * 0.7f, d * 0.7f));
            var sef = spark.gameObject.AddComponent<EmberField>(); sef.count = 4; sef.color = Color.white;
        }

        // Silver/pewter coin stack with a star stamp + metallic glint (Section G/J).
        private void BuildCoinStack(Transform icon, float d)
        {
            var glow = UiWidgets.Glow(icon, UiTheme.A(Hex("#e6ebf1"), 0.35f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(d * 1.3f, d * 1.3f), 1.8f);
            glow.transform.SetAsFirstSibling();
            for (int k = 0; k < 3; k++)
            {
                var coin = UiWidgets.Rect("Coin", icon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, (k - 1) * d * 0.22f), new Vector2(d * 0.92f, d * 0.34f));
                var ci = coin.gameObject.AddComponent<Image>(); ci.raycastTarget = false;
                ci.sprite = UiTex.Disc(Hex("#aab2bb"), 48); // silver mid #aab2bb
            }
            // Star stamp on the top coin face (#e6ebf1 hi).
            var star = UiWidgets.Rect("Star", icon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, d * 0.22f), new Vector2(d * 0.34f, d * 0.34f));
            var si = star.gameObject.AddComponent<Image>(); si.raycastTarget = false; si.sprite = UiTex.Diamond(Hex("#e6ebf1"), 32);
            // Metallic glint sweep (tiny shine sparkles).
            var spark = UiWidgets.Rect("Glint", icon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(d * 0.7f, d * 0.7f));
            var sef = spark.gameObject.AddComponent<EmberField>(); sef.count = 3; sef.color = Hex("#eef2f7");
        }

        // Fallback gold gem for an unrecognized material (display-only; keeps within DNA, Section L).
        private void BuildGenericGem(Transform icon, float d)
        {
            var glow = UiWidgets.Glow(icon, UiTheme.A(Hex("#f0d27a"), 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(d * 1.3f, d * 1.3f), 1.7f);
            glow.transform.SetAsFirstSibling();
            var gem = UiWidgets.Rect("Gem", icon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(d * 0.8f, d * 0.8f));
            var gi = gem.gameObject.AddComponent<Image>(); gi.raycastTarget = false; gi.sprite = UiTex.Diamond(UiTheme.GoldHi, 48);
        }

        // ====================================================================================================
        // COLLECT (cobalt gold-edged primary — the ONLY exit → Router.Pop(); acknowledge only, §L)
        // ====================================================================================================
        private void BuildCollect(Transform field, Vector2 anchor, Vector2 size)
        {
            // Outer cobalt glow (steady; the brightest interactive element), pulses ±10% (Section H/I).
            var glow = UiWidgets.Glow(field, UiTheme.A(Hex("#4f8bff"), 0.5f), anchor, anchor, Vector2.zero, size * 1.35f);
            var pg = glow.gameObject.AddComponent<PulseGraphic>(); pg.target = glow; pg.min = 0.34f; pg.max = 0.55f; pg.period = 1.6f;

            var rt = UiWidgets.Rect("Btn_COLLECT", field, anchor, anchor, Vector2.zero, size);
            _collectRt = rt;
            // Royal/cobalt body #2b56c8→#4f8bff gloss with a bright top bevel (Section G).
            var fill = rt.gameObject.AddComponent<Image>(); fill.sprite = UiTex.VGradient(Hex("#4f8bff"), Hex("#2b56c8"), 32);
            var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = fill;
            var cb = btn.colors; cb.normalColor = Color.white; cb.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f); cb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); cb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f); cb.fadeDuration = 0.08f; btn.colors = cb;
            btn.onClick.AddListener(Collect); // acknowledge → dismiss (server-auth; client mints nothing, §L)
            // Gold ornate edge (beveled 9-slice rim).
            var rim = UiWidgets.Rect("Collect_Rim", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var ri = rim.gameObject.AddComponent<Image>(); ri.raycastTarget = false; ri.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 8); ri.type = Image.Type.Sliced;
            // COLLECT label: white on blue, dark outline, +4% track, ~32 (Section F).
            var lbl = UiWidgets.Label(rt, UiTheme.Track("COLLECT"), 32, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Color.white);
            lbl.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(Hex("#0a1840"), 0.9f);

            // Hidden + non-interactive until the entry timeline raises it in (Section I 560 ms beat).
            _collectCg = rt.gameObject.AddComponent<CanvasGroup>();
            _collectCg.alpha = 0f; _collectCg.interactable = false; _collectCg.blocksRaycasts = false;
        }

        // ====================================================================================================
        // LIFECYCLE / ENTRY TIMELINE (Section I)
        // ====================================================================================================
        public override void OnShow()
        {
            AudioManager.Instance?.PlayMenuMusic();
            StartCoroutine(Reveal());
        }

        // Panel pop → burst bloom → title flare → staggered item pops + count-up → divider wipe → COLLECT raise.
        private IEnumerator Reveal()
        {
            // 70 ms: RewardPanel scale 0.90→1.0 + α 0→1 over 220 ms (ease-out-back, slight overshoot).
            if (_panelCg != null) { _panelCg.alpha = 0f; _panelCg.transform.localScale = Vector3.one * 0.90f; }
            if (_titleCg != null) _titleCg.alpha = 0f;
            yield return Wait(0.07f);
            yield return Tween(0.22f, k =>
            {
                if (_panelCg == null) return;
                _panelCg.alpha = k;
                float s = Mathf.Lerp(0.90f, 1f, EaseOutBack(k));
                _panelCg.transform.localScale = new Vector3(s, s, 1f);
            });
            if (_panelCg != null) { _panelCg.alpha = 1f; _panelCg.transform.localScale = Vector3.one; }

            // 160 ms: RewardBurst scale 0.7→1.0 + α flares in (rotation already running via Spin).
            if (_burst != null) _burst.localScale = Vector3.one * 0.7f;
            yield return Tween(0.26f, k => { if (_burst != null) _burst.localScale = Vector3.one * Mathf.Lerp(0.7f, 1f, k); });
            if (_burst != null) _burst.localScale = Vector3.one;

            // 220 ms: Title "REWARD!" pop (scale 0.85→1.0 + bloom flare).
            if (_titleCg != null)
            {
                _titleCg.transform.localScale = Vector3.one * 0.85f;
                yield return Tween(0.20f, k =>
                {
                    if (_titleCg == null) return;
                    _titleCg.alpha = k;
                    float s = Mathf.Lerp(0.85f, 1f, EaseOutBack(k));
                    _titleCg.transform.localScale = new Vector3(s, s, 1f);
                });
                _titleCg.alpha = 1f; _titleCg.transform.localScale = Vector3.one;
            }

            // 300 ms: each RewardItem staggers in (80 ms apart): scale 0.6→1.0 (ease-out-back) + count-up.
            for (int i = 0; i < _cards.Length; i++)
            {
                yield return PopItem(i);
                yield return Wait(0.08f);
            }

            // 520 ms: Divider wipes in left→right (scaleX 0→1, 180 ms) — pivot is already at the left edge.
            if (_divider != null)
            {
                _divider.localScale = new Vector3(0f, 1f, 1f);
                yield return Tween(0.18f, k => { if (_divider != null) _divider.localScale = new Vector3(k, 1f, 1f); });
                if (_divider != null) _divider.localScale = Vector3.one;
            }

            // 560 ms: COLLECT fades/raises in (+16 px → 0, 180 ms) and begins its glow pulse.
            EnableCollect();
        }

        // Item reveal: scale 0→1 ease-out-back, then count "+N" up over ~0.4 s (Section I).
        private IEnumerator PopItem(int i)
        {
            var card = _cards[i];
            if (card == null) yield break;
            yield return Tween(0.18f, k =>
            {
                float s = Mathf.Lerp(0f, 1f, EaseOutBack(k));
                card.localScale = new Vector3(s, s, 1f);
            });
            card.localScale = Vector3.one;
            _counts[i]?.To(_itemAmounts[i], 0.4f); // count-up on arrival (display-only target)
        }

        private void EnableCollect()
        {
            if (_collectCg == null) return;
            _collectCg.interactable = true; _collectCg.blocksRaycasts = true;
            if (_collectRt != null) StartCoroutine(RaiseCollect());
        }

        // COLLECT raise (+16 px → 0) + fade-in (Section I 560 ms beat).
        private IEnumerator RaiseCollect()
        {
            var basePos = _collectRt.anchoredPosition;
            _collectRt.anchoredPosition = basePos + new Vector2(0, -16f);
            yield return Tween(0.18f, k =>
            {
                if (_collectCg != null) _collectCg.alpha = k;
                if (_collectRt != null) _collectRt.anchoredPosition = basePos + new Vector2(0, Mathf.Lerp(-16f, 0f, k));
            });
            if (_collectCg != null) _collectCg.alpha = 1f;
            if (_collectRt != null) _collectRt.anchoredPosition = basePos;
        }

        // ====================================================================================================
        // INPUT / COLLECT (acknowledge only — never mints currency; §L)
        // ====================================================================================================
        // Back key (Escape) is treated as COLLECT — never silently discard a reward (Section K OnBackKey).
        private void Update()
        {
            if (!_collected && Input.GetKeyDown(KeyCode.Escape)) Collect();
        }

        // Acknowledge the grant and dismiss. The grant is server-authoritative (already applied / applied on
        // ack server-side) — this overlay records nothing and writes NO balance (Section L). → Router.Pop().
        private void Collect()
        {
            if (_collected) return;
            _collected = true;
            AudioManager.Instance?.Click();   // confirm SFX (builders already click; this guards the Escape path)
            StartCoroutine(Dismiss());
        }

        // COLLECT press feedback + panel scale 1→0.96 + α 1→0 (ease-in), then pop the overlay (Section I OnCollect).
        private IEnumerator Dismiss()
        {
            if (_collectRt != null) yield return Tween(0.08f, k => { if (_collectRt != null) { float s = Mathf.Lerp(1f, 0.96f, k); _collectRt.localScale = new Vector3(s, s, 1f); } });
            yield return Tween(0.16f, k =>
            {
                if (_panelCg == null) return;
                _panelCg.alpha = 1f - k;
                float s = Mathf.Lerp(1f, 0.96f, k);
                _panelCg.transform.localScale = new Vector3(s, s, 1f);
            });
            Router.Pop(); // reveal the underlying screen; the server-auth wallet refresh happens there, not here
        }

        // ====================================================================================================
        // HELPERS
        // ====================================================================================================
        // Parse the display-only "+N MATERIAL" config into typed items (defensive; falls back to a sensible item).
        private Item[] ParseRewards()
        {
            var src = (Rewards != null && Rewards.Length > 0) ? Rewards : new[] { "+40 GEMS", "+500 COINS" };
            int n = Mathf.Clamp(src.Length, 1, 4); // Section L: do NOT exceed 4 inline items
            var outp = new Item[n];
            for (int i = 0; i < n; i++)
            {
                string raw = src[i] ?? "";
                string upper = raw.ToUpperInvariant();
                Material mat = upper.Contains("GEM") ? Material.Gems
                            : (upper.Contains("COIN") || upper.Contains("SILVER") || upper.Contains("GOLD")) ? Material.Coins
                            : Material.Generic;
                int amount = ParseDigits(raw);
                outp[i] = new Item { Mat = mat, Amount = amount, Label = raw };
            }
            return outp;
        }

        private int[] _itemAmounts; // per-item count-up targets (display-only; mirrors parsed Item.Amount)

        private static int ParseDigits(string s)
        {
            long v = 0; bool any = false;
            for (int i = 0; i < s.Length; i++) { char c = s[i]; if (c >= '0' && c <= '9') { any = true; v = v * 10 + (c - '0'); if (v > 999999) { v = 999999; break; } } }
            return any ? (int)v : 0;
        }

        // Small unscaled tween driver (menus run at Time.timeScale = 0).
        private IEnumerator Tween(float duration, System.Action<float> step)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                step(Mathf.Clamp01(t / duration));
                yield return null;
            }
            step(1f);
        }

        private IEnumerator Wait(float s) { float t = 0f; while (t < s) { t += Time.unscaledDeltaTime; yield return null; } }

        private static float EaseOutBack(float x) { const float c1 = 1.70158f, c3 = c1 + 1f; float p = x - 1f; return 1f + c3 * p * p * p + c1 * p * p; }

        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}

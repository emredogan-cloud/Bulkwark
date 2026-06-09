// BULWARK — ENDLESS RESULT (UI Construction Bible · 15). Presentation-only, REMOVABLE.
//
// Forensic build of design/EndlessResultDesign.png per Sections A–O: a POST-MATCH result for an Endless/survival
// run — no win/loss verdict, only "how far did you get?". Ashen-Horde ember/oxblood on near-black: a spiked
// black-iron panel under a jagged crown-spike crest, oxblood war-banners draped on both top corners, the fixed
// title "THE HORDE PREVAILS", a giant ember "Waves Survived" hero number (the hottest/largest glyph), a cream
// Score with a conditional gold "NEW BEST!" ribbon (crowns at its ends), two gold-framed reward tiles (silver
// coins + blue/gold winged Pass-XP badge), and RETRY (oxblood primary) + Main Menu (dark-stone secondary).
// All numbers/best/rewards are DISPLAY-ONLY local stubs — the grant is server-authoritative; this UI never writes
// any balance/best/XP. NO ECS/gameplay/backend (§12); CTAs go through the existing MatchPresentation seam.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-15 Endless (survival) result. Presentation-only (run-state read-only; §12).</summary>
    public sealed class EndlessResultScreen : UiScreen
    {
        // ---- Display-only stub payload (mock numbers; server-authoritative grant — UI never writes them, §12). ----
        // In the wired flow these arrive via OnShow(payload) from ECS run-state + server best/reward.
        public static int PendingWaves    = 34;
        public static int PendingScore    = 145200;
        public static bool PendingNewBest = true;     // NEW BEST ribbon shown iff true (else omitted, gap closed)
        public static int PendingCoins    = 12450;
        public static int PendingPassXP   = 2350;

        // Panel geometry (E): 0.50·W ≈ 1170 × 0.90·H ≈ 972, centred. Aspect ≈ 1.20:1.
        private const float PW = 1170f, PH = 972f;

        private CanvasGroup _panelCg;
        private CountUp _wavesCount, _scoreCount, _coinsCount, _xpCount;
        private Image _wavesGlow, _retryGlow;
        private RectTransform _ribbon;
        private bool _resolved;

        protected override void Build()
        {
            // =================== FULL-BLEED BACKGROUND + FX (Rect; ignores safe area) ===================
            // BG_HordeField: dark ember-lit ruined battlefield, pushed far back.
            var bg = UiLayers.Plate(Rect, "endlessresult", 0.30f);
            bg.gameObject.AddComponent<KenBurns>().from = 1.05f;
            // Faint red rim-glows (horde fire) low + sides.
            UiWidgets.Glow(Rect, UiTheme.A(Hex("#7a1c0a"), 0.32f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 120), new Vector2(1700, 760), 1.7f);
            UiWidgets.Glow(Rect, UiTheme.A(Hex("#5a1208"), 0.22f), new Vector2(0.12f, 0.18f), new Vector2(0.12f, 0.18f), Vector2.zero, new Vector2(900, 900), 1.8f);
            UiWidgets.Glow(Rect, UiTheme.A(Hex("#5a1208"), 0.22f), new Vector2(0.88f, 0.18f), new Vector2(0.88f, 0.18f), Vector2.zero, new Vector2(900, 900), 1.8f);
            // BG_Vignette: heavy → near-black corners.
            UiWidgets.Vignette(Rect, 0.7f);
            // FX_EmberDrift: slow rising red/orange embers across the whole field (behind scrim).
            var emberHost = UiWidgets.Rect("FX_EmberDrift", Rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var ef = emberHost.gameObject.AddComponent<EmberField>(); ef.count = 34; ef.color = Hex("#ff5a1e");
            // BG_DarkenScrim: black ≈0.45.
            UiWidgets.Stretch("BG_DarkenScrim", Rect, new Color(0, 0, 0, 0.45f));

            // =================== RESULT PANEL (SafeContent; centred card) ===================
            var panelGo = new GameObject("ResultPanel", typeof(RectTransform), typeof(CanvasGroup));
            var panel = (RectTransform)panelGo.transform; panel.SetParent(SafeContent, false);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(PW, PH); panel.anchoredPosition = Vector2.zero;
            _panelCg = panelGo.GetComponent<CanvasGroup>();

            // Panel_Frame (spiked black-iron 9-slice) + Panel_Interior (near-black charcoal). OrnateFrame returns
            // the inner FIELD — all interior rows parent inside it. Iron, not gold: recolor frame after build.
            var field = UiWidgets.OrnateFrame(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(PW, PH), Hex("#0c0a08"), false, 26f);
            IronizeFrame(panel);                 // recolor gold molding/finials → spiked dark iron w/ ember glints
            // Faint red inner glow (bottom/center) — the horde-fire ambiance (G).
            UiWidgets.Glow(field, UiTheme.A(Hex("#3a1410"), 0.5f), new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f), Vector2.zero, new Vector2(900, 520), 1.9f);

            // Banner_Left / Banner_Right (oxblood war-banners draped over the top corners, mirrored).
            Banner(panel, true);
            Banner(panel, false);

            // Crest_Spike (jagged crown-spike horde sigil; breaks the top frame, top z).
            BuildCrest(panel);

            // =================== INTERIOR ROWS (fy = fraction of PANEL height from TOP → anchorY = 1 − fy) ===================
            // Title_Horde "THE HORDE PREVAILS" — oxblood/ember serif, low red glow. (fy 0.14)
            UiWidgets.Glow(field, UiTheme.A(Hex("#d8452b"), 0.28f), new Vector2(0.5f, 0.86f), new Vector2(0.5f, 0.86f), Vector2.zero, new Vector2(0.78f * PW, 0.16f * PH), 1.9f);
            UiWidgets.TitleLabel(field, "THE HORDE PREVAILS", 56, new Vector2(0.5f, 0.86f), new Vector2(0.5f, 0.86f),
                Vector2.zero, new Vector2(0.92f * PW, 0.10f * PH), TextAnchor.MiddleCenter, Hex("#f08050"), Hex("#7a1f1a"));

            // Waves_Group: label (fy 0.25) + giant ember value (fy 0.34) — the hero glyph.
            UiWidgets.Label(field, "Waves Survived:", 32, new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f),
                Vector2.zero, new Vector2(0.8f * PW, 0.05f * PH), TextAnchor.MiddleCenter, Hex("#d8cfbc"));
            // Strong ember bloom behind the number (largest, hottest, most-bloomed element).
            _wavesGlow = UiWidgets.Glow(field, UiTheme.A(Hex("#ff5a1e"), 0.55f), new Vector2(0.5f, 0.66f), new Vector2(0.5f, 0.66f),
                Vector2.zero, new Vector2(0.40f * PW, 0.30f * PH), 1.7f);
            var wavesVal = UiWidgets.Label(field, "0", 162, new Vector2(0.5f, 0.66f), new Vector2(0.5f, 0.66f),
                Vector2.zero, new Vector2(0.6f * PW, 0.18f * PH), TextAnchor.MiddleCenter, Hex("#db2b01"));
            var wg = wavesVal.gameObject.AddComponent<UiGradientText>(); wg.top = Hex("#ff6a2a"); wg.bottom = Hex("#8a1c0a");
            var wsh = wavesVal.GetComponent<Shadow>(); if (wsh != null) wsh.effectDistance = new Vector2(0, -4); // deepen the hero drop shadow (Label added it)
            wavesVal.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(Hex("#2a0a04"), 0.95f);
            wavesVal.gameObject.AddComponent<PulseScale>().period = 2.4f; // faint heat-shimmer on the glyph
            _wavesCount = panelGo.AddComponent<CountUp>(); _wavesCount.Bind(wavesVal, v => v.ToString("N0"), 0f);

            // Score_Group: label (fy 0.44) + cream value (fy 0.50).
            UiWidgets.Label(field, "Score:", 30, new Vector2(0.5f, 0.56f), new Vector2(0.5f, 0.56f),
                Vector2.zero, new Vector2(0.6f * PW, 0.04f * PH), TextAnchor.MiddleCenter, Hex("#d8cfbc"));
            var scoreVal = UiWidgets.Label(field, "0", 56, new Vector2(0.5f, 0.50f), new Vector2(0.5f, 0.50f),
                Vector2.zero, new Vector2(0.7f * PW, 0.06f * PH), TextAnchor.MiddleCenter, Hex("#f3ead2"));
            _scoreCount = panelGo.AddComponent<CountUp>(); _scoreCount.Bind(scoreVal, v => v.ToString("N0"), 0f);

            // NewBest_Ribbon (fy 0.57) — gold banner + flanking crowns; built only on a record (negative rule 3).
            if (PendingNewBest) BuildNewBest(field);

            // Rewards_Label (fy 0.65) + two gold-framed dark tiles (fy 0.77).
            UiWidgets.Label(field, UiTheme.Track("Rewards"), 26, new Vector2(0.5f, 0.35f), new Vector2(0.5f, 0.35f),
                Vector2.zero, new Vector2(0.6f * PW, 0.04f * PH), TextAnchor.MiddleCenter, Hex("#c8a55a"));
            // Row width ≈0.66·PW; each tile ≈0.30·PW × 0.14·PH; gap ≈0.04·PW.
            float tileW = 0.30f * PW, tileH = 0.14f * PH, half = 0.5f * (tileW + 0.04f * PW);
            _coinsCount = BuildCoinSlot(field, new Vector2(0.5f, 0.23f), new Vector2(-half, 0), tileW, tileH);
            _xpCount    = BuildXpSlot(field, new Vector2(0.5f, 0.23f), new Vector2(half, 0), tileW, tileH);

            // Buttons_Group (fy 0.91): RETRY (left, oxblood primary) + Main Menu (right, dark-stone secondary).
            // Row width ≈0.84·PW; each ≈0.42·PW; gap ≈0.02·PW; height ≈0.085·PH.
            float btnW = 0.42f * PW, btnH = 0.085f * PH, bhalf = 0.5f * (btnW + 0.02f * PW);
            BuildRetry(field, new Vector2(0.5f, 0.09f), new Vector2(-bhalf, 0), new Vector2(btnW, btnH));
            BuildMainMenu(field, new Vector2(0.5f, 0.09f), new Vector2(bhalf, 0), new Vector2(btnW, btnH));

            // Top-left back → Main Menu alias (B/Esc → hub). Parent under SafeContent.
            UiWidgets.BackButton(SafeContent, () => Router.Pop());
        }

        // ---- Spiked black-iron frame: recolor the OrnateFrame's gold molding + finials to dark iron + ember glints.
        private void IronizeFrame(Transform panel)
        {
            var frame = panel.Find("OrnateFrame");
            if (frame == null) return;
            // OrnateFrame puts the molding Image on the group GO; tint it to cast-iron.
            var fimg = frame.GetComponent<Image>(); if (fimg != null) fimg.color = Hex("#2a241c");
            // (finials are OFF for this frame; the jagged crest is built separately as the horde sigil.)
            // Ember-red glints catching the frame edge.
            UiWidgets.Glow(frame, UiTheme.A(Hex("#b8401c"), 0.5f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -10), new Vector2(420, 90), 1.6f);
        }

        // ---- Oxblood war-banner draped over a top corner (mirrored). Torn-hem ragged cloth + dark iron trim. ----
        private void Banner(Transform panel, bool left)
        {
            float ax = left ? 0f : 1f, dir = left ? 1f : -1f;
            float bw = 0.11f * PW, bh = 0.50f * PH;
            var b = UiWidgets.Rect(left ? "Banner_Left" : "Banner_Right", panel,
                new Vector2(ax, 1f), new Vector2(ax, 1f), new Vector2(dir * (bw * 0.34f), 34f), new Vector2(bw, bh));
            var img = b.gameObject.AddComponent<Image>(); img.raycastTarget = false;
            img.sprite = UiTex.VGradient(Hex("#8a241c"), Hex("#5a1410"), 64);  // deep red cloth, darker at the hem
            // Dark iron trim down the inner edge.
            var trim = UiWidgets.Rect("Trim", b, new Vector2(left ? 1f : 0f, 0f), new Vector2(left ? 1f : 0f, 1f), Vector2.zero, new Vector2(8f, 0f));
            var ti = trim.gameObject.AddComponent<Image>(); ti.raycastTarget = false; ti.color = Hex("#1a1814");
            // Ashen sigil (jagged emblem) in darker red/black.
            var sig = UiWidgets.Rect("Sigil", b, new Vector2(0.5f, 0.74f), new Vector2(0.5f, 0.74f), Vector2.zero, new Vector2(bw * 0.5f, bw * 0.5f));
            var si = sig.gameObject.AddComponent<Image>(); si.raycastTarget = false; si.sprite = UiTex.Diamond(Hex("#3a0a06"), 48);
            sig.localRotation = Quaternion.Euler(0, 0, 45f);
            // Torn/ragged hem (small dark triangles via rotated diamonds along the bottom).
            for (int i = 0; i < 3; i++)
            {
                var t = UiWidgets.Rect("Hem", b, new Vector2((i + 0.5f) / 3f, 0f), new Vector2((i + 0.5f) / 3f, 0f), new Vector2(0, -10f), new Vector2(bw * 0.42f, bw * 0.42f));
                var tg = t.gameObject.AddComponent<Image>(); tg.raycastTarget = false; tg.sprite = UiTex.Diamond(Hex("#5a1410"), 48);
                t.localRotation = Quaternion.Euler(0, 0, 45f);
            }
        }

        // ---- Crest_Spike: jagged crown-of-blades horde sigil at top-center, breaking the frame (top z). ----
        private void BuildCrest(Transform panel)
        {
            var crest = UiWidgets.Rect("Crest_Spike", panel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, 18f), new Vector2(360, 150));
            // Five iron blades fanning up (center tallest) — a tribal crown of blades, not a smooth gold crown.
            float[] xs = { -120, -62, 0, 62, 120 };
            float[] hs = { 70, 104, 140, 104, 70 };
            float[] ang = { 22, 11, 0, -11, -22 };
            for (int i = 0; i < xs.Length; i++)
            {
                var blade = UiWidgets.Rect("Spike", crest, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(xs[i], 6f), new Vector2(26f, hs[i]));
                var bi = blade.gameObject.AddComponent<Image>(); bi.raycastTarget = false;
                bi.sprite = UiTex.VGradient(Hex("#5a5048"), Hex("#1a1814"), 64);  // worn-metal highlight → dark iron
                blade.localRotation = Quaternion.Euler(0, 0, ang[i]);
                // Ember-red glint catching the blade tip.
                var tip = UiWidgets.Rect("Glint", blade, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -6f), new Vector2(22f, 22f));
                var tg = tip.gameObject.AddComponent<Image>(); tg.raycastTarget = false; tg.sprite = UiTex.Diamond(Hex("#b8401c"), 32);
            }
            UiWidgets.Glow(crest, UiTheme.A(Hex("#b8401c"), 0.4f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(320, 220), 1.7f);
            crest.SetAsLastSibling();
        }

        // ---- NEW BEST! ribbon (conditional): gold banner + flanking gold crowns, persistent gentle shimmer. ----
        private void BuildNewBest(Transform field)
        {
            float rw = 0.46f * PW, rh = 0.06f * PH;
            _ribbon = UiWidgets.Rect("NewBest_Ribbon", field, new Vector2(0.5f, 0.43f), new Vector2(0.5f, 0.43f), Vector2.zero, new Vector2(rw, rh + 24f));
            UiWidgets.Glow(_ribbon, UiTheme.A(Hex("#f0d27a"), 0.4f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(rw * 1.1f, rh * 2.6f), 1.7f);
            // Gold ribbon banner (behind the text).
            var banner = UiWidgets.Rect("Ribbon_Banner", _ribbon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(rw, rh));
            var bi = banner.gameObject.AddComponent<Image>(); bi.raycastTarget = false; bi.sprite = UiTex.VGradient(Hex("#f0d27a"), Hex("#caa04a"), 64);
            // NEW BEST! text on the ribbon.
            var txt = UiWidgets.Label(banner, UiTheme.Track("NEW BEST!"), 32, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Hex("#fff0c0"));
            txt.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(Hex("#3a2a08"), 0.9f);
            // Flanking small gold crowns at the ribbon ends.
            Crown(_ribbon, -rw * 0.5f);
            Crown(_ribbon, rw * 0.5f);
            _ribbon.gameObject.AddComponent<PulseScale>().period = 2.5f; // gentle shimmer/breathe
        }

        private void Crown(Transform parent, float x)
        {
            var c = UiWidgets.Rect("Ribbon_Crown", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, 0), new Vector2(0.05f * PW, 0.05f * PW));
            var ci = c.gameObject.AddComponent<Image>(); ci.raycastTarget = false; ci.sprite = UiTex.Diamond(Hex("#f0d27a"), 48);
        }

        // ---- Reward slot: silver coin stack + value. Returns its CountUp. ----
        private CountUp BuildCoinSlot(Transform field, Vector2 anchor, Vector2 pos, float w, float h)
        {
            var slot = MakeSlot(field, "Reward_Slot_Coins", anchor, pos, w, h);
            // Icon_Coins: silver/steel coin stack (upper area).
            var icon = UiWidgets.Rect("Icon_Coins", slot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -h * 0.30f), new Vector2(h * 0.42f, h * 0.42f));
            var ig = icon.gameObject.AddComponent<Image>(); ig.raycastTarget = false; ig.sprite = UiTex.Disc(Hex("#9aa0a8"), 48);
            var band = UiWidgets.Rect("CoinBand", icon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -2), new Vector2(h * 0.42f, h * 0.12f));
            var bg = band.gameObject.AddComponent<Image>(); bg.raycastTarget = false; bg.color = Hex("#5a6068");
            // Coins_Value (bottom area).
            var val = UiWidgets.Label(slot, "0", 34, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, h * 0.20f), new Vector2(w * 0.9f, h * 0.3f), TextAnchor.MiddleCenter, Hex("#f3ead2"));
            var cu = slot.gameObject.AddComponent<CountUp>(); cu.Bind(val, v => v.ToString("N0"), 0f);
            return cu;
        }

        // ---- Reward slot: blue/gold winged "XP" battle-pass badge + "Pass XP" + value. Returns its CountUp. ----
        private CountUp BuildXpSlot(Transform field, Vector2 anchor, Vector2 pos, float w, float h)
        {
            var slot = MakeSlot(field, "Reward_Slot_XP", anchor, pos, w, h);
            UiWidgets.Glow(slot, UiTheme.A(Hex("#2b56c8"), 0.45f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), Vector2.zero, new Vector2(h * 0.7f, h * 0.7f), 1.7f);
            // Icon_XPBadge: royal-blue heraldic shield.
            var badge = UiWidgets.Rect("Icon_XPBadge", slot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -h * 0.30f), new Vector2(h * 0.40f, h * 0.40f));
            var sh = badge.gameObject.AddComponent<Image>(); sh.raycastTarget = false; sh.sprite = UiTex.VGradient(Hex("#2b56c8"), Hex("#1f3e78"), 64);
            badge.localRotation = Quaternion.Euler(0, 0, 45f); // diamond/shield silhouette
            // Gold wings.
            var wl = UiWidgets.Rect("WingL", badge, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(-2, 0), new Vector2(h * 0.18f, h * 0.10f));
            var wli = wl.gameObject.AddComponent<Image>(); wli.raycastTarget = false; wli.sprite = UiTex.Diamond(Hex("#caa04a"), 32);
            var wr = UiWidgets.Rect("WingR", badge, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(2, 0), new Vector2(h * 0.18f, h * 0.10f));
            var wri = wr.gameObject.AddComponent<Image>(); wri.raycastTarget = false; wri.sprite = UiTex.Diamond(Hex("#caa04a"), 32);
            // Gold "XP" monogram (counter-rotated so it reads upright on the diamond shield).
            var xp = UiWidgets.Label(badge, "XP", 22, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(h * 0.4f, h * 0.4f), TextAnchor.MiddleCenter, Hex("#f0d27a"));
            xp.transform.localRotation = Quaternion.Euler(0, 0, -45f);
            // XP_SubLabel "Pass XP" + XP_Value (bottom area, split left/right).
            UiWidgets.Label(slot, "Pass XP", 22, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-w * 0.20f, h * 0.20f), new Vector2(w * 0.5f, h * 0.3f), TextAnchor.MiddleCenter, Hex("#b8a878"));
            var val = UiWidgets.Label(slot, "0", 32, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(w * 0.22f, h * 0.20f), new Vector2(w * 0.45f, h * 0.3f), TextAnchor.MiddleCenter, Hex("#f3ead2"));
            var cu = slot.gameObject.AddComponent<CountUp>(); cu.Bind(val, v => v.ToString("N0"), 0f);
            return cu;
        }

        // ---- Gold-framed dark recessed reward tile (shared chrome). ----
        private RectTransform MakeSlot(Transform field, string name, Vector2 anchor, Vector2 pos, float w, float h)
        {
            var slot = UiWidgets.Rect(name, field, anchor, anchor, pos, new Vector2(w, h));
            var inner = slot.gameObject.AddComponent<Image>(); inner.raycastTarget = false; inner.sprite = UiTex.VGradient(Hex("#171109"), Hex("#0e0a06"), 32); // dark recessed interior
            var frameRt = UiWidgets.Rect("Slot_Frame", slot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var fi = frameRt.gameObject.AddComponent<Image>(); fi.raycastTarget = false; fi.sprite = UiTex.Frame(Hex("#caa04a"), Hex("#af874e"), Hex("#6b5320"), 48, 7); fi.type = Image.Type.Sliced;
            return slot;
        }

        // ---- CTA_Retry: oxblood-red primary, iron/gold frame, inner red rim, breathing glow → OnRetry. ----
        private void BuildRetry(Transform field, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            _retryGlow = UiWidgets.Glow(field, UiTheme.A(Hex("#e0604a"), 0.5f), anchor, anchor, pos, size * 1.35f);
            var pg = _retryGlow.gameObject.AddComponent<PulseGraphic>(); pg.target = _retryGlow; pg.min = 0.3f; pg.max = 0.6f; pg.period = 1.8f;
            var rt = UiWidgets.Rect("CTA_Retry", field, anchor, anchor, pos, size);
            var fill = rt.gameObject.AddComponent<Image>(); fill.sprite = UiTex.VGradient(Hex("#b84130"), Hex("#5a1410"), 32); // red gradient
            var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = fill;
            var cb = btn.colors; cb.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f); cb.pressedColor = new Color(0.85f, 0.82f, 0.82f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); MatchPresentation.OnRetry(); });
            // Inner red rim-glow.
            var rim = UiWidgets.Rect("Retry_Rim", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var ri = rim.gameObject.AddComponent<Image>(); ri.raycastTarget = false; ri.sprite = UiTex.Frame(Hex("#e0604a"), Hex("#caa04a"), Hex("#6b5320"), 48, 8); ri.type = Image.Type.Sliced;
            var lbl = UiWidgets.Label(rt, UiTheme.Track("RETRY"), 40, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Hex("#fff3ec"));
            lbl.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(Hex("#3a0a06"), 0.9f);
        }

        // ---- CTA_MainMenu: dark stone/iron secondary, matte (no glow) → OnContinue (hub). ----
        private void BuildMainMenu(Transform field, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var rt = UiWidgets.Rect("CTA_MainMenu", field, anchor, anchor, pos, size);
            var fill = rt.gameObject.AddComponent<Image>(); fill.sprite = UiTex.VGradient(Hex("#3a342a"), Hex("#2a2620"), 32); // dark stone, faint top highlight
            var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = fill;
            var cb = btn.colors; cb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); cb.pressedColor = new Color(0.85f, 0.85f, 0.88f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); MatchPresentation.OnContinue(); });
            // Iron/gold frame (same chrome as the panel).
            var rim = UiWidgets.Rect("Menu_Rim", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var ri = rim.gameObject.AddComponent<Image>(); ri.raycastTarget = false; ri.sprite = UiTex.Frame(Hex("#5a5048"), Hex("#2a241c"), Hex("#1a1814"), 48, 7); ri.type = Image.Type.Sliced;
            var lbl = UiWidgets.Label(rt, "Main Menu", 38, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Hex("#f3ecdc"));
            lbl.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(Hex("#1a140a"), 0.9f);
        }

        // =================== LIFECYCLE / REVEAL ===================
        public override void OnShow()
        {
            AudioManager.Instance?.PlayMenuMusic();
            StartCoroutine(Reveal());
        }

        // Ominous build: panel pops → count-ups ignite (waves first/hottest) → NEW BEST → rewards → CTAs.
        // Tap anywhere during the reveal snaps all values + NEW BEST to end-state (§I skip rule).
        private IEnumerator Reveal()
        {
            if (_panelCg == null) yield break;
            _panelCg.alpha = 0f; _panelCg.transform.localScale = Vector3.one * 0.9f;
            float t = 0f; const float d = 0.4f;
            while (t < d) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d); _panelCg.alpha = k; _panelCg.transform.localScale = Vector3.one * Mathf.Lerp(0.9f, 1f, k); yield return null; }
            _panelCg.alpha = 1f; _panelCg.transform.localScale = Vector3.one;

            if (_resolved) yield break;
            yield return Wait(0.45f);                       // title settle
            yield return Wait(0.40f);                       // waves label
            _wavesCount?.To(PendingWaves, 0.8f);           // hero ember count-up (rises then settles)
            yield return Wait(0.65f);
            _scoreCount?.To(PendingScore, 0.8f);           // score count-up
            yield return Wait(0.75f);
            if (_ribbon != null) yield return PopIn(_ribbon, 0.35f); // NEW BEST pop (if record)
            yield return Wait(0.25f);
            _coinsCount?.To(PendingCoins, 0.7f);
            _xpCount?.To(PendingPassXP, 0.6f);             // reward count-ups
        }

        // Tap-to-skip: snap every value + reveal NEW BEST instantly, enable CTAs (they are always live).
        private void Update()
        {
            if (_resolved) return;
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0) ResolveAll();
        }

        private void ResolveAll()
        {
            if (_resolved) return;
            _resolved = true;
            _wavesCount?.Snap(PendingWaves);
            _scoreCount?.Snap(PendingScore);
            _coinsCount?.Snap(PendingCoins);
            _xpCount?.Snap(PendingPassXP);
            if (_ribbon != null) _ribbon.localScale = Vector3.one; // NEW BEST appears snapped on skip
        }

        private IEnumerator PopIn(RectTransform rt, float dur)
        {
            float t = 0f;
            while (t < dur && !_resolved) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / dur); float s = Mathf.Lerp(0.7f, 1f, EaseOutBack(k)); rt.localScale = new Vector3(s, s, 1f); yield return null; }
            rt.localScale = Vector3.one;
        }

        private IEnumerator Wait(float s) { float t = 0f; while (t < s && !_resolved) { t += Time.unscaledDeltaTime; yield return null; } }

        private static float EaseOutBack(float x) { const float c1 = 1.70158f, c3 = c1 + 1f; float p = x - 1f; return 1f + c3 * p * p * p + c1 * p * p; }

        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}

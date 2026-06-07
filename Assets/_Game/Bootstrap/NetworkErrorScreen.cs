// BULWARK — NETWORK ERROR (UI Construction Bible · 39). Presentation-only, REMOVABLE.
//
// Full-screen "CONNECTION LOST" recovery overlay (design/NetworkErrorDesign.png). Forensic build per Sections
// A–O: a raycast-absorbing ~60% black scrim (blocks the dimmed live screen beneath, NO scrim-tap close) under a
// centred ~0.50W×0.58H landscape ornate gold-frame panel — gem finial + four corner flourishes, a serif gold
// "CONNECTION LOST" title, a distressed metal shield (wifi arcs + red crack) hero glyph LEFT, body + "Possible
// causes:" + three line-icon causes RIGHT, and a button row: RETRY (cobalt, primary, brightest) + MAIN MENU
// (gold/dark outline, secondary). §12: NO ECS / NO networking / mutates nothing — RETRY is DISPLAY-ONLY (brief
// "retrying…" spinner → optional OnRetry hook → Router.Pop); MAIN MENU / back-key both bail via Router.Pop.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-39 full-screen connection-lost overlay. Presentation-only (§12: no networking, no state writes).</summary>
    public sealed class NetworkErrorScreen : UiScreen
    {
        /// <summary>Optional display-only hook: if set, invoked when RETRY is pressed (host may re-attempt). The screen
        /// itself never networks — it always shows a brief "retrying…" state then dismisses (Router.Pop).</summary>
        public static System.Action OnRetry;

        // Panel geometry — spec §E: ≈0.50W × 0.58H of 2340×1080 = 1170×626 px, landscape, centred.
        private const float PanelW = 1170f;
        private const float PanelH = 626f;

        private CanvasGroup _panelCg;     // panel scale/alpha entry (§I OnShow)
        private RectTransform _shield;     // hero glyph (entry shake §I)
        private Image _crackGlow;          // red danger glow on the fracture (one-shot flare §I/§J)
        private Button _retryBtn;          // primary CTA (in-flight disable + spin §H)
        private RectTransform _retryIcon;  // refresh glyph (spins while "retrying…" §I)
        private Text _retryLabel;          // swaps to "RETRYING" in-flight
        private Button _menuBtn;           // secondary CTA
        private bool _retrying;            // guards re-entrancy

        protected override void Build()
        {
            // ---- Scrim: full-bleed, raycast ON → blocks the dimmed live screen beneath (§D, §L: no scrim-tap close). ----
            // Parented under Rect (full-bleed, ignores safe area → dims the notch too, §D).
            var scrim = UiWidgets.Stretch("Scrim", Rect, new Color(0f, 0f, 0f, 0.60f));
            scrim.raycastTarget = true; // absorbs all input; tap is intentionally inert (cancelOnScrim=false).

            // ---- ErrorPanel: centred landscape ornate gold frame over an obsidian field (§D/§E/§G). ----
            var panelGo = new GameObject("ErrorPanel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            var panel = (RectTransform)panelGo.transform; panel.SetParent(SafeContent, false); // centre clamped inside safe area
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f); panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(PanelW, PanelH); panel.anchoredPosition = Vector2.zero;
            _panelCg = panelGo.GetComponent<CanvasGroup>();
            panelGo.GetComponent<Image>().color = new Color(0, 0, 0, 0); // raycast backstop only (frame art drawn by OrnateFrame)

            // Ornate gold molding + obsidian field (frame border ~26px 9-slice, §E). Returns the inner field rect.
            var field = UiWidgets.OrnateFrame(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(PanelW, PanelH), Hex("#0c0e14"), false, 26f);

            BuildFinialAndFlourishes(panel);

            // ---- Title "CONNECTION LOST" — serif gold, centred, baseline ~0.16 from panel top (§E/§F ~64px). ----
            // anchorY = 1 − 0.16 = 0.84.
            UiWidgets.TitleLabel(field, "CONNECTION LOST", 64, new Vector2(0.5f, 0.84f), new Vector2(0.5f, 0.84f),
                Vector2.zero, new Vector2(PanelW - 120f, 110f), TextAnchor.MiddleCenter, Hex("#f0d27a"), UiTheme.Gold);

            // ---- Body region split: LeftCluster (shield) ~0.40W, RightCluster (text) ~0.55W (§E). ----
            BuildLeftCluster(field);
            BuildRightCluster(field);

            // ---- ButtonRow: centred ~0.86 from panel top → anchorY 0.14 (§E). ----
            BuildButtonRow(field);
        }

        // TopGemFinial (blue/gold, overhangs top edge) + four corner flourishes (§C/§D/§G).
        private void BuildFinialAndFlourishes(RectTransform panel)
        {
            // Gem finial — anchor top-centre, overhangs ~30px above the top edge (§E).
            var gem = UiWidgets.Rect("TopGemFinial", panel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, 6f), new Vector2(64, 64));
            var gImg = gem.gameObject.AddComponent<Image>(); gImg.raycastTarget = false; gImg.sprite = UiTex.Diamond(Hex("#3f6bff"), 48);
            var gemHi = UiWidgets.Rect("GemHi", gem, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(30, 30));
            var ghImg = gemHi.gameObject.AddComponent<Image>(); ghImg.raycastTarget = false; ghImg.sprite = UiTex.Diamond(UiTheme.GoldHi, 32); // blue/gold facet
            UiWidgets.Glow(gem, UiTheme.A(Hex("#4f8bff"), 0.55f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(150, 150));
            gem.gameObject.AddComponent<PulseScale>(); // steady faint bloom (§J one-shot glint stand-in)

            // Four corner flourishes — gold diamond bosses tucked into each corner (raycast off).
            Vector2[] corners = { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
            string[] tags = { "CornerFlourish_BL", "CornerFlourish_BR", "CornerFlourish_TL", "CornerFlourish_TR" };
            for (int i = 0; i < corners.Length; i++)
            {
                var c = corners[i];
                var b = UiWidgets.Rect(tags[i], panel, c, c, new Vector2((c.x < 0.5f ? 1 : -1) * 16f, (c.y < 0.5f ? 1 : -1) * 16f), new Vector2(46, 46));
                var img = b.gameObject.AddComponent<Image>(); img.raycastTarget = false; img.sprite = UiTex.Diamond(UiTheme.GoldHi, 48);
            }
        }

        // LeftCluster → ShieldGlyph: distressed steel shield + wifi arcs + red crack/lightning (§D/§G hero art).
        private void BuildLeftCluster(RectTransform field)
        {
            // Centre of the left ~0.40 of panel width, vertically centred in the body region (~0.30–0.72 → ~0.50).
            var cluster = UiWidgets.Rect("LeftCluster", field, new Vector2(0.21f, 0.50f), new Vector2(0.21f, 0.50f), Vector2.zero, new Vector2(300, 300));
            // Focal glow behind the shield.
            UiWidgets.Glow(cluster, UiTheme.A(Hex("#9aa6b6"), 0.22f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300, 300), 1.6f);

            // Shield body — height ~0.34 of panel (~210px), preserve-aspect (§E). Built as a code "shield" silhouette.
            _shield = UiWidgets.Rect("ShieldGlyph", cluster, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(176, 210));
            var sImg = _shield.gameObject.AddComponent<Image>(); sImg.raycastTarget = false;
            sImg.sprite = UiTex.VGradient(Hex("#cdd3da"), Hex("#3a3f47"), 64); sImg.preserveAspect = true; // steel hi → shadow
            // Beveled steel rim around the shield (worn metal edge).
            var rim = UiWidgets.Rect("ShieldRim", _shield, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rImg = rim.gameObject.AddComponent<Image>(); rImg.raycastTarget = false;
            rImg.sprite = UiTex.Frame(Hex("#cdd3da"), Hex("#8a929c"), Hex("#3a3f47"), 48, 7); rImg.type = Image.Type.Sliced;

            // Wifi arcs (three concentric discs, dimming outward) centred high on the shield.
            float[] arc = { 120f, 86f, 52f };
            float[] arcA = { 0.18f, 0.30f, 0.5f };
            for (int i = 0; i < arc.Length; i++)
            {
                var a = UiWidgets.Rect("WifiArc" + i, _shield, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(arc[i], arc[i]));
                var aImg = a.gameObject.AddComponent<Image>(); aImg.raycastTarget = false;
                aImg.sprite = UiTex.Frame(UiTheme.A(Hex("#cdd3da"), arcA[i]), UiTheme.A(Hex("#8a929c"), arcA[i]), UiTheme.A(Hex("#3a3f47"), arcA[i]), 48, 5);
                aImg.type = Image.Type.Sliced;
            }
            // Wifi base dot.
            var dot = UiWidgets.Rect("WifiDot", _shield, new Vector2(0.5f, 0.40f), new Vector2(0.5f, 0.40f), Vector2.zero, new Vector2(20, 20));
            var dImg = dot.gameObject.AddComponent<Image>(); dImg.raycastTarget = false; dImg.sprite = UiTex.Disc(Hex("#cdd3da"), 32);

            // Red crack / lightning bolt splitting the shield (#d8452b→#7a1f1a) — a thin rotated gradient sliver.
            var crack = UiWidgets.Rect("ShieldCrack", _shield, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(6, 0), new Vector2(16, 220));
            var cImg = crack.gameObject.AddComponent<Image>(); cImg.raycastTarget = false;
            cImg.sprite = UiTex.VGradient(Hex("#d8452b"), Hex("#7a1f1a"), 64); crack.localRotation = Quaternion.Euler(0, 0, 12f);
            var crack2 = UiWidgets.Rect("ShieldCrack2", _shield, new Vector2(0.5f, 0.35f), new Vector2(0.5f, 0.35f), new Vector2(-2, 0), new Vector2(12, 110));
            var c2Img = crack2.gameObject.AddComponent<Image>(); c2Img.raycastTarget = false;
            c2Img.sprite = UiTex.VGradient(Hex("#d8452b"), Hex("#7a1f1a"), 64); crack2.localRotation = Quaternion.Euler(0, 0, -28f);

            // Faint red danger glow + a few embers at the fracture (§J — very low rate, one-shot flare on show).
            _crackGlow = UiWidgets.Glow(_shield, UiTheme.A(Hex("#d8452b"), 0f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(200, 240), 1.7f);
            var embers = UiWidgets.Rect("CrackEmbers", _shield, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(120, 200));
            var ef = embers.gameObject.AddComponent<EmberField>(); ef.count = 6; ef.color = Hex("#d8452b"); // restrained, reassuring not alarming
        }

        // RightCluster → Body_Msg + "Possible causes:" + three line-icon causes (§D/§E/§F).
        private void BuildRightCluster(RectTransform field)
        {
            // Right ~0.55 of panel width (x≈0.42–0.97); left-aligned text column.
            float left = 0.42f * PanelW - PanelW * 0.5f; // panel-local x of the column left edge (pivot-centre)
            float colW = (0.97f - 0.42f) * PanelW;        // ~0.55W column width

            // Body_Msg (2 lines, wrapping) — top of the body region (~0.30 from top → anchorY 0.70).
            var body = MakeWrapText(field, "Body_Msg",
                "The connection to the server was lost. Please check your network and try again.",
                28, new Vector2(0f, 0.70f), new Vector2(0f, 0.70f), new Vector2(left, 0), new Vector2(colW, 110),
                TextAnchor.UpperLeft, Hex("#d9d2c2"));
            ((RectTransform)body.transform).pivot = new Vector2(0f, 1f); // grow downward from its top

            // "Possible causes:" label (gold) with a thin flanking gold rule (§D/§F ~24px).
            UiWidgets.Label(field, "Possible causes:", 24, new Vector2(0f, 0.52f), new Vector2(0f, 0.52f), new Vector2(left, 0), new Vector2(colW, 34), TextAnchor.MiddleLeft, Hex("#cdb474"));
            UiWidgets.Divider(field, new Vector2(0f, 0.52f), new Vector2(0f, 0.52f), new Vector2(left + 290f, 0), colW - 300f, 3f, UiTheme.A(Hex("#cdb474"), 0.7f));

            // CausesList — three rows (each ~32px tall), icon Ø ~26px + text (§E). Explicitly point-anchored for layout-math fidelity.
            string[] causes = { "Unstable internet connection", "Network signal is weak", "Server temporarily unavailable" };
            int[] glyph = { 0, 1, 2 }; // 0=wifi waves, 1=signal bars, 2=globe (line-art stand-ins)
            float[] rowY = { 0.42f, 0.34f, 0.26f }; // staggered down the column
            for (int i = 0; i < causes.Length; i++)
            {
                BuildCauseRow(field, causes[i], glyph[i], left, colW, rowY[i]);
            }
        }

        private void BuildCauseRow(RectTransform field, string text, int glyph, float left, float colW, float y)
        {
            var row = UiWidgets.Rect("Cause_" + glyph, field, new Vector2(0f, y), new Vector2(0f, y), new Vector2(left, 0), new Vector2(colW, 32));
            // Line-icon (muted gold/cream line-art stand-in): wifi=diamond, bars=disc, globe=frame ring.
            var icon = UiWidgets.Rect("CauseIcon", row, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(14, 0), new Vector2(26, 26));
            var iImg = icon.gameObject.AddComponent<Image>(); iImg.raycastTarget = false;
            iImg.sprite = glyph == 0 ? UiTex.Diamond(Hex("#cdb474"), 32)
                        : glyph == 1 ? UiTex.Disc(Hex("#cdb474"), 32)
                        : UiTex.Frame(Hex("#cdb474"), Hex("#cdb474"), UiTheme.A(Hex("#cdb474"), 0.6f), 32, 5);
            if (glyph == 2) iImg.type = Image.Type.Sliced; // globe = thin ring
            UiWidgets.Label(row, text, 24, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(42, 0), new Vector2(colW - 50f, 32), TextAnchor.MiddleLeft, Hex("#c9bfa6"));
        }

        // ButtonRow → Btn_RETRY (cobalt primary) + Btn_MAIN_MENU (gold/dark outline secondary) (§D/§E/§H).
        private void BuildButtonRow(RectTransform field)
        {
            // Two buttons each ≈0.40 of panel width × 72px, ~28px gap, centred at anchorY 0.14.
            float btnW = 0.40f * PanelW;      // ~468
            float btnH = 80f;                  // ≥72px spec / ≥88px touch headroom
            float halfGap = 14f;               // 28px gap total
            float dx = (btnW * 0.5f) + halfGap;

            _retryBtn = BuildRetryButton(field, new Vector2(0.5f, 0.14f), new Vector2(-dx, 0), new Vector2(btnW, btnH));
            _menuBtn = BuildMenuButton(field, new Vector2(0.5f, 0.14f), new Vector2(dx, 0), new Vector2(btnW, btnH));

            // Optional decorative centre separator dot (non-interactive, §C/§E).
            var sep = UiWidgets.Rect("BtnSeparator", field, new Vector2(0.5f, 0.14f), new Vector2(0.5f, 0.14f), Vector2.zero, new Vector2(12, 12));
            var sImg = sep.gameObject.AddComponent<Image>(); sImg.raycastTarget = false; sImg.sprite = UiTex.Diamond(UiTheme.A(UiTheme.GoldHi, 0.85f), 32);
        }

        // Btn_RETRY — cobalt gloss + steady glow + white "RETRY" + refresh icon (brightest, primary §G/§H).
        private Button BuildRetryButton(RectTransform field, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var rt = UiWidgets.Rect("Btn_RETRY", field, anchor, anchor, pos, size);

            // Pulsing cobalt rim glow (§I idle ±10%, 1.6s).
            var glow = UiWidgets.Glow(rt, UiTheme.A(UiTheme.IronBlueHi, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size * 1.45f);
            var pg = glow.gameObject.AddComponent<PulseGraphic>(); pg.target = glow; pg.min = 0.35f; pg.max = 0.65f; pg.period = 1.6f;

            var body = rt.gameObject.AddComponent<Image>();
            body.sprite = UiTex.VGradient(Hex("#4f8bff"), Hex("#1e3aa0"), 32); // cobalt gloss #2b56c8→#4f8bff range
            var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = body;
            var cb = btn.colors; cb.normalColor = Color.white; cb.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f);
            cb.pressedColor = new Color(0.85f, 0.85f, 0.92f, 1f); cb.disabledColor = new Color(0.55f, 0.6f, 0.72f, 0.7f);
            cb.fadeDuration = 0.08f; btn.colors = cb;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); OnRetryPressed(); });

            // Gold-cast rim (sits over the body edge).
            var rim = UiWidgets.Rect("Rim", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rimImg = rim.gameObject.AddComponent<Image>(); rimImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 7); rimImg.type = Image.Type.Sliced; rimImg.raycastTarget = false;

            // Refresh / circular-arrow icon (spins while retrying, §I). Disc + diamond stand-in for the arrow head.
            _retryIcon = UiWidgets.Rect("RetryIcon", rt, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(46, 0), new Vector2(38, 38));
            var ring = _retryIcon.gameObject.AddComponent<Image>(); ring.raycastTarget = false;
            ring.sprite = UiTex.Frame(Color.white, Hex("#cfe0ff"), Hex("#1e3aa0"), 32, 6); ring.type = Image.Type.Sliced;
            var head = UiWidgets.Rect("ArrowHead", _retryIcon, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(8, 0), new Vector2(16, 16));
            var hImg = head.gameObject.AddComponent<Image>(); hImg.raycastTarget = false; hImg.sprite = UiTex.Diamond(Color.white, 32);

            // Label "RETRY" — white, UPPERCASE, +4% track (§F ~32px).
            _retryLabel = UiWidgets.Label(rt, UiTheme.Track("RETRY"), 32, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Color.white);
            ((RectTransform)_retryLabel.transform).offsetMin = new Vector2(70, 0);
            _retryLabel.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.85f);
            return btn;
        }

        // Btn_MAIN_MENU — dark stone capsule + gold beveled rim + gold/cream "MAIN MENU" + home icon; NO glow (secondary §G/§H).
        private Button BuildMenuButton(RectTransform field, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var rt = UiWidgets.Rect("Btn_MAIN_MENU", field, anchor, anchor, pos, size);
            var body = rt.gameObject.AddComponent<Image>();
            body.sprite = UiTex.VGradient(Hex("#262a33"), Hex("#14161e"), 32); // dark stone (dimmer than RETRY → primary dominates)
            var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = body;
            var cb = btn.colors; cb.normalColor = Color.white; cb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            cb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); cb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
            cb.fadeDuration = 0.08f; btn.colors = cb;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); OnMainMenuPressed(); });

            // Gold beveled outline rim.
            var rim = UiWidgets.Rect("Rim", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rimImg = rim.gameObject.AddComponent<Image>(); rimImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 7); rimImg.type = Image.Type.Sliced; rimImg.raycastTarget = false;

            // Home icon (gold/cream line-art stand-in): diamond roof + disc base.
            var home = UiWidgets.Rect("HomeIcon", rt, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(44, 0), new Vector2(34, 34));
            var roof = home.gameObject.AddComponent<Image>(); roof.raycastTarget = false; roof.sprite = UiTex.Diamond(Hex("#e9dcc0"), 32);
            var baseRt = UiWidgets.Rect("HomeBase", home, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 4), new Vector2(22, 16));
            var bImg = baseRt.gameObject.AddComponent<Image>(); bImg.raycastTarget = false; bImg.sprite = UiTex.Solid(Hex("#e9dcc0"));

            // Label "MAIN MENU" — gold/cream, UPPERCASE, +3% track (§F ~30px).
            var lbl = UiWidgets.Label(rt, UiTheme.Track("MAIN MENU"), 30, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Hex("#e9dcc0"));
            ((RectTransform)lbl.transform).offsetMin = new Vector2(64, 0);
            lbl.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.85f);
            return btn;
        }

        // A wrapping (multi-line) shadowed Text — Label() defaults to overflow, so enable wrap here (§F body 2 lines).
        private static Text MakeWrapText(Transform parent, string name, string text, int size, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 sz, TextAnchor align, Color col)
        {
            var t = UiWidgets.Label(parent, text, size, aMin, aMax, pos, sz, align, col);
            t.gameObject.name = name;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            return t;
        }

        // ---- Lifecycle / animation (§I) ----
        public override void OnShow()
        {
            AudioManager.Instance?.PlayMenuMusic();
            StartCoroutine(EnterSequence());
        }

        // OnShow timeline (§I): panel scale-in, shield entry + single shake + crack flare, then a one-shot glow settle.
        private IEnumerator EnterSequence()
        {
            if (_panelCg == null) yield break;
            // Panel scale 0.92→1.0 + α 0→1 over ~220ms (ease-out-back, slight overshoot).
            _panelCg.alpha = 0f; float t = 0f; const float d = 0.22f;
            while (t < d) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d); _panelCg.alpha = k; _panelCg.transform.localScale = Vector3.one * Mathf.Lerp(0.92f, 1f, EaseOutBack(k)); yield return null; }
            _panelCg.alpha = 1f; _panelCg.transform.localScale = Vector3.one;

            // Crack glow flares once, then settles to a faint danger ember (§I/§J).
            yield return FlareCrack();
            // Single horizontal shield shake (±5px, ~200ms) — one entry shake only (§I/§L: do not over-animate).
            yield return ShakeShield();
        }

        private IEnumerator FlareCrack()
        {
            if (_crackGlow == null) yield break;
            float t = 0f; const float d = 0.2f;
            while (t < d) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d); var c = _crackGlow.color; c.a = Mathf.Lerp(0f, 0.6f, k); _crackGlow.color = c; yield return null; }
            // settle back to a faint steady danger glow
            t = 0f; const float d2 = 0.35f;
            while (t < d2) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d2); var c = _crackGlow.color; c.a = Mathf.Lerp(0.6f, 0.2f, k); _crackGlow.color = c; yield return null; }
        }

        private IEnumerator ShakeShield()
        {
            if (_shield == null) yield break;
            var home = _shield.anchoredPosition; float t = 0f; const float d = 0.2f;
            while (t < d) { t += Time.unscaledDeltaTime; float k = t / d; float x = Mathf.Sin(k * Mathf.PI * 4f) * 5f * (1f - k); _shield.anchoredPosition = home + new Vector2(x, 0); yield return null; }
            _shield.anchoredPosition = home;
        }

        // ---- Events (§K) ----
        // RETRY → DISPLAY-ONLY: disable + spin the icon ("retrying…"), optionally fire OnRetry, then dismiss (Pop).
        // §L/§12: never fakes a real reconnect and never networks — this is recovery chrome; the host owns the real retry.
        private void OnRetryPressed()
        {
            if (_retrying) return;
            _retrying = true;
            StartCoroutine(RetrySequence());
        }

        private IEnumerator RetrySequence()
        {
            // Disable both buttons; show the in-flight "retrying…" state (§H).
            if (_retryBtn != null) _retryBtn.interactable = false;
            if (_menuBtn != null) _menuBtn.interactable = false;
            if (_retryLabel != null) _retryLabel.text = UiTheme.Track("RETRYING");

            Spin spin = null;
            if (_retryIcon != null) { spin = _retryIcon.gameObject.AddComponent<Spin>(); spin.degPerSec = -450f; } // ~0.8s/rev

            // Brief spinner window (display-only; no real network call).
            float t = 0f; const float d = 0.9f;
            while (t < d) { t += Time.unscaledDeltaTime; yield return null; }

            if (spin != null) Destroy(spin);

            // Hand off to the optional host hook (it may perform the real re-attempt); then dismiss this overlay.
            OnRetry?.Invoke();
            Router?.Toast("Reconnecting…");
            Router?.Pop();
        }

        // MAIN MENU / back-key → bail to the hub (clean teardown). Presentation seam = Router.Pop (§K/§L).
        private void OnMainMenuPressed()
        {
            if (_retrying) return;
            Router?.Pop();
        }

        // Back-key maps to MAIN MENU (safe bail) — never silently dismiss without resolving (§K OnBackKey).
        private void Update()
        {
            if (_retrying) return;
            if (Input.GetKeyDown(KeyCode.Escape)) OnMainMenuPressed();
        }

        private static float EaseOutBack(float x) { const float c1 = 1.70158f, c3 = c1 + 1f; return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f); }
        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}

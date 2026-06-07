// BULWARK — SETTINGS (UI Construction Bible · 36). Presentation-only, REMOVABLE.
//
// Forensic code-built rebuild of design/SettingsScreenDesign.png on the UiRouter shell (spec 36 §A–O): a
// torch-lit-armory full-bleed BG + vignette; a gold "SETTINGS" plate with crown/wing ornaments + gem/gold
// currency chips (top); a left 7-tab rail (General selected, gold highlight); and a scrolling 2×2 panel grid
// — AUDIO (Music/SFX/Voice slider rows + per-channel mute), GRAPHICS (Quality/Resolution/Frame-Rate segmented
// + Shadows/Bloom toggles), ACCOUNT (avatar/name/level/XP + blue Change-Name/Link), OTHER (Vibration/Push/
// Battery toggles + faction emblem) — plus a LOGOUT(red)/PRIVACY/RESET action bar. §12: NO ECS, NO gameplay/
// balance/economy/backend — every control is DISPLAY-ONLY (sliders/toggles/segments animate visuals but persist
// nothing) EXCEPT the per-channel mute toggle, which drives AudioManager.ToggleMute()/Muted (presentation audio,
// the one live behavior per spec §A/H/L). Other rows surface Router.Toast (LOGOUT/RESET note the Confirm gate).

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-36 landscape Settings. Presentation-only (mute real via AudioManager; everything else display-only/stub).</summary>
    public sealed class SettingsScreen : UiScreen
    {
        // ---- Exact spec hexes not in UiTheme (§B/F/G) ----
        private static readonly Color TabSel    = Hex("#f4dd8c"); // selected tab label
        private static readonly Color TabUnsel   = Hex("#9a8a6a"); // unselected tab label
        private static readonly Color LabelCol    = Hex("#d9d2c2"); // row labels
        private static readonly Color PercentCol   = Hex("#f0d27a"); // % values + slider fill highlight
        private static readonly Color SegSel      = Hex("#2a2010"); // segmented selected (dark-on-gold) text
        private static readonly Color SegUnsel     = Hex("#b8b0a0"); // segmented unselected text
        private static readonly Color OnGreen     = Hex("#3fd07a"); // ON accent (toggles)
        private static readonly Color OffGrey     = Hex("#8c8270"); // OFF accent
        private static readonly Color NameCol     = Hex("#f3ead2"); // account name
        private static readonly Color MetaCol     = Hex("#c9bfa6"); // "Level 45"
        private static readonly Color XpCol       = Hex("#d9d2c2"); // XP value
        private static readonly Color PanelFill   = Hex("#0c0e14"); // panel obsidian top
        private static readonly Color PanelFillLo  = Hex("#14161e"); // panel obsidian bottom
        private static readonly Color TrackDark   = Hex("#1a1c24"); // recessed slider/toggle groove

        // Staggered intro: each panel/the action bar gets its own CanvasGroup; faded in OnShow.
        private readonly System.Collections.Generic.List<CanvasGroup> _stagger = new System.Collections.Generic.List<CanvasGroup>(5);

        protected override void Build()
        {
            // ===== BackgroundLayer (full-bleed under the cutout → Rect) =====
            UiWidgets.Stretch("BG_Dungeon", Rect, UiTheme.Charcoal, "bg_menu");
            // Warm brazier focal glow + faint embers (atmospheric, low-key; never over controls — kept upper area).
            UiWidgets.Glow(Rect, UiTheme.A(UiTheme.Ember, 0.18f), new Vector2(0.2f, 0.85f), new Vector2(0.2f, 0.85f), Vector2.zero, new Vector2(1100, 900), 1.7f);
            UiWidgets.Glow(Rect, UiTheme.A(UiTheme.Ember, 0.15f), new Vector2(0.85f, 0.85f), new Vector2(0.85f, 0.85f), Vector2.zero, new Vector2(1000, 800), 1.7f);
            var emberZone = UiWidgets.Rect("Braziers", Rect, new Vector2(0, 0.62f), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            var ef = emberZone.gameObject.AddComponent<EmberField>(); ef.count = 10; ef.color = UiTheme.Ember; // very low rate (§J)
            UiWidgets.Vignette(Rect, 0.6f); // heavily vignetted (§B)

            // ===== TopBar (§E: height ≈0.105) =====
            // Back (top-left chevron) — task-mandated signature on SafeContent.
            UiWidgets.BackButton(SafeContent, () => Router.Pop());

            BuildTitle();

            // Currency cluster top-right: Gold rightmost (idx 0), Gems left of it (idx 1) — values display-only.
            UiWidgets.CurrencyChip(SafeContent, UiTheme.Gold, UiStub.Gold, 0, out _);
            UiWidgets.CurrencyChip(SafeContent, UiTheme.AmethystHi, UiStub.Gems, 1, out _);

            // ===== BodyRow: TabRail (left) + ContentArea (right) =====
            BuildTabRail();
            BuildContent();
        }

        // ---- Title plate: crown + flanking wing/laurel flourishes + "SETTINGS" (§C/F) ----
        private void BuildTitle()
        {
            var grp = UiWidgets.Rect("TitlePlate", SafeContent, new Vector2(0.5f, 0.945f), new Vector2(0.5f, 0.945f), Vector2.zero, new Vector2(900, 110));
            UiWidgets.Glow(grp, UiTheme.A(UiTheme.GoldHi, 0.28f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820, 220), 1.6f);
            // Crown finial above the word.
            var crown = UiWidgets.Finial(grp, new Vector2(0.5f, 1f), new Vector2(0, -6), 56f); crown.color = UiTheme.GoldHi;
            // Wing/laurel flourishes flanking the title (diamonds rotated to read as swept wings).
            for (int s = -1; s <= 1; s += 2)
            {
                var wing = UiWidgets.Finial(grp, new Vector2(0.5f, 0.5f), new Vector2(330 * s, 0), 64f);
                wing.color = UiTheme.A(UiTheme.Gold, 0.9f); wing.transform.localRotation = Quaternion.Euler(0, 0, 45 * -s);
                var tip = UiWidgets.Finial(grp, new Vector2(0.5f, 0.5f), new Vector2(380 * s, 0), 36f);
                tip.color = UiTheme.A(UiTheme.GoldHi, 0.85f);
            }
            UiWidgets.TitleLabel(grp, "SETTINGS", 60, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600, 90),
                TextAnchor.MiddleCenter, Hex("#f0d27a"), UiTheme.Gold);
        }

        // ---- Left tab rail: 7 exclusive tabs, General selected (gold highlight bar) (§D/E) ----
        private void BuildTabRail()
        {
            // Rail strip x≈[0.012,0.177] over the body height (anchorY 0.01→0.895).
            var rail = UiWidgets.Rect("TabRail", SafeContent, new Vector2(0.012f, 0.01f), new Vector2(0.177f, 0.895f), Vector2.zero, Vector2.zero);
            var railBg = rail.gameObject.AddComponent<Image>();
            railBg.sprite = UiTex.VGradient(Hex("#15161e"), Hex("#0b0c12"), 64); railBg.raycastTarget = false;
            var railRim = UiWidgets.Rect("RailRim", rail, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var railRimImg = railRim.gameObject.AddComponent<Image>();
            railRimImg.sprite = UiTex.Frame(UiTheme.A(UiTheme.GoldHi, 0.7f), UiTheme.A(UiTheme.Gold, 0.6f), UiTheme.GoldShadow, 48, 6);
            railRimImg.type = Image.Type.Sliced; railRimImg.raycastTarget = false;

            string[] tabs = { "General", "Audio", "Graphics", "Controls", "Account", "Language", "Support" };
            int n = tabs.Length;
            float top = 0.965f, pitch = 0.135f; // 7 tabs equally spaced down the rail (anchorY)
            for (int i = 0; i < n; i++)
            {
                bool active = i == 0; // General selected today; others stubbed
                string tb = tabs[i];
                float y = top - i * pitch;
                var tabRt = UiWidgets.Rect("Tab_" + tb, rail, new Vector2(0.5f, y), new Vector2(0.5f, y), Vector2.zero, new Vector2(330, 98));
                var hit = tabRt.gameObject.AddComponent<Image>(); hit.color = new Color(0, 0, 0, 0); // raycast target
                var btn = tabRt.gameObject.AddComponent<Button>(); btn.targetGraphic = hit;
                var cb = btn.colors; cb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); cb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
                btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); if (!active) Router.Toast(tb + " — coming soon"); });

                if (active)
                {
                    // Warm gold highlight bar (deep gold→bronze so the bright label reads on top) + soft pulsing glow (§G/J).
                    var bar = UiWidgets.Rect("Highlight", tabRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(316, 84));
                    var barImg = bar.gameObject.AddComponent<Image>(); barImg.sprite = UiTex.VGradient(UiTheme.Gold, UiTheme.GoldShadow, 32); barImg.raycastTarget = false;
                    var pg = bar.gameObject.AddComponent<PulseGraphic>(); pg.target = barImg; pg.min = 0.9f; pg.max = 1f; pg.period = 1.6f;
                }
                // Icon (gold line-art placeholder diamond).
                var ico = UiWidgets.Rect("Icon", tabRt, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(40, 0), new Vector2(36, 36));
                var icoImg = ico.gameObject.AddComponent<Image>(); icoImg.raycastTarget = false;
                icoImg.sprite = UiTex.Diamond(active ? UiTheme.GoldHi : UiTheme.A(UiTheme.GoldHi, 0.55f), 32);
                // Label (selected #f4dd8c bright gold / unselected muted grey — exact §F hexes).
                UiWidgets.Label(tabRt, tb, 28, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(40, 0), Vector2.zero,
                    TextAnchor.MiddleLeft, active ? TabSel : TabUnsel);
            }
        }

        // ---- ContentArea: scrolling 2×2 panel grid + bottom action bar (§D/E) ----
        private void BuildContent()
        {
            // Host over content region x≈[0.19,0.99], anchorY 0.01→0.895 (below TopBar). ScrollRect (rows overflow).
            var host = UiWidgets.Rect("ContentArea", SafeContent, new Vector2(0.19f, 0.01f), new Vector2(0.99f, 0.895f), Vector2.zero, Vector2.zero);
            var scroll = host.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 36f;

            var viewport = UiWidgets.Rect("Viewport", host, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var vImg = viewport.gameObject.AddComponent<Image>(); vImg.color = new Color(0, 0, 0, 0); vImg.raycastTarget = true;
            viewport.gameObject.AddComponent<RectMask2D>();

            // Tall content (top-anchored). Heights chosen so the 2×2 grid + action bar read at full height and
            // overflow scrolls only when the viewport is shorter (per task: ScrollRect on overflow).
            const float gridH = 1180f;     // two panel rows
            const float barH = 150f;       // bottom action bar
            const float gap = 24f;
            float totalH = gridH + gap + barH;
            var content = UiWidgets.Rect("Content", viewport, new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, new Vector2(0, totalH));
            content.pivot = new Vector2(0.5f, 1f);
            scroll.viewport = viewport; scroll.content = content;

            // 2×2 grid: two columns ~0.49 each with a ~0.02 center gutter; top row taller than bottom (§E).
            // Anchors are fractions of the CONTENT rect (full width / fixed height → deterministic).
            const float lX0 = 0f, lX1 = 0.49f, rX0 = 0.51f, rX1 = 1f;
            // Row Y split (anchorY within content): top panels 1.0→~0.515, bottom ~0.495→~0.13, bar below.
            const float topY1 = 1f, topY0 = 0.515f, botY1 = 0.495f, botY0 = 0.13f;

            BuildAudioPanel(content,    lX0, lX1, topY0, topY1);
            BuildGraphicsPanel(content, rX0, rX1, topY0, topY1);
            BuildAccountPanel(content,  lX0, lX1, botY0, botY1);
            BuildOtherPanel(content,    rX0, rX1, botY0, botY1);
            BuildActionBar(content);
        }

        // Common framed sub-panel (obsidian fill + gold rim + centered header w/ flourishes). Returns inner body rect.
        private RectTransform Panel(RectTransform content, float x0, float x1, float y0, float y1, string header, out CanvasGroup cg)
        {
            var go = new GameObject("Panel_" + header, typeof(RectTransform), typeof(CanvasGroup));
            var rt = (RectTransform)go.transform; rt.SetParent(content, false);
            rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1); rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            cg = go.GetComponent<CanvasGroup>(); _stagger.Add(cg);

            var fill = go.AddComponent<Image>(); fill.sprite = UiTex.VGradient(PanelFillLo, PanelFill, 64); fill.raycastTarget = false;
            var rim = UiWidgets.Rect("Rim", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rimImg = rim.gameObject.AddComponent<Image>();
            rimImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 64, 12); rimImg.type = Image.Type.Sliced; rimImg.raycastTarget = false;

            // Centered gold caps header with flanking flourishes (§B/F).
            UiWidgets.TitleLabel(rt, header, 30, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -38), new Vector2(420, 44),
                TextAnchor.MiddleCenter, Hex("#f0d27a"), UiTheme.Gold);
            UiWidgets.Divider(rt, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-150, -64), 180, 3f, UiTheme.A(UiTheme.Gold, 0.7f));
            UiWidgets.Divider(rt, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(150, -64), 180, 3f, UiTheme.A(UiTheme.Gold, 0.7f));

            // Body region under the header (rows parent here).
            var body = UiWidgets.Rect("Body", rt, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            body.offsetMin = new Vector2(28, 26); body.offsetMax = new Vector2(-28, -88);
            return body;
        }

        // ===== AUDIO panel: Music/SFX/Voice slider rows (display-only) + per-channel mute (only Music wired live) =====
        private void BuildAudioPanel(RectTransform content, float x0, float x1, float y0, float y1)
        {
            var body = Panel(content, x0, x1, y0, y1, "AUDIO", out _);
            // AudioRow(body, label, fy-from-top, value01, wireMute). Music=100%, SFX=100%, Voice=80% (§C/M).
            AudioRow(body, "Music",         0.18f, 1.00f, wireMute: true);  // mute → AudioManager (the one live control)
            AudioRow(body, "Sound Effects", 0.50f, 1.00f, wireMute: false); // display-only (no per-channel mixer yet)
            AudioRow(body, "Voice",         0.82f, 0.80f, wireMute: false); // display-only
        }

        private void AudioRow(RectTransform body, string label, float fy, float value01, bool wireMute)
        {
            float y = 1f - fy; // anchorY within body (already an inset region)
            var row = UiWidgets.Rect("Row_" + label, body, new Vector2(0, y), new Vector2(1, y), Vector2.zero, new Vector2(0, 64));
            // Speaker icon (gold disc).
            var spk = UiWidgets.Rect("Icon_Speaker", row, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(20, 0), new Vector2(32, 32));
            var spkImg = spk.gameObject.AddComponent<Image>(); spkImg.sprite = UiTex.Disc(UiTheme.GoldHi, 32); spkImg.raycastTarget = false;
            // Label (fixed width ~0.28 of panel).
            UiWidgets.Label(row, label, 26, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(64, 0), new Vector2(280, 36), TextAnchor.MiddleLeft, LabelCol);
            // Percent text (right of slider, before mute).
            var pct = UiWidgets.Label(row, Mathf.RoundToInt(value01 * 100f) + "%", 24, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-96, 0), new Vector2(70, 34), TextAnchor.MiddleRight, PercentCol);
            // Slider (track + gold fill + round gold handle) — stretched between the label and the percent/mute.
            // Display-only; drag updates visuals + percent only.
            BuildSlider(row, new Vector2(0.36f, 0.5f), new Vector2(0.79f, 0.5f), Vector2.zero, new Vector2(0, 28), value01, pct);
            // Mute toggle (speaker; lit gold = on / grey+slash = muted).
            BuildMute(row, wireMute);
        }

        // A code-built slider (standard uGUI Slider mechanics): recessed track + gold fill (Slider drives its
        // anchors) + round gold handle. Drag updates fill/handle/percent — visual only, persists nothing (stub §H).
        private void BuildSlider(RectTransform parent, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, float value01, Text percentLabel)
        {
            var go = new GameObject("Slider", typeof(RectTransform));
            var rt = (RectTransform)go.transform; rt.SetParent(parent, false);
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.anchoredPosition = pos; rt.sizeDelta = size;

            // Recessed dark groove (full-width background).
            var track = UiWidgets.Rect("Track", rt, new Vector2(0, 0.5f), new Vector2(1, 0.5f), Vector2.zero, new Vector2(0, 12));
            var trackImg = track.gameObject.AddComponent<Image>(); trackImg.sprite = UiTex.VGradient(UiTheme.ChannelDark, TrackDark, 16); trackImg.raycastTarget = false;

            // Fill area + fill (Simple image; the Slider component drives fillRect's width via its anchors).
            var fillArea = UiWidgets.Rect("FillArea", rt, new Vector2(0, 0.5f), new Vector2(1, 0.5f), Vector2.zero, new Vector2(0, 12));
            var fillRt = UiWidgets.Rect("Fill", fillArea, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            var fill = fillRt.gameObject.AddComponent<Image>(); fill.sprite = UiTex.VGradient(UiTheme.GoldFillHi, UiTheme.GoldFillLo, 16); fill.raycastTarget = false;

            // Handle slide area + round gold cabochon handle (with a faint constant glow — hover-only is pointer-platform; subtle always-on is the safe stand-in §J).
            var handleArea = UiWidgets.Rect("HandleSlideArea", rt, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, new Vector2(-28, 0));
            var handle = UiWidgets.Rect("Handle", handleArea, new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(28, 28));
            UiWidgets.Glow(handle, UiTheme.A(UiTheme.GoldFillHi, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(50, 50));
            var handleImg = handle.gameObject.AddComponent<Image>(); handleImg.sprite = UiTex.Disc(UiTheme.GoldHi, 48);

            var slider = go.AddComponent<Slider>();
            slider.direction = Slider.Direction.LeftToRight; slider.minValue = 0f; slider.maxValue = 1f; slider.wholeNumbers = false;
            slider.fillRect = fillRt; slider.handleRect = handle; slider.targetGraphic = handleImg;
            slider.value = Mathf.Clamp01(value01);
            slider.onValueChanged.AddListener(v =>
            {
                if (percentLabel != null) percentLabel.text = Mathf.RoundToInt(v * 100f) + "%"; // live visual only — NOT persisted (stub §H/K)
            });
        }

        // Per-channel mute toggle. Music wires AudioManager.ToggleMute()/Muted (the one live control); others stub.
        private void BuildMute(RectTransform row, bool wire)
        {
            var rt = UiWidgets.Rect("MuteToggle", row, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-22, 0), new Vector2(44, 44));
            var img = rt.gameObject.AddComponent<Image>();
            var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = img;
            var cb = btn.colors; cb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); cb.pressedColor = new Color(0.82f, 0.82f, 0.86f, 1f); cb.fadeDuration = 0.06f; btn.colors = cb;

            bool LiveMuted() => wire && AudioManager.Instance != null && AudioManager.Instance.Muted;
            bool localMuted = wire && LiveMuted(); // stub channels start unmuted (visual)
            void Paint() { bool m = wire ? LiveMuted() : localMuted; img.sprite = UiTex.Disc(m ? OffGrey : UiTheme.GoldHi, 48); }
            Paint();

            // Slash overlay when muted (toggled with state).
            var slash = UiWidgets.Rect("Slash", rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(40, 5));
            var slashImg = slash.gameObject.AddComponent<Image>(); slashImg.color = OffGrey; slashImg.raycastTarget = false;
            slash.localRotation = Quaternion.Euler(0, 0, 45);
            void PaintSlash() { slash.gameObject.SetActive(wire ? LiveMuted() : localMuted); }
            PaintSlash();

            btn.onClick.AddListener(() =>
            {
                AudioManager.Instance?.Click();
                if (wire) AudioManager.Instance?.ToggleMute(); // FUNCTIONAL: actually mutes both channels (§H/K)
                else localMuted = !localMuted;                 // display-only (no per-channel mixer yet)
                Paint(); PaintSlash();
            });
        }

        // ===== GRAPHICS panel: Quality/Resolution/FrameRate segmented + Shadows/Bloom toggles (all display-only) =====
        private void BuildGraphicsPanel(RectTransform content, float x0, float x1, float y0, float y1)
        {
            var body = Panel(content, x0, x1, y0, y1, "GRAPHICS", out _);
            SegmentRow(body, "Quality",    0.06f, new[] { "LOW", "MEDIUM", "HIGH", "ULTRA" }, 3);   // ULTRA selected
            SegmentRow(body, "Resolution", 0.27f, new[] { "50%", "75%", "100%" }, 2);              // 100% selected
            SegmentRow(body, "Frame Rate", 0.48f, new[] { "30 FPS", "60 FPS", "120 FPS" }, 1);     // 60 FPS selected
            ToggleRow(body, "Shadows", 0.69f, true);  // ON
            ToggleRow(body, "Bloom",   0.86f, true);  // ON
        }

        private void SegmentRow(RectTransform body, string label, float fy, string[] options, int selected)
        {
            float y = 1f - fy;
            var row = UiWidgets.Rect("Row_" + label, body, new Vector2(0, y), new Vector2(1, y), Vector2.zero, new Vector2(0, 64));
            UiWidgets.Label(row, label, 26, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(8, 0), new Vector2(360, 36), TextAnchor.MiddleLeft, LabelCol);
            BuildSegment(row, 0.56f, 48f, options, selected);
        }

        // Segmented control: dark capsule w/ gold rim; selected option gold-filled (dark text), others ghost.
        // Right-anchored, width = widthFrac of the row; display-only — clicking moves the gold fill but persists nothing (§H stub).
        private void BuildSegment(RectTransform parent, float widthFrac, float height, string[] options, int selected)
        {
            var cap = UiWidgets.Rect("Segmented", parent, new Vector2(1f - widthFrac, 0.5f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(0, height));
            cap.pivot = new Vector2(1f, 0.5f);
            var capImg = cap.gameObject.AddComponent<Image>(); capImg.sprite = UiTex.VGradient(TrackDark, UiTheme.ChannelDark, 16); capImg.raycastTarget = false;
            var capRim = UiWidgets.Rect("Rim", cap, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var capRimImg = capRim.gameObject.AddComponent<Image>(); capRimImg.sprite = UiTex.Frame(UiTheme.A(UiTheme.GoldHi, 0.7f), UiTheme.A(UiTheme.Gold, 0.55f), UiTheme.GoldShadow, 48, 4); capRimImg.type = Image.Type.Sliced; capRimImg.raycastTarget = false;

            int count = options.Length;
            var fills = new Image[count];
            var labels = new Text[count];
            int sel = selected;
            for (int i = 0; i < count; i++)
            {
                int idx = i;
                float fx0 = i / (float)count, fx1 = (i + 1) / (float)count;
                var opt = UiWidgets.Rect("Opt_" + options[i], cap, new Vector2(fx0, 0), new Vector2(fx1, 1), Vector2.zero, Vector2.zero);
                opt.offsetMin = new Vector2(3, 3); opt.offsetMax = new Vector2(-3, -3);
                var optImg = opt.gameObject.AddComponent<Image>(); // gold fill when selected (raycast target)
                optImg.sprite = UiTex.VGradient(UiTheme.GoldHi, UiTheme.Gold, 16);
                optImg.color = i == sel ? Color.white : new Color(1, 1, 1, 0f);
                fills[i] = optImg;
                var btn = opt.gameObject.AddComponent<Button>(); btn.targetGraphic = optImg;
                var cb = btn.colors; cb.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f); cb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
                var lbl = UiWidgets.Label(opt, options[i], 22, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, i == sel ? SegSel : SegUnsel);
                labels[i] = lbl;
                if (i > 0) UiWidgets.Divider(cap, new Vector2(fx0, 0.5f), new Vector2(fx0, 0.5f), Vector2.zero, 2, height * 0.6f, UiTheme.A(UiTheme.Gold, 0.4f));
                btn.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.Click();
                    sel = idx; // store new selection (stub: not applied/persisted §H/K)
                    for (int k = 0; k < count; k++)
                    {
                        fills[k].color = k == sel ? Color.white : new Color(1, 1, 1, 0f);
                        labels[k].color = k == sel ? SegSel : SegUnsel;
                    }
                });
            }
        }

        // ===== Shared pill toggle (Shadows/Bloom/Vibration/Push/Battery): ON=knob-right+green track / OFF=knob-left =====
        private void ToggleRow(RectTransform body, string label, float fy, bool on)
        {
            float y = 1f - fy;
            var row = UiWidgets.Rect("Row_" + label, body, new Vector2(0, y), new Vector2(1, y), Vector2.zero, new Vector2(0, 56));
            UiWidgets.Label(row, label, 26, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(8, 0), new Vector2(620, 36), TextAnchor.MiddleLeft, LabelCol);
            BuildToggle(row, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-8, 0), on);
        }

        // A code-built toggle: a pill track + a sliding gold knob. ON=green-gold track + knob right. Display-only (stub).
        private void BuildToggle(RectTransform parent, Vector2 aMin, Vector2 aMax, Vector2 pos, bool on)
        {
            var size = new Vector2(72, 34);
            var rt = UiWidgets.Rect("Toggle", parent, aMin, aMax, pos, size); rt.pivot = new Vector2(1f, 0.5f);
            var trackImg = rt.gameObject.AddComponent<Image>(); // pill track (gold rim added below)
            bool state = on;
            void PaintTrack() { trackImg.sprite = UiTex.VGradient(state ? Hex("#2e8a52") : TrackDark, state ? OnGreen : UiTheme.ChannelDark, 16); }
            PaintTrack();
            var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = trackImg;
            var cb = btn.colors; cb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); cb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); cb.fadeDuration = 0.06f; btn.colors = cb;

            var rim = UiWidgets.Rect("Rim", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rimImg = rim.gameObject.AddComponent<Image>(); rimImg.sprite = UiTex.Frame(UiTheme.A(UiTheme.GoldHi, 0.6f), UiTheme.A(UiTheme.Gold, 0.5f), UiTheme.GoldShadow, 48, 4); rimImg.type = Image.Type.Sliced; rimImg.raycastTarget = false;

            // Sliding gold knob — center-anchored, ±19px = left(OFF)/right(ON), so the slide never jumps anchors.
            const float knobX = 19f;
            var knob = UiWidgets.Rect("Knob", rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(state ? knobX : -knobX, 0), new Vector2(28, 28));
            var knobImg = knob.gameObject.AddComponent<Image>(); knobImg.sprite = UiTex.Disc(UiTheme.GoldHi, 48); knobImg.raycastTarget = false;
            var glow = UiWidgets.Glow(knob, UiTheme.A(OnGreen, state ? 0.5f : 0f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(46, 46));

            btn.onClick.AddListener(() =>
            {
                AudioManager.Instance?.Click();
                state = !state; // display-only — visual flip, persists nothing (stub §H/K)
                StartCoroutine(FlipKnob(knob, state ? knobX : -knobX)); // 120ms ease-out slide (§I)
                PaintTrack();
                glow.color = UiTheme.A(OnGreen, state ? 0.5f : 0f);
            });
        }

        // Knob slides toward toX over ~120 ms ease-out (§I toggle flip); track crossfade handled by caller.
        private IEnumerator FlipKnob(RectTransform knob, float toX)
        {
            float fromX = knob.anchoredPosition.x, t = 0f; const float d = 0.12f;
            while (t < d && knob != null) { t += Time.unscaledDeltaTime; float k = 1f - Mathf.Pow(1f - Mathf.Clamp01(t / d), 2f); var p = knob.anchoredPosition; p.x = Mathf.Lerp(fromX, toX, k); knob.anchoredPosition = p; yield return null; }
            if (knob != null) { var fp = knob.anchoredPosition; fp.x = toX; knob.anchoredPosition = fp; }
        }

        // ===== ACCOUNT panel: avatar + name + level + XP bar + blue Change-Name/Link buttons (display-only) =====
        private void BuildAccountPanel(RectTransform content, float x0, float x1, float y0, float y1)
        {
            var body = Panel(content, x0, x1, y0, y1, "ACCOUNT", out _);

            // Avatar in a beveled gold (faction-tinted) ring.
            var avatar = UiWidgets.Rect("Avatar", body, new Vector2(0, 1f), new Vector2(0, 1f), new Vector2(60, -56), new Vector2(96, 96));
            var avImg = avatar.gameObject.AddComponent<Image>(); avImg.sprite = UiTex.Disc(UiTheme.IronBanner, 64); avImg.raycastTarget = false;
            var avRing = UiWidgets.Rect("Ring", avatar, new Vector2(-0.12f, -0.12f), new Vector2(1.12f, 1.12f), Vector2.zero, Vector2.zero);
            var avRingImg = avRing.gameObject.AddComponent<Image>(); avRingImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 8); avRingImg.type = Image.Type.Sliced; avRingImg.raycastTarget = false;

            // Identity block (name / level).
            UiWidgets.Label(body, "StickKing", 30, new Vector2(0, 1f), new Vector2(0, 1f), new Vector2(126, -36), new Vector2(360, 40), TextAnchor.MiddleLeft, NameCol); // seed username (keep, §L)
            UiWidgets.Label(body, "Level 45", 24, new Vector2(0, 1f), new Vector2(0, 1f), new Vector2(126, -76), new Vector2(360, 34), TextAnchor.MiddleLeft, MetaCol);

            // XP bar (gold fill in a dark groove) labeled "25600" — client never writes XP (§L). Ratio is interpretive (§N).
            // Left-anchored under the avatar/name block, ~0.7 of panel width (fixed px), height ~16 (§E/G).
            var xpBg = UiWidgets.Rect("XPBar", body, new Vector2(0, 1f), new Vector2(1, 1f), new Vector2(0, -130), new Vector2(-60, 16));
            xpBg.pivot = new Vector2(0.5f, 1f); xpBg.offsetMin = new Vector2(60, xpBg.offsetMin.y); // inset left edge under avatar
            var xpBgImg = xpBg.gameObject.AddComponent<Image>(); xpBgImg.sprite = UiTex.VGradient(UiTheme.ChannelDark, TrackDark, 16); xpBgImg.raycastTarget = false;
            var xpFillRt = UiWidgets.Rect("Fill", xpBg, new Vector2(0, 0), new Vector2(0.64f, 1), Vector2.zero, Vector2.zero); // fill ratio (interpretive)
            var xpFill = xpFillRt.gameObject.AddComponent<Image>(); xpFill.sprite = UiTex.VGradient(UiTheme.GoldFillHi, UiTheme.GoldFillLo, 16); xpFill.raycastTarget = false;
            // XP value label sits just below the bar's right end.
            UiWidgets.Label(body, "25600", 22, new Vector2(1, 1f), new Vector2(1, 1f), new Vector2(-8, -156), new Vector2(160, 28), TextAnchor.MiddleRight, XpCol);

            // Blue Change Name / Link Account (display-only → would open rename/link flows, §H/K stubs).
            BlueButton(body, "Change Name",  new Vector2(0, 0f), new Vector2(0.48f, 0f), new Vector2(0, 36), new Vector2(0, 56), () => Router.Toast("Change Name — account flow pending"));
            BlueButton(body, "Link Account", new Vector2(0.52f, 0f), new Vector2(1f, 0f), new Vector2(0, 36), new Vector2(0, 56), () => Router.Toast("Link Account — account flow pending"));
        }

        private void BlueButton(RectTransform parent, string label, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("Btn_" + label, typeof(RectTransform));
            var rt = (RectTransform)go.transform; rt.SetParent(parent, false);
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = new Vector2(0.5f, 0f);
            rt.offsetMin = new Vector2(0, pos.y); rt.offsetMax = new Vector2(0, pos.y + size.y);
            var img = go.AddComponent<Image>(); img.sprite = UiTex.VGradient(UiTheme.IronBlueHi, UiTheme.IronBlue, 32);
            var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
            var cb = btn.colors; cb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); cb.pressedColor = new Color(0.82f, 0.82f, 0.86f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); onClick?.Invoke(); });
            var rim = UiWidgets.Rect("Rim", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rimImg = rim.gameObject.AddComponent<Image>(); rimImg.sprite = UiTex.Frame(UiTheme.A(UiTheme.IronBlueHi, 0.8f), UiTheme.IronBlue, Hex("#11244f"), 48, 5); rimImg.type = Image.Type.Sliced; rimImg.raycastTarget = false;
            var lbl = UiWidgets.Label(rt, label, 24, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Color.white);
            lbl.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.7f);
        }

        // ===== OTHER panel: Vibration/Push/Battery toggles (display-only) + decorative faction emblem =====
        private void BuildOtherPanel(RectTransform content, float x0, float x1, float y0, float y1)
        {
            var body = Panel(content, x0, x1, y0, y1, "OTHER", out _);
            // Toggle rows live in the upper area; the faction emblem decorates the lower-right (so they never collide).
            ToggleRow(body, "Vibration",          0.10f, true); // ON
            ToggleRow(body, "Push Notifications", 0.30f, true); // ON
            ToggleRow(body, "Battery Saver Mode", 0.50f, true); // ON

            // FactionEmblem (crossed-swords shield) — decorative only, raycast off (§D/G).
            var emblem = UiWidgets.Rect("FactionEmblem", body, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-78, 78), new Vector2(132, 132));
            UiWidgets.Glow(emblem, UiTheme.A(UiTheme.IronBlueHi, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(170, 170), 1.6f);
            var shield = emblem.gameObject.AddComponent<Image>(); shield.sprite = UiTex.VGradient(UiTheme.IronSteel, Hex("#1a2138"), 64); shield.raycastTarget = false;
            var shRim = UiWidgets.Rect("Rim", emblem, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var shRimImg = shRim.gameObject.AddComponent<Image>(); shRimImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 7); shRimImg.type = Image.Type.Sliced; shRimImg.raycastTarget = false;
            // Crossed swords (two rotated gold diamonds).
            for (int s = -1; s <= 1; s += 2)
            {
                var sword = UiWidgets.Rect("Sword", emblem, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(20, 110));
                var swImg = sword.gameObject.AddComponent<Image>(); swImg.sprite = UiTex.VGradient(UiTheme.GoldHi, UiTheme.Gold, 16); swImg.raycastTarget = false;
                sword.localRotation = Quaternion.Euler(0, 0, 35 * s);
            }
            emblem.gameObject.AddComponent<PulseScale>().period = 3f; // soft focal bloom feel (§J)
        }

        // ===== BottomActionBar: LOGOUT(red) · PRIVACY POLICY · RESET SETTINGS (§D/E) =====
        private void BuildActionBar(RectTransform content)
        {
            var go = new GameObject("BottomActionBar", typeof(RectTransform), typeof(CanvasGroup));
            var bar = (RectTransform)go.transform; bar.SetParent(content, false);
            bar.anchorMin = new Vector2(0, 0); bar.anchorMax = new Vector2(1, 0); bar.pivot = new Vector2(0.5f, 0f);
            bar.anchoredPosition = new Vector2(0, 8); bar.sizeDelta = new Vector2(0, 130);
            _stagger.Add(go.GetComponent<CanvasGroup>());

            // LOGOUT (danger red, leftmost) → DESTRUCTIVE: routes through the existing Confirm modal (§L / spec 37),
            // whose OnConfirm performs the display-only stub (sign-out is account-system work, not done here).
            ActionButton(bar, "LOGOUT",         0.00f, 0.30f, UiTheme.Oxblood, Hex("#fff3ec"),
                () => Confirm("Log Out", "Are you sure you want to log out?", "LOG OUT",
                    () => Router.Toast("Logged out — account flow pending")));
            // PRIVACY POLICY → opens legal/URL (non-destructive stub).
            ActionButton(bar, "PRIVACY POLICY", 0.34f, 0.64f, UiTheme.A(UiTheme.Charcoal, 0.95f), Hex("#e9dcc0"),
                () => Router.Toast("Privacy Policy — link pending"));
            // RESET SETTINGS → DESTRUCTIVE: Confirm gate (§L), then the display-only reset stub.
            ActionButton(bar, "RESET SETTINGS", 0.68f, 0.98f, UiTheme.A(UiTheme.Charcoal, 0.95f), Hex("#e9dcc0"),
                () => Confirm("Reset Settings", "Reset all settings to their defaults?", "RESET",
                    () => Router.Toast("Settings reset — persistence pending")));
        }

        private void ActionButton(RectTransform bar, string label, float x0, float x1, Color body, Color textCol, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("Btn_" + label, typeof(RectTransform));
            var rt = (RectTransform)go.transform; rt.SetParent(bar, false);
            rt.anchorMin = new Vector2(x0, 0.5f); rt.anchorMax = new Vector2(x1, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(0, 64); rt.anchoredPosition = Vector2.zero;
            var img = go.AddComponent<Image>(); img.sprite = UiTex.VGradient(UiWidgets.Lighten(body, 0.22f), UiWidgets.Darken(body, 0.28f), 32);
            var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
            var cb = btn.colors; cb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); cb.pressedColor = new Color(0.82f, 0.82f, 0.86f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); onClick?.Invoke(); });
            var rim = UiWidgets.Rect("Rim", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rimImg = rim.gameObject.AddComponent<Image>(); rimImg.sprite = UiTex.Frame(UiTheme.A(UiTheme.GoldHi, 0.7f), UiTheme.A(UiTheme.Gold, 0.6f), UiTheme.GoldShadow, 48, 5); rimImg.type = Image.Type.Sliced; rimImg.raycastTarget = false;
            var lbl = UiWidgets.Label(rt, UiTheme.Track(label), 24, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, textCol);
            lbl.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.7f);
        }

        // Confirm gate for destructive actions (§L). Configures the existing reusable Confirm modal (spec 37) and
        // pushes it; onConfirm runs the display-only stub. NO file is edited — this only sets its static config.
        private void Confirm(string title, string message, string confirmText, System.Action onConfirm)
        {
            ConfirmModalScreen.Mode = ConfirmModalScreen.Variant.Confirm;
            ConfirmModalScreen.Title = title;
            ConfirmModalScreen.Message = message;
            ConfirmModalScreen.ConfirmText = confirmText;
            ConfirmModalScreen.CancelText = "CANCEL";
            ConfirmModalScreen.CancelOnScrim = true;
            ConfirmModalScreen.OnConfirm = onConfirm;
            Router.Show<ConfirmModalScreen>();
        }

        public override void OnShow()
        {
            AudioManager.Instance?.PlayMenuMusic();
            StartCoroutine(StaggerIn());
        }

        // Panels + action bar fade/scale-in staggered (§I OnShow timeline).
        private IEnumerator StaggerIn()
        {
            for (int i = 0; i < _stagger.Count; i++) { if (_stagger[i] != null) _stagger[i].alpha = 0f; }
            for (int i = 0; i < _stagger.Count; i++)
            {
                var cg = _stagger[i];
                if (cg == null) continue;
                float t = 0f; const float d = 0.18f;
                while (t < d && cg != null)
                {
                    t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d);
                    cg.alpha = k; cg.transform.localScale = Vector3.one * Mathf.Lerp(0.98f, 1f, k);
                    yield return null;
                }
                if (cg != null) { cg.alpha = 1f; cg.transform.localScale = Vector3.one; }
                // ~60 ms between panels.
                float g = 0f; while (g < 0.06f) { g += Time.unscaledDeltaTime; yield return null; }
            }
        }

        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}

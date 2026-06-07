// BULWARK — CONFIRM / TOAST / INSUFFICIENT / NETERR MODAL SHEET (UI Construction Bible · 37). Presentation-only, REMOVABLE.
//
// One reusable utility-overlay class reproducing the 4-in-1 sheet (design/ConfirmModalDesign.png): ①Confirm
// (gold-frame panel, two buttons), ②Toast (non-blocking green-glow pill, auto-dismiss), ③Insufficient Gems
// (shield crest + violet deficit gem + BUY MORE→Store) and ④Connection Lost (shield crest + wifi/red-✕ +
// RETRY w/ in-flight spinner). Centered ornate panel over a raycast-absorbing dim scrim (like PauseModal);
// the Toast is the documented non-blocking exception (root raycast off). §12 PRESENTATION-ONLY: NO ECS / NO
// Unity.Entities / NO gameplay/balance/AI/economy/backend — the modal NEVER mutates a balance; it only invokes
// the caller's server/stub-validated OnConfirm callback and routes (BUY MORE→StoreScreen). Deleting this file
// removes the overlay 100%.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-37 reusable utility overlay: Confirm / Toast / Insufficient-Gems / Connection-Lost.
    /// Configure the statics, then <c>Router.Show&lt;ConfirmModalScreen&gt;()</c>. Presentation-only (§12).</summary>
    public sealed class ConfirmModalScreen : UiScreen
    {
        public enum Variant { Confirm, Toast, Insufficient, NetError }

        // ---- Reusable per-call config (set by the caller BEFORE Router.Show<ConfirmModalScreen>()) ----
        public static Variant Mode = Variant.Confirm;
        public static string Title = "Confirm";
        public static string Message = "Are you sure?";
        public static string ConfirmText = "CONFIRM";
        public static string CancelText = "CANCEL";
        public static System.Action OnConfirm;       // invoked on confirm (server/stub-validated by the caller)
        public static int DeficitGems = 100;          // ③ deficit value shown beside the big violet gem
        public static bool CancelOnScrim = true;      // ① taps on the scrim dismiss; ③④ require a button (per spec H)
        public static float ToastSeconds = 2.0f;      // ② total lifetime (~2 s, configurable per spec I)

        // ---- Snapshot of the statics taken at Build() (so a later caller's config can't mutate this instance) ----
        private Variant _mode;
        private string _title, _message, _confirmText, _cancelText, _hint;
        private int _deficit;
        private bool _cancelOnScrim;
        private float _toastSeconds;
        private System.Action _onConfirm;

        private CanvasGroup _panelCg;     // ①③④ seat coroutine target
        private CanvasGroup _toastCg;     // ② in/hold/out coroutine target
        private RectTransform _toastBar;  // ② slide target
        private Button _retryBtn;         // ④ disabled while a retry is "in flight"
        private RectTransform _spinner;   // ④ in-flight spinner (Spin)

        // ============================================================================================
        // BUILD
        // ============================================================================================
        protected override void Build()
        {
            // Snapshot the config, then null the one-shot callback so a stale OnConfirm can't leak to the next show.
            _mode = Mode; _title = Title; _message = Message; _confirmText = ConfirmText; _cancelText = CancelText;
            _deficit = Mathf.Max(0, DeficitGems); _cancelOnScrim = CancelOnScrim; _toastSeconds = Mathf.Max(0.6f, ToastSeconds);
            _onConfirm = OnConfirm; OnConfirm = null;

            if (_mode == Variant.Toast) { BuildToast(); return; }

            // ---- Shared scrim (full-bleed, raycast ON → blocks every tap to the screen beneath; bleeds under the cutout) ----
            var scrim = UiWidgets.Stretch("Scrim_Dim", Rect, new Color(0f, 0f, 0f, 0.58f)); // ~58% per spec E
            scrim.raycastTarget = true;
            if (_cancelOnScrim && _mode == Variant.Confirm) // ① only; ③④ require an explicit button (negative rule)
            {
                var sbtn = scrim.gameObject.AddComponent<Button>(); sbtn.transition = Selectable.Transition.None;
                sbtn.onClick.AddListener(Dismiss);
            }

            switch (_mode)
            {
                case Variant.Confirm:      BuildConfirm(); break;
                case Variant.Insufficient: BuildInsufficient(); break;
                case Variant.NetError:     BuildNetError(); break;
            }
        }

        // ----------------------------------------------------------------------------------------- ① CONFIRM
        private void BuildConfirm()
        {
            var field = MakePanel(700f, 432f, Hex("#0c0e14"));

            // Title "CONFIRM" — prestige serif gold bevel (~48 px), #f0d27a.
            UiWidgets.TitleLabel(field, _title, 48, new Vector2(0.5f, 0.80f), new Vector2(0.5f, 0.80f), Vector2.zero,
                new Vector2(620, 80), TextAnchor.MiddleCenter, Hex("#f0d27a"), UiTheme.Gold);

            // Body "Spend 150 [violet gem] gems?" — sentence-case, #d9d2c2, with an inline violet gem glyph.
            BuildBodyWithGemGlyph(field, _message, 0.55f, new Vector2(600, 130));

            // ButtonRow (bottom-centre): CONFIRM (blue, primary, brightest) + CANCEL (outline secondary). Equal size.
            float btnW = 700f * 0.42f, btnH = 96f, gap = 24f, half = btnW * 0.5f + gap * 0.5f, by = 0.22f;
            PrimaryButton(field, _confirmText, new Vector2(0.5f, by), new Vector2(-half, 0), new Vector2(btnW, btnH), Confirm);
            OutlineButton(field, _cancelText, new Vector2(0.5f, by), new Vector2(half, 0), new Vector2(btnW, btnH), Dismiss);

            // Close ✕ (top-right inside the frame) — also dismisses.
            CloseButton(field);
        }

        // ------------------------------------------------------------------------------- ③ INSUFFICIENT GEMS
        private void BuildInsufficient()
        {
            var field = MakePanel(770f, 562f, Hex("#0c0e14"));
            ShieldCrest(field); // amber/bronze shield emblem above title

            // Amber-gold warning title (~44 px), #f0b24a.
            UiWidgets.TitleLabel(field, _title, 44, new Vector2(0.5f, 0.80f), new Vector2(0.5f, 0.80f), Vector2.zero,
                new Vector2(680, 70), TextAnchor.MiddleCenter, Hex("#f0b24a"), Hex("#c98a2e"));

            // Body_Need (~26 px), #d9d2c2, wrapping (2 lines).
            BodyText(field, _message, 26, 0.62f, new Vector2(640, 110), Hex("#d9d2c2"));

            // DeficitCluster: large violet gem (Ø~96) + value "100" (~40 px Black, #e9dcc0) with glow.
            var gemRt = UiWidgets.Rect("Icon_Gem", field, new Vector2(0.5f, 0.46f), new Vector2(0.5f, 0.46f), new Vector2(-70, 0), new Vector2(96, 96));
            UiWidgets.Glow(gemRt, UiTheme.A(UiTheme.AmethystHi, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(170, 170));
            var gem = gemRt.gameObject.AddComponent<Image>(); gem.raycastTarget = false; gem.sprite = UiTex.Diamond(UiTheme.AmethystHi, 48);
            gemRt.gameObject.AddComponent<PulseScale>().period = 2.2f; // soft amethyst bloom pulse (spec J)
            UiWidgets.Label(field, _deficit.ToString(), 40, new Vector2(0.5f, 0.46f), new Vector2(0.5f, 0.46f), new Vector2(40, 0),
                new Vector2(200, 70), TextAnchor.MiddleLeft, Hex("#e9dcc0")).gameObject.AddComponent<Outline>();

            // Body_Hint (~24 px), #b8b0a0.
            BodyText(field, _hint ?? "Purchase more gems to continue.", 24, 0.30f, new Vector2(620, 60), Hex("#b8b0a0"));

            // BUY MORE (blue, primary) → routes to the Store; NEVER auto-deducts (negative rule).
            PrimaryButton(field, _confirmText, new Vector2(0.5f, 0.15f), Vector2.zero, new Vector2(770f * 0.55f, 96f), BuyMore);
            CloseButton(field);
        }

        // ----------------------------------------------------------------------------------- ④ CONNECTION LOST
        private void BuildNetError()
        {
            var field = MakePanel(770f, 540f, Hex("#0c0e14"));
            ShieldCrest(field);

            UiWidgets.TitleLabel(field, _title, 44, new Vector2(0.5f, 0.80f), new Vector2(0.5f, 0.80f), Vector2.zero,
                new Vector2(680, 70), TextAnchor.MiddleCenter, Hex("#f0b24a"), Hex("#c98a2e"));

            BodyText(field, _message, 26, 0.62f, new Vector2(660, 120), Hex("#d9d2c2"));

            // Icon_WifiError: steel wifi arcs + bright red ✕ (#d8452b) overlay + faint danger glow; single entry shake.
            BuildWifiError(field, new Vector2(0.5f, 0.42f));

            // RETRY (blue, primary) with an in-flight spinner; presentation-only re-attempt → dismiss.
            _retryBtn = PrimaryButton(field, _confirmText, new Vector2(0.5f, 0.16f), Vector2.zero, new Vector2(770f * 0.48f, 96f), Retry);
            _spinner = UiWidgets.Rect("Spinner", _retryBtn.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(48, 48));
            var sp = _spinner.gameObject.AddComponent<Image>(); sp.raycastTarget = false;
            sp.sprite = UiTex.Frame(Hex("#eaf2ff"), UiTheme.A(Hex("#eaf2ff"), 0.35f), UiTheme.A(Color.black, 0f), 32, 6); sp.type = Image.Type.Sliced;
            _spinner.gameObject.AddComponent<Spin>().degPerSec = -260f; _spinner.gameObject.SetActive(false);
            CloseButton(field);
        }

        // ----------------------------------------------------------------------------------- ② TOAST (non-blocking)
        private void BuildToast()
        {
            // Root raycast OFF so the toast NEVER blocks input (negative rule). No scrim. Parent on SafeContent
            // so it respects the safe area; pivot top so it slides down from the top band (~0.12 from top).
            var rootGo = new GameObject("ToastRoot", typeof(RectTransform), typeof(CanvasGroup));
            var root = (RectTransform)rootGo.transform; root.SetParent(SafeContent, false);
            root.anchorMin = root.anchorMax = new Vector2(0.5f, 1f); root.pivot = new Vector2(0.5f, 1f);
            root.anchoredPosition = new Vector2(0f, -1080f * 0.12f); root.sizeDelta = new Vector2(640, 120);
            _toastCg = rootGo.GetComponent<CanvasGroup>(); _toastCg.blocksRaycasts = false; _toastCg.interactable = false;

            // ToastBar — rounded gold-rimmed dark pill (#10131a) with a green emissive glow bleeding outward.
            _toastBar = UiWidgets.Rect("ToastBar", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560, 80));
            UiWidgets.Glow(_toastBar, UiTheme.A(Hex("#3fd07a"), 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620, 130));
            var bar = _toastBar.gameObject.AddComponent<Image>(); bar.sprite = UiTex.VGradient(Hex("#161a22"), Hex("#10131a"), 32); bar.raycastTarget = false;
            var rim = UiWidgets.Rect("Rim", _toastBar, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rimg = rim.gameObject.AddComponent<Image>(); rimg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 5); rimg.type = Image.Type.Sliced; rimg.raycastTarget = false;

            // Check medallion (green disc + white ✓) Ø~48, left.
            var med = UiWidgets.Rect("Icon_CheckMedallion", _toastBar, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(46, 0), new Vector2(48, 48));
            var disc = med.gameObject.AddComponent<Image>(); disc.raycastTarget = false; disc.sprite = UiTex.Disc(Hex("#3fd07a"), 48);
            UiWidgets.Label(med, "✓", 36, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Color.white);

            // Toast text (~30 px Bold), #eafff0, left-aligned after the medallion (point-anchored left of the bar).
            UiWidgets.Label(_toastBar, _message, 30, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(96, 0), new Vector2(420, 60), TextAnchor.MiddleLeft, Hex("#eafff0"));
        }

        // ============================================================================================
        // SHARED PANEL CHROME (①③④): ornate frame + corner flourishes + top gem finial
        // ============================================================================================
        private RectTransform MakePanel(float w, float h, Color fill)
        {
            var panelGo = new GameObject("ModalPanel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            var panel = (RectTransform)panelGo.transform; panel.SetParent(SafeContent, false);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f); panel.sizeDelta = new Vector2(w, h); panel.anchoredPosition = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0, 0, 0, 0f); // raycast-true backing so taps on the panel don't fall to the scrim
            _panelCg = panelGo.GetComponent<CanvasGroup>();

            var field = UiWidgets.OrnateFrame(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(w, h), fill, false, 24f);
            CornerFlourishes(panel, w, h);

            // TopGemFinial — gold/blue faceted gem overhanging the top edge (~28 px), with bloom + a glint pulse.
            var gem = UiWidgets.Rect("TopGemFinial", panel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, 0), new Vector2(64, 64));
            UiWidgets.Glow(gem, UiTheme.A(Hex("#6f8bff"), 0.6f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(140, 140));
            var gimg = gem.gameObject.AddComponent<Image>(); gimg.raycastTarget = false; gimg.sprite = UiTex.Diamond(Hex("#6f8bff"), 48);
            gem.gameObject.AddComponent<PulseScale>();
            return field;
        }

        private void CornerFlourishes(Transform panel, float w, float h)
        {
            Vector2[] corners = { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
            foreach (var c in corners)
            {
                var rt = UiWidgets.Rect("CornerFlourish", panel, c, c, new Vector2((c.x < 0.5f ? 1 : -1) * 16, (c.y < 0.5f ? 1 : -1) * 16), new Vector2(46, 46));
                var img = rt.gameObject.AddComponent<Image>(); img.raycastTarget = false; img.sprite = UiTex.Diamond(UiTheme.GoldHi, 48);
            }
        }

        // Amber/bronze shield emblem with a warning glyph + warm glow, overlapping the top frame (③④).
        private void ShieldCrest(Transform field)
        {
            var crest = UiWidgets.Rect("Crest_Shield", field, new Vector2(0.5f, 0.98f), new Vector2(0.5f, 0.98f), new Vector2(0, 18), new Vector2(80, 80));
            UiWidgets.Glow(crest, UiTheme.A(UiTheme.Ember, 0.45f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(150, 150));
            var sh = crest.gameObject.AddComponent<Image>(); sh.raycastTarget = false; sh.sprite = UiTex.VGradient(Hex("#caa04a"), Hex("#8a6a28"), 64);
            // Warning glyph "!" on the shield face.
            UiWidgets.Label(crest, "!", 48, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Hex("#3a2607"));
        }

        // Steel wifi arcs (stacked discs) + a bright red ✕ overlay + faint danger glow; single entry shake.
        private void BuildWifiError(Transform field, Vector2 anchor)
        {
            var grp = UiWidgets.Rect("Icon_WifiError", field, anchor, anchor, Vector2.zero, new Vector2(96, 96));
            UiWidgets.Glow(grp, UiTheme.A(Hex("#d8452b"), 0.35f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(160, 160));
            // Three steel wifi arcs (discs of decreasing size suggest the fan).
            float[] sizes = { 92f, 64f, 36f };
            foreach (var s in sizes)
            {
                var a = UiWidgets.Rect("Arc", grp, new Vector2(0.5f, 0.35f), new Vector2(0.5f, 0.35f), Vector2.zero, new Vector2(s, s));
                var ai = a.gameObject.AddComponent<Image>(); ai.raycastTarget = false; ai.sprite = UiTex.Disc(UiTheme.A(new Color(0.55f, 0.58f, 0.64f), 0.85f), 48);
            }
            // Red ✕ overlay (bright danger).
            UiWidgets.Label(grp, "✕", 64, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(96, 96), TextAnchor.MiddleCenter, Hex("#d8452b"))
                .gameObject.AddComponent<Outline>().effectColor = UiTheme.A(Color.black, 0.7f);
        }

        // ============================================================================================
        // TEXT / BUTTON BUILDERS
        // ============================================================================================
        // ① body with an inline violet gem glyph laid into the text flow (legacy-Text stand-in for the TMP inline
        // sprite per spec L). Splits "... <number> gems ..." and seats a small gem Image between the parts.
        private void BuildBodyWithGemGlyph(Transform field, string msg, float y, Vector2 size)
        {
            int g = msg.IndexOf("gems", System.StringComparison.OrdinalIgnoreCase);
            if (g < 0) { BodyText(field, msg, 28, y, size, Hex("#d9d2c2")); return; }

            string pre = msg.Substring(0, g).TrimEnd();
            string post = msg.Substring(g);
            var holder = UiWidgets.Rect("Body_Prompt", field, new Vector2(0.5f, y), new Vector2(0.5f, y), Vector2.zero, size);
            float third = size.x / 3f;
            var lpart = UiWidgets.Label(holder, pre + " ", 28, new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-12, 0), new Vector2(third, 60), TextAnchor.MiddleRight, Hex("#d9d2c2"));
            var glyph = UiWidgets.Rect("GemGlyph", holder, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(30, 30));
            var gi = glyph.gameObject.AddComponent<Image>(); gi.raycastTarget = false; gi.sprite = UiTex.Diamond(UiTheme.AmethystHi, 32);
            UiWidgets.Label(holder, post, 28, new Vector2(0.5f, 0.5f), new Vector2(1, 0.5f), new Vector2(28, 0), new Vector2(third, 60), TextAnchor.MiddleLeft, Hex("#d9d2c2"));
            // (lpart kept left-of-glyph; the holder spans the full body width so the line reads "Spend 150 [gem] gems?")
            lpart.horizontalOverflow = HorizontalWrapMode.Overflow;
        }

        private Text BodyText(Transform field, string msg, int size, float y, Vector2 sz, Color col)
        {
            var t = UiWidgets.Label(field, msg, size, new Vector2(0.5f, y), new Vector2(0.5f, y), Vector2.zero, sz, TextAnchor.MiddleCenter, col);
            t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        // Primary cobalt CTA (CONFIRM / BUY MORE / RETRY): royal-blue gloss + inner rim + soft pulsing outer glow
        // (the brightest object — secondary must never exceed it).
        private Button PrimaryButton(Transform field, string label, Vector2 anchor, Vector2 pos, Vector2 size, System.Action onClick)
        {
            var rt = UiWidgets.Rect("Btn_" + label, field, anchor, anchor, pos, size);
            var gl = UiWidgets.Glow(rt, UiTheme.A(UiTheme.IronBlueHi, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size * 1.5f);
            var pg = gl.gameObject.AddComponent<PulseGraphic>(); pg.target = gl; pg.min = 0.35f; pg.max = 0.65f; pg.period = 1.6f; // ±10% rim glow, 1.6 s
            var bimg = rt.gameObject.AddComponent<Image>(); bimg.sprite = UiTex.VGradient(UiTheme.IronBlueHi, UiWidgets.Darken(UiTheme.IronBlue, 0.18f), 32);
            var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = bimg;
            var cb = btn.colors; cb.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f); cb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); cb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f); cb.fadeDuration = 0.08f; btn.colors = cb;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); onClick?.Invoke(); });
            var rim = UiWidgets.Rect("Rim", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rimg = rim.gameObject.AddComponent<Image>(); rimg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 6); rimg.type = Image.Type.Sliced; rimg.raycastTarget = false;
            var lbl = UiWidgets.Label(rt, label, 28, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Color.white); // #ffffff on blue
            lbl.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.85f);
            return btn;
        }

        // Secondary CANCEL: transparent dark fill + gold/grey beveled rim, no glow (must stay dimmer than primary).
        private Button OutlineButton(Transform field, string label, Vector2 anchor, Vector2 pos, Vector2 size, System.Action onClick)
        {
            var rt = UiWidgets.Rect("Btn_" + label, field, anchor, anchor, pos, size);
            var bimg = rt.gameObject.AddComponent<Image>(); bimg.sprite = UiTex.VGradient(UiTheme.A(Hex("#1a1d26"), 0.6f), UiTheme.A(Hex("#0c0e14"), 0.6f), 32);
            var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = bimg;
            var cb = btn.colors; cb.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f); cb.pressedColor = new Color(0.82f, 0.82f, 0.86f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); onClick?.Invoke(); });
            var rim = UiWidgets.Rect("Rim", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rimg = rim.gameObject.AddComponent<Image>(); rimg.sprite = UiTex.Frame(UiTheme.A(UiTheme.GoldHi, 0.8f), UiTheme.A(UiTheme.Gold, 0.7f), UiTheme.A(UiTheme.GoldShadow, 0.7f), 48, 6); rimg.type = Image.Type.Sliced; rimg.raycastTarget = false;
            var lbl = UiWidgets.Label(rt, label, 28, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Hex("#cdbf9e")); // gold/grey
            lbl.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.7f);
            return btn;
        }

        // A small close ✕ at the panel's top-right inner corner — Pops (treated as cancel/dismiss).
        private void CloseButton(Transform field)
        {
            var rt = UiWidgets.Rect("Btn_Close", field, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-34, -34), new Vector2(56, 56));
            var img = rt.gameObject.AddComponent<Image>(); img.sprite = UiTex.Disc(UiTheme.A(Hex("#1a1d26"), 0.9f), 48);
            var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = img;
            var cb = btn.colors; cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f); cb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); Dismiss(); });
            UiWidgets.Label(rt, "✕", 30, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, UiTheme.ParchGold);
        }

        // ============================================================================================
        // BEHAVIOUR (events) — the modal NEVER mutates a balance; it only invokes the caller / routes / pops.
        // ============================================================================================
        // DEVICE-VALIDATION FIX (rc 0.0.88): pop THIS modal BEFORE invoking the callback. If OnConfirm navigates
        // (e.g. Pause→Surrender does Router.Clear()+Show<EndScreen>), a Pop AFTER it would pop the freshly-pushed
        // screen and strand the player on a bare battlefield. Snapshot → dismiss self → then invoke.
        private void Confirm() { var cb = _onConfirm; Dismiss(); cb?.Invoke(); }  // ① dismiss self, then caller's callback
        private void BuyMore() { Router.Show<StoreScreen>(); }        // ③ route to Store (no auto-deduct); pop happens via the push covering us
        private void Dismiss() { Router.Pop(); }                      // CANCEL / CLOSE / OK / scrim → pop, no action

        // ④ presentation-only "retry": disable the button + show the spinner, then resolve by dismissing. There is
        // no real network call here (§12) — the caller owns the actual re-attempt; this reproduces the in-flight UX.
        private void Retry()
        {
            if (_retryBtn == null) { Dismiss(); return; }
            if (!_retryBtn.interactable) return; // already in flight
            _retryBtn.interactable = false;
            if (_spinner != null) _spinner.gameObject.SetActive(true);
            StartCoroutine(RetryResolve());
        }

        private IEnumerator RetryResolve()
        {
            float t = 0f; const float d = 0.9f; // brief in-flight beat (unscaled)
            while (t < d) { t += Time.unscaledDeltaTime; yield return null; }
            var cb = _onConfirm; Dismiss(); cb?.Invoke(); // dismiss self first, then resume (callback may navigate)
        }

        // ============================================================================================
        // LIFECYCLE / ANIMATION (UiFx pulses run themselves; these are the per-instance seat coroutines)
        // ============================================================================================
        public override void OnShow()
        {
            AudioManager.Instance?.PlayMenuMusic();
            if (_mode == Variant.Toast) StartCoroutine(ToastLife());
            else StartCoroutine(SeatPanel());
        }

        // ①③④ slide+pop: panel scale 0.92→1 + α 0→1 over ~200 ms (ease-out-back). Mirrors PauseModal.
        private IEnumerator SeatPanel()
        {
            if (_panelCg == null) yield break;
            _panelCg.alpha = 0f; float t = 0f; const float d = 0.2f;
            while (t < d) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d); _panelCg.alpha = k; _panelCg.transform.localScale = Vector3.one * Mathf.Lerp(0.92f, 1f, EaseOutBack(k)); yield return null; }
            _panelCg.alpha = 1f; _panelCg.transform.localScale = Vector3.one;
        }

        // ② in (α0→1 + slide −24→0 + scale 0.96→1, ease-out-back) → hold → out (α1→0 + slide +12 + scale 1→0.98) → pop.
        private IEnumerator ToastLife()
        {
            if (_toastCg == null || _toastBar == null) yield break;
            float baseY = _toastBar.anchoredPosition.y;
            _toastCg.alpha = 0f;
            float t = 0f; const float din = 0.2f;
            while (t < din) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / din); float e = EaseOutBack(k);
                _toastCg.alpha = k; _toastBar.anchoredPosition = new Vector2(_toastBar.anchoredPosition.x, baseY + Mathf.Lerp(-24f, 0f, e)); _toastBar.localScale = Vector3.one * Mathf.Lerp(0.96f, 1f, e); yield return null; }
            _toastCg.alpha = 1f; _toastBar.localScale = Vector3.one; _toastBar.anchoredPosition = new Vector2(_toastBar.anchoredPosition.x, baseY);

            float hold = 0f; float holdDur = Mathf.Max(0.2f, _toastSeconds - din - 0.22f);
            while (hold < holdDur) { hold += Time.unscaledDeltaTime; yield return null; }

            t = 0f; const float dout = 0.22f;
            while (t < dout) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / dout);
                _toastCg.alpha = 1f - k; _toastBar.anchoredPosition = new Vector2(_toastBar.anchoredPosition.x, baseY + Mathf.Lerp(0f, 12f, k)); _toastBar.localScale = Vector3.one * Mathf.Lerp(1f, 0.98f, k); yield return null; }
            Dismiss(); // auto-expire (never persistent)
        }

        private static float EaseOutBack(float x) { const float c1 = 1.70158f, c3 = c1 + 1f; return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f); }
        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}

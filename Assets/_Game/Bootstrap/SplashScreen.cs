// BULWARK — SPLASH SCREEN (UI Implementation · WP-01). Presentation-only, REMOVABLE.
//
// The first landscape screen on the UiRouter shell (reports/ui-production/packages/WP-01... and the frozen
// design/SplashScreenDesign.png). Composition (landscape, safe-area-correct):
//   • full-bleed background key art (keyed "bg_splash", falls back to "bg_menu", then a solid dark color);
//   • BULWARK wordmark + faction line inside an ornate frame region (the mockup frame is empty / logo-ready —
//     final logo art is a PENDING asset, so a styled wordmark stands in; branding must become original BULWARK);
//   • "TAP TO BEGIN" prompt; a full-screen tap-catcher dismisses on tap (with an 8 s safety auto-advance).
// On dismiss it fades out THROUGH the UiRouter, revealing the Main Menu (today the legacy UiFlow menu; when
// WP-04 builds a UiRouter MainMenuScreen this becomes Router.Replace<MainMenuScreen>()).
//
// AUDIO: plays menu music on show + a click on dismiss (AudioManager, null-safe).
// BOUNDARY: NO gameplay interaction, NO ECS access, NO backend dependency — only PlaceholderAssets (sprites),
// AudioManager (audio), UiRouter (navigation), and the PresentationState.RouterOwnsEntry seam (§12).

using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>WP-01 landscape splash on the UiRouter shell. Presentation-only.</summary>
    public sealed class SplashScreen : UiScreen
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            // The router shell owns the entry: suppress the legacy UiFlow splash and show the landscape one.
            PresentationState.RouterOwnsEntry = true;
            UiRouter.Instance.Show<SplashScreen>();
            Debug.Log("[UI] SplashScreen (WP-01) booted via UiRouter.");
        }

        private const float SafetyTimeout = 8f; // auto-advance if the player never taps
        private static readonly Color Dark = new Color(0.06f, 0.07f, 0.10f, 1f);
        private static readonly Color Gold = new Color(1f, 0.85f, 0.30f);

        private Font _font;
        private Image _bg;
        private Text _tapLabel;
        private bool _bgApplied;
        private bool _dismissing;
        private float _elapsed;

        protected override void Build()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Full-bleed background (parented on the full-bleed screen root; extends under a side cutout).
            _bg = StretchImage("Bg", Rect);
            _bg.transform.SetAsFirstSibling(); // render behind SafeContent + tap-catcher
            ApplyBackground();

            // --- content under the safe area ---
            // Ornate frame placeholder behind the wordmark (uses the "panel" placeholder sprite if available).
            var frameGo = new GameObject("Frame", typeof(RectTransform), typeof(Image));
            var frt = (RectTransform)frameGo.transform;
            frt.SetParent(SafeContent, false);
            frt.anchorMin = frt.anchorMax = new Vector2(0.5f, 0.60f);
            frt.sizeDelta = new Vector2(1300, 380);
            frt.anchoredPosition = Vector2.zero;
            var frameImg = frameGo.GetComponent<Image>();
            frameImg.raycastTarget = false;
            var panel = Spr("panel");
            if (panel != null) { frameImg.sprite = panel; frameImg.type = Image.Type.Sliced; frameImg.color = new Color(1f, 1f, 1f, 0.85f); }
            else frameImg.color = new Color(0f, 0f, 0f, 0.30f);

            // BULWARK wordmark (final logo art pending — styled text stands in; mockup frame is empty/logo-ready).
            Label(SafeContent, "BULWARK", 140, new Vector2(0.5f, 0.60f), new Vector2(1200, 200), Color.white);
            Label(SafeContent, "IRON PACT   ·   ASHEN HORDE", 40, new Vector2(0.5f, 0.485f), new Vector2(1200, 70), new Color(1f, 1f, 1f, 0.8f));

            // TAP TO BEGIN prompt (bottom-center).
            _tapLabel = Label(SafeContent, "TAP TO BEGIN", 48, new Vector2(0.5f, 0.12f), new Vector2(1000, 90), Gold);

            // Full-screen transparent tap-catcher (topmost child of the screen root) — tap anywhere to begin.
            var tap = StretchImage("TapCatcher", Rect);
            tap.color = new Color(0f, 0f, 0f, 0f);          // invisible but raycastable
            var btn = tap.gameObject.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.targetGraphic = tap;
            btn.onClick.AddListener(Dismiss);
        }

        public override void OnShow()
        {
            AudioManager.Instance?.PlayMenuMusic(); // audio integration (menu/splash music)
        }

        private void Update()
        {
            // Late-bound placeholder art loads async — swap the bg in once it is ready (one-shot).
            if (!_bgApplied && PlaceholderAssets.Instance != null && PlaceholderAssets.Instance.Ready)
                ApplyBackground();

            // Gentle pulse on the prompt (no tween lib; unscaled — menus run at timeScale 0).
            if (_tapLabel != null)
            {
                var c = _tapLabel.color;
                c.a = 0.55f + 0.45f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 2f));
                _tapLabel.color = c;
            }

            // Safety auto-advance if the player never taps.
            _elapsed += Time.unscaledDeltaTime;
            if (_elapsed >= SafetyTimeout) Dismiss();
        }

        private void Dismiss()
        {
            if (_dismissing) return;
            _dismissing = true;
            AudioManager.Instance?.Click();
            // Boot flow (frozen): Splash → Login → Loading → Main Menu, all on the UiRouter shell.
            Router.Replace<LoginScreen>();
        }

        private void ApplyBackground()
        {
            if (_bg == null) return;
            var s = Spr("bg_splash");
            if (s == null) s = Spr("bg_menu");
            if (s != null) { _bg.sprite = s; _bg.color = Color.white; _bg.preserveAspect = false; }
            else _bg.color = Dark;
            if (PlaceholderAssets.Instance != null && PlaceholderAssets.Instance.Ready) _bgApplied = true;
        }

        // ---- tiny code-built helpers (match UiFlow conventions) ----
        private static Image StretchImage(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            return go.GetComponent<Image>();
        }

        private Text Label(Transform parent, string text, int size, Vector2 anchor, Vector2 sz, Color col)
        {
            var go = new GameObject("Label_" + text, typeof(RectTransform), typeof(Text));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = anchor;
            rt.sizeDelta = sz;
            rt.anchoredPosition = Vector2.zero;
            var t = go.GetComponent<Text>();
            t.font = _font; t.text = text; t.fontSize = size; t.alignment = TextAnchor.MiddleCenter; t.color = col;
            t.horizontalOverflow = HorizontalWrapMode.Overflow; t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false; // the tap-catcher handles input
            var sh = go.AddComponent<Shadow>(); sh.effectColor = new Color(0f, 0f, 0f, 0.8f); sh.effectDistance = new Vector2(2f, -2f);
            return t;
        }

        private static Sprite Spr(string key) =>
            PlaceholderAssets.Instance != null ? PlaceholderAssets.Instance.Get(key) : null;
    }
}

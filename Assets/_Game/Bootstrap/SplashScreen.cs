// BULWARK — SPLASH SCREEN (UI Construction Bible · 02). Presentation-only, REMOVABLE.
//
// Screen #1 of the finalized NO-LOGIN boot chain: Splash → Loading → Main Menu (the splash advances DIRECTLY to
// Loading; there is no login gate). Forensic build of design/SplashScreenDesign.png per the Bible's Sections A–O:
// a full-bleed cinematic vista (cool-left Iron Pact / warm-right Ashen Horde temperature split, god-rays, embers,
// vignette), a centred ornate cast-gold brand plaque (logo-ready field — BULWARK wordmark stands in for pending
// logo art), and one low-centre "TAP TO BEGIN" CTA that floats over the art (no box/pill) and pulses to invite a
// tap. Tap anywhere (or auto-advance) → Loading. Authored matte-painting + ornate frame art are pending (Section
// N); code-built primitives (UiTex gradients/glows/frame) stand in. NO gameplay/ECS/backend (§12).

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-02 landscape splash on the UiRouter shell. Presentation-only.</summary>
    public sealed class SplashScreen : UiScreen
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            PresentationState.RouterOwnsEntry = true; // the router owns the entry; suppress the legacy UiFlow splash
            UiRouter.Instance.Show<SplashScreen>();
            Debug.Log("[UI] SplashScreen (Bible-02) booted via UiRouter.");
        }

        private const float SafetyTimeout = 6f; // auto-advance if the player never taps (Bible I: ≈3–5 s after idle)
        private Image _bg, _ctaGlow;
        private Text _cta;
        private CanvasGroup _plaqueCg, _ctaCg;
        private bool _bgApplied, _dismissing;
        private float _elapsed;

        protected override void Build()
        {
            // ---- BG_Layer (full-bleed, ignores safe area) ----
            _bg = UiLayers.Plate(Rect, "splash", 0.22f); // LAYER 0 clean plate
            _bg.gameObject.AddComponent<KenBurns>(); // slow Ken-Burns push-out 1.06 → 1.00
            ApplyBackground();
            UiLayers.Character(SafeContent, "king", new Vector2(0.17f, 0.42f), Vector2.zero, 640f); // LAYER 1 hero

            // Temperature split: cool steel-blue LEFT (Iron Pact) vs hot ember-orange RIGHT (Ashen) — never symmetric.
            var coolRt = UiWidgets.Rect("Temp_CoolLeft", Rect, Vector2.zero, new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
            coolRt.anchorMin = Vector2.zero; coolRt.anchorMax = new Vector2(0.55f, 1f); coolRt.offsetMin = Vector2.zero; coolRt.offsetMax = Vector2.zero;
            var cool = coolRt.gameObject.AddComponent<Image>(); cool.raycastTarget = false;
            cool.sprite = UiTex.HGradient(UiTheme.A(UiTheme.IronBanner, 0.5f), UiTheme.A(UiTheme.IronBanner, 0f), 64);
            var warmRt = UiWidgets.Rect("Temp_WarmRight", Rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            warmRt.anchorMin = new Vector2(0.45f, 0f); warmRt.anchorMax = Vector2.one; warmRt.offsetMin = Vector2.zero; warmRt.offsetMax = Vector2.zero;
            var warm = warmRt.gameObject.AddComponent<Image>(); warm.raycastTarget = false;
            warm.sprite = UiTex.HGradient(UiTheme.A(UiTheme.Ember2, 0f), UiTheme.A(UiTheme.Oxblood, 0.55f), 64);

            // Warm god-rays from the right horizon (additive-look glow), upper-right.
            UiWidgets.Glow(Rect, UiTheme.A(UiTheme.Ember, 0.5f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-360, -260), new Vector2(1500, 1100), 1.4f);
            // Sparse ember drift over the lower/right battlefield.
            var emberHost = UiWidgets.Rect("Embers", Rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(300, 360), new Vector2(1700, 720));
            emberHost.gameObject.AddComponent<EmberField>().count = 16;
            // Vignette (≈55%) darkens the four corners.
            UiWidgets.Vignette(Rect, 0.55f);

            // ---- BrandPlaque_Group (centred, fy≈0.245 from top → anchorY 0.755) ----
            var plaqueGo = new GameObject("BrandPlaque_Group", typeof(RectTransform), typeof(CanvasGroup));
            var plaqueRt = (RectTransform)plaqueGo.transform; plaqueRt.SetParent(SafeContent, false);
            plaqueRt.anchorMin = plaqueRt.anchorMax = new Vector2(0.5f, 0.755f);
            plaqueRt.sizeDelta = new Vector2(960, 432); plaqueRt.anchoredPosition = Vector2.zero;
            _plaqueCg = plaqueGo.GetComponent<CanvasGroup>();
            UiWidgets.Glow(plaqueRt, UiTheme.A(UiTheme.GoldHi, 0.18f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1180, 640)); // focal bloom
            var field = UiWidgets.OrnateFrame(plaqueRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(960, 432), UiTheme.FieldDark, true, 22f);
            // Brand_Logo — placeholder wordmark (final logo art pending; styled per Section F).
            UiWidgets.TitleLabel(field, "BULWARK", 150, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(740, 240), TextAnchor.MiddleCenter);

            // ---- CTA_Group (bottom-centre, fy≈0.90 from top → anchorY 0.10) ----
            var ctaGo = new GameObject("CTA_Group", typeof(RectTransform), typeof(CanvasGroup));
            var ctaRt = (RectTransform)ctaGo.transform; ctaRt.SetParent(SafeContent, false);
            ctaRt.anchorMin = ctaRt.anchorMax = new Vector2(0.5f, 0.10f);
            ctaRt.sizeDelta = new Vector2(900, 80); ctaRt.anchoredPosition = Vector2.zero;
            _ctaCg = ctaGo.GetComponent<CanvasGroup>();
            _ctaGlow = UiWidgets.Glow(ctaRt, UiTheme.A(UiTheme.Gold, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(640, 150)); // soft glow, not a pill
            // Side flourishes.
            var fl = UiWidgets.Finial(ctaRt, new Vector2(0.5f, 0.5f), new Vector2(-260, 0), 24f);
            var fr = UiWidgets.Finial(ctaRt, new Vector2(0.5f, 0.5f), new Vector2(260, 0), 24f);
            fl.color = UiTheme.Gold; fr.color = UiTheme.Gold;
            _cta = UiWidgets.TitleLabel(ctaRt, "TAP TO BEGIN", 44, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560, 70), TextAnchor.MiddleCenter, UiTheme.ParchGold, UiTheme.Gold);
            _cta.gameObject.AddComponent<PulseGraphic>().target = _cta; // idle "tap me" pulse (Section J)

            // ---- TapCatcher (topmost, full-screen) ----
            var tap = UiWidgets.Stretch("TapCatcher", SafeContent, new Color(0, 0, 0, 0));
            var btn = tap.gameObject.AddComponent<Button>(); btn.transition = Selectable.Transition.None; btn.targetGraphic = tap;
            btn.onClick.AddListener(Dismiss);
        }

        public override void OnShow()
        {
            AudioManager.Instance?.PlayMenuMusic();
            if (_plaqueCg != null) { _plaqueCg.alpha = 0f; }
            if (_ctaCg != null) { _ctaCg.alpha = 0f; }
            StartCoroutine(Entrance());
        }

        // Section I entrance timeline (relative to OnShow), unscaled (menus run at timeScale 0).
        private IEnumerator Entrance()
        {
            // t 0.40 → 1.10: plaque scales in 0.92 → 1.00 with focal bloom.
            yield return Wait(0.40f);
            float t = 0f; const float pd = 0.70f;
            while (t < pd) { t += Time.unscaledDeltaTime; float k = EaseOutBack(Mathf.Clamp01(t / pd)); if (_plaqueCg != null) { _plaqueCg.alpha = Mathf.Clamp01(t / pd); _plaqueCg.transform.localScale = Vector3.one * Mathf.Lerp(0.92f, 1f, k); } yield return null; }
            if (_plaqueCg != null) { _plaqueCg.alpha = 1f; _plaqueCg.transform.localScale = Vector3.one; }
            // t 1.20 → 1.60: CTA fades in then begins idle pulse.
            yield return Wait(0.10f);
            t = 0f; const float cd = 0.40f;
            while (t < cd) { t += Time.unscaledDeltaTime; if (_ctaCg != null) _ctaCg.alpha = Mathf.Clamp01(t / cd); yield return null; }
            if (_ctaCg != null) _ctaCg.alpha = 1f;
        }

        private void Update()
        {
            if (!_bgApplied && PlaceholderAssets.Instance != null && PlaceholderAssets.Instance.Ready) ApplyBackground();
            _elapsed += Time.unscaledDeltaTime;
            if (_elapsed >= SafetyTimeout) Dismiss();
        }

        private void Dismiss()
        {
            if (_dismissing) return;
            _dismissing = true;
            AudioManager.Instance?.Click();
            StartCoroutine(ExitToLoading());
        }

        // Exit: CTA flash → screen fade → push Loading (NO login between — Bible boot flow).
        private IEnumerator ExitToLoading()
        {
            if (_cta != null) { var g = _cta.GetComponent<PulseGraphic>(); if (g != null) g.enabled = false; _cta.color = new Color(1f, 0.96f, 0.86f); }
            yield return Wait(0.08f);
            float t = 0f; const float fd = 0.35f;
            while (t < fd && Group != null) { t += Time.unscaledDeltaTime; Group.alpha = 1f - Mathf.Clamp01(t / fd); yield return null; }
            Router.Replace<LoadingScreen>();
        }

        private void ApplyBackground()
        {
            if (_bg == null) return;
            // LAYER-0 plate is the background now; do NOT override it (that caused the doubling). Mark applied so boot advances.
            _bgApplied = true;
        }

        private static IEnumerator Wait(float s) { float t = 0f; while (t < s) { t += Time.unscaledDeltaTime; yield return null; } }
        private static float EaseOutBack(float x) { const float c1 = 1.70158f, c3 = c1 + 1f; return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f); }
        private static Sprite Spr(string key) => PlaceholderAssets.Instance != null ? PlaceholderAssets.Instance.Get(key) : null;
    }
}

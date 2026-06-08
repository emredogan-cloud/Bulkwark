// BULWARK — LOADING SCREEN (UI Construction Bible · 03). Presentation-only, REMOVABLE.
//
// Boot chain screen #2: Splash → Loading → Main Menu (advances DIRECTLY to the Main Menu; no login between).
// Forensic build of design/LoadingScreenDesign.png per Sections A–O: an "armies clash" cinematic (left-blue Iron
// Pact / right-red Ashen flanks converging on a central burning citadel + fire glow), with a centred UPPERCASE
// gold "LOADING" label, an ornate gold determinate progress bar (recessed obsidian channel + gradient fill +
// sheen + leading-edge tip glow + end-caps), and a tabular gold percentage that always agrees with the fill.
// Non-interactive; auto-advances at 100%. Authored matte-painting art pending (Section N). NO ECS/gameplay (§12).

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-03 landscape loading screen. Presentation-only.</summary>
    public sealed class LoadingScreen : UiScreen
    {
        private const float Duration = 1.6f; // presentation stub fill time (Phase-3 spec: 1.6 s)
        private Image _bg;
        private UiWidgets.BarParts _bar;
        private CountUp _count;
        private float _t;
        private bool _done, _bgApplied;

        protected override void Build()
        {
            // ---- BG_Layer (full-bleed) ----
            _bg = UiLayers.Plate(Rect, "loading", 0.20f); // LAYER 0 clean plate
            _bg.gameObject.AddComponent<KenBurns>().from = 1.05f;
            ApplyBackground();

            // Left-blue / right-red flank framing (symmetric about the centre citadel).
            var lRt = UiWidgets.Rect("Flank_Left", Rect, Vector2.zero, new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
            lRt.anchorMin = Vector2.zero; lRt.anchorMax = new Vector2(0.42f, 1f); lRt.offsetMin = Vector2.zero; lRt.offsetMax = Vector2.zero;
            var l = lRt.gameObject.AddComponent<Image>(); l.raycastTarget = false;
            l.sprite = UiTex.HGradient(UiTheme.A(UiTheme.IronBanner, 0.55f), UiTheme.A(UiTheme.IronBanner, 0f), 64);
            var rRt = UiWidgets.Rect("Flank_Right", Rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            rRt.anchorMin = new Vector2(0.58f, 0f); rRt.anchorMax = Vector2.one; rRt.offsetMin = Vector2.zero; rRt.offsetMax = Vector2.zero;
            var r = rRt.gameObject.AddComponent<Image>(); r.raycastTarget = false;
            r.sprite = UiTex.HGradient(UiTheme.A(UiTheme.AshBanner, 0f), UiTheme.A(UiTheme.AshBanner, 0.55f), 64);

            // Central fire glow at the citadel base (centre, fy≈0.50), warm additive-look.
            UiWidgets.Glow(Rect, UiTheme.A(UiTheme.Ember, 0.6f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(900, 760), 1.5f);
            var emberHost = UiWidgets.Rect("Embers", Rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 380), new Vector2(1200, 720));
            emberHost.gameObject.AddComponent<EmberField>().count = 18;
            UiWidgets.Vignette(Rect, 0.60f); // heaviest boot frame
            // LAYER 1: two stick armies converging on the citadel, placed ABOVE the atmosphere
            UiLayers.Army(Rect, "blue", new Vector2(0f, 0f), new Vector2(0.52f, 0f), 90f, 230f);
            UiLayers.Army(Rect, "red", new Vector2(0.48f, 0f), new Vector2(1f, 0f), 90f, 230f);

            // ---- LoadingHUD_Group (low-centre) ----
            // LOADING label (fy≈0.74 → anchorY 0.26) with flank flourishes.
            UiWidgets.TitleLabel(SafeContent, "LOADING", 50, new Vector2(0.5f, 0.26f), new Vector2(0.5f, 0.26f), Vector2.zero, new Vector2(900, 80), TextAnchor.MiddleCenter);
            var flL = UiWidgets.Finial(SafeContent, new Vector2(0.5f, 0.26f), new Vector2(-300, 0), 22f); flL.color = UiTheme.Gold;
            var flR = UiWidgets.Finial(SafeContent, new Vector2(0.5f, 0.26f), new Vector2(300, 0), 22f); flR.color = UiTheme.Gold;

            // Progress bar (track ≈0.58 W = 1357, vertical centre fy≈0.815 → anchorY 0.185).
            _bar = UiWidgets.GoldBar(SafeContent, new Vector2(0.5f, 0.185f), new Vector2(0.5f, 0.185f), Vector2.zero, new Vector2(1357, 34), 0f);

            // Percentage (fy≈0.90 → anchorY 0.10), tabular gold, agrees with fill.
            var pct = UiWidgets.Label(SafeContent, "0%", 38, new Vector2(0.5f, 0.10f), new Vector2(0.5f, 0.10f), Vector2.zero, new Vector2(400, 56), TextAnchor.MiddleCenter, UiTheme.ParchGold);
            pct.gameObject.AddComponent<UiGradientText>();
            _count = gameObject.AddComponent<CountUp>();
            _count.Bind(pct, v => v + "%", 0f);
        }

        private void Update()
        {
            if (!_bgApplied && PlaceholderAssets.Instance != null && PlaceholderAssets.Instance.Ready) ApplyBackground();
            if (_done) return;
            _t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(_t / Duration);
            if (_bar.Fill != null) { _bar.Fill.fillAmount = p; UiWidgets.UpdateBarTip(_bar); }
            _count?.To(p * 100f, 0.15f);
            if (p >= 1f) { _done = true; StartCoroutine(CompleteThenAdvance()); }
        }

        // 100%: tip-glow flare + soft full-bar bloom, then exit to the Main Menu (Bible default destination).
        private IEnumerator CompleteThenAdvance()
        {
            _count?.Snap(100f);
            if (_bar.Tip != null) { var img = _bar.Tip.GetComponent<Image>(); if (img != null) img.color = UiTheme.A(UiTheme.GoldFillHi, 1f); }
            float t = 0f; const float fd = 0.35f;
            yield return Wait(0.25f); // flare hold
            while (t < fd && Group != null) { t += Time.unscaledDeltaTime; Group.alpha = 1f - Mathf.Clamp01(t / fd); yield return null; }
            Router.Replace<MainMenuScreen>();
        }

        private void ApplyBackground()
        {
            if (_bg == null) return;
            // LAYER-0 plate is the background now; do NOT override it. Mark applied so the loading flow proceeds.
            _bgApplied = true;
        }

        private static IEnumerator Wait(float s) { float t = 0f; while (t < s) { t += Time.unscaledDeltaTime; yield return null; } }
        private static Sprite Spr(string key) => PlaceholderAssets.Instance != null ? PlaceholderAssets.Instance.Get(key) : null;
    }
}

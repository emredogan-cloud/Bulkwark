// BULWARK — LOADING SCREEN (UI Implementation · WP-02). Presentation-only, REMOVABLE.
//
// Landscape loading screen on the UiRouter shell (design/LoadingScreenDesign.png): full-bleed key art +
// LOADING label + progress bar + percent. In the boot flow it sits between Login and the Main Menu; the bar
// fills over a short duration (a presentation stub for "world/assets ready" — there is no gameplay/world
// dependency here while GATE-1 recovery is active), then transitions to the Main Menu. NO ECS/gameplay/backend.

using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>WP-02 landscape loading screen. Presentation-only.</summary>
    public sealed class LoadingScreen : UiScreen
    {
        private const float Duration = 1.6f; // presentation stub fill time (no real load gate under GATE-1)
        private Image _fill;
        private Text _pct;
        private float _t;
        private bool _done;

        protected override void Build()
        {
            UiWidgets.Stretch("Bg", Rect, UiWidgets.Dark, "bg_menu");
            UiWidgets.LabelAt(SafeContent, "LOADING", 56, new Vector2(0.5f, 0.24f), new Vector2(900, 80), TextAnchor.MiddleCenter, Color.white);
            _fill = UiWidgets.ProgressBar(SafeContent, new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.15f), Vector2.zero, new Vector2(1500, 36), UiWidgets.Gold, 0f);
            _pct = UiWidgets.LabelAt(SafeContent, "0%", 34, new Vector2(0.5f, 0.09f), new Vector2(400, 50), TextAnchor.MiddleCenter, new Color(1, 1, 1, 0.85f));
        }

        private void Update()
        {
            if (_done) return;
            _t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(_t / Duration);
            if (_fill != null) _fill.fillAmount = p;
            if (_pct != null) _pct.text = Mathf.RoundToInt(p * 100f) + "%";
            if (p >= 1f) { _done = true; Router.Replace<MainMenuScreen>(); }
        }
    }
}

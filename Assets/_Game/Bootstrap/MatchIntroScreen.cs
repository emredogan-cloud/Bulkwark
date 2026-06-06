// BULWARK — MATCH INTRO / VS (UI Implementation · WP-12). Presentation-only, REMOVABLE.
//
// Landscape pre-battle VS framing on the UiRouter shell (design/MatchIntroDesign.png): mode banner + Iron Pact
// VS Ashen Horde + a tip. Auto-advances (or tap-to-skip) to MatchPresentation.Begin(), which hides the shell
// and starts the existing battle. NO ECS/gameplay/backend.

using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>WP-12 landscape match intro. Presentation-only.</summary>
    public sealed class MatchIntroScreen : UiScreen
    {
        private const float Dwell = 2.2f;
        private float _t;
        private bool _started;

        protected override void Build()
        {
            UiWidgets.Stretch("Bg", Rect, UiWidgets.Dark, "bg_battle");
            UiWidgets.LabelAt(SafeContent, MatchPresentation.Mode.ToUpperInvariant(), 46, new Vector2(0.5f, 0.86f), new Vector2(1000, 70), TextAnchor.MiddleCenter, UiWidgets.Gold);
            UiWidgets.LabelAt(SafeContent, "IRON PACT", 64, new Vector2(0.26f, 0.52f), new Vector2(640, 100), TextAnchor.MiddleCenter, UiWidgets.IronBlue);
            UiWidgets.LabelAt(SafeContent, "VS", 96, new Vector2(0.5f, 0.52f), new Vector2(220, 150), TextAnchor.MiddleCenter, UiWidgets.Gold);
            UiWidgets.LabelAt(SafeContent, "ASHEN HORDE", 64, new Vector2(0.74f, 0.52f), new Vector2(640, 100), TextAnchor.MiddleCenter, UiWidgets.AshRed);
            UiWidgets.LabelAt(SafeContent, "Tip: Upgrade your Units and Commanders to dominate the battlefield.", 26, new Vector2(0.5f, 0.14f), new Vector2(1300, 50), TextAnchor.MiddleCenter, new Color(1, 1, 1, 0.7f));

            // Tap-to-skip (full-bleed, transparent).
            var tap = UiWidgets.Stretch("Tap", Rect, new Color(0, 0, 0, 0));
            var btn = tap.gameObject.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.targetGraphic = tap;
            btn.onClick.AddListener(Go);
        }

        public override void OnShow() => AudioManager.Instance?.PlayBattleMusic();

        private void Update()
        {
            _t += Time.unscaledDeltaTime;
            if (_t >= Dwell) Go();
        }

        private void Go()
        {
            if (_started) return;
            _started = true;
            MatchPresentation.Begin();
        }
    }
}

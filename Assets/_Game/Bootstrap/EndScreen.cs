// BULWARK — END FLOW (Victory / Defeat) (UI Implementation · WP-13). Presentation-only, REMOVABLE.
//
// Landscape end screen on the UiRouter shell (design/VictoryScreenDesign.png + DefeatScreenDesign.png). The
// outcome is the REAL ECS MatchState.Outcome (read read-only by UiFlow, which sets PendingVictory via
// MatchPresentation.OnMatchDecided) — or a presentation-only Surrender (Defeat). Victory shows a reward +
// match-time readout (DISPLAY-ONLY placeholders — the reward economy + match-timer surfacing are not wired
// yet) + CONTINUE; Defeat shows RETRY + CONTINUE. NO ECS/gameplay/backend (outcome is consumed read-only).

using UnityEngine;

namespace Bulwark.Bootstrap
{
    /// <summary>WP-13 landscape Victory/Defeat. Presentation-only (outcome read-only via MatchState).</summary>
    public sealed class EndScreen : UiScreen
    {
        /// <summary>Set by MatchPresentation before this screen is shown (true = Victory).</summary>
        public static bool PendingVictory = true;

        private bool _victory;

        protected override void Build()
        {
            _victory = PendingVictory;
            UiWidgets.Stretch("Bg", Rect, new Color(0f, 0f, 0f, 0.85f), _victory ? "bg_victory" : "bg_defeat");

            UiWidgets.LabelAt(SafeContent, _victory ? "VICTORY" : "DEFEAT", 120, new Vector2(0.5f, 0.74f), new Vector2(1300, 200), TextAnchor.MiddleCenter, _victory ? UiWidgets.Gold : UiWidgets.AshRed);
            UiWidgets.LabelAt(SafeContent, _victory ? "The enemy statue has fallen!" : "Your statue has fallen.", 36, new Vector2(0.5f, 0.60f), new Vector2(1200, 60), TextAnchor.MiddleCenter, Color.white);

            if (_victory)
            {
                // DISPLAY-ONLY reward + time (reward economy & match-timer surfacing are pending; not granted).
                UiWidgets.LabelAt(SafeContent, "Reward:  Gems 1746        Time  05:28", 32, new Vector2(0.5f, 0.48f), new Vector2(1100, 50), TextAnchor.MiddleCenter, UiWidgets.Gem);
                UiWidgets.Panel(SafeContent, new Vector2(0.5f, 0.36f), new Vector2(0.5f, 0.36f), Vector2.zero, new Vector2(220, 190), new Color(0.5f, 0.4f, 0.15f, 0.85f)); // reward chest placeholder
                UiWidgets.Button(SafeContent, "CONTINUE", new Vector2(0.5f, 0.13f), new Vector2(0.5f, 0.13f), Vector2.zero, new Vector2(420, 120), UiWidgets.IronBlue, MatchPresentation.OnContinue, 44);
            }
            else
            {
                UiWidgets.Button(SafeContent, "RETRY",    new Vector2(0.37f, 0.16f), new Vector2(0.37f, 0.16f), Vector2.zero, new Vector2(380, 116), new Color(0.7f, 0.2f, 0.2f), MatchPresentation.OnRetry, 42);
                UiWidgets.Button(SafeContent, "CONTINUE", new Vector2(0.63f, 0.16f), new Vector2(0.63f, 0.16f), Vector2.zero, new Vector2(380, 116), UiWidgets.IronBlue, MatchPresentation.OnContinue, 42);
            }
        }

        public override void OnShow() => AudioManager.Instance?.PlayEndStinger(_victory); // single fire (guarded; reset on match start)
    }
}

// BULWARK — MATCH PRESENTATION (UI Implementation · WP-12/13 bridge). Presentation-only, REMOVABLE.
//
// Bridges the UiRouter shell to the EXISTING battle orchestration (UiFlow + BattleHud + the ECS sim) without
// duplicating or altering it. The legacy UiFlow remains the match orchestrator (it toggles Time.timeScale,
// shows the textured BattleHud via PresentationState.InMatch, and watches the ECS MatchState read-only); this
// class just sequences the shell around it: Mode Select → Match Intro → (hide shell) start battle → on decided
// → shell Victory/Defeat → Continue/Retry. An in-match Pause button is a lightweight shell overlay so the
// BattleHud bindings are left 100% untouched. NO ECS/gameplay/balance/AI change (surrender is a presentation-
// only abandonment; it writes nothing to the sim).

using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Sequences the shell around the existing battle. Presentation-only (WP-12/13).</summary>
    public static class MatchPresentation
    {
        private static string _mode = "Classic";
        private static GameObject _pauseOverlay;

        /// <summary>The mode chosen at Mode Select (display/label only).</summary>
        public static string Mode => _mode;

        /// <summary>Entry from Mode Select: show the VS intro, which calls Begin() when done.</summary>
        public static void StartMatch(string mode)
        {
            _mode = mode;
            UiRouter.Instance.Replace<MatchIntroScreen>();
        }

        /// <summary>Hide the shell so the battlefield + BattleHud are visible, then start the existing battle.</summary>
        internal static void Begin()
        {
            UiRouter.Instance.Clear();
            if (UiFlow.Instance != null) UiFlow.Instance.BeginMatchFromShell(_mode);
            else { Time.timeScale = 1f; PresentationState.InMatch = true; } // fallback if the legacy flow is absent
            EnsurePauseOverlay();
        }

        /// <summary>Called by UiFlow (read-only MatchState watcher) when the match resolves.</summary>
        public static void OnMatchDecided(bool victory)
        {
            UiRouter.Instance.Clear(); // defensive: no stale shell screen should sit under the end screen
            DestroyPauseOverlay();
            Time.timeScale = 0f; PresentationState.InMatch = false; // freeze + hide the in-match HUD
            EndScreen.PendingVictory = victory;
            UiRouter.Instance.Show<EndScreen>();
        }

        /// <summary>Presentation-only surrender (abandon the match → Defeat). Writes nothing to the sim.</summary>
        public static void Surrender()
        {
            DestroyPauseOverlay();
            UiRouter.Instance.Clear();
            Time.timeScale = 0f; PresentationState.InMatch = false;
            EndScreen.PendingVictory = false;
            UiRouter.Instance.Show<EndScreen>();
        }

        /// <summary>Victory/Defeat → CONTINUE: return to the Main Menu.</summary>
        public static void OnContinue()
        {
            DestroyPauseOverlay();
            Time.timeScale = 0f; PresentationState.InMatch = false;
            if (UiFlow.Instance != null) UiFlow.Instance.ReturnToMenuFromShell();
            UiRouter.Instance.Replace<MainMenuScreen>();
        }

        /// <summary>Defeat → RETRY. KNOWN BLOCKER (gameplay, not UI): a true rematch needs the ECS battle WORLD to
        /// reset (MatchState→Ongoing, units/statues respawned). The sim supports ONE battle per launch and exposes
        /// no reset; re-entering a finished battle would immediately re-resolve. Per the binding rules we do NOT
        /// add gameplay, so RETRY degrades to returning to the menu with a notice. See WP-13 report.</summary>
        public static void OnRetry()
        {
            UiRouter.Instance.Toast("Rematch needs a battle reset (gameplay feature pending) — returning to menu");
            OnContinue();
        }

        /// <summary>Open the Pause modal (from the in-match pause overlay button).</summary>
        public static void ShowPause() => UiRouter.Instance.Show<PauseModal>();

        // A safe-area-fitted, persistent in-match Pause button on the shell canvas (NOT a stacked screen, so
        // Clear() during a match does not remove it). BattleHud is left untouched.
        private static void EnsurePauseOverlay()
        {
            if (_pauseOverlay != null) return;
            _pauseOverlay = new GameObject("InMatchOverlay", typeof(RectTransform));
            var rt = (RectTransform)_pauseOverlay.transform;
            rt.SetParent(UiRouter.Instance.transform, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            _pauseOverlay.AddComponent<SafeAreaFitter>();
            UiWidgets.Button(rt, "II", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-80, -80), new Vector2(110, 110), new Color(0.2f, 0.2f, 0.26f, 0.92f), ShowPause, 44);
        }

        private static void DestroyPauseOverlay()
        {
            if (_pauseOverlay != null) { Object.Destroy(_pauseOverlay); _pauseOverlay = null; }
        }
    }
}

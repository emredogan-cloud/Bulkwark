// BULWARK — PAUSE MODAL (UI Implementation · WP-12). Presentation-only, REMOVABLE.
//
// In-match pause on the UiRouter shell (design/PauseModalDesign.png): a dim scrim over the frozen battlefield
// + RESUME / SETTINGS / SURRENDER. Pausing toggles Time.timeScale (a permitted §12 control the legacy debug
// HUD already used — NO gameplay rule change). Surrender is a presentation-only abandonment (→ Defeat), it
// writes nothing to the sim. NO ECS/gameplay/balance change.

using UnityEngine;

namespace Bulwark.Bootstrap
{
    /// <summary>WP-12 in-match pause. Presentation-only.</summary>
    public sealed class PauseModal : UiScreen
    {
        protected override void Build()
        {
            // Dim scrim (the battlefield + HUD render beneath the shell, so they show through).
            UiWidgets.Stretch("Scrim", Rect, new Color(0f, 0f, 0f, 0.6f));

            var panel = UiWidgets.Panel(SafeContent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720, 660), UiWidgets.PanelCol);
            var p = panel.transform;
            UiWidgets.LabelAt(p, "PAUSED", 64, new Vector2(0.5f, 0.82f), new Vector2(660, 100), TextAnchor.MiddleCenter, Color.white);
            UiWidgets.Button(p, "RESUME",    new Vector2(0.5f, 0.60f), new Vector2(0.5f, 0.60f), Vector2.zero, new Vector2(520, 110), UiWidgets.IronBlue,           Resume, 40);
            UiWidgets.Button(p, "SETTINGS",  new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), Vector2.zero, new Vector2(520, 110), UiWidgets.Grey,               () => Router.Show<SettingsScreen>(), 40);
            UiWidgets.Button(p, "SURRENDER", new Vector2(0.5f, 0.24f), new Vector2(0.5f, 0.24f), Vector2.zero, new Vector2(520, 110), new Color(0.7f, 0.2f, 0.2f), Surrender, 40);
        }

        public override void OnShow() => Time.timeScale = 0f; // pause the battle while this modal is up

        private void Resume()
        {
            Time.timeScale = 1f;
            Router.Pop();
        }

        private void Surrender() => MatchPresentation.Surrender();
    }
}

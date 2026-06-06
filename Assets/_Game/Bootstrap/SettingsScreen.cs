// BULWARK — SETTINGS (UI Implementation · WP-10). Presentation-only, REMOVABLE.
//
// Landscape settings on the UiRouter shell (design/SettingsScreenDesign.png): left tab rail, an Audio panel,
// in-session toggles, and Logout/Privacy/Reset. Per the objective, functionality that exists is real (the
// SOUND mute toggle drives AudioManager); functionality without a system yet (volume sliders, graphics,
// account, settings-persistence) is a clearly-labelled PLACEHOLDER (a settings-persistence store is GATE-3).
// NO ECS/gameplay/backend.

using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>WP-10 landscape settings. Presentation-only (mute real; rest placeholder).</summary>
    public sealed class SettingsScreen : UiScreen
    {
        protected override void Build()
        {
            UiWidgets.Stretch("Bg", Rect, UiWidgets.Dark, "bg_menu");
            UiWidgets.BackButton(SafeContent, () => Router.Pop());
            UiWidgets.LabelAt(SafeContent, "SETTINGS", 60, new Vector2(0.5f, 0.93f), new Vector2(900, 90), TextAnchor.MiddleCenter, Color.white);

            // Left tab rail (General active; others placeholder).
            string[] tabs = { "GENERAL", "AUDIO", "GRAPHICS", "CONTROLS", "ACCOUNT", "LANGUAGE", "SUPPORT" };
            for (int i = 0; i < tabs.Length; i++)
            {
                float y = 0.82f - i * 0.10f; bool active = i == 0; string tb = tabs[i];
                UiWidgets.Button(SafeContent, tb, new Vector2(0.12f, y), new Vector2(0.12f, y), Vector2.zero, new Vector2(280, 76),
                    active ? UiWidgets.IronBlue : UiWidgets.Grey, () => { if (!active) Router.Toast(tb + " — coming soon"); }, 28);
            }

            // Panel.
            var panel = UiWidgets.Panel(SafeContent, new Vector2(0.58f, 0.50f), new Vector2(0.58f, 0.50f), Vector2.zero, new Vector2(1120, 740), UiWidgets.PanelCol);
            var p = panel.transform;
            UiWidgets.LabelAt(p, "AUDIO", 36, new Vector2(0.5f, 0.92f), new Vector2(900, 50), TextAnchor.MiddleCenter, UiWidgets.Gold);

            // SOUND mute toggle — REAL (AudioManager).
            bool muted = AudioManager.Instance != null && AudioManager.Instance.Muted;
            var mute = UiWidgets.Button(p, "SOUND: " + (muted ? "OFF" : "ON"), new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), Vector2.zero, new Vector2(560, 90), UiWidgets.IronBlue, null, 34);
            var mt = mute.GetComponentInChildren<Text>();
            mute.onClick.AddListener(() => { AudioManager.Instance?.ToggleMute(); if (mt != null) mt.text = "SOUND: " + (AudioManager.Instance != null && AudioManager.Instance.Muted ? "OFF" : "ON"); });

            // Placeholder volume rows (no volume store yet).
            UiWidgets.LabelAt(p, "Music / SFX / Voice volume — placeholder (settings store pending GATE-3)", 24, new Vector2(0.5f, 0.62f), new Vector2(1000, 40), TextAnchor.MiddleCenter, new Color(1, 1, 1, 0.6f));

            // In-session toggles (placeholder — not persisted).
            Toggle(p, "Vibration", 0.50f);
            Toggle(p, "Push Notifications", 0.40f);
            Toggle(p, "Battery Saver", 0.30f);

            // Bottom actions.
            UiWidgets.Button(p, "LOGOUT", new Vector2(0.25f, 0.07f), new Vector2(0.25f, 0.07f), Vector2.zero, new Vector2(300, 84), new Color(0.7f, 0.2f, 0.2f), () => Router.Toast("Logout — pending account system (GATE-3)"), 30);
            UiWidgets.Button(p, "PRIVACY", new Vector2(0.5f, 0.07f), new Vector2(0.5f, 0.07f), Vector2.zero, new Vector2(300, 84), UiWidgets.Grey, () => Router.Toast("Privacy Policy — link pending"), 30);
            UiWidgets.Button(p, "RESET", new Vector2(0.75f, 0.07f), new Vector2(0.75f, 0.07f), Vector2.zero, new Vector2(300, 84), UiWidgets.Grey, () => Router.Toast("Reset settings — placeholder"), 30);

            UiWidgets.LabelAt(SafeContent, "v0.1.0  (presentation build)", 22, new Vector2(0.98f, 0.02f), new Vector2(520, 30), TextAnchor.MiddleRight, new Color(1, 1, 1, 0.4f));
        }

        public override void OnShow() => AudioManager.Instance?.PlayMenuMusic();

        // A placeholder in-session toggle button (not persisted — needs a settings store).
        private void Toggle(Transform p, string label, float y)
        {
            bool on = true;
            var b = UiWidgets.Button(p, label + ": ON", new Vector2(0.5f, y), new Vector2(0.5f, y), Vector2.zero, new Vector2(560, 72), UiWidgets.Grey, null, 28);
            var t = b.GetComponentInChildren<Text>();
            b.onClick.AddListener(() => { on = !on; if (t != null) t.text = label + ": " + (on ? "ON" : "OFF"); });
        }
    }
}

// BULWARK — COMMANDER SELECT (UI Implementation · WP-09). Presentation-only, REMOVABLE.
//
// Landscape commander select on the UiRouter shell (design/CommanderSelectDesign.png): Iron Pact WARDEN vs
// Ashen WARCHIEF, each with active/passive ability cards + commander level + SELECT. Mirrors canon CommanderDef
// x2 via UiStub (display-only; real CommanderDef/ProgressionService binding is GATE-3). SELECT records the
// display-only choice. NO ECS/gameplay/backend.

using UnityEngine;

namespace Bulwark.Bootstrap
{
    /// <summary>WP-09 landscape commander select. Presentation-only.</summary>
    public sealed class CommanderSelectScreen : UiScreen
    {
        protected override void Build()
        {
            UiWidgets.Stretch("Bg", Rect, UiWidgets.Dark, "bg_menu");
            UiWidgets.BackButton(SafeContent, () => Router.Pop());
            UiWidgets.LabelAt(SafeContent, "SELECT COMMANDER", 60, new Vector2(0.5f, 0.93f), new Vector2(1100, 90), TextAnchor.MiddleCenter, Color.white);

            BuildCommander(0.26f, UiStub.WardenName, UiStub.WardenTitle, UiStub.WardenActive, UiStub.WardenActiveDesc, UiStub.WardenPassive, UiStub.WardenPassiveDesc, UiStub.WardenLevel, UiWidgets.IronBlue);
            BuildCommander(0.74f, UiStub.WarchiefName, UiStub.WarchiefTitle, UiStub.WarchiefActive, UiStub.WarchiefActiveDesc, UiStub.WarchiefPassive, UiStub.WarchiefPassiveDesc, UiStub.WarchiefLevel, UiWidgets.AshRed);

            UiWidgets.LabelAt(SafeContent, "VS", 72, new Vector2(0.5f, 0.50f), new Vector2(160, 110), TextAnchor.MiddleCenter, UiWidgets.Gold);
        }

        public override void OnShow() => AudioManager.Instance?.PlayMenuMusic();

        private void BuildCommander(float fx, string name, string title, string active, string activeDesc, string passive, string passiveDesc, int level, Color tint)
        {
            var card = UiWidgets.Panel(SafeContent, new Vector2(fx, 0.47f), new Vector2(fx, 0.47f), Vector2.zero, new Vector2(720, 780), new Color(0.10f, 0.11f, 0.16f, 0.95f));
            var t = card.transform;
            UiWidgets.LabelAt(t, name, 50, new Vector2(0.5f, 0.93f), new Vector2(680, 70), TextAnchor.MiddleCenter, tint);
            UiWidgets.LabelAt(t, title, 24, new Vector2(0.5f, 0.86f), new Vector2(680, 40), TextAnchor.MiddleCenter, new Color(1, 1, 1, 0.7f));
            UiWidgets.Panel(t, new Vector2(0.5f, 0.67f), new Vector2(0.5f, 0.67f), Vector2.zero, new Vector2(320, 270), new Color(tint.r, tint.g, tint.b, 0.30f)); // portrait placeholder
            UiWidgets.LabelAt(t, "ACTIVE — " + active, 28, new Vector2(0.5f, 0.47f), new Vector2(680, 40), TextAnchor.MiddleCenter, UiWidgets.Gold);
            UiWidgets.LabelAt(t, activeDesc, 22, new Vector2(0.5f, 0.40f), new Vector2(660, 64), TextAnchor.MiddleCenter, new Color(1, 1, 1, 0.85f));
            UiWidgets.LabelAt(t, "PASSIVE — " + passive, 28, new Vector2(0.5f, 0.30f), new Vector2(680, 40), TextAnchor.MiddleCenter, UiWidgets.Gold);
            UiWidgets.LabelAt(t, passiveDesc, 22, new Vector2(0.5f, 0.23f), new Vector2(660, 64), TextAnchor.MiddleCenter, new Color(1, 1, 1, 0.85f));
            UiWidgets.LabelAt(t, "Commander Level " + level, 24, new Vector2(0.5f, 0.14f), new Vector2(680, 40), TextAnchor.MiddleCenter, Color.white);
            string n = name;
            UiWidgets.Button(t, "SELECT", new Vector2(0.5f, 0.05f), new Vector2(0.5f, 0.05f), Vector2.zero, new Vector2(360, 92), tint, () => { UiStub.SelectedCommander = n; Router.Toast(n + " selected"); Router.Pop(); }, 36);
        }
    }
}

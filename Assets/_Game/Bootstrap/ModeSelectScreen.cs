// BULWARK — MODE SELECT (UI Implementation · WP-05). Presentation-only, REMOVABLE.
//
// The landscape 5-card mode select on the UiRouter shell (design/ModScreenDesign.png). Frozen decisions (D2):
// cards = Classic / Campaign / Tournament / Endless / Online Battle (Missions→Campaign, Multiplayer→Online
// relabel). Per the freeze, Classic/Tournament/Endless currently launch the SAME skirmish (mode rules are
// gameplay, deferred) — they hand off to MatchPresentation (which bridges to the existing battle, no gameplay
// change). Campaign (needs the un-built Campaign map) and Online (needs the un-built async matchmaking screen)
// surface "coming soon" rather than dead/invented flows. A Commander button opens the pre-battle Commander
// select. NO ECS/gameplay/balance change (match start is via the existing seam only).

using UnityEngine;

namespace Bulwark.Bootstrap
{
    /// <summary>WP-05 landscape mode select. Presentation-only.</summary>
    public sealed class ModeSelectScreen : UiScreen
    {
        private struct Mode { public string Name; public Color Tint; public bool Playable; }

        protected override void Build()
        {
            UiWidgets.Stretch("Bg", Rect, UiWidgets.Dark, "bg_menu");
            UiWidgets.BackButton(SafeContent, () => Router.Pop());
            UiWidgets.LabelAt(SafeContent, "CHOOSE MODE", 64, new Vector2(0.5f, 0.92f), new Vector2(900, 100), TextAnchor.MiddleCenter, Color.white);

            var modes = new[]
            {
                new Mode{ Name="CLASSIC",    Tint=new Color(0.45f,0.55f,0.30f), Playable=true  },
                new Mode{ Name="CAMPAIGN",   Tint=UiWidgets.IronBlue,           Playable=false },
                new Mode{ Name="TOURNAMENT", Tint=UiWidgets.AshRed,             Playable=true  },
                new Mode{ Name="ENDLESS",    Tint=UiWidgets.Purple,             Playable=true  },
                new Mode{ Name="ONLINE",     Tint=new Color(0.2f,0.4f,0.7f),    Playable=false },
            };

            // Five cards spread horizontally.
            int n = modes.Length;
            for (int i = 0; i < n; i++)
            {
                float fx = (i + 0.5f) / n; // 0.1, 0.3, 0.5, 0.7, 0.9
                var m = modes[i];
                var card = UiWidgets.Panel(SafeContent, new Vector2(fx, 0.50f), new Vector2(fx, 0.50f), Vector2.zero, new Vector2(300, 560), new Color(0.12f, 0.12f, 0.16f, 0.95f));
                UiWidgets.LabelAt(card.transform, m.Name, 40, new Vector2(0.5f, 0.10f), new Vector2(320, 70), TextAnchor.MiddleCenter, Color.white);
                string mode = m.Name; bool playable = m.Playable;
                var btn = UiWidgets.Button(card.transform, playable ? "PLAY" : "SOON", new Vector2(0.5f, 0.06f), new Vector2(0.5f, 0.06f), new Vector2(0, -40), new Vector2(240, 84), m.Tint,
                    () => OnMode(mode, playable), 34);
                // a faux character/art block (placeholder until per-mode art exists)
                UiWidgets.Panel(card.transform, new Vector2(0.5f, 0.58f), new Vector2(0.5f, 0.58f), Vector2.zero, new Vector2(240, 320), new Color(m.Tint.r, m.Tint.g, m.Tint.b, 0.35f));
            }

            // Pre-battle: commander selection.
            UiWidgets.Button(SafeContent, "COMMANDER", new Vector2(1, 0), new Vector2(1, 0), new Vector2(-200, 90), new Vector2(320, 96), UiWidgets.Grey, () => Router.Show<CommanderSelectScreen>(), 34);
        }

        private void OnMode(string mode, bool playable)
        {
            if (!playable)
            {
                Router.Toast(mode + " — coming soon");
                return;
            }
            // Hand off to the existing battle via the presentation seam (no gameplay change).
            MatchPresentation.StartMatch(mode);
        }
    }
}

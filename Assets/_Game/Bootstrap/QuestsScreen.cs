// BULWARK — QUESTS (UI Implementation · WP-08). Presentation-only, REMOVABLE.
//
// Landscape Quests on the UiRouter shell (design/QuestsScreenDesign.png): Daily/Weekly sub-tabs, a list of
// quest rows (icon, title, progress bar, reward chip, CLAIM when complete), and a reset timer. Binds to UiStub
// (display-only; real QuestService binding is GATE-3). Claiming grants the reward to the stub wallet. NO ECS/
// gameplay/backend.

using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>WP-08 landscape Quests. Presentation-only.</summary>
    public sealed class QuestsScreen : UiScreen
    {
        protected override void Build()
        {
            UiWidgets.Stretch("Bg", Rect, UiWidgets.Dark, "bg_menu");
            UiWidgets.BackButton(SafeContent, () => Router.Pop());
            UiWidgets.CurrencyChip(SafeContent, UiWidgets.Gold, UiStub.Gold, 0, out _);
            UiWidgets.CurrencyChip(SafeContent, UiWidgets.Gem, UiStub.Gems, 1, out _);

            UiWidgets.LabelAt(SafeContent, "DAILY QUESTS", 56, new Vector2(0.5f, 0.92f), new Vector2(900, 90), TextAnchor.MiddleCenter, Color.white);
            UiWidgets.Button(SafeContent, "DAILY", new Vector2(0.42f, 0.82f), new Vector2(0.42f, 0.82f), Vector2.zero, new Vector2(220, 70), UiWidgets.Gold, null, 30);
            UiWidgets.Button(SafeContent, "WEEKLY", new Vector2(0.58f, 0.82f), new Vector2(0.58f, 0.82f), Vector2.zero, new Vector2(220, 70), UiWidgets.Grey, () => Router.Toast("Weekly quests — coming soon"), 30);
            UiWidgets.LabelAt(SafeContent, "Resets in 13:46:21", 28, new Vector2(0.85f, 0.82f), new Vector2(300, 40), TextAnchor.MiddleRight, new Color(1, 1, 1, 0.7f));

            var quests = UiStub.DailyQuests;
            for (int i = 0; i < quests.Length; i++)
            {
                var q = quests[i];
                float y = 0.70f - i * 0.135f;
                var row = UiWidgets.Panel(SafeContent, new Vector2(0.5f, y), new Vector2(0.5f, y), Vector2.zero, new Vector2(1600, 130), new Color(0.12f, 0.13f, 0.18f, 0.95f));
                var rt = row.transform;
                UiWidgets.Label(rt, q.Title, 32, new Vector2(0, 0.65f), new Vector2(0, 0.65f), new Vector2(60, 0), new Vector2(700, 50), TextAnchor.MiddleLeft, Color.white);
                float frac = q.Target > 0 ? (float)q.Progress / q.Target : 1f;
                UiWidgets.ProgressBar(rt, new Vector2(0, 0.28f), new Vector2(0, 0.28f), new Vector2(420, 0), new Vector2(760, 26), UiWidgets.IronBlue, frac);
                UiWidgets.Label(rt, q.Progress + " / " + q.Target, 24, new Vector2(0, 0.28f), new Vector2(0, 0.28f), new Vector2(420, 0), new Vector2(760, 30), TextAnchor.MiddleCenter, Color.white);
                UiWidgets.Label(rt, (q.Gem ? "Gems " : "Gold ") + q.Reward, 28, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-360, 0), new Vector2(240, 44), TextAnchor.MiddleRight, q.Gem ? UiWidgets.Gem : UiWidgets.Gold);

                bool complete = q.Progress >= q.Target;
                int idx = i;
                string lbl = q.Claimed ? "CLAIMED" : (complete ? "CLAIM" : "...");
                Color bc = q.Claimed ? new Color(0.2f, 0.3f, 0.2f) : (complete ? new Color(0.2f, 0.6f, 0.25f) : UiWidgets.Grey);
                UiWidgets.Button(rt, lbl, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-130, 0), new Vector2(200, 90), bc, () => Claim(idx), 30);
            }
        }

        public override void OnShow() => AudioManager.Instance?.PlayMenuMusic();

        private void Claim(int i)
        {
            if (i < 0 || i >= UiStub.DailyQuests.Length) return;
            var q = UiStub.DailyQuests[i];
            if (q.Claimed) { Router.Toast("Already claimed"); return; }
            if (q.Progress < q.Target) { Router.Toast("Quest not complete"); return; }
            if (q.Gem) UiStub.GrantGems(q.Reward); else UiStub.GrantGold(q.Reward);
            UiStub.DailyQuests[i].Claimed = true; // struct-array element mutation (display-only)
            Router.Toast("Claimed " + (q.Gem ? "Gems " : "Gold ") + q.Reward);
            Router.Replace<QuestsScreen>(); // refresh the list
        }
    }
}

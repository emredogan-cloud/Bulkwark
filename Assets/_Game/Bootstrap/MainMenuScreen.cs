// BULWARK — MAIN MENU / HUB (UI Implementation · WP-04). Presentation-only, REMOVABLE.
//
// The landscape hub on the UiRouter shell (design/MainMenuDesign.png): currency chips (Gold + Gems — frozen
// model D3, no Energy), BULWARK wordmark, center button stack (PLAY / CAMPAIGN / ONLINE BATTLE / CHESTS /
// STORE), right rail (QUESTS / UNITS / CLAN / LEADERBOARD / SETTINGS) and a bottom feature bar (DAILY / SPIN /
// FREE / EVENTS). Destinations BUILT in this execution sequence route to their screens; destinations NOT in
// this sequence (or DEFERRED/ADR-gated per the freeze) surface a "coming soon" toast rather than dead buttons
// (never invented). Currency values come from UiStub (display-only). NO ECS/gameplay/backend.

using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>WP-04 landscape hub. Presentation-only.</summary>
    public sealed class MainMenuScreen : UiScreen
    {
        private Text _goldText, _gemText;

        protected override void Build()
        {
            UiWidgets.Stretch("Bg", Rect, UiWidgets.Dark, "bg_menu");

            // Currency wallet (top-right): Gold rightmost (index 0), Gems left of it (index 1).
            _goldText = UiWidgets.CurrencyChip(SafeContent, UiWidgets.Gold, UiStub.Gold, 0, out _);
            _gemText = UiWidgets.CurrencyChip(SafeContent, UiWidgets.Gem, UiStub.Gems, 1, out _);

            // Wordmark.
            UiWidgets.LabelAt(SafeContent, "BULWARK", 96, new Vector2(0.5f, 0.90f), new Vector2(1200, 150), TextAnchor.MiddleCenter, Color.white);

            // Center button stack.
            UiWidgets.Button(SafeContent, "PLAY",          new Vector2(0.5f, 0.70f), new Vector2(0.5f, 0.70f), Vector2.zero, new Vector2(720, 120), UiWidgets.Gold,      () => Router.Show<ModeSelectScreen>(), 52);
            UiWidgets.Button(SafeContent, "CAMPAIGN",      new Vector2(0.5f, 0.565f), new Vector2(0.5f, 0.565f), Vector2.zero, new Vector2(640, 104), UiWidgets.IronBlue,  () => Router.Show<ModeSelectScreen>(), 40);
            UiWidgets.Button(SafeContent, "ONLINE BATTLE", new Vector2(0.5f, 0.445f), new Vector2(0.5f, 0.445f), Vector2.zero, new Vector2(640, 104), UiWidgets.IronBlue,  () => Router.Show<ModeSelectScreen>(), 40);
            UiWidgets.Button(SafeContent, "CHESTS",        new Vector2(0.5f, 0.325f), new Vector2(0.5f, 0.325f), Vector2.zero, new Vector2(640, 104), UiWidgets.Purple,    () => Router.Toast("Chests — pending ADR (loot-box decision)"), 40);
            UiWidgets.Button(SafeContent, "STORE",         new Vector2(0.5f, 0.205f), new Vector2(0.5f, 0.205f), Vector2.zero, new Vector2(640, 104), new Color(0.7f,0.2f,0.2f), () => Router.Show<StoreScreen>(), 40);

            // Right rail.
            RailButton("QUESTS",      0.82f, () => Router.Show<QuestsScreen>());
            RailButton("UNITS",       0.66f, () => Router.Toast("Army / Units — coming soon"));
            RailButton("CLAN",        0.50f, () => Router.Toast("Clan — deferred (post-MVP backend)"));
            RailButton("LEADERBOARD", 0.34f, () => Router.Toast("Leaderboard — deferred (post-MVP backend)"));
            RailButton("SETTINGS",    0.18f, () => Router.Show<SettingsScreen>());

            // Bottom feature bar.
            FeatureButton("DAILY",  0.13f, () => Router.Toast("Daily Reward — coming soon"));
            FeatureButton("SPIN",   0.29f, () => Router.Toast("Lucky Spin — pending ADR (gacha decision)"));
            FeatureButton("FREE",   0.45f, () => Router.Toast("Free Rewards — coming soon"));
            FeatureButton("EVENTS", 0.61f, () => Router.Toast("Events — coming soon"));
        }

        public override void OnShow()
        {
            // Refresh wallet (it may have changed in a child screen, e.g. Store).
            if (_goldText != null) _goldText.text = UiStub.Gold.ToString("N0");
            if (_gemText != null) _gemText.text = UiStub.Gems.ToString("N0");
            AudioManager.Instance?.PlayMenuMusic();
        }

        private void RailButton(string caption, float y, UnityEngine.Events.UnityAction onClick)
            => UiWidgets.IconButton(SafeContent, caption, new Vector2(1, y), new Vector2(1, y), new Vector2(-120, 0), new Vector2(120, 120), UiWidgets.Grey, onClick);

        private void FeatureButton(string caption, float x, UnityEngine.Events.UnityAction onClick)
            => UiWidgets.IconButton(SafeContent, caption, new Vector2(x, 0), new Vector2(x, 0), new Vector2(0, 110), new Vector2(150, 130), new Color(0.25f, 0.22f, 0.40f), onClick);
    }
}

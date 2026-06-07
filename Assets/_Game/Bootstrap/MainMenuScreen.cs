// BULWARK — MAIN MENU / HUB (UI Construction Bible · 05). Presentation-only, REMOVABLE.
//
// Root hub after Loading (no login). Forensic build of design/MainMenuDesign.png per Sections A–O: a bright
// daylight kingdom backdrop; top-centre-right BULWARK logo (placeholder wordmark substituted per 00/01) + red
// RISE ribbon; the function-coded primary button column center-right (PLAY gold/orange widest+topmost+glowing →
// CAMPAIGN blue → ONLINE BATTLE green → CHESTS purple → STORE red); Gold+Gems currency pills top-right with the
// green "+"; a five-icon right rail (Quests/Units/Clan/Leaderboard/Settings); and a bottom live-ops row
// (Daily Reward/Lucky Spin/Free Rewards/Events). Currency values come from UiStub (display-only). Routes for
// not-yet-built destinations surface a "coming soon" toast (wired to real screens in the finalization pass).
// NO ECS/gameplay/backend (§12).

using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-05 landscape hub. Presentation-only.</summary>
    public sealed class MainMenuScreen : UiScreen
    {
        private Text _goldText, _gemText;

        protected override void Build()
        {
            // ---- BG_Layer: bright daylight hub (never darken). Placeholder art lifted with a sky gradient. ----
            UiWidgets.Stretch("KeyArt_Base", Rect, UiTheme.Charcoal, "bg_menu");
            var sky = UiWidgets.Rect("Sky", Rect, new Vector2(0, 0.5f), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            sky.anchorMin = new Vector2(0, 0.45f); sky.anchorMax = Vector2.one; sky.offsetMin = Vector2.zero; sky.offsetMax = Vector2.zero;
            var skyImg = sky.gameObject.AddComponent<Image>(); skyImg.raycastTarget = false;
            skyImg.sprite = UiTex.VGradient(UiTheme.A(UiTheme.IronBlueHi, 0.45f), UiTheme.A(UiTheme.IronBlueHi, 0f), 64);
            UiWidgets.Glow(Rect, UiTheme.A(UiTheme.GoldHi, 0.3f), new Vector2(0.2f, 1f), new Vector2(0.2f, 1f), new Vector2(0, -120), new Vector2(1300, 900), 1.5f); // upper-left god-rays
            UiWidgets.Vignette(Rect, 0.32f); // gentle only

            // ---- Currency pills (top-right): Gold rightmost (idx 0), Gems left of it (idx 1) ----
            _goldText = UiWidgets.CurrencyChip(SafeContent, UiTheme.Gold, UiStub.Gold, 0, out _);
            _gemText = UiWidgets.CurrencyChip(SafeContent, UiTheme.AmethystHi, UiStub.Gems, 1, out _);

            // ---- Logo (top-centre-right) + red RISE ribbon ----
            UiWidgets.TitleLabel(SafeContent, "BULWARK", 104, new Vector2(0.61f, 0.87f), new Vector2(0.61f, 0.87f), Vector2.zero, new Vector2(1000, 150), TextAnchor.MiddleCenter);
            var ribbon = UiWidgets.Card(SafeContent, new Vector2(0.61f, 0.785f), new Vector2(0.61f, 0.785f), Vector2.zero, new Vector2(360, 64), UiTheme.A(UiTheme.Oxblood, 0.95f));
            UiWidgets.Label(ribbon, UiTheme.Track("RISE"), 38, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, UiTheme.ParchGold);

            // ---- Primary button column (center-right, right edge fx≈0.84) ----
            PrimaryButton("PLAY",          0.645f, 1324, UiTheme.Ember,    () => Router.Show<ModeSelectScreen>(), 52, true);
            PrimaryButton("CAMPAIGN",      0.510f, 1180, UiTheme.IronBlue, () => Router.Show<CampaignMapScreen>());
            PrimaryButton("ONLINE BATTLE", 0.410f, 1180, new Color(0.11f, 0.54f, 0.23f), () => Router.Show<OnlineBattleScreen>());
            PrimaryButton("CHESTS",        0.310f, 1180, UiTheme.Amethyst, () => Router.Show<ChestsScreen>());
            PrimaryButton("STORE",         0.210f, 1180, UiTheme.Oxblood,  () => Router.Show<StoreScreen>());

            // ---- Right rail (fx≈0.955) ----
            RailTile("QUESTS",      0.78f, () => Router.Show<QuestsScreen>(), true, 3);
            RailTile("UNITS",       0.67f, () => Router.Show<UnitsArmyScreen>());
            RailTile("CLAN",        0.56f, () => Router.Show<ClanScreen>());
            RailTile("LEADERBOARD", 0.45f, () => Router.Show<LeaderboardScreen>());
            RailTile("SETTINGS",    0.34f, () => Router.Show<SettingsScreen>());

            // ---- Bottom live-ops row (fy≈0.90 → anchorY 0.10) ----
            LiveTile("DAILY REWARD", 0.05f, () => Router.Show<DailyRewardScreen>(), true, 1);
            LiveTile("LUCKY SPIN",   0.16f, () => Router.Show<LuckySpinScreen>());
            LiveTile("FREE REWARDS", 0.27f, () => Router.Show<FreeRewardsScreen>());
            LiveTile("EVENTS",       0.38f, () => Router.Show<EventsHubScreen>());
        }

        public override void OnShow()
        {
            if (_goldText != null) _goldText.text = UiStub.Gold.ToString("N0");
            if (_gemText != null) _gemText.text = UiStub.Gems.ToString("N0");
            AudioManager.Instance?.PlayMenuMusic();
        }

        // Right-aligned to fx0.84 (pos.x = -width/2 so the right edge lands on the anchor).
        private void PrimaryButton(string label, float anchorY, float width, Color body, UnityEngine.Events.UnityAction onClick, int fontSize = 44, bool glow = false)
            => UiWidgets.GemButton(SafeContent, label, new Vector2(0.84f, anchorY), new Vector2(0.84f, anchorY), new Vector2(-width * 0.5f, 0), new Vector2(width, 84), body, onClick, fontSize, glow);

        private void RailTile(string caption, float y, UnityEngine.Events.UnityAction onClick, bool badge = false, int count = 0)
            => UiWidgets.IconTile(SafeContent, caption, new Vector2(0.955f, y), new Vector2(0.955f, y), Vector2.zero, 100f, UiWidgets.Grey, onClick, badge, count);

        private void LiveTile(string caption, float x, UnityEngine.Events.UnityAction onClick, bool badge = false, int count = 0)
            => UiWidgets.IconTile(SafeContent, caption, new Vector2(x, 0.10f), new Vector2(x, 0.10f), Vector2.zero, 110f, UiTheme.Amethyst, onClick, badge, count);
    }
}

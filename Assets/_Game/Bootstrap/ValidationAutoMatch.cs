// VALIDATION-ONLY AUTO-MATCH HOOK (presentation, §12, REMOVABLE). Editor/standalone validation aid.
//
// When the env var BULWARK_AUTOMATCH=1 is set, this auto-enters a battle a few seconds after boot, so the
// battlefield presentation (Phase 4 parallax) can be screenshotted on a runtime build WITHOUT simulated input
// (Unity's Linux Input System ignores synthetic X events, and device install is intermittently MIUI-locked).
// It only ever calls the SAME MatchPresentation.StartMatch the CLASSIC button calls — it triggers no new sim
// logic and is OFF unless the env var is set, so normal play / device builds are completely unaffected.
// Optional: BULWARK_BIOME=<grass|ash|snow|volcanic|dead> selects the battlefield biome for the capture.

using System.Collections;
using UnityEngine;

namespace Bulwark.Bootstrap
{
    /// <summary>Env-gated validation hook: auto-starts a match so the battlefield can be captured. Presentation-only.</summary>
    public sealed class ValidationAutoMatch : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            string v = System.Environment.GetEnvironmentVariable("BULWARK_AUTOMATCH");
            string sc = System.Environment.GetEnvironmentVariable("BULWARK_SHOWSCREEN");
            if ((string.IsNullOrEmpty(v) || v == "0") && string.IsNullOrEmpty(sc)) return; // validation-only; off for normal play
            var go = new GameObject("ValidationAutoMatch");
            go.AddComponent<ValidationAutoMatch>();
            DontDestroyOnLoad(go);
            Debug.Log("[AUTOMATCH] enabled via BULWARK_AUTOMATCH.");
        }

        private IEnumerator Start()
        {
            string biome = System.Environment.GetEnvironmentVariable("BULWARK_BIOME");
            if (!string.IsNullOrEmpty(biome)) BattlefieldParallax.Biome = biome;
            yield return new WaitForSecondsRealtime(6f); // let boot → menu settle
            string show = System.Environment.GetEnvironmentVariable("BULWARK_SHOWSCREEN");
            if (!string.IsNullOrEmpty(show)) { Debug.Log("[AUTOSHOW] opening " + show); ShowScreen(show); yield break; }
            Debug.Log("[AUTOMATCH] starting Classic match (biome=" + BattlefieldParallax.Biome + ").");
            MatchPresentation.StartMatch("Classic");      // shows the VS intro
            yield return new WaitForSecondsRealtime(1.5f);
            MatchPresentation.Begin();                    // clear the shell + start the battle (InMatch=true)
            Debug.Log("[AUTOMATCH] battle begun (InMatch should be true).");
        }

        // Validation: open a named meta screen for runtime screenshotting (input simulation can't reach the standalone).
        private static void ShowScreen(string n)
        {
            var r = UiRouter.Instance; if (r == null) return;
            switch (n.ToLowerInvariant())
            {
                case "store": r.Show<StoreScreen>(); break;
                case "leaderboard": r.Show<LeaderboardScreen>(); break;
                case "profile": r.Show<ProfileScreen>(); break;
                case "clan": r.Show<ClanScreen>(); break;
                case "quests": r.Show<QuestsScreen>(); break;
                case "daily": r.Show<DailyRewardScreen>(); break;
                case "tournament": r.Show<TournamentLadderScreen>(); break;
                case "settings": r.Show<SettingsScreen>(); break;
                case "chests": r.Show<ChestsScreen>(); break;
                case "spells": r.Show<SpellsScreen>(); break;
                case "skins": r.Show<SkinsScreen>(); break;
                case "units": r.Show<UnitsArmyScreen>(); break;
                case "events": r.Show<EventsHubScreen>(); break;
                case "chestopen": r.Show<ChestOpenResultScreen>(); break;
                case "reward": r.Show<RewardGrantScreen>(); break;
                case "endless": r.Show<EndlessResultScreen>(); break;
                case "luckyspin": r.Show<LuckySpinScreen>(); break;
                case "freerewards": r.Show<FreeRewardsScreen>(); break;
                case "battlepass": r.Show<BattlePassScreen>(); break;
                case "online": r.Show<OnlineBattleScreen>(); break;
                case "commander": r.Show<CommanderSelectScreen>(); break;
                case "mainmenu": r.Show<MainMenuScreen>(); break;
                case "modeselect": r.Show<ModeSelectScreen>(); break;
                case "campaignmap": r.Show<CampaignMapScreen>(); break;
                default: Debug.LogWarning("[AUTOSHOW] unknown screen " + n); break;
            }
        }
    }
}

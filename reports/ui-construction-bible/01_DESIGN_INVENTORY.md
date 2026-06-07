# BULWARK — UI CONSTRUCTION BIBLE · 01 · DESIGN INVENTORY

**Analysis only.** Date: 2026-06-07. Every image in `/design` (38) is catalogued; **no screen skipped.**
Each has a canonical screen ID, its spec filename, source filename, resolution, aspect ratio, category, and
intended purpose. Mockups are authored at mixed aspect ratios (art-tool artifact) → **all normalize to the
2340×1080 (≈19.5:9) landscape production canvas**; per-screen specs use fraction-based layout.

## Canonical screen table

| # | Spec file | Screen ID | Source `/design` file | Res | Aspect | Category | Purpose |
|---|---|---|---|---|---|---|---|
| 02 | `02_Splash_SPEC.md` | Splash | SplashScreenDesign.png | 1915×821 | 2.33:1 | Boot | Brand title; tap/auto → Loading |
| 03 | `03_Loading_SPEC.md` | Loading | LoadingScreenDesign.png | 1915×821 | 2.33:1 | Boot | Progress + key art; **→ Main Menu directly** |
| 04 | `04_Login_SPEC.md` | Login/Auth | LoginAuthDesign.png | 1672×941 | 1.78:1 | Boot ⚠️**OUT-OF-FLOW** | Guest/social login — **deprecated (no-login constraint)**; spec'd for completeness |
| 05 | `05_MainMenu_SPEC.md` | Main Menu | MainMenuDesign.png | 1915×821 | 2.33:1 | Hub | Root hub: play/campaign/online/chests/store + rail + currencies |
| 06 | `06_ModeSelect_SPEC.md` | Mode Select | ModScreenDesign.png | 1915×821 | 2.33:1 | Mode | Pick a mode (5 cards) |
| 07 | `07_MatchIntro_SPEC.md` | Match Intro | MatchIntroDesign.png | 1536×1024 | 1.5:1 | Mode/Match | Pre-battle VS framing |
| 08 | `08_BattleHud_SPEC.md` | Battle HUD | BattleHudDesign.png | 1672×941 | 1.78:1 | In-Match | In-match HUD: statue HP, gold/pop, train, garrison/defend/attack |
| 09 | `09_InMatchSpellHud_SPEC.md` | In-Match Spell HUD | InMatchSpellHudDesign.png | 1672×941 | 1.78:1 | In-Match | Spell cast slots + cooldown/telegraph |
| 10 | `10_InMatchBanner_SPEC.md` | In-Match Banner | InMatchBannerDesign.png | 1824×862 | 2.12:1 | In-Match | Objective/event/wave banner overlay |
| 11 | `11_Pause_SPEC.md` | Pause | PauseModalDesign.png | 1782×883 | 2.02:1 | In-Match (modal) | Resume/Settings/Surrender |
| 12 | `12_Victory_SPEC.md` | Victory | VictoryScreenDesign.png | 1908×824 | 2.32:1 | Result | Win: reward + time + continue |
| 13 | `13_Defeat_SPEC.md` | Defeat | DefeatScreenDesign.png | 1915×821 | 2.33:1 | Result | Loss: retry/continue |
| 14 | `14_CampaignResult_SPEC.md` | Campaign Result | CampaignResultDesign.png | 1672×941 | 1.78:1 | Result | Campaign level clear: stars/rewards/next |
| 15 | `15_EndlessResult_SPEC.md` | Endless Result | EndlessResultDesign.png | 1672×941 | 1.78:1 | Result | Endless: waves survived/score |
| 16 | `16_LadderResult_SPEC.md` | Ladder Result | LadderResultDesign.png | 1672×941 | 1.78:1 | Result | Async ladder: rank delta/rewards |
| 17 | `17_Store_SPEC.md` | Store | StoreScreenDesign.png | 1672×941 | 1.78:1 | Shop | Gem packs/bundles/IAP + shop tabs |
| 18 | `18_Spells_SPEC.md` | Spells | SpellsScreenDesign.png | 1914×822 | 2.33:1 | Shop | Spell orbs + detail/buy |
| 19 | `19_Skins_SPEC.md` | Skins | SkinsScreenDesign.png | 1910×823 | 2.32:1 | Shop | Cosmetic sets + equip (⚠️stat-modifier ADR) |
| 20 | `20_Chests_SPEC.md` | Chests | ChestsScreenDesign.png | 1914×822 | 2.33:1 | Shop | Chest slots/timers/open (⚠️loot-box ADR) |
| 21 | `21_ChestOpenResult_SPEC.md` | Chest Open Result | ChestOpenResultDesign.png | 1536×1024 | 1.5:1 | Shop/Reward | Loot reveal (⚠️gacha ADR) |
| 22 | `22_UnitsArmy_SPEC.md` | Units / Army | UnitsArmyDesign.png | 1672×941 | 1.78:1 | Meta | Unit collection + upgrade |
| 23 | `23_CommanderSelect_SPEC.md` | Commander Select | CommanderSelectDesign.png | 1672×941 | 1.78:1 | Meta | Warden vs Warchief abilities/select |
| 24 | `24_Profile_SPEC.md` | Profile | ProfileScreenDesign.png | 1783×882 | 2.02:1 | Meta | Player profile/stats/equipped |
| 25 | `25_BattlePass_SPEC.md` | Battle Pass | BattlePassDesign.png | 1774×887 | 2.0:1 | Meta/Live-ops | Seasonal tier track free/premium |
| 26 | `26_Quests_SPEC.md` | Quests | QuestsScreenDesign.png | 1754×897 | 1.96:1 | Meta/Live-ops | Daily/weekly objectives + claim |
| 27 | `27_CampaignMap_SPEC.md` | Campaign Map | CampaignMapDesign.png | 1679×937 | 1.79:1 | Meta/Mode | Level-select world map (nodes/stars) |
| 28 | `28_DailyReward_SPEC.md` | Daily Reward | DailyRewardDesign.png | 1536×1024 | 1.5:1 | Live-ops | Login streak calendar |
| 29 | `29_LuckySpin_SPEC.md` | Lucky Spin | LuckySpinDesign.png | 1536×1024 | 1.5:1 | Live-ops | Prize wheel (⚠️gacha ADR) |
| 30 | `30_FreeRewards_SPEC.md` | Free Rewards | FreeRewardsDesign.png | 1536×1024 | 1.5:1 | Live-ops | Opt-in rewarded-ad offers |
| 31 | `31_EventsHub_SPEC.md` | Events Hub | EventsHubDesign.png | 1536×1024 | 1.5:1 | Live-ops | Limited-time events/modifiers |
| 32 | `32_OnlineBattle_SPEC.md` | Online Battle | OnlineBattleDesign.png | 1536×1024 | 1.5:1 | Competitive | Async ghost matchmaking VS |
| 33 | `33_TournamentLadder_SPEC.md` | Tournament Ladder | TournamentLadderDesign.png | 1672×941 | 1.78:1 | Competitive | Async ladder/bracket progression |
| 34 | `34_Leaderboard_SPEC.md` | Leaderboard | LeaderboardScreenDesign.png | 1782×883 | 2.02:1 | Competitive | Global/Friends/Season ranking |
| 35 | `35_Clan_SPEC.md` | Clan | ClanScreenDesign.png | 1829×860 | 2.13:1 | Social | Clan hub: members/chat/war/chest (now REAL — defect fixed) |
| 36 | `36_Settings_SPEC.md` | Settings | SettingsScreenDesign.png | 1915×821 | 2.33:1 | System | Audio/graphics/account/options |
| 37 | `37_ConfirmModal_SPEC.md` | Confirm/Toast/Insufficient/NetErr (sheet) | ConfirmModalDesign.png | 1536×1024 | 1.5:1 | Utility | 4-in-1 reusable modal sheet |
| 38 | `38_RewardGrant_SPEC.md` | Reward Grant | RewardGrantDesign.png | 1536×1024 | 1.5:1 | Utility | "You received" reward popup |
| 39 | `39_NetworkError_SPEC.md` | Network Error | NetworkErrorDesign.png | 1536×1024 | 1.5:1 | Utility | Connection-lost / retry |

## Notes
- **38 screens total** (02–39). The 9 designs added since the prior freeze: BattleHud, InMatchSpellHud,
  InMatchBanner, CampaignMap, CampaignResult, EndlessResult, LadderResult, TournamentLadder, and a
  **regenerated Clan** (now a real clan hub — the earlier Leaderboard-duplicate defect is resolved, verified).
- **`04_Login`** is documented but **excluded from the finalized boot flow** (no-login constraint:
  Splash→Loading→Main Menu directly). Spec'd per the no-skip rule; flagged out-of-flow.
- **ADR-gated content** (Skins stat-modifiers, Chests/ChestOpenResult/Lucky Spin gacha) is spec'd forensically
  here; the ADRs govern *implementation*, not the visual spec.
- Categories drive grouping; the boot/match/result/shop/meta/live-ops/competitive/social/system/utility split
  matches the navigation in `00 §3`.

> Next: per-screen `NN_<Screen>_SPEC.md` (Sections A–O each), then `99_UI_CONSTRUCTION_BIBLE.md` (embeds all).

// BULWARK — Campaign Act 1 controller (Phase 3 §13 3.3, §6, §7 Classic/Campaign, §9).
// WHAT THIS DOES
//   A meta/MonoBehaviour orchestrator (§12) that:
//     1) Loads ONE CampaignLevelDef (authored DATA — ~20 of these make Act 1),
//     2) Builds a BattleLaunchConfig (map, enemy faction/composition from the level's waves, the
//        player's SERVER-OWNED resolved upgrade levels) and LAUNCHES the existing ECS vertical-slice
//        battle through the IBattleLauncher SEAM (the Bootstrap layer reuses BattleBootstrap), then
//     3) AWAITS / WATCHES the launcher for Victory/Defeat (the launcher maps the ECS MatchState
//        outcome to the meta-side BattleOutcome — the modes layer never references the sim),
//     4) On Victory: computes the §9 DIMINISHING first-clear gem reward and GRANTS it (Gems/Silver/
//        PassXP) through the SERVER-AUTHORITATIVE reward seam, and awards a star if the clear was
//        fast enough.
//
// CANON THIS OBEYS
//   • §12 boundary: this is the meta/MonoBehaviour CONTROL layer. It WRITES setup data (a plain
//     BattleLaunchConfig DTO) that the Bootstrap launcher bakes into the ECS world, and READS the
//     launcher's outcome. It runs NO battle simulation, adds NO new sim system, and (after this
//     integration pass) takes NO assembly reference on Unity.Entities / Bulwark.Sim / Bulwark.Bootstrap
//     — only the IBattleLauncher seam. The battle itself is the unchanged Phase-2 ECS slice.
//   • SERVER AUTHORITY over currency (INVIOLABLE, §9/§12): the gem/silver/PassXP grant goes through
//     IModeRewardWallet → module 3.1's canonical IServerEconomy (Assets/_Services/Economy/ServerEconomy.cs;
//     the leveling service lives in Progression.cs). The client NEVER mutates a balance and NEVER
//     writes a raw wallet value — it requests a DELTA and the SERVER derives + commits the balance.
//     playCount is SERVER-OWNED (read via the wallet); the diminishing formula here only computes the
//     REQUEST.
//   • §9 gem rule: firstClearGem = max(EconomyConfig.gemDiminishFloor,
//                                      level.firstClearGemBase − EconomyConfig.gemDiminishDecrement × playCount).
//     Replays still grant the (diminished) Gems + Silver. No gem grant is client-trusted.
//   • CAPPED / no-P2W (§3): rewards are currency only — never a stat/upgrade/cap change. The player's
//     resolved upgrade levels carried to the launcher are SERVER-OWNED + cap-clamped (UpgradesService).
//   • NO save-state logging (§4/§12): never logs/serializes wallet, profile, or match state.
//   • NO monetization (Phase 4): gems are EARNED here only; nothing is purchasable.
//   • §12 RC→SO→literal: reward DEFAULTS live in the EconomyConfig SO; an optional ConfigResolver
//     lets RemoteConfig override the rates live (server-owned tuning).
//
// SCAFFOLD STATUS: authored, NOT compiled (no Unity 6 / Entities / BaaS — ADR-0-001/-2-001).
// DEFERRED (no runtime here): the real battle launch + MatchState read on device (inside the Bootstrap
//   IBattleLauncher impl), the BaaS grant round-trip, the server-owned playCount increment, and any
//   GATE-3 playable validation.

using System.Collections.Generic;
using UnityEngine;
using Bulwark.Data;
using Bulwark.Services.Analytics;
using Bulwark.Services.Config;
using Bulwark.Services.Economy;
using Bulwark.Game.Modes.Economy;

namespace Bulwark.Game.Modes.Campaign
{
    /// <summary>
    /// Place on a GameObject driving a campaign level. Assign the CampaignLevelDef to play; wire the
    /// reward seam + the battle launcher via Initialize(). On Victory it grants the §9 diminishing reward.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CampaignController : MonoBehaviour
    {
        // ----- The level being played (authored DATA; one of ~20 Act-1 levels). -----
        [Header("Campaign level (§13 3.3 / §7 — authored CampaignLevelDef)")]
        [Tooltip("The Act-1 level to load: map id, enemy faction/composition, rewards, star objective.")]
        public CampaignLevelDef level;

        [Header("Economy (§9 — SERVER-OWNED defaults; RC overrides live)")]
        [Tooltip("EconomyConfig SO: gem diminish floor/decrement. SO/literal tier of the RC→SO→literal resolver.")]
        public EconomyConfig economy;

        [Header("Player loadout (§5.5/§6 — content ids only; resolved levels come from the server)")]
        [Tooltip("Player commander content id (CommanderDef.id). Empty ⇒ no commander.")]
        public string playerCommanderId;

        // ----- Injected seams (NOT serialized; wired by the meta bootstrap). -----
        // The reward wallet is SERVER-AUTHORITATIVE (3.1 IServerEconomy). The launcher is the §12 battle
        // bridge (Bootstrap implements it over BattleBootstrap). UpgradesService supplies the SERVER-OWNED
        // resolved upgrade levels for the player roster. The ConfigResolver layers RC over the SO.
        private IModeRewardWallet _wallet;
        private IBattleLauncher _launcher;
        private UpgradesService _upgrades;
        private IAnalytics _analytics;
        private ConfigResolver _config;

        // ----- Run state (in-memory only; the SERVER owns persistence). -----
        private bool _battleRunning;
        private bool _resolved;
        private float _battleStartTime;

        // RC keys (RC→SO→literal). Absent ⇒ the SO/literal default below is used.
        private const string KeyGemFloor = "economy.gemDiminishFloor";
        private const string KeyGemDecrement = "economy.gemDiminishDecrement";

        /// <summary>
        /// Wire the server-authoritative seams + the battle launcher. Called by the meta bootstrap
        /// BEFORE PlayLevel. (Injected, not serialized: the reward path must be a server seam and the
        /// battle bridge must be the IBattleLauncher interface, not an inspector-bound ECS component.)
        /// </summary>
        public void Initialize(IModeRewardWallet wallet, IBattleLauncher launcher,
                               UpgradesService upgrades, IAnalytics analytics, ConfigResolver config = null)
        {
            _wallet = wallet;
            _launcher = launcher;
            _upgrades = upgrades;
            _analytics = analytics;
            _config = config;
        }

        /// <summary>
        /// Build the BattleLaunchConfig from the level DATA + the player's SERVER-OWNED resolved upgrade
        /// levels, then launch the ECS battle through the IBattleLauncher seam. The launcher (Bootstrap
        /// layer) configures BattleBootstrap and bakes the resolved upgrades (C2). async void: a
        /// fire-and-forget launch; Update() also polls the launcher so this is robust to either path.
        /// </summary>
        public async void PlayLevel()
        {
            if (level == null) { Debug.LogError("[CampaignController] 'level' unassigned."); return; }
            if (_launcher == null) { Debug.LogError("[CampaignController] No battle launcher wired — call Initialize()."); return; }

            BattleLaunchConfig config = BuildLaunchConfig();

            _battleRunning = true;
            _resolved = false;
            _battleStartTime = Time.time;

            _analytics?.Emit("campaign_level_started", new Dictionary<string, object>
            {
                { "levelId", level.id ?? "unknown" },
                { "index", level.index },
            });

            // Launch through the seam. The launcher resolves to the player-perspective outcome; we also
            // poll TryGetOutcome in Update() in case the launch result is observed there first.
            BattleOutcome outcome = await _launcher.LaunchAsync(config);
            HandleOutcome(outcome);
        }

        private void Update()
        {
            if (!_battleRunning || _resolved || _launcher == null) return;
            // Poll the seam (no ECS MatchState read here — that lives behind the launcher, §12 boundary).
            if (_launcher.TryGetOutcome(out BattleOutcome outcome) && outcome != BattleOutcome.Ongoing)
                HandleOutcome(outcome);
        }

        private void HandleOutcome(BattleOutcome outcome)
        {
            if (_resolved || outcome == BattleOutcome.Ongoing) return;
            _resolved = true;
            _battleRunning = false;
            float clearSeconds = Time.time - _battleStartTime;

            if (outcome == BattleOutcome.Victory)
                ResolveVictory(clearSeconds);
            else
                ResolveDefeat();
        }

        // ---------------------------------------------------------------- launch config build

        /// <summary>
        /// Translate the authored level DATA into a plain BattleLaunchConfig the launcher bakes into the
        /// ECS world. Enemy faction + per-wave composition + start gold come straight from the level
        /// (the waves index the enemy faction's authored catalog — no invented units/balance, §15). The
        /// player's resolved upgrade levels are pulled from the SERVER-OWNED UpgradesService (cap-clamped).
        /// </summary>
        private BattleLaunchConfig BuildLaunchConfig()
        {
            var config = new BattleLaunchConfig
            {
                MapId = level.mapId,
                EnemyFaction = level.enemyFaction,
                EnemyStartGold = level.enemyStartGold,
                PlayerCommanderId = playerCommanderId,
                EnemyCommanderId = null, // campaign AI commander is the bootstrap's authored default (DEFERRED).
            };

            // Enemy waves (PvE composition) → plain DTO list. Side-relative unit indices index the enemy
            // faction's authored catalog; this controller introduces no units or balance.
            if (level.waves != null)
            {
                for (int i = 0; i < level.waves.Count; i++)
                {
                    EnemyWave w = level.waves[i];
                    config.EnemyWaves.Add(new BattleWave(w.unitIndex, w.count, w.spawnDelay));
                }
            }

            // Player's SERVER-OWNED, cap-clamped resolved upgrade levels (§13 3.2 / C2). The launcher
            // folds these into UnitSpawnStats via UpgradeMath.ResolveUpgradedStat (never over-cap).
            AppendResolvedUpgrades(config.ResolvedUpgrades);
            return config;
        }

        /// <summary>
        /// Pull every (unitId, stat) the SERVER-OWNED UpgradesService knows about at its current level
        /// and add the cap-clamped level as a ResolvedUpgrade. No upgrade is client-trusted: the level
        /// is whatever the server-hydrated cache holds, clamped to the track's maxLevel inside the service.
        /// With no UpgradesService wired the list is empty (the bake uses base stats — never over-cap).
        /// </summary>
        private void AppendResolvedUpgrades(List<ResolvedUpgrade> into)
        {
            if (_upgrades == null) return;
            // The set of tracks is authored in UpgradesConfig; UpgradesService exposes the per-track
            // current (server-owned, cap-clamped) level. We carry every track's level so the launcher can
            // resolve the matching baked unit stat. (Level 0 entries are harmless — base stat unchanged.)
            foreach (UpgradeTrack track in _upgrades.Tracks)
            {
                int lvl = _upgrades.CurrentLevel(track.unitId, track.stat);
                into.Add(new ResolvedUpgrade(track.unitId, track.stat, lvl));
            }
        }

        // ---------------------------------------------------------------- victory / defeat

        /// <summary>
        /// Victory: read the SERVER-OWNED playCount, compute the §9 diminishing gem REQUEST, then GRANT
        /// Gems + Silver + PassXP through the server-authoritative seam (the server validates + confirms),
        /// and award a star if clearSeconds ≤ starClearTimeSeconds. Replays grant the diminished Gems + Silver.
        /// </summary>
        private async void ResolveVictory(float clearSeconds)
        {
            bool star = level.starClearTimeSeconds > 0f && clearSeconds <= level.starClearTimeSeconds;

            _analytics?.Emit("campaign_level_cleared", new Dictionary<string, object>
            {
                { "levelId", level.id ?? "unknown" },
                { "clearSeconds", clearSeconds },
                { "star", star },
            });

            if (_wallet == null)
            {
                // No server seam wired ⇒ NO grant. We never fall back to a local mutation (§9 inviolable).
                Debug.LogWarning("[CampaignController] No reward wallet wired — reward grant DEFERRED (server seam).");
                return;
            }

            // SERVER-OWNED playCount (NEVER counted client-side). Drives the diminishing rule.
            int playCount = await _wallet.GetCampaignPlayCountAsync(level.id);

            // §9 diminishing first-clear gem REQUEST = max(floor, base − dec×playCount). The server
            // re-derives + validates this; the client only requests. Floor/decrement resolve RC→SO→literal.
            int floor = ResolveInt(KeyGemFloor, economy != null ? economy.gemDiminishFloor : 5);
            int decrement = ResolveInt(KeyGemDecrement, economy != null ? economy.gemDiminishDecrement : 5);
            long gemRequest = ComputeDiminishingGems(level.firstClearGemBase, floor, decrement, playCount);

            // Grant Gems (diminishing), then Silver + PassXP (per-clear). Each is server-validated;
            // we update only from the confirmed result. A rejected grant changes nothing (truth = server).
            if (gemRequest > 0)
                await _wallet.GrantAsync(Currency.Gems, gemRequest, "campaign_clear:" + level.id);
            if (level.silverReward > 0)
                await _wallet.GrantAsync(Currency.Silver, level.silverReward, "campaign_clear:" + level.id);
            if (level.passXpReward > 0)
                await _wallet.GrantAsync(Currency.PassXP, level.passXpReward, "campaign_clear:" + level.id);

            // NOTE: incrementing the server-owned playCount + persisting the star/unlock is a SERVER
            // operation (BaaS Cloud Script) — DEFERRED behind IModeRewardWallet/IServerEconomy.
        }

        private void ResolveDefeat()
        {
            _analytics?.Emit("campaign_level_failed", new Dictionary<string, object>
            {
                { "levelId", level.id ?? "unknown" },
            });
            // No reward on defeat. No save-state written. Player may retry (a new PlayLevel call).
        }

        // ---------------------------------------------------------------- §9 reward formula (pure)

        /// <summary>
        /// §9 diminishing-returns first-clear gem rule: max(floor, base − decrement×playCount), clamped ≥ 0.
        /// Pure + deterministic so the client REQUEST matches what the server re-derives. playCount is
        /// the SERVER-OWNED count of prior plays (0 ⇒ true first clear).
        /// </summary>
        public static long ComputeDiminishingGems(int firstClearGemBase, int floor, int decrement, int playCount)
        {
            if (firstClearGemBase <= 0) return 0;            // level grants no gems
            int clampedFloor = floor < 0 ? 0 : floor;
            int dec = decrement < 0 ? 0 : decrement;
            int pc = playCount < 0 ? 0 : playCount;

            long diminished = (long)firstClearGemBase - (long)dec * pc;
            long result = diminished > clampedFloor ? diminished : clampedFloor;
            return result < 0 ? 0 : result;
        }

        // ---------------------------------------------------------------- helpers

        private int ResolveInt(string key, int literalDefault)
        {
            if (_config == null) return literalDefault;
            Resolved<int> r = _config.ResolveInt(key, literalDefault);
            return r.Value;
        }
    }
}

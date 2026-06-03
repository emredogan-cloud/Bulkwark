// BULWARK — Endless (survival) controller (Phase 3 §13 3.3, §5.1, §7 Endless).
// WHAT THIS DOES
//   A meta/MonoBehaviour orchestrator (§12) for the wave-survival mode:
//     1) Reuses the existing ECS battle through the IBattleLauncher SEAM (the Bootstrap layer maps it
//        onto BattleBootstrap) as the combat substrate,
//     2) Runs the AdaptiveDirector (pure C#) to scale each wave's strength from EconomyConfig and
//        ADAPT it within endlessDirectorBand around the player's recent performance (§5.1),
//     3) On each survived wave, GRANTS silverPerEndlessWave through the SERVER-AUTHORITATIVE reward
//        seam; on run end, grants passXpPerBattle and SUBMITS the leaderboard score (a STUB),
//     4) WATCHES the launcher's outcome: a player-statue Defeat ends the run.
//
// CANON THIS OBEYS
//   • §12 boundary: meta/MonoBehaviour CONTROL only — it WRITES wave setup the ECS sim reads (via the
//     launcher seam) and READS the launcher's outcome. No battle simulation here, and (after this
//     integration pass) NO assembly reference on Unity.Entities / Bulwark.Sim / Bulwark.Bootstrap —
//     only IBattleLauncher. The director math lives in the pure-C# AdaptiveDirector.
//   • SERVER AUTHORITY over currency (INVIOLABLE, §9/§12): silver-per-wave + PassXP-per-battle are
//     granted via IModeRewardWallet → module 3.1's canonical IServerEconomy (the client requests a
//     DELTA; the SERVER derives + commits the balance). The client NEVER mutates a balance.
//   • §5.1 curve + band: strength = endlessBaseWaveStrength × (1+endlessGrowthPerWave)^wave, fenced to
//     ±endlessDirectorBand of the base (AdaptiveDirector enforces the fence).
//   • Leaderboard = a STUB (decision-log §3: async ladder uses server STAT-SANITY checks, NOT deterministic
//     replays; no real-time PvP). Submission here only emits the score to the seam; the server validates.
//   • NO save-state logging (§4/§12). NO new currencies/units/mechanics. NO monetization.
//
// SCAFFOLD STATUS: authored, NOT compiled (no Unity 6 / Entities / BaaS — ADR-0-001/-2-001).
// DEFERRED: wave-spawn wiring into the ECS world (the BattleBootstrap endless-wave seam, behind
//   IBattleLauncher.StageEndlessWave), the BaaS grant round-trips, the leaderboard submit round-trip,
//   GATE-3 validation.

using System.Collections.Generic;
using UnityEngine;
using Bulwark.Data;
using Bulwark.Services.Analytics;
using Bulwark.Services.Config;
using Bulwark.Game.Modes.Economy;

namespace Bulwark.Game.Modes.Endless
{
    /// <summary>
    /// Place on a GameObject driving an Endless run. Assign the EconomyConfig; wire the reward seam +
    /// battle launcher via Initialize(). On each survived wave it grants Silver (server-validated) and
    /// on run end grants PassXP + submits the score.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EndlessController : MonoBehaviour
    {
        [Header("Economy + director (§5.1/§9 — SERVER-OWNED defaults; RC overrides live)")]
        [Tooltip("EconomyConfig SO: silverPerEndlessWave, passXpPerBattle, endless curve + band. SO/literal tier.")]
        public EconomyConfig economy;

        [Tooltip("Smoothing window (waves) for the adaptive director's recent-performance rating.")]
        [Min(1)] public int directorRatingWindow = 3;

        [Tooltip("Seconds a wave must be survived before the next is emitted (control pacing; not balance).")]
        [Min(0.1f)] public float secondsPerWave = 20f;

        [Header("Player loadout (§5.1 — faction for the launch config)")]
        public Faction playerFaction = Faction.IronPact;

        // ----- Injected seams (NOT serialized; wired by the meta bootstrap). -----
        private IModeRewardWallet _wallet;
        private IBattleLauncher _launcher;
        private IAnalytics _analytics;
        private IEndlessLeaderboard _leaderboard;
        private ConfigResolver _config;

        // ----- Director + run state (in-memory; the SERVER owns currency/leaderboard truth). -----
        private AdaptiveDirector _director;
        private int _wave;
        private bool _runActive;
        private bool _resolved;
        private float _waveStartTime;

        // RC keys (RC→SO→literal). Absent ⇒ SO/literal default.
        private const string KeySilverPerWave = "economy.silverPerEndlessWave";
        private const string KeyPassXpPerBattle = "economy.passXpPerBattle";
        private const string KeyBaseStrength = "economy.endlessBaseWaveStrength";
        private const string KeyGrowthPerWave = "economy.endlessGrowthPerWave";
        private const string KeyDirectorBand = "economy.endlessDirectorBand";

        /// <summary>
        /// Wire the server-authoritative seams + the battle launcher + the leaderboard stub. Called by the
        /// meta bootstrap BEFORE StartRun. (Injected, not serialized: rewards must route through a server
        /// seam and the battle bridge must be the IBattleLauncher interface, not an ECS component.)
        /// </summary>
        public void Initialize(IModeRewardWallet wallet, IBattleLauncher launcher, IAnalytics analytics,
                               IEndlessLeaderboard leaderboard = null, ConfigResolver config = null)
        {
            _wallet = wallet;
            _launcher = launcher;
            _analytics = analytics;
            _leaderboard = leaderboard ?? new StubEndlessLeaderboard();
            _config = config;
        }

        /// <summary>
        /// Build the AdaptiveDirector from the (RC→SO→literal) curve params and launch the run through the
        /// IBattleLauncher seam. The first wave's strength is staged into the ECS battle via the seam.
        /// </summary>
        public async void StartRun()
        {
            if (_launcher == null) { Debug.LogError("[EndlessController] No battle launcher wired — call Initialize()."); return; }
            if (economy == null) { Debug.LogError("[EndlessController] 'economy' (EconomyConfig) unassigned."); return; }

            float baseStrength = ResolveFloat(KeyBaseStrength, economy.endlessBaseWaveStrength);
            float growth = ResolveFloat(KeyGrowthPerWave, economy.endlessGrowthPerWave);
            float band = ResolveFloat(KeyDirectorBand, economy.endlessDirectorBand);
            _director = new AdaptiveDirector(baseStrength, growth, band, directorRatingWindow);

            _wave = 0;
            _runActive = true;
            _resolved = false;
            _waveStartTime = Time.time;

            _analytics?.Emit("endless_run_started", null);

            // Launch the battle substrate with wave-0 strength staged. The launch resolves only at run
            // END (Defeat) — survived waves are paced by time in Update(); a Defeat from the seam ends it.
            var config = new BattleLaunchConfig
            {
                PlayerFaction = playerFaction,
                EnemyFaction = Faction.AshenHorde, // Endless foe = the authored AI faction (no new content).
                EndlessWaveStrength = _director.WaveStrength(_wave),
            };
            BattleOutcome outcome = await _launcher.LaunchAsync(config);
            if (!_resolved && outcome == BattleOutcome.Defeat)
                EndRun(reason: "defeat");
        }

        private void Update()
        {
            if (!_runActive || _resolved || _launcher == null) return;

            // Defeat (player statue down) ends the run immediately — polled via the seam (no ECS read here).
            if (_launcher.TryGetOutcome(out BattleOutcome outcome) && outcome == BattleOutcome.Defeat)
            {
                EndRun(reason: "defeat");
                return;
            }

            // Otherwise pace waves by time survived. Each elapsed wave window = one survived wave.
            if (Time.time - _waveStartTime >= secondsPerWave)
                CompleteWaveAndAdvance();
        }

        // ---------------------------------------------------------------- wave lifecycle

        /// <summary>
        /// The current wave was survived: grant Silver (server-validated), feed the director the player's
        /// performance read so the next wave adapts, then emit the next (stronger, fenced) wave.
        /// </summary>
        private void CompleteWaveAndAdvance()
        {
            int survived = _wave;

            // Performance read for the director (§5.1 adaptation). We derive a normalized [0,1] rating from
            // how much margin the player had this wave; with no runtime hooks yet we use a neutral 0.5 so the
            // director stays on the authored curve. The real read (statue HP %, army size vs wave) is a
            // DEFERRED ECS query (behind the launcher) — documented, not invented (no balance asserted).
            float performance = ReadWavePerformanceOrNeutral();
            _director.ReportWavePerformance(performance);

            // Silver per survived wave — SERVER-VALIDATED grant (client never mutates a balance).
            int silverPerWave = ResolveInt(KeySilverPerWave, economy.silverPerEndlessWave);
            if (_wallet != null && silverPerWave > 0)
                GrantSilverForWave(silverPerWave, survived);

            _analytics?.Emit("endless_wave_survived", new Dictionary<string, object>
            {
                { "wave", survived },
                { "strength", _director.WaveStrength(survived) },
                { "rating", _director.GetWaveRating(survived) },
            });

            _wave++;
            _waveStartTime = Time.time;
            EmitWave(_wave);
        }

        // async void: fire-and-forget server grant; the run loop does not block on the round-trip.
        private async void GrantSilverForWave(int amount, int wave)
        {
            await _wallet.GrantAsync(Currency.Silver, amount, "endless_wave:" + wave);
        }

        /// <summary>
        /// Stage wave <paramref name="wave"/> for the ECS battle via the launcher seam. The director gives
        /// the strength; the concrete spawn (which/how many enemy units) is applied behind
        /// IBattleLauncher.StageEndlessWave — a DEFERRED BattleBootstrap endless-wave seam. This controller
        /// introduces NO units or balance; it only passes the director's strength scalar to the seam.
        /// </summary>
        private void EmitWave(int wave)
        {
            _launcher?.StageEndlessWave(wave, _director.WaveStrength(wave));
        }

        /// <summary>
        /// Ends the run: grant PassXP-per-battle (server-validated), submit the final wave count to the
        /// leaderboard STUB (server stat-sanity-checked, decision-log §3), and stop the loop.
        /// </summary>
        private async void EndRun(string reason)
        {
            _resolved = true;
            _runActive = false;
            int finalWaves = _wave;

            _analytics?.Emit("endless_run_ended", new Dictionary<string, object>
            {
                { "waves", finalWaves },
                { "reason", reason ?? "unknown" },
            });

            if (_wallet != null)
            {
                int passXp = ResolveInt(KeyPassXpPerBattle, economy.passXpPerBattle);
                if (passXp > 0)
                    await _wallet.GrantAsync(Currency.PassXP, passXp, "endless_run_end");
            }

            // Leaderboard submit = STUB. The server stat-sanity-checks the score (no deterministic replay).
            if (_leaderboard != null)
                await _leaderboard.SubmitScoreAsync(finalWaves);
        }

        // ---------------------------------------------------------------- helpers

        /// <summary>
        /// Normalized [0,1] player-performance read for the director. With no runtime ECS hooks yet this
        /// returns a neutral 0.5 (keeps the wave on the authored curve). The real read — derived from the
        /// player statue's HP fraction and surviving army vs wave strength — is a DEFERRED ECS query
        /// (behind the launcher).
        /// </summary>
        private float ReadWavePerformanceOrNeutral()
        {
            return 0.5f; // neutral until the ECS performance hook lands (DEFERRED); never an invented balance value
        }

        private int ResolveInt(string key, int literalDefault)
        {
            if (_config == null) return literalDefault;
            Resolved<int> r = _config.ResolveInt(key, literalDefault);
            return r.Value;
        }

        private float ResolveFloat(string key, float literalDefault)
        {
            if (_config == null) return literalDefault;
            Resolved<float> r = _config.ResolveFloat(key, literalDefault);
            return r.Value;
        }
    }

    /// <summary>
    /// Endless leaderboard submission seam. Per decision-log §3 the async ladder uses SERVER STAT-SANITY
    /// checks (NOT deterministic replays); this is the client submit point — the server validates the score.
    /// </summary>
    public interface IEndlessLeaderboard
    {
        /// <summary>Submit the final survived-wave count for server-side stat-sanity validation + ranking.</summary>
        System.Threading.Tasks.Task<bool> SubmitScoreAsync(int waves);
    }

    /// <summary>
    /// No-op leaderboard stub: lets the Endless flow be wired/tested without a BaaS leaderboard.
    /// Records nothing persistent and logs NO payload (§4/§12). Replaced by a BaaS adapter in integration.
    /// </summary>
    public sealed class StubEndlessLeaderboard : IEndlessLeaderboard
    {
        public int LastSubmittedWaves { get; private set; } = -1;

        public System.Threading.Tasks.Task<bool> SubmitScoreAsync(int waves)
        {
            LastSubmittedWaves = waves; // in-memory only; the real submit is a DEFERRED server round-trip
            return System.Threading.Tasks.Task.FromResult(true);
        }
    }
}

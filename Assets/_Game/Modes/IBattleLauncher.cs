// BULWARK — META→BATTLE BRIDGE seam (Phase 3 integration §13 3.3, §12 boundary).
//
// WHY THIS FILE EXISTS (integration C1)
//   The mode controllers (Campaign / Endless) must LAUNCH the existing ECS vertical-slice battle and
//   learn its outcome — but §12 forbids the META/MODE layer (Bulwark.Game.Modes) from taking a hard
//   reference on the ECS battle-sim / bootstrap assemblies. So the modes talk ONLY to this narrow
//   INTERFACE (IBattleLauncher). A thin implementation in the Bootstrap layer (Bulwark.Bootstrap,
//   which already references Bulwark.Sim) maps a BattleLaunchConfig onto BattleBootstrap and reports
//   the resolved outcome. The dependency points Bootstrap → Modes (Bootstrap implements the seam),
//   never Modes → Bootstrap, so the assembly graph stays ACYCLIC:
//       Modes → { Bulwark.Services, Bulwark.Data }      (no Unity.Entities / Sim / Bootstrap)
//       Bootstrap → { Bulwark.Sim, Bulwark.Data, Bulwark.Game.Modes }   (implements IBattleLauncher)
//   This mirrors how AsyncLadderController already uses IAsyncBattleLauncher (GhostSnapshot.cs).
//
// INVIOLABLE constraints honored here
//   • SERVER AUTHORITY (§9/§12): this seam carries NO currency and writes NO balance. It only stages
//     a battle and reports an outcome; rewards still flow through IModeRewardWallet → IServerEconomy.
//   • CAPPED progression / no P2W (§3/§15): the per-unit RESOLVED UPGRADE LEVELS carried in the
//     config are the player's SERVER-OWNED, cap-clamped levels (UpgradesService.CurrentLevel). The
//     Bootstrap launcher applies them via Upgrades.ResolveUpgradedStat (cap-clamped); it can NEVER
//     exceed maxLevel. No new units/spells/maps/factions/commanders are introduced by this seam —
//     every id references AUTHORED data the bootstrap already owns.
//   • NO save-state logging (§4/§12): the config + outcome carry no wallet/profile/save payload.
//
// SCAFFOLD STATUS: authored, NOT compiled (no Unity 6 / Entities — ADR-0-001/-2-001). The actual
//   play (battle build + MatchState read on device) is DEFERRED to the Bootstrap launcher; the modes
//   side is fully wired to THIS interface (no undefined type is referenced by a controller).

using System.Collections.Generic;
using System.Threading.Tasks;
using Bulwark.Data;

namespace Bulwark.Game.Modes
{
    /// <summary>
    /// Battle outcome from the META layer's point of view (player perspective). Mirrors the SIM's
    /// <c>Bulwark.Sim.MatchOutcome</c> {Ongoing,Victory,Defeat} but is declared HERE so the meta/mode
    /// module does NOT take a hard reference on the ECS sim assembly (§12 boundary). The Bootstrap
    /// launcher maps MatchOutcome → this enum at battle end.
    /// </summary>
    public enum BattleOutcome : byte
    {
        Ongoing = 0, // battle not finished / not yet reported
        Victory = 1, // player toppled the enemy statue
        Defeat  = 2, // the enemy toppled the player's statue
    }

    /// <summary>
    /// One enemy spawn directive handed to the launcher (PvE composition). Side-relative
    /// <see cref="unitIndex"/> indexes the enemy faction's authored unit catalog — NO units invented.
    /// Mirrors <see cref="Bulwark.Data.EnemyWave"/> shape but is a plain DTO carried across the seam.
    /// </summary>
    public readonly struct BattleWave
    {
        public readonly int UnitIndex;   // index into the enemy faction's authored unit catalog
        public readonly int Count;
        public readonly float SpawnDelay; // seconds after level start / prior wave

        public BattleWave(int unitIndex, int count, float spawnDelay)
        {
            UnitIndex = unitIndex; Count = count; SpawnDelay = spawnDelay;
        }
    }

    /// <summary>
    /// A RESOLVED, cap-clamped upgrade level for one (unitId, stat) track that the SERVER confirmed the
    /// player owns (UpgradesService.CurrentLevel — already clamped to the track's maxLevel). The
    /// Bootstrap launcher folds these into the baked UnitSpawnStats via the pure
    /// <c>UpgradeMath.ResolveUpgradedStat</c>. Because the level is server-owned + cap-clamped, the bake
    /// can never exceed the HARD CAP (§3/§15 — no P2W).
    /// </summary>
    public readonly struct ResolvedUpgrade
    {
        public readonly string UnitId;     // references UnitDef.id
        public readonly UpgradeStat Stat;
        public readonly int Level;          // SERVER-owned, cap-clamped level (0 = no upgrade)

        public ResolvedUpgrade(string unitId, UpgradeStat stat, int level)
        {
            UnitId = unitId; Stat = stat; Level = level;
        }
    }

    /// <summary>
    /// Everything the launcher needs to stand up one battle, as a PLAIN DTO (no UnityEngine / no ECS),
    /// so the modes layer can build it without referencing the sim/bootstrap assemblies. All ids
    /// reference AUTHORED data the Bootstrap layer already owns (maps/units/spells/commanders); this
    /// DTO introduces NO content and NO balance. The player's <see cref="ResolvedUpgrades"/> are the
    /// SERVER-OWNED, cap-clamped levels the launcher bakes into the player roster (§13 3.2 / C2).
    /// </summary>
    public sealed class BattleLaunchConfig
    {
        /// <summary>MapDef.id selecting the battlefield (§11). The launcher resolves it against its catalog.</summary>
        public string MapId;

        // ---- Player side (Team 0) ----
        public Faction PlayerFaction = Faction.IronPact;
        /// <summary>Player's SERVER-OWNED, cap-clamped upgrade levels — baked into UnitSpawnStats (C2).</summary>
        public List<ResolvedUpgrade> ResolvedUpgrades = new List<ResolvedUpgrade>();
        /// <summary>Content ids (SpellDef.id) of the player's drafted spells (≤3). Empty ⇒ none.</summary>
        public List<string> PlayerDraftedSpellIds = new List<string>();
        /// <summary>Player commander content id (CommanderDef.id). Null/empty ⇒ no commander.</summary>
        public string PlayerCommanderId;
        /// <summary>Player commander level (server-owned, cap-clamped by commanderMaxLevel). 0 ⇒ base.</summary>
        public int PlayerCommanderLevel;

        // ---- Enemy side (Team 1) ----
        public Faction EnemyFaction = Faction.AshenHorde;
        /// <summary>Enemy PvE composition (campaign waves). Empty ⇒ the bootstrap's default AI behaviour.</summary>
        public List<BattleWave> EnemyWaves = new List<BattleWave>();
        public int EnemyStartGold = 200;
        /// <summary>Enemy commander content id (CommanderDef.id). Null/empty ⇒ none.</summary>
        public string EnemyCommanderId;
        public int EnemyCommanderLevel;

        // ---- Endless director (optional) ----
        /// <summary>For Endless: the director's strength scalar for the wave being staged (≥0).
        /// Ignored by campaign. The launcher scales the AI wave spawn by it (no balance invented here).</summary>
        public float EndlessWaveStrength;
    }

    /// <summary>
    /// The meta→battle bridge the mode controllers call. A thin Bootstrap-layer implementation
    /// configures BattleBootstrap from the <see cref="BattleLaunchConfig"/> (map / factions / enemy
    /// composition / spell loadouts / commanders), applies the player's RESOLVED upgrade levels to the
    /// baked catalog (C2), starts the battle, and resolves to the player-perspective
    /// <see cref="BattleOutcome"/> when MatchState reports Victory/Defeat. The launch itself is DEFERRED
    /// (no Unity runtime here) — see the documented stub in the Bootstrap layer + integrationNotes.
    /// </summary>
    public interface IBattleLauncher
    {
        /// <summary>
        /// Configure + launch ONE battle from <paramref name="config"/>. PLAYER is Team 0; the enemy /
        /// waves are Team 1. Resolves to the player-perspective outcome when the battle ends. The
        /// implementation NEVER mutates a currency balance (server authority) and NEVER exceeds an
        /// upgrade/commander cap (the resolved levels are already cap-clamped).
        /// </summary>
        Task<BattleOutcome> LaunchAsync(BattleLaunchConfig config);

        /// <summary>
        /// Re-stage the next Endless wave into the running battle at the director's strength
        /// (<paramref name="waveStrength"/>), without rebuilding the world. No-op until the
        /// BattleBootstrap endless-wave seam lands (DEFERRED). Introduces no units/balance — it scales
        /// the AI wave spawn the bootstrap already owns.
        /// </summary>
        void StageEndlessWave(int waveIndex, float waveStrength);

        /// <summary>
        /// True once the launched battle has resolved; <paramref name="outcome"/> carries the
        /// player-perspective result. Polled by the controllers in place of a direct MatchState read
        /// (which would require the ECS sim assembly). Returns false while the battle is Ongoing.
        /// </summary>
        bool TryGetOutcome(out BattleOutcome outcome);
    }
}

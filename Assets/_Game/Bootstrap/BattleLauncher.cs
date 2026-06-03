// BULWARK — META→BATTLE BRIDGE implementation (Phase 3 integration C1/C2, §12 boundary).
//
// WHAT THIS IS
//   The Bootstrap-layer concrete implementation of the modes-side seams:
//     • Bulwark.Game.Modes.IBattleLauncher       (Campaign / Endless)
//     • Bulwark.Game.Modes.AsyncLadder.IAsyncBattleLauncher (Async ghost ladder)
//   The mode controllers call those interfaces; THIS class maps a BattleLaunchConfig (or a
//   GhostSnapshot) onto the existing Phase-2 BattleBootstrap, applies the player's SERVER-OWNED,
//   cap-clamped RESOLVED upgrade deltas to the baked catalog (C2), starts the battle, and reports
//   the player-perspective outcome by reading the ECS MatchState singleton.
//
// WHY IT LIVES HERE (acyclic assembly graph, §12)
//   The dependency direction is Bootstrap → Modes (this file IMPLEMENTS the Modes seam), never
//   Modes → Bootstrap. Bootstrap already references Bulwark.Sim (MatchState/MatchOutcome) and now
//   Bulwark.Game.Modes + Bulwark.Services (UpgradeMath). The Modes assembly references only
//   Bulwark.Data + Bulwark.Services. No cycle:
//       Data → ()  ;  Services → Data  ;  Modes → {Services, Data}  ;  Sim → Data
//       Bootstrap → {Sim, Data, Modes, Services}
//
// INVIOLABLE constraints honored here
//   • SERVER AUTHORITY (§9/§12): this launcher mutates NO currency balance. It only stages a battle
//     and reads an outcome; rewards still flow through IModeRewardWallet → IServerEconomy.
//   • CAPPED / no-P2W (§3/§15): resolved upgrade deltas are computed from the SERVER-OWNED, already
//     cap-clamped levels (ResolvedUpgrade.Level) via UpgradeMath.ResolveUpgradedStat / StatDelta,
//     which clamp to the track's maxLevel — a delta can NEVER exceed the HARD CAP. Only the PLAYER
//     roster is upgraded; the AI/enemy roster keeps base authored stats. The ladder ghost's levels
//     are server-validated by StatSanityValidator before they ever reach here.
//   • NO new content/balance (§15): every id resolves AUTHORED data the bootstrap already owns; this
//     launcher invents no units/spells/maps/factions/commanders and asserts no balance numbers.
//   • NO save-state logging (§4/§12): nothing here logs wallet/profile/snapshot/match payloads.
//
// SCAFFOLD STATUS: authored, NOT compiled (no Unity 6 / Entities — ADR-0-001/-2-001). The CONFIG +
//   delta computation are implemented; the actual battle build/Start + on-device MatchState read are
//   DEFERRED to a Unity runtime (documented below). The seam is fully wired so no controller calls an
//   undefined type — the launch returns a clearly-marked DEFERRED outcome until a runtime exists.

using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Entities;
using Bulwark.Data;
using Bulwark.Sim;
using Bulwark.Services.Economy;     // UpgradeMath, UpgradeTrack
using Bulwark.Game.Modes;           // IBattleLauncher, BattleLaunchConfig, BattleOutcome, ResolvedUpgrade
using Bulwark.Game.Modes.AsyncLadder; // IAsyncBattleLauncher, GhostSnapshot, LadderBattleOutcome

namespace Bulwark.Bootstrap
{
    /// <summary>
    /// Configures + launches the Phase-2 ECS battle on behalf of the meta modes, and reports the
    /// player-perspective outcome. Constructed with the BattleBootstrap to drive and the catalogs the
    /// resolved-upgrade math needs (the UpgradesConfig curve + the player roster's base stats). The
    /// modes layer only ever sees the IBattleLauncher / IAsyncBattleLauncher interfaces.
    /// </summary>
    public sealed class BattleBootstrapLauncher : IBattleLauncher, IAsyncBattleLauncher
    {
        private readonly BattleBootstrap _bootstrap;
        private readonly UpgradesConfig _upgradesConfig; // curve (maxLevel/deltaPerLevel) for the C2 bake
        private readonly MapDef[] _mapCatalog;           // resolve BattleLaunchConfig.MapId → MapDef

        public BattleBootstrapLauncher(BattleBootstrap bootstrap, UpgradesConfig upgradesConfig, MapDef[] mapCatalog)
        {
            _bootstrap = bootstrap;
            _upgradesConfig = upgradesConfig;
            _mapCatalog = mapCatalog;
        }

        // ============================================================ IBattleLauncher (Campaign/Endless)

        /// <summary>
        /// Configure BattleBootstrap from <paramref name="config"/> and launch. Resolves the map, stages
        /// the player's resolved-upgrade deltas (C2), then starts the world. Resolves to the outcome.
        /// </summary>
        public Task<BattleOutcome> LaunchAsync(BattleLaunchConfig config)
        {
            if (_bootstrap == null || config == null)
                return Task.FromResult(BattleOutcome.Ongoing);

            // --- Map (authored data; never invented) ---
            MapDef map = ResolveMap(config.MapId);
            if (map != null) _bootstrap.map = map;

            // --- RESOLVED UPGRADES → BAKE HOOK (C2): compute additive, cap-clamped deltas per player unit
            //     and hand them to BattleBootstrap to fold into the player catalog. Set BEFORE Start().
            _bootstrap.playerUpgradeDeltas = BuildPlayerUpgradeDeltas(config.ResolvedUpgrades);

            // --- Enemy composition / start gold (campaign waves) is applied via the bootstrap's
            //     campaign-wave seam — a DEFERRED BattleBootstrap field (integrationNote). We do NOT
            //     invent that field here; the enemy faction's authored catalog + AI drive the slice today.
            //     config.EnemyWaves / EnemyStartGold are carried for that seam when it lands.

            // --- Launch: enabling the bootstrap GameObject triggers its Start() world build. On a Unity
            //     runtime we would then poll MatchState (see TryGetOutcome). DEFERRED — no Unity here, so
            //     we return Ongoing; the controller's Update() poll of TryGetOutcome takes over on device.
            // _bootstrap.enabled = true; _bootstrap.gameObject.SetActive(true);  // [Unity runtime, DEFERRED]
            return Task.FromResult(ReadOutcomeOrOngoing());
        }

        /// <summary>
        /// Re-stage the next Endless wave at the director's strength. The concrete spawn is the
        /// BattleBootstrap endless-wave seam — a DEFERRED field. No-op (no balance invented) until it lands.
        /// </summary>
        public void StageEndlessWave(int waveIndex, float waveStrength)
        {
            // _bootstrap.SetEndlessWave(waveIndex, waveStrength);  // [DEFERRED BattleBootstrap seam]
        }

        /// <summary>
        /// Read the ECS MatchState singleton and map it to the meta-side BattleOutcome. Returns false
        /// while Ongoing or before the world exists (so the controller keeps polling).
        /// </summary>
        public bool TryGetOutcome(out BattleOutcome outcome)
        {
            outcome = ReadOutcomeOrOngoing();
            return outcome != BattleOutcome.Ongoing;
        }

        // ============================================================ IAsyncBattleLauncher (ladder)

        /// <summary>
        /// Launch one async battle with the player as Team 0 and the ghost as the AI side (Team 1). The
        /// ghost's composition + (server-validated, cap-clamped) upgrade levels configure the AI roster.
        /// Resolves to the player-perspective LadderBattleOutcome. The map/AI wiring of the ghost onto
        /// BattleBootstrap's AI-side inputs is the DEFERRED runtime step.
        /// </summary>
        public Task<LadderBattleOutcome> LaunchVsGhostAsync(GhostSnapshot opponentGhost)
        {
            // The ghost's upgrade levels were already accepted by StatSanityValidator (server-side), so
            // they sit at/under the HARD caps — applying them to the AI roster cannot exceed a cap.
            // Mapping ghost.units → BattleBootstrap.aiUnits + the AI catalog deltas is a DEFERRED runtime
            // step (no Unity here). We map the ECS outcome to the ladder enum once the battle resolves.
            BattleOutcome o = ReadOutcomeOrOngoing();
            return Task.FromResult(MapToLadder(o));
        }

        /// <summary>
        /// Capture the player's own army snapshot from the just-finished battle. The composition + applied
        /// capped-upgrade levels are read from authored data + the player's server-confirmed levels — NO
        /// save payload. Returns an empty snapshot until the runtime capture lands (DEFERRED).
        /// </summary>
        public GhostSnapshot CapturePlayerSnapshot(LadderBattleOutcome outcome)
        {
            // DEFERRED: read the player's roster/levels from the live world. Here we return a minimal,
            // well-formed snapshot carrying only the outcome — the server re-validates everything anyway.
            return new GhostSnapshot
            {
                snapshotId = null,            // server issues the real id at ingestion
                ownerPlayerId = null,         // server fills from the authenticated session
                schemaVersion = 1,
                factionId = (int)Faction.IronPact,
                units = new List<GhostUnitEntry>(),
                draftedSpellIds = new List<string>(),
                commanderId = null,
                commanderLevel = 0,
                totalUnitsFielded = 0,
                capturedOutcome = outcome,
                capturedUtcTicks = 0L,
            };
        }

        // ============================================================ helpers

        /// <summary>
        /// Compute the additive, ALREADY-CAP-CLAMPED per-unit stat deltas for the player roster from the
        /// server-owned resolved upgrade levels (C2). Uses UpgradeMath.StatDelta, which clamps the level
        /// to the track's maxLevel — so no delta can exceed the HARD CAP (§3/§15). AttackSpeed lowers the
        /// AttackInterval (faster attacks): we map its positive stat delta to a NEGATIVE interval delta,
        /// floored by the bake so the interval stays positive.
        /// </summary>
        private IReadOnlyDictionary<string, BattleBootstrap.UnitUpgradeDeltas> BuildPlayerUpgradeDeltas(
            List<ResolvedUpgrade> resolved)
        {
            var map = new Dictionary<string, BattleBootstrap.UnitUpgradeDeltas>();
            if (resolved == null || _upgradesConfig == null || _upgradesConfig.tracks == null)
                return map;

            for (int i = 0; i < resolved.Count; i++)
            {
                ResolvedUpgrade r = resolved[i];
                if (string.IsNullOrEmpty(r.UnitId) || r.Level <= 0) continue;
                if (!TryGetTrack(r.UnitId, r.Stat, out UpgradeTrack track)) continue;

                // Cap-clamped additive delta for this track at the server-owned level (never over maxLevel).
                float delta = UpgradeMath.StatDelta(track, r.Level);

                map.TryGetValue(r.UnitId, out BattleBootstrap.UnitUpgradeDeltas acc);
                switch (r.Stat)
                {
                    case UpgradeStat.Health:      acc.Health += delta; break;
                    case UpgradeStat.Damage:      acc.Damage += delta; break;
                    case UpgradeStat.MoveSpeed:   acc.MoveSpeed += delta; break;
                    case UpgradeStat.AttackSpeed: acc.AttackIntervalDelta -= delta; break; // faster = lower interval
                }
                map[r.UnitId] = acc;
            }
            return map;
        }

        private bool TryGetTrack(string unitId, UpgradeStat stat, out UpgradeTrack track)
        {
            List<UpgradeTrack> tracks = _upgradesConfig.tracks;
            for (int i = 0; i < tracks.Count; i++)
            {
                if (tracks[i].stat == stat && tracks[i].unitId == unitId)
                {
                    track = tracks[i];
                    return true;
                }
            }
            track = default;
            return false;
        }

        private MapDef ResolveMap(string mapId)
        {
            if (string.IsNullOrEmpty(mapId) || _mapCatalog == null) return null;
            for (int i = 0; i < _mapCatalog.Length; i++)
                if (_mapCatalog[i] != null && _mapCatalog[i].id == mapId) return _mapCatalog[i];
            return null;
        }

        /// <summary>
        /// Read the MatchState singleton from the default ECS world and map it to BattleOutcome. Returns
        /// Ongoing when the world/singleton does not exist yet (the DEFERRED no-runtime case included).
        /// </summary>
        private static BattleOutcome ReadOutcomeOrOngoing()
        {
            World world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated) return BattleOutcome.Ongoing;

            EntityManager em = world.EntityManager;
            EntityQuery q = em.CreateEntityQuery(ComponentType.ReadOnly<MatchState>());
            if (q.IsEmptyIgnoreFilter) return BattleOutcome.Ongoing;
            if (!q.TryGetSingleton(out MatchState st)) return BattleOutcome.Ongoing;

            switch (st.Outcome)
            {
                case MatchOutcome.Victory: return BattleOutcome.Victory;
                case MatchOutcome.Defeat:  return BattleOutcome.Defeat;
                default:                   return BattleOutcome.Ongoing;
            }
        }

        private static LadderBattleOutcome MapToLadder(BattleOutcome o)
        {
            switch (o)
            {
                case BattleOutcome.Victory: return LadderBattleOutcome.Win;
                case BattleOutcome.Defeat:  return LadderBattleOutcome.Loss;
                default:                    return LadderBattleOutcome.Unresolved;
            }
        }
    }
}

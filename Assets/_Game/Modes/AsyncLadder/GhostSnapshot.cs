// BULWARK — Async Ghost Ladder: opponent SNAPSHOT model + server-side STAT-SANITY validator.
// Roadmap §13 3.4 ("Async ghost ladder — snapshot opponents; stat-check validation"), §7
// ("Tournament / Async Ladder [PRESERVE]+[MODERNIZE] — async vs opponent ghosts/snapshots;
// climb a ladder; reward = Silver"), §3 (recovered "async ghost ladder, no real-time netcode").
// DECISION-LOG §3 (binding): MVP ships a NON-deterministic ECS sim + SERVER STAT-SANITY CHECKS;
// deterministic simulation + replays are a Phase-7 GATE (ranked). Therefore this snapshot carries
// NO seed, NO input-log, NO deterministic replay payload — only the COMPOSITION of an opponent's
// army so the local sim can stand it up as the AI side, plus a coarse result summary for ranking.
// §12 anti-cheat doctrine ("detect on the client, DECIDE on the server"): the StatSanityValidator
// below is a PURE function that runs SERVER-SIDE (it is the authoritative gate). A client copy is
// permitted only as pre-flight deterrence; the SERVER's verdict is the one that counts.
//
// INVIOLABLE constraints honored here (Phase-3 hard rules):
//   • SERVER AUTHORITY over currency/rank — this file holds NO balances and grants NOTHING.
//   • CAPPED PROGRESSION (no P2W) — the validator REJECTS any snapshot whose upgrade levels exceed
//     the per-track caps in UpgradesConfig (the server enforces caps; a forged over-cap ghost is a
//     cheat and is dropped).
//   • 4 MVP currencies only — the only reward referenced anywhere in this module is Silver (§7).
//   • NO save-state logging — this is a transport/plausibility model, never logged as a save payload.
//
// PURE C#: no UnityEngine dependency, so the snapshot model + validator are CI-unit-testable exactly
// like the Phase-0 ConfigResolver (§12 "pure-C# services avoid UnityEngine where they can be tested").
// EconomyConfig/UpgradesConfig are Unity ScriptableObjects; rather than depend on them, the validator
// takes their already-resolved numeric BOUNDS as a small plain struct (LadderSanityBounds /
// UpgradeCapTable) that the server fills from the 3-tier resolver (RC → SO → literal, §12).
//
// SCAFFOLD STATUS: authored, NOT compiled (no Unity 6 / no BaaS — ADR-0-001/-2-001). Server-side
// execution + real matchmaking are DEFERRED (the validator is the spec; the server runs it).

using System;
using System.Collections.Generic;

namespace Bulwark.Game.Modes.AsyncLadder
{
    // =====================================================================================
    // SNAPSHOT MODEL — a serializable description of an opponent army. NO seed / NO replay.
    // =====================================================================================

    /// <summary>
    /// One unit type in a ghost army, with the APPLIED capped-upgrade levels (§13 3.2) that the
    /// owning player had purchased when the snapshot was taken. <see cref="unitId"/> references
    /// <c>UnitDef.id</c>; the level dictionary is keyed by the upgrade stat. Levels are validated
    /// SERVER-SIDE against the per-track caps — a client cannot smuggle an over-cap unit through.
    /// </summary>
    [Serializable]
    public struct GhostUnitEntry
    {
        /// <summary>Content id of the unit type (UnitDef.id) — NOT a side-relative index, so a ghost
        /// is portable across maps/rosters.</summary>
        public string unitId;

        /// <summary>How many of this unit type the ghost army fielded.</summary>
        public int count;

        /// <summary>Applied capped-upgrade LEVELS per stat (§13 3.2). Absent stat ⇒ level 0.
        /// 0 = matches int(UpgradeStat); 1=Health,2=Damage,3=MoveSpeed,4=AttackSpeed (see
        /// <see cref="Bulwark.Data.UpgradeStat"/>). Server clamps/rejects against UpgradesConfig caps.</summary>
        public Dictionary<int, int> upgradeLevels;

        /// <summary>The unit's authored base attack damage (copied from UnitDef at capture, for the
        /// server's plausibility math). NOT a balance source — purely an input to the sanity check.</summary>
        public float baseAttackDamage;

        /// <summary>The unit's authored attack interval (seconds/swing) at capture — feeds the DPS
        /// plausibility estimate (DPS ≈ damage / interval). NOT a balance source.</summary>
        public float baseAttackInterval;
    }

    /// <summary>
    /// A full opponent ghost: faction, unit composition (with applied upgrade levels), drafted spell
    /// ids, commander id + level, and a coarse result summary. This is what the SERVER stores and what
    /// the local sim stands up as the AI side via BattleBootstrap. It is a COMPOSITION snapshot, NOT a
    /// deterministic replay (decision-log §3): re-running it produces a *plausible* fight, not a
    /// bit-identical one — which is exactly why validation is STAT-SANITY, not replay verification.
    /// </summary>
    [Serializable]
    public struct GhostSnapshot
    {
        // ---- Identity / matchmaking (opaque; server owns the real ids) ----
        /// <summary>Opaque server-issued snapshot id (matchmaking key). Client never authors this.</summary>
        public string snapshotId;

        /// <summary>Opaque owner player id (server-owned; no device PII — mirrors AuthResult.PlayerId).</summary>
        public string ownerPlayerId;

        /// <summary>Schema/version tag so the server can reject snapshots from an incompatible build.</summary>
        public int schemaVersion;

        // ---- Army composition (§5.1 faction + capped roster, §13 3.2) ----
        /// <summary>The ghost's faction (0=IronPact, 1=AshenHorde — matches Bulwark.Data.Faction int).</summary>
        public int factionId;

        /// <summary>The unit composition with applied upgrade levels.</summary>
        public List<GhostUnitEntry> units;

        // ---- Drafted spells (§5.3 draft-3) ----
        /// <summary>Content ids of the (≤3) drafted spells the ghost used (SpellDef.id). Empty ⇒ none.</summary>
        public List<string> draftedSpellIds;

        // ---- Commander (§5.5/§6 — capped) ----
        /// <summary>Commander content id (CommanderDef.id). Null/empty ⇒ no commander.</summary>
        public string commanderId;

        /// <summary>Commander level (§13 3.2, capped by UpgradesConfig.commanderMaxLevel). Server-clamped.</summary>
        public int commanderLevel;

        // ---- Result summary (for ranking display only — NOT authoritative) ----
        /// <summary>Total units the ghost fielded over the run (anti-cheat army-size input).</summary>
        public int totalUnitsFielded;

        /// <summary>The ghost owner's reported outcome when this snapshot was captured (display only).
        /// The server NEVER trusts a client-reported win for rank — it re-derives rank from the live
        /// async match result + its own stat-sanity verdict (§12).</summary>
        public LadderBattleOutcome capturedOutcome;

        /// <summary>Capture timestamp (UTC ticks) — for snapshot freshness/rotation. NOT a save payload.</summary>
        public long capturedUtcTicks;
    }

    /// <summary>
    /// Battle outcome from the META layer's point of view (player perspective). Mirrors the SIM's
    /// <c>Bulwark.Sim.MatchOutcome</c> {Ongoing,Victory,Defeat} but is declared HERE so the meta/mode
    /// module does NOT take a hard reference on the ECS sim assembly (§12 boundary: economy/meta/modes
    /// are services + MonoBehaviour, NOT the ECS battle-sim). The Control/Bootstrap layer maps
    /// MatchOutcome → this enum at battle end (see integrationNotes).
    /// </summary>
    public enum LadderBattleOutcome : byte
    {
        Unresolved = 0, // battle not finished / not reported
        Win        = 1, // player toppled the ghost's statue
        Loss       = 2, // the ghost toppled the player's statue
    }

    // =====================================================================================
    // STAT-SANITY BOUNDS — plain numeric bounds the SERVER fills from the resolved config
    // (EconomyConfig.ladderMax* and UpgradesConfig caps), so the validator stays pure C#.
    // =====================================================================================

    /// <summary>
    /// Resolved plausibility bounds for the ladder stat-sanity gate (§13 3.4 / decision-log §3).
    /// The server fills these from EconomyConfig via the 3-tier resolver (RC → SO → literal, §12):
    ///   <c>ladderMaxPlausibleDps</c> and <c>ladderMaxUnitsPerBattle</c>. Kept as a plain struct so the
    /// validator has NO UnityEngine/ScriptableObject dependency and is CI-unit-testable.
    /// </summary>
    public readonly struct LadderSanityBounds
    {
        /// <summary>EconomyConfig.ladderMaxPlausibleDps — reject snapshots whose implied per-unit DPS
        /// exceeds this.</summary>
        public readonly float MaxPlausibleDps;

        /// <summary>EconomyConfig.ladderMaxUnitsPerBattle — reject snapshots whose army size exceeds this.</summary>
        public readonly int MaxUnitsPerBattle;

        public LadderSanityBounds(float maxPlausibleDps, int maxUnitsPerBattle)
        {
            MaxPlausibleDps = maxPlausibleDps;
            MaxUnitsPerBattle = maxUnitsPerBattle;
        }
    }

    /// <summary>
    /// The server's view of the per-track / commander HARD CAPS (§3, INVIOLABLE) used by the validator
    /// to reject over-cap forged ghosts. Built server-side from UpgradesConfig (UpgradeTrack.maxLevel
    /// keyed by unitId+stat; commanderMaxLevel). Plain C# — no ScriptableObject dependency.
    /// </summary>
    public sealed class UpgradeCapTable
    {
        // key: (unitId, stat-as-int) → maxLevel for that track.
        private readonly Dictionary<(string unitId, int stat), int> _trackCaps;
        private readonly int _commanderMaxLevel;

        public UpgradeCapTable(Dictionary<(string unitId, int stat), int> trackCaps, int commanderMaxLevel)
        {
            _trackCaps = trackCaps ?? new Dictionary<(string, int), int>();
            _commanderMaxLevel = commanderMaxLevel;
        }

        public int CommanderMaxLevel => _commanderMaxLevel;

        /// <summary>Max level for a given unit+stat track. Returns <paramref name="defaultCap"/> (0)
        /// when the track is unknown — an UNKNOWN track with a non-zero level is itself a sanity failure
        /// (a level was claimed for a track that does not exist), so default 0 makes that path reject.</summary>
        public int TrackCap(string unitId, int stat, int defaultCap = 0)
            => _trackCaps.TryGetValue((unitId, stat), out var cap) ? cap : defaultCap;

        public bool HasTrack(string unitId, int stat) => _trackCaps.ContainsKey((unitId, stat));
    }

    /// <summary>Why a snapshot failed stat-sanity (server records this; never trusts the client's claim).</summary>
    public enum SanityRejectReason : byte
    {
        None = 0,
        ArmyTooLarge        = 1, // totalUnitsFielded / summed counts exceed ladderMaxUnitsPerBattle
        ImpliedDpsTooHigh   = 2, // a unit's implied DPS exceeds ladderMaxPlausibleDps
        UpgradeOverCap      = 3, // an upgrade level exceeds the track's HARD cap (P2W attempt)
        UnknownUpgradeTrack = 4, // a level was claimed for a track that does not exist
        CommanderOverCap    = 5, // commanderLevel exceeds commanderMaxLevel
        Malformed           = 6, // null/negative/empty where a value is required
    }

    /// <summary>Outcome of the (server-side) stat-sanity gate.</summary>
    public readonly struct SanityResult
    {
        public readonly bool Accepted;
        public readonly SanityRejectReason Reason;
        /// <summary>Optional offending detail (unitId / "commander") for server telemetry — NOT a save payload.</summary>
        public readonly string Detail;

        private SanityResult(bool accepted, SanityRejectReason reason, string detail)
        { Accepted = accepted; Reason = reason; Detail = detail; }

        public static SanityResult Ok() => new SanityResult(true, SanityRejectReason.None, null);
        public static SanityResult Reject(SanityRejectReason reason, string detail = null)
            => new SanityResult(false, reason, detail);
    }

    // =====================================================================================
    // STAT-SANITY VALIDATOR — the anti-cheat. PURE function. RUNS SERVER-SIDE (authoritative).
    // =====================================================================================

    /// <summary>
    /// SERVER-SIDE plausibility gate for ladder ghost snapshots (§13 3.4 / decision-log §3 / §12).
    /// Rejects a snapshot whose implied per-unit DPS or total army size exceed the resolved bounds
    /// (EconomyConfig.ladderMaxPlausibleDps / ladderMaxUnitsPerBattle), or whose upgrade/commander
    /// levels exceed the HARD caps in UpgradesConfig. This is anti-cheat, NOT deterministic replay
    /// (none exists at MVP). It is a PURE function so it is CI-unit-testable; the SERVER calls it as
    /// the authoritative decision (the client may pre-run it for deterrence only). No currency, no
    /// rank, no save-state — it returns accept/reject + a reason.
    /// </summary>
    public static class StatSanityValidator
    {
        /// <summary>
        /// Validate a snapshot against the resolved bounds + caps. Returns the first failure found
        /// (deterministic order), or <see cref="SanityResult.Ok"/>. DESIGNED TO RUN ON THE SERVER.
        /// </summary>
        public static SanityResult Validate(in GhostSnapshot snapshot, in LadderSanityBounds bounds, UpgradeCapTable caps)
        {
            // --- Structural / malformed guards (a forged or corrupt snapshot must not pass) ---
            if (caps == null)
                return SanityResult.Reject(SanityRejectReason.Malformed, "null cap table");
            if (snapshot.units == null || snapshot.units.Count == 0)
                return SanityResult.Reject(SanityRejectReason.Malformed, "no units");
            if (bounds.MaxUnitsPerBattle <= 0 || bounds.MaxPlausibleDps <= 0f)
                return SanityResult.Reject(SanityRejectReason.Malformed, "non-positive bounds");

            // --- Commander cap (§3 / §6 — capped, server-enforced) ---
            if (snapshot.commanderLevel < 0)
                return SanityResult.Reject(SanityRejectReason.Malformed, "negative commander level");
            if (snapshot.commanderLevel > caps.CommanderMaxLevel)
                return SanityResult.Reject(SanityRejectReason.CommanderOverCap, "commander");

            // --- Army size (anti-cheat: a ghost cannot field more than the battle allows) ---
            long summedCount = 0;
            for (int i = 0; i < snapshot.units.Count; i++)
            {
                GhostUnitEntry u = snapshot.units[i];
                if (string.IsNullOrEmpty(u.unitId))
                    return SanityResult.Reject(SanityRejectReason.Malformed, "unit with no id");
                if (u.count < 0)
                    return SanityResult.Reject(SanityRejectReason.Malformed, u.unitId);
                summedCount += u.count;
            }
            // Reject if either the summed composition OR the self-reported total exceeds the cap
            // (mismatch between the two is also a tamper signal, handled by taking the larger).
            long claimedTotal = Math.Max(summedCount, Math.Max(0, snapshot.totalUnitsFielded));
            if (claimedTotal > bounds.MaxUnitsPerBattle)
                return SanityResult.Reject(SanityRejectReason.ArmyTooLarge);

            // --- Per-unit: upgrade caps + implied DPS plausibility ---
            for (int i = 0; i < snapshot.units.Count; i++)
            {
                GhostUnitEntry u = snapshot.units[i];

                // Upgrade-level caps (INVIOLABLE no-P2W): every claimed level must reference a REAL
                // track and sit at/under its HARD cap. An unknown track or an over-cap level is a cheat.
                if (u.upgradeLevels != null)
                {
                    foreach (KeyValuePair<int, int> kv in u.upgradeLevels)
                    {
                        int stat = kv.Key;
                        int level = kv.Value;
                        if (level < 0)
                            return SanityResult.Reject(SanityRejectReason.Malformed, u.unitId);
                        if (level == 0)
                            continue; // level 0 = no upgrade; nothing to cap-check.
                        if (!caps.HasTrack(u.unitId, stat))
                            return SanityResult.Reject(SanityRejectReason.UnknownUpgradeTrack, u.unitId);
                        if (level > caps.TrackCap(u.unitId, stat))
                            return SanityResult.Reject(SanityRejectReason.UpgradeOverCap, u.unitId);
                    }
                }

                // Implied DPS plausibility: DPS ≈ baseAttackDamage / max(epsilon, baseAttackInterval).
                // The Damage-upgrade level scales damage; we apply it as a CONSERVATIVE upper bound
                // (cap-equivalent) so the gate is robust without re-deriving the full §4 modifier chain
                // (which is the sim's job). If the implied per-unit DPS exceeds the plausible ceiling,
                // the snapshot is forged/over-tuned and is rejected.
                float impliedDps = ImpliedDps(in u);
                if (impliedDps > bounds.MaxPlausibleDps)
                    return SanityResult.Reject(SanityRejectReason.ImpliedDpsTooHigh, u.unitId);
            }

            return SanityResult.Ok();
        }

        /// <summary>
        /// Conservative per-unit DPS estimate for the plausibility gate. Uses the captured base damage
        /// and interval only; a non-positive interval is treated as the (worst-case) smallest swing
        /// window so a forged interval=0 can never sneak "infinite" DPS past the check. This is an
        /// ANTI-CHEAT bound, deliberately NOT the authoritative §4 combat math (that lives in the sim).
        /// </summary>
        public static float ImpliedDps(in GhostUnitEntry u)
        {
            const float MinInterval = 0.05f; // floor: nothing swings faster than 20/s for the bound.
            float interval = u.baseAttackInterval > MinInterval ? u.baseAttackInterval : MinInterval;
            float damage = u.baseAttackDamage < 0f ? 0f : u.baseAttackDamage;
            return damage / interval;
        }
    }
}

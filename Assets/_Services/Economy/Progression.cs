// BULWARK — CAPPED commander progression (Phase 3 §13 3.2 / §6 power budget / §3 anti-P2W / §12).
//
// WHAT THIS IS
// The meta-layer service for COMMANDER LEVELS — the second capped progression track (alongside
// per-unit upgrades in Upgrades.cs). A commander gains levels for a rising Silver cost, HARD-CAPPED
// at UpgradesConfig.commanderMaxLevel. Leveling improves ABILITY AVAILABILITY / QUALITY — but ONLY
// WITHIN the §6 commander power budget (≤10–15%). It NEVER lifts a commander above that cap: leveling
// scales how much of the already-capped budget is realised (and ability uptime/quality), it does not
// raise the ceiling. Identity, not power (CommanderDef header: "Commander = identity, NOT power").
//
// SERVER AUTHORITY (HARD RULE 1, INVIOLABLE)
// Commander level + Silver are SERVER-OWNED. The client never self-grants a level or mutates a
// balance: TryLevelUpAsync spends Silver and advances the level in ONE server-validated transaction
// (the ICommanderEconomy seam below; the canonical StubServerEconomy in ServerEconomy.cs implements
// it alongside IServerEconomy), and only the server's CONFIRMED response advances the read cache.
// Caps + costs are SERVER-ENFORCED.
//
// CAP + BUDGET DISCIPLINE (HARD RULE 3 + §6, INVIOLABLE — no P2W)
//   • Level is CLAMPED to [0, commanderMaxLevel]; the service refuses to request a level-up at the cap.
//   • The realised power fraction is HARD-CLAMPED to the CommanderDef's PowerBudgetPct (itself
//     §6-bounded ≤0.15 by the schema [Range]). ResolveBudgetFraction can ONLY return a value in
//     [0, powerBudgetPct] — there is NO level that yields more than the budget. This mirrors the
//     sim's runtime hard-clamp (BattleBootstrap header / CommanderAbilitySystem) so meta and sim agree.
//   • RANKED NORMALIZATION (§16, mandatory): in ranked the commander effect is NORMALIZED to the
//     budget REGARDLESS of level — i.e. level confers no ranked power advantage at all. Stub honored
//     by the sim (CommanderDef.rankedNormalized); the real normalization land is DEFERRED.
//
// NO SAVE-STATE LOGGING (HARD RULE 2): never logs wallet/profile/save payloads (event name + tiny
// non-PII numerics only). §12 BOUNDARY: pure-C# meta service (Bulwark.Services.Economy), NOT ECS;
// all budget/cost MATH is in the pure CommanderProgressionMath (no UnityEngine, CI-testable).
//
// SCAFFOLD STATUS: authored, NOT compiled (no Unity 6 / no BaaS — ADR-0-001/-2-001). The server
// spend/grant, the ranked-normalization land, and the RC retune of the curve are DEFERRED.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bulwark.Data; // UpgradesConfig, CommanderDef, Currency
using Bulwark.Services.Analytics;

namespace Bulwark.Services.Economy
{
    /// <summary>The server's confirmed answer to a commander level-up (no save-state payload, HARD RULE 2).</summary>
    public readonly struct CommanderLevelResult
    {
        public readonly bool Confirmed;        // true only if the SERVER committed the transaction
        public readonly int  NewLevel;         // server-authoritative new commander level (== from on reject)
        public readonly long NewSilverBalance; // server-authoritative Silver after the charge
        public readonly UpgradeRejectReason Reason; // canonical reason enum (ServerEconomy.cs); UX only

        public CommanderLevelResult(bool confirmed, int newLevel, long newSilverBalance, UpgradeRejectReason reason)
        {
            Confirmed = confirmed;
            NewLevel = newLevel;
            NewSilverBalance = newSilverBalance;
            Reason = reason;
        }
    }

    /// <summary>
    /// Server-authoritative seam for a commander level-up: atomically charge Silver AND advance the
    /// commander's level in ONE transaction, with the SERVER re-validating cap/cost/balance/from-level.
    /// This is the COMMANDER counterpart to module 3.1's IServerEconomy (ServerEconomy.cs); it is a
    /// distinct, narrow contract (different method shape), and at integration the SAME canonical stub
    /// (StubServerEconomy) implements BOTH — one coherent seam, one shared simulated wallet/Progress
    /// map (A3). Implemented by a BaaS adapter at MVP (decision-log §3); the stub stands in here.
    /// Uses UpgradeRejectReason from ServerEconomy.cs (the canonical reason enum) for its UX reason.
    /// </summary>
    public interface ICommanderEconomy
    {
        /// <summary>
        /// Atomic, SERVER-VALIDATED level-up: charge <paramref name="costSilver"/> Silver and advance
        /// <paramref name="commanderId"/> from <paramref name="expectedFromLevel"/> to +1, re-checking
        /// the hard cap and the cost server-side. Returns the CONFIRMED new level + Silver balance, or
        /// a non-confirm (nothing mutated) on any rejection. The client trusts only this answer.
        /// </summary>
        Task<CommanderLevelResult> SpendForCommanderLevelAsync(
            string commanderId, long costSilver, int expectedFromLevel, int hardCapMaxLevel);

        /// <summary>Last server-confirmed Silver balance, for read-cache display (server-owned, may be stale).</summary>
        long CachedBalance(Currency currency);
    }

    // =====================================================================================
    // PURE MATH (no UnityEngine, no server) — CI-unit-testable like ConfigResolver.
    // =====================================================================================

    /// <summary>
    /// Pure commander-progression math: level clamp, rising Silver cost, the normalized
    /// progress fraction, and the BUDGET-BOUNDED realised power fraction. NO UnityEngine, NO async.
    /// Every method that yields power clamps the result into [0, powerBudgetPct], so NO level can
    /// ever exceed the §6 commander cap (anti-P2W, INVIOLABLE).
    /// </summary>
    public static class CommanderProgressionMath
    {
        /// <summary>Clamp a commander level into [0, maxLevel]. The cap is INVIOLABLE.</summary>
        public static int ClampLevel(int level, int maxLevel)
        {
            int cap = maxLevel < 0 ? 0 : maxLevel;
            if (level < 0) return 0;
            if (level > cap) return cap;
            return level;
        }

        /// <summary>True once at the HARD CAP — no further level-up is allowed (§3/§15).</summary>
        public static bool IsAtCap(int level, int maxLevel) => ClampLevel(level, maxLevel) >= (maxLevel < 0 ? 0 : maxLevel);

        /// <summary>
        /// Silver cost to buy the level AFTER <paramref name="fromLevel"/>:
        /// round(baseCostSilver * costGrowth^fromLevel). 0 at the cap (nothing to buy). fromLevel is
        /// clamped, so a tampered over-cap level can't cheapen the cost.
        /// </summary>
        public static long NextCostSilver(int baseCostSilver, float costGrowth, int fromLevel, int maxLevel)
        {
            int lvl = ClampLevel(fromLevel, maxLevel);
            if (lvl >= (maxLevel < 0 ? 0 : maxLevel)) return 0;
            double growth = costGrowth <= 0f ? 1.0 : costGrowth;
            double cost = baseCostSilver * Math.Pow(growth, lvl);
            if (cost < 0d) cost = 0d;
            long rounded = (long)Math.Round(cost, MidpointRounding.AwayFromZero);
            return rounded < 0 ? 0 : rounded;
        }

        /// <summary>
        /// Normalized progress in [0,1]: level / maxLevel (0 when maxLevel is 0). This is the lever
        /// that scales ability availability/quality — it does NOT scale the ceiling. At max level it
        /// is 1.0 (the full, already-capped budget is realised), never above.
        /// </summary>
        public static float NormalizedProgress(int level, int maxLevel)
        {
            int cap = maxLevel <= 0 ? 0 : maxLevel;
            if (cap == 0) return 0f;
            return ClampLevel(level, cap) / (float)cap;
        }

        /// <summary>
        /// The realised commander power fraction at a level, HARD-BOUNDED by the §6 budget:
        ///   ranked        → ALWAYS powerBudgetPct (normalized; level confers no ranked advantage).
        ///   non-ranked    → NormalizedProgress(level) * powerBudgetPct  (∈ [0, powerBudgetPct]).
        /// The output is clamped into [0, powerBudgetPct] defensively, so NO input can exceed the §6
        /// cap. powerBudgetPct is itself ≤0.15 by the CommanderDef schema; this never raises it.
        /// </summary>
        public static float ResolveBudgetFraction(int level, int maxLevel, float powerBudgetPct, bool ranked)
        {
            float cap = powerBudgetPct < 0f ? 0f : powerBudgetPct; // never negative; schema bounds ≤0.15
            float frac = ranked ? cap : NormalizedProgress(level, maxLevel) * cap;
            if (frac < 0f) return 0f;
            if (frac > cap) return cap; // INVIOLABLE §6 ceiling — leveling cannot exceed the budget
            return frac;
        }
    }

    // =====================================================================================
    // THE SERVICE — server-validated commander leveling + read-cache + ranked normalization stub.
    // =====================================================================================

    /// <summary>
    /// Drives CAPPED commander levels over UpgradesConfig.commander* curve. Server-owned levels are
    /// read from PlayerProfile.Progress; level-ups go through ICommanderEconomy and only a SERVER
    /// confirmation advances a level (HARD RULE 1). Leveling realises more of the §6 power budget
    /// (and ability uptime/quality) but NEVER exceeds it (HARD RULE 3 / §6); ranked is normalized to
    /// the budget regardless of level. All math delegates to the pure CommanderProgressionMath (§12).
    /// </summary>
    public sealed class ProgressionService
    {
        private readonly UpgradesConfig _config;
        private readonly ICommanderEconomy _server;
        private readonly IAnalytics _analytics;

        // Server-owned read CACHE of commander levels, keyed by commanderId. Hydrated from the
        // authoritative profile and reconciled on every confirmed level-up. DISPLAY/PRE-FLIGHT only
        // (HARD RULE 1) — NOT the source of truth.
        private readonly Dictionary<string, int> _levelCache = new Dictionary<string, int>(StringComparer.Ordinal);

        public ProgressionService(UpgradesConfig config, ICommanderEconomy server, IAnalytics analytics = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _server = server ?? throw new ArgumentNullException(nameof(server));
            _analytics = analytics;
        }

        /// <summary>The HARD CAP for commander levels (server-enforced; §3/§15).</summary>
        public int MaxLevel => _config.commanderMaxLevel < 0 ? 0 : _config.commanderMaxLevel;

        // ---- Profile hydration (server is the record of truth) -------------------------------

        /// <summary>
        /// Seed the read cache from the SERVER-AUTHORITATIVE profile. Each commander's level lives
        /// under ProgressKey(commanderId) as a decimal-int string. Unparseable/out-of-range values
        /// become level 0, clamped to the cap — a tampered profile can never grant over-cap power.
        /// NO payload logged (HARD RULE 2). Pass the commander ids the player owns (MVP: 2 total).
        /// </summary>
        public void HydrateFromProfile(IReadOnlyDictionary<string, string> progress, IEnumerable<string> commanderIds)
        {
            _levelCache.Clear();
            if (commanderIds == null) return;
            foreach (string id in commanderIds)
            {
                if (string.IsNullOrEmpty(id)) continue;
                int level = 0;
                if (progress != null
                    && progress.TryGetValue(ProgressKey(id), out string raw)
                    && int.TryParse(raw, out int parsed))
                {
                    level = parsed;
                }
                _levelCache[id] = CommanderProgressionMath.ClampLevel(level, MaxLevel);
            }
        }

        /// <summary>The server-owned key under which a commander's level lives in PlayerProfile.Progress.</summary>
        public static string ProgressKey(string commanderId) => $"cmd.{commanderId}.level";

        // ---- Reads (server-owned cache; never the truth) -------------------------------------

        /// <summary>The server-owned current commander level (cap-clamped). 0 if un-hydrated/unknown.</summary>
        public int CurrentLevel(string commanderId)
        {
            int cached = _levelCache.TryGetValue(commanderId ?? string.Empty, out int v) ? v : 0;
            return CommanderProgressionMath.ClampLevel(cached, MaxLevel);
        }

        /// <summary>True once the commander is at its HARD CAP — no further level-up allowed (§3/§15).</summary>
        public bool IsAtCap(string commanderId) => CommanderProgressionMath.IsAtCap(CurrentLevel(commanderId), MaxLevel);

        /// <summary>
        /// Silver cost to buy the NEXT commander level. 0 at cap. Pre-flight UX value — the SERVER
        /// re-derives and validates the real charge (HARD RULE 1).
        /// </summary>
        public long NextCostSilver(string commanderId)
            => CommanderProgressionMath.NextCostSilver(
                _config.commanderBaseCostSilver, _config.commanderCostGrowth, CurrentLevel(commanderId), MaxLevel);

        /// <summary>Normalized progress in [0,1] (level/maxLevel) — the ability availability/quality lever.</summary>
        public float NormalizedProgress(string commanderId)
            => CommanderProgressionMath.NormalizedProgress(CurrentLevel(commanderId), MaxLevel);

        /// <summary>
        /// The realised commander power fraction for a commander at its current level, HARD-BOUNDED by
        /// the CommanderDef's §6 budget. In ranked it is normalized to the full budget regardless of
        /// level; out of ranked it scales with level WITHIN the budget. Never exceeds powerBudgetPct.
        /// The sim hard-clamps to the same budget at runtime; this keeps meta and sim in agreement.
        /// </summary>
        public float ResolveBudgetFraction(CommanderDef commander, bool ranked)
        {
            if (commander == null) return 0f;
            return CommanderProgressionMath.ResolveBudgetFraction(
                CurrentLevel(commander.id), MaxLevel, commander.powerBudgetPct, ranked);
        }

        /// <summary>
        /// RANKED-NORMALIZATION stub (§16, mandatory): in ranked the effect is normalized to the budget
        /// regardless of level. Honors the CommanderDef.rankedNormalized flag — when normalization is
        /// enabled (or the caller forces ranked) leveling confers NO power advantage. The real
        /// normalization land in the sim/ladder is DEFERRED (decision-log §3 — stat-sanity, no replays).
        /// </summary>
        public float ResolveNormalizedRankedFraction(CommanderDef commander)
        {
            if (commander == null) return 0f;
            // rankedNormalized==true → force the ranked branch (level-independent budget).
            bool ranked = commander.rankedNormalized;
            return CommanderProgressionMath.ResolveBudgetFraction(
                CurrentLevel(commander.id), MaxLevel, commander.powerBudgetPct, ranked);
        }

        // ---- The capped, server-validated level-up ------------------------------------------

        /// <summary>
        /// Attempt to level a commander. PRE-FLIGHT (UX) rejects an at/over-cap request WITHOUT hitting
        /// the server (§3/§15). Otherwise the SERVER atomically charges Silver + advances the level; the
        /// level only advances on a server CONFIRM (HARD RULE 1), after which the read cache reconciles
        /// to the server-returned level. The client never self-grants and never exceeds the cap. Leveling
        /// realises more of the §6 budget but NEVER raises the ceiling. Analytics: name + non-PII
        /// numerics only (HARD RULE 2 — no wallet payload).
        /// </summary>
        public async Task<CommanderLevelResult> TryLevelUpAsync(string commanderId)
        {
            if (string.IsNullOrEmpty(commanderId))
            {
                Emit("commander_levelup_rejected_unknown", commanderId, -1);
                return new CommanderLevelResult(false, 0, _server.CachedBalance(Currency.Silver), UpgradeRejectReason.UnknownTrack);
            }

            int fromLevel = CurrentLevel(commanderId); // server-owned, cap-clamped

            // HARD CAP pre-flight (§3/§15): refuse to even REQUEST an at/over-cap level-up.
            if (CommanderProgressionMath.IsAtCap(fromLevel, MaxLevel))
            {
                Emit("commander_levelup_rejected_at_cap", commanderId, fromLevel);
                return new CommanderLevelResult(false, fromLevel, _server.CachedBalance(Currency.Silver), UpgradeRejectReason.AtHardCap);
            }

            long cost = CommanderProgressionMath.NextCostSilver(
                _config.commanderBaseCostSilver, _config.commanderCostGrowth, fromLevel, MaxLevel);

            // SERVER-AUTHORITATIVE spend+grant (DEFERRED round-trip: stub here, BaaS at integration).
            CommanderLevelResult result = await _server.SpendForCommanderLevelAsync(
                commanderId, cost, fromLevel, MaxLevel);

            if (result.Confirmed)
            {
                _levelCache[commanderId] = CommanderProgressionMath.ClampLevel(result.NewLevel, MaxLevel);
                Emit("commander_levelup_confirmed", commanderId, result.NewLevel);
            }
            else
            {
                Emit("commander_levelup_rejected_server", commanderId, fromLevel);
            }
            return result;
        }

        // ---- internals -----------------------------------------------------------------------

        /// <summary>Consolidated analytics event: name + tiny non-PII numerics only (HARD RULE 2).</summary>
        private void Emit(string name, string commanderId, int level)
        {
            if (_analytics == null) return;
            _analytics.Emit(name, new Dictionary<string, object>
            {
                { "commander", commanderId },
                { "level", level },
            });
        }
    }
}

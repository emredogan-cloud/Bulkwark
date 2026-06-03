// BULWARK — CAPPED per-unit upgrade service (Phase 3 §13 3.2 / §3 anti-P2W / §12 / §15).
//
// WHAT THIS IS
// The meta-layer service that drives the persistent, HARD-CAPPED per-unit upgrade tracks
// (UpgradeTrack in UpgradesConfig). For a (unitId, UpgradeStat) it exposes:
//   • CurrentLevel(...)  — SERVER-OWNED, read from the authoritative PlayerProfile.Progress
//                          (never trusted from a local mutation — HARD RULE 1).
//   • NextCostSilver(...)— round(baseCostSilver * costGrowth^level) Silver for the next level.
//   • StatDelta(...)     — the bounded additive stat delta = deltaPerLevel * clamped(level).
//   • TryUpgradeAsync(...)— spends Silver through the SERVER economy seam (server-validated)
//                          and increments the level ONLY on the server's confirmed response.
//                          REJECTS at/over maxLevel (the HARD CAP — never exceeded, §3/§15).
//   • ResolveUpgradedStat(...) — a PURE helper (no server, no UnityEngine) the battle bake
//                          uses to fold a unit's stat to its upgraded, cap-clamped value.
//
// SERVER AUTHORITY (HARD RULE 1, INVIOLABLE)
// Currency and progression are SERVER-OWNED. This client NEVER mutates an authoritative
// balance or an upgrade level locally: every spend + level-grant is one atomic server
// round-trip (IServerEconomy.SpendForUpgradeAsync), and the client only updates its read
// cache from the server's CONFIRMED response. Caps + costs are SERVER-ENFORCED; the client's
// checks here are pre-flight UX (fail fast / disable buttons) and anti-tamper deterrence —
// they are NOT the source of truth and never grant power on their own.
//
// CAP DISCIPLINE (HARD RULE 3, INVIOLABLE — no P2W)
// maxLevel is a HARD CAP. The service refuses to even REQUEST an upgrade at/above it, the
// pure resolver CLAMPS level to [0, maxLevel] before computing any delta, and the server is
// the final cap authority. There is no code path that yields power beyond deltaPerLevel*maxLevel.
//
// NO SAVE-STATE LOGGING (HARD RULE 2, INVIOLABLE)
// This never logs/serializes wallet, profile, or progress payloads. Analytics emits an
// EVENT NAME + tiny non-PII numeric facts (unit/stat/level/source) only — never the wallet.
//
// §12 BOUNDARY
// Pure-C# meta service (Bulwark.Services.Economy), NOT ECS battle-sim. The PURE helpers
// (cost / delta / ResolveUpgradedStat) deliberately avoid UnityEngine so they are CI-unit-
// testable exactly like the Phase-0 ConfigResolver. The service reads UpgradesConfig (a
// ScriptableObject) for the curve, but all *math* lives in UpgradeMath (no UnityEngine).
//
// SCAFFOLD STATUS: authored, NOT compiled (no Unity 6 / no BaaS here — ADR-0-001/-2-001).
// Every server round-trip is a SEAM call against a stub; the real BaaS spend/grant validation,
// the RC retune of the curve, and the analytics land are DEFERRED (no backend in this env).

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bulwark.Data; // UpgradeTrack, UpgradesConfig, UpgradeStat, Currency
using Bulwark.Services.Analytics;

namespace Bulwark.Services.Economy
{
    // =====================================================================================
    // SERVER ECONOMY SEAM (spend/grant) — the SERVER-AUTHORITATIVE currency contract.
    //
    // OWNERSHIP NOTE: this seam is shared with module 3.1 (Server economy / wallet). 3.1 owns
    // the CANONICAL definition (ServerEconomy.cs). At integration (THIS PASS) the duplicate
    // IServerEconomy / UpgradeSpendResult / UpgradeRejectReason that were previously declared
    // here were REMOVED so exactly ONE declaration of each survives (the canonical one in
    // ServerEconomy.cs). UpgradesService below now CONSUMES the canonical types — it depends only
    // on the two members the canonical IServerEconomy exposes for it: SpendForUpgradeAsync(...)
    // and CachedBalance(...). See ServerEconomy.cs for the canonical definitions.
    // =====================================================================================

    // =====================================================================================
    // PURE MATH (no UnityEngine, no server) — CI-unit-testable like ConfigResolver.
    // All curve math + the cap clamp live here so they can be proven without the editor/backend.
    // =====================================================================================

    /// <summary>
    /// Pure, dependency-free upgrade math: cost curve, bounded stat delta, and the cap-clamped
    /// stat resolve the battle bake uses. NO UnityEngine, NO async, NO server — deterministic and
    /// unit-testable on CI. Every method CLAMPS level into the track's [0, maxLevel] cap first, so
    /// no caller can compute power beyond the HARD CAP (§3/§15), even with a tampered level.
    /// </summary>
    public static class UpgradeMath
    {
        /// <summary>Clamp a level into the track's hard cap range [0, maxLevel]. The cap is INVIOLABLE.</summary>
        public static int ClampLevel(int level, int maxLevel)
        {
            int cap = maxLevel < 0 ? 0 : maxLevel;
            if (level < 0) return 0;
            if (level > cap) return cap;
            return level;
        }

        /// <summary>True once a track is maxed; a maxed track can NEVER be upgraded further (HARD CAP).</summary>
        public static bool IsAtCap(int level, int maxLevel) => ClampLevel(level, maxLevel) >= (maxLevel < 0 ? 0 : maxLevel);

        /// <summary>
        /// Silver cost to buy the level AFTER <paramref name="fromLevel"/>:
        /// round(baseCostSilver * costGrowth^fromLevel). Returns 0 (and IsAtCap true) at the cap —
        /// there is nothing to buy. fromLevel is clamped, so a tampered over-cap level can't lower cost.
        /// </summary>
        public static long NextCostSilver(in UpgradeTrack track, int fromLevel)
        {
            int lvl = ClampLevel(fromLevel, track.maxLevel);
            if (lvl >= (track.maxLevel < 0 ? 0 : track.maxLevel)) return 0; // at cap → no purchasable level
            double growth = track.costGrowth <= 0f ? 1.0 : track.costGrowth;
            double cost = track.baseCostSilver * Math.Pow(growth, lvl);
            if (cost < 0d) cost = 0d;
            // round-half-away-from-zero to match the §13 3.2 "round(...)" intent (deterministic).
            long rounded = (long)Math.Round(cost, MidpointRounding.AwayFromZero);
            return rounded < 0 ? 0 : rounded;
        }

        /// <summary>
        /// The BOUNDED additive stat delta at a given level: deltaPerLevel * clamp(level). Because
        /// the level is clamped to maxLevel, the delta tops out at deltaPerLevel*maxLevel — the
        /// upgrade can NEVER push a stat beyond its capped ceiling (§3/§15, no P2W).
        /// </summary>
        public static float StatDelta(in UpgradeTrack track, int level)
            => track.deltaPerLevel * ClampLevel(level, track.maxLevel);

        /// <summary>
        /// PURE resolve the battle bake uses (§13 3.2): fold a unit's BASE stat to its upgraded value
        /// for one track at one level. The level is CLAMPED to the track's hard cap first, so the
        /// result is provably within the capped ceiling regardless of the level passed in. Used by
        /// BattleBootstrap when baking UnitSpawnStats (see integrationNotes — the hook is described,
        /// NOT edited here). Additive model mirrors the §4 base→modifier chain; never multiplicative
        /// stacking that could blow past the cap.
        /// </summary>
        public static float ResolveUpgradedStat(float baseStat, in UpgradeTrack track, int level)
            => baseStat + StatDelta(track, level);
    }

    // =====================================================================================
    // THE SERVICE — orchestrates server-validated spend/grant + read-cache + analytics.
    // =====================================================================================

    /// <summary>
    /// Drives the CAPPED per-unit upgrade tracks over UpgradesConfig. Server-owned levels are read
    /// from the authoritative PlayerProfile.Progress; spends/grants go through IServerEconomy and
    /// only the server's confirmed response advances a level (HARD RULE 1). The client cannot grant
    /// itself a level or exceed maxLevel (HARD RULE 3). Reads the curve from UpgradesConfig but
    /// delegates ALL math to the pure UpgradeMath so the logic stays CI-testable (§12).
    /// </summary>
    public sealed class UpgradesService
    {
        private readonly UpgradesConfig _config;
        private readonly IServerEconomy _server;
        private readonly IAnalytics _analytics;

        // Server-owned read CACHE of upgrade levels, keyed (unitId, stat). Hydrated from the
        // authoritative profile (HydrateFromProfile) and reconciled on every confirmed spend.
        // This is a DISPLAY/PRE-FLIGHT cache (HARD RULE 1) — NOT the source of truth.
        private readonly Dictionary<TrackKey, int> _levelCache = new Dictionary<TrackKey, int>();

        public UpgradesService(UpgradesConfig config, IServerEconomy server, IAnalytics analytics = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _server = server ?? throw new ArgumentNullException(nameof(server));
            _analytics = analytics; // optional; null = no telemetry
        }

        // ---- Profile hydration (server is the record of truth) -------------------------------

        /// <summary>
        /// Seed the read cache from the SERVER-AUTHORITATIVE profile (§9/§12). Progress values are
        /// stored as decimal-int strings under the per-track key (ProgressKey). Anything unparseable
        /// or out-of-range is treated as level 0 and CLAMPED to the cap — a tampered/garbage profile
        /// value can never grant over-cap power on the client. NO payload is logged (HARD RULE 2).
        /// </summary>
        public void HydrateFromProfile(IReadOnlyDictionary<string, string> progress)
        {
            _levelCache.Clear();
            if (progress == null) return;
            foreach (UpgradeTrack track in _config.tracks)
            {
                int level = 0;
                if (progress.TryGetValue(ProgressKey(track.unitId, track.stat), out string raw)
                    && int.TryParse(raw, out int parsed))
                {
                    level = parsed;
                }
                _levelCache[new TrackKey(track.unitId, track.stat)] = UpgradeMath.ClampLevel(level, track.maxLevel);
            }
        }

        /// <summary>The server-owned key under which a track's level lives in PlayerProfile.Progress.</summary>
        public static string ProgressKey(string unitId, UpgradeStat stat) => $"upg.{unitId}.{(int)stat}";

        /// <summary>
        /// The authored upgrade tracks (read-only view of UpgradesConfig.tracks). Used by the battle
        /// bridge to enumerate every (unitId, stat) and read its SERVER-OWNED current level for the bake
        /// (C2). Never null — an unconfigured asset yields an empty list.
        /// </summary>
        public IReadOnlyList<UpgradeTrack> Tracks => _config.tracks ?? EmptyTracks;
        private static readonly List<UpgradeTrack> EmptyTracks = new List<UpgradeTrack>();

        // ---- Reads (server-owned cache; never the truth) -------------------------------------

        /// <summary>
        /// The server-owned current level for a track (from the read cache), CLAMPED to the cap.
        /// Returns 0 for an un-hydrated or unknown track. This is a cache read — the server remains
        /// the authority; the value only changes here via HydrateFromProfile or a confirmed spend.
        /// </summary>
        public int CurrentLevel(string unitId, UpgradeStat stat)
        {
            if (!TryGetTrack(unitId, stat, out UpgradeTrack track)) return 0;
            int cached = _levelCache.TryGetValue(new TrackKey(unitId, stat), out int v) ? v : 0;
            return UpgradeMath.ClampLevel(cached, track.maxLevel);
        }

        /// <summary>True once the track is at its HARD CAP — no further upgrade is allowed (§3/§15).</summary>
        public bool IsAtCap(string unitId, UpgradeStat stat)
            => TryGetTrack(unitId, stat, out UpgradeTrack track)
               && UpgradeMath.IsAtCap(CurrentLevel(unitId, stat), track.maxLevel);

        /// <summary>
        /// Silver cost to buy the NEXT level from the current (server-owned) level. 0 when at cap.
        /// Pre-flight UX value; the SERVER re-derives and validates the real charge (HARD RULE 1).
        /// </summary>
        public long NextCostSilver(string unitId, UpgradeStat stat)
            => TryGetTrack(unitId, stat, out UpgradeTrack track)
               ? UpgradeMath.NextCostSilver(track, CurrentLevel(unitId, stat))
               : 0;

        /// <summary>The bounded stat delta at the current (cap-clamped) level. Battle bake / UI read.</summary>
        public float CurrentStatDelta(string unitId, UpgradeStat stat)
            => TryGetTrack(unitId, stat, out UpgradeTrack track)
               ? UpgradeMath.StatDelta(track, CurrentLevel(unitId, stat))
               : 0f;

        /// <summary>
        /// Resolve a base stat to its upgraded, cap-clamped value at the CURRENT server-owned level.
        /// The thin instance wrapper over the pure UpgradeMath.ResolveUpgradedStat the bake calls.
        /// </summary>
        public float ResolveUpgradedStat(float baseStat, string unitId, UpgradeStat stat)
            => TryGetTrack(unitId, stat, out UpgradeTrack track)
               ? UpgradeMath.ResolveUpgradedStat(baseStat, track, CurrentLevel(unitId, stat))
               : baseStat;

        // ---- The capped, server-validated upgrade -------------------------------------------

        /// <summary>
        /// Attempt to buy the next level of a track. PRE-FLIGHT (UX) checks reject obviously-invalid
        /// requests — unknown track, or already at the HARD CAP (§3/§15) — WITHOUT hitting the server.
        /// Otherwise it asks the SERVER to atomically charge Silver + grant the level. The level only
        /// advances if the server CONFIRMS (HARD RULE 1); the read cache is then reconciled to the
        /// server-returned authoritative level. The client never self-grants and never exceeds the cap.
        /// Analytics emits an event NAME + tiny non-PII numerics (HARD RULE 2 — no wallet payload).
        /// </summary>
        public async Task<UpgradeSpendResult> TryUpgradeAsync(string unitId, UpgradeStat stat)
        {
            if (!TryGetTrack(unitId, stat, out UpgradeTrack track))
            {
                Emit("upgrade_rejected_unknown_track", unitId, stat, -1);
                return UpgradeSpendResult.Rejected(0, _server.CachedBalance(Currency.Silver), UpgradeRejectReason.UnknownTrack);
            }

            int fromLevel = CurrentLevel(unitId, stat); // server-owned, cap-clamped

            // HARD CAP pre-flight (§3/§15): refuse to even REQUEST an at/over-cap upgrade.
            if (UpgradeMath.IsAtCap(fromLevel, track.maxLevel))
            {
                Emit("upgrade_rejected_at_cap", unitId, stat, fromLevel);
                return UpgradeSpendResult.Rejected(fromLevel, _server.CachedBalance(Currency.Silver), UpgradeRejectReason.AtHardCap);
            }

            long cost = UpgradeMath.NextCostSilver(track, fromLevel);

            // SERVER-AUTHORITATIVE spend+grant (DEFERRED round-trip: stub here, BaaS at integration).
            // We pass the hard cap so the server re-enforces it; the client's check is deterrence only.
            UpgradeSpendResult result = await _server.SpendForUpgradeAsync(
                unitId, stat, cost, fromLevel, track.maxLevel);

            if (result.Confirmed)
            {
                // Reconcile the read cache to the SERVER's confirmed level (re-clamped defensively).
                _levelCache[new TrackKey(unitId, stat)] = UpgradeMath.ClampLevel(result.NewLevel, track.maxLevel);
                Emit("upgrade_confirmed", unitId, stat, result.NewLevel);
            }
            else
            {
                Emit("upgrade_rejected_server", unitId, stat, fromLevel);
            }
            return result;
        }

        // ---- internals -----------------------------------------------------------------------

        /// <summary>Find the authored UpgradeTrack for a (unitId, stat). False if none is configured.</summary>
        public bool TryGetTrack(string unitId, UpgradeStat stat, out UpgradeTrack track)
        {
            List<UpgradeTrack> tracks = _config.tracks;
            if (tracks != null)
            {
                for (int i = 0; i < tracks.Count; i++)
                {
                    if (tracks[i].stat == stat && string.Equals(tracks[i].unitId, unitId, StringComparison.Ordinal))
                    {
                        track = tracks[i];
                        return true;
                    }
                }
            }
            track = default;
            return false;
        }

        /// <summary>
        /// Emit a single consolidated analytics event: NAME + tiny non-PII numeric facts only
        /// (unit id, stat int, level). NEVER the wallet/profile/save payload (HARD RULE 2).
        /// </summary>
        private void Emit(string name, string unitId, UpgradeStat stat, int level)
        {
            if (_analytics == null) return;
            _analytics.Emit(name, new Dictionary<string, object>
            {
                { "unit", unitId },
                { "stat", (int)stat },
                { "level", level },
            });
        }

        /// <summary>Composite cache key for a (unitId, stat) track.</summary>
        private readonly struct TrackKey : IEquatable<TrackKey>
        {
            public readonly string UnitId;
            public readonly UpgradeStat Stat;
            public TrackKey(string unitId, UpgradeStat stat) { UnitId = unitId; Stat = stat; }
            public bool Equals(TrackKey other) => Stat == other.Stat && string.Equals(UnitId, other.UnitId, StringComparison.Ordinal);
            public override bool Equals(object obj) => obj is TrackKey k && Equals(k);
            public override int GetHashCode()
            {
                unchecked { return ((UnitId != null ? UnitId.GetHashCode() : 0) * 397) ^ (int)Stat; }
            }
        }
    }
}

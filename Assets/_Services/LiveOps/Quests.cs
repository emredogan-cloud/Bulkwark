// BULWARK — QuestService (live-ops retention). Phase 4 §13 4.5, §8/§10.
//
// WHAT THIS IS: a daily/weekly quest tracker over QuestDef[] (Bulwark.Data). Progress is keyed by
// objectiveKey/targetCount and is SERVER-OWNED (telemetry-driven): the client reports observed play
// events, the server holds the authoritative counter, and on completion the server GRANTS the
// gameplay-safe RewardGrant. The client never self-grants (§12 SERVER-AUTHORITATIVE).
//
// INVIOLABLE FAIRNESS:
//  * Quests REWARD play, they never GATE it. There is NO energy/stamina/ticket cost to start, play,
//    or claim a quest (decision-log §1 CUT: no energy/stamina gates). Completing a quest costs the
//    player NOTHING and consumes no resource.
//  * Every reward is a RewardGrant that MUST pass MonetizationSafety.IsGameplaySafe before it is
//    granted — cosmetic / Silver / Gems / PassXP / CosmeticShards / ChestKey only. The RewardKind
//    enum has no power member, so a quest can never grant stats/units/upgrades/ranked advantage
//    (rule #1). A malformed/non-safe reward is REFUSED (fail-closed), the quest stays unclaimed.
//  * All grants route through the Phase-3 server seams (IServerEconomy for currency-kind rewards,
//    IEntitlementService for cosmetic/shard/chest-key entitlements). No client-side balance math.
//
// This file also defines the SHARED live-ops seams reused by LoginStreak.cs:
//   * ITrustedClock — trusted/server-anchored "now" (daily/weekly cadence must never trust the
//     device clock; the real source is the server, DEFERRED).
//   * RewardFulfillment — the one place a gameplay-safe RewardGrant is mapped to a server grant.
//
// PURE C# (no UnityEngine) so it is CI-unit-testable like ServerEconomy (§12 meta = services, NOT
// the ECS battle sim). SCAFFOLD STATUS: authored, NOT compiled here (CI validates).
// DEFERRED: the REAL server round-trip (BaaS adapter behind IBackendClient / IServerEconomy /
// IEntitlementService), the trusted-time source, and analytics landing — no backend in this env.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bulwark.Data;
using Bulwark.Services.Analytics;
using Bulwark.Services.Economy;
using Bulwark.Services.Monetization;

namespace Bulwark.Services.LiveOps
{
    /// <summary>
    /// Trusted "now" for cadence boundaries. Daily/weekly rollover and login streaks MUST anchor to a
    /// trusted source (the server), NEVER the freely-settable device clock — otherwise a player could
    /// re-roll dailies or fake a streak by changing the system time. The default implementation reads
    /// the OS clock as a stand-in; the REAL trusted source (server time fetched at auth) is DEFERRED.
    /// </summary>
    public interface ITrustedClock
    {
        /// <summary>Trusted UTC "now". Server-anchored in production (DEFERRED).</summary>
        DateTimeOffset UtcNow { get; }
    }

    /// <summary>
    /// Stand-in clock that reads the OS UTC clock. PROVISIONAL ONLY — production replaces this with a
    /// server-anchored clock so cadence/streaks cannot be gamed by changing the device time (DEFERRED).
    /// </summary>
    public sealed class SystemTrustedClock : ITrustedClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// The SINGLE place a gameplay-safe <see cref="RewardGrant"/> becomes a SERVER grant. Currency-kind
    /// rewards (Silver/Gems/PassXP) route through <see cref="IServerEconomy"/>; entitlement-kind rewards
    /// (Cosmetic/CosmeticShards/ChestKey) route through <see cref="IEntitlementService"/>. EVERY grant is
    /// guarded by <see cref="MonetizationSafety.IsGameplaySafe"/> first (fail-closed). The client never
    /// self-grants — it adopts only the server's confirmation (§12). No save-state payload is logged (§12).
    ///
    /// Shared by QuestService and LoginStreakService so reward fulfillment is defined ONCE.
    /// </summary>
    public static class RewardFulfillment
    {
        /// <summary>Terse, non-PII outcome codes for a reward grant (no payload — §12).</summary>
        public const string NotGameplaySafe = "not_gameplay_safe"; // FAILED the §8/§9/§10 safety guard
        public const string ZeroAmount      = "zero_amount";        // amount==0: nothing to grant (treated OK no-op)
        public const string GrantOk         = "ok";

        /// <summary>
        /// Grant ONE gameplay-safe reward through the appropriate server seam. Returns true only if the
        /// SERVER confirmed the grant (or the reward was a benign zero-amount no-op). Returns false —
        /// granting NOTHING — if the reward is not gameplay-safe or the server rejected it. The caller
        /// must treat a false result as "not awarded" and leave the quest/streak step UNCLAIMED.
        /// </summary>
        /// <param name="grant">The reward to grant (must be gameplay-safe).</param>
        /// <param name="economy">Server-authoritative currency seam (Silver/Gems/PassXP).</param>
        /// <param name="entitlements">Server-authoritative entitlement seam (cosmetic/shards/chest-key).</param>
        /// <param name="source">Terse audit tag (e.g. "quest:win_battles"); NOT a payload, NOT logged here.</param>
        /// <param name="analytics">Optional analytics wrapper; only minimized/non-PII deltas are emitted.</param>
        public static async Task<bool> TryGrantAsync(
            RewardGrant grant,
            IServerEconomy economy,
            IEntitlementService entitlements,
            string source,
            GameAnalytics analytics = null)
        {
            // FAIL-CLOSED §8/§9/§10: an unsafe/malformed reward grants NOTHING (rule #1, GATE 4).
            if (!MonetizationSafety.IsGameplaySafe(grant))
                return false;

            switch (grant.kind)
            {
                case RewardKind.Silver:
                case RewardKind.Gems:
                case RewardKind.PassXP:
                {
                    // amount==0 is a benign no-op (IsGameplaySafe permits >=0); nothing to grant.
                    if (grant.amount == 0) return true;
                    Currency currency = ToCurrency(grant.kind);
                    EconomyResult res = await economy.GrantAsync(currency, grant.amount, source);
                    if (res.Ok) analytics?.CurrencyGrant(currency, grant.amount, source); // delta+code only (§12)
                    return res.Ok;
                }

                case RewardKind.Cosmetic:
                {
                    // A specific cosmetic: a server-validated, gameplay-safe entitlement (§6). IsGameplaySafe
                    // already required a non-empty cosmeticId.
                    EntitlementResult er = await entitlements.GrantAsync(grant.cosmeticId, source);
                    return er.Ok;
                }

                case RewardKind.CosmeticShards:
                {
                    // Shards are a cosmetic CRAFT currency (§8), NOT one of the 4 wallet currencies, so they
                    // are held as a server entitlement, not in IServerEconomy. amount==0 is a no-op.
                    if (grant.amount == 0) return true;
                    // Encode the shard quantity in the audit source so the server credits the right amount;
                    // the entitlement id keeps the §9 gem-spend-style "cosmetic." prefix shape for the server.
                    string entId = "cosmetic.shards";
                    EntitlementResult er = await entitlements.GrantAsync(entId, source + ":+" + grant.amount.ToString());
                    return er.Ok;
                }

                case RewardKind.ChestKey:
                {
                    // Convenience: queues/opens an EARNED chest (no power, §8). amount==0 is a no-op.
                    if (grant.amount == 0) return true;
                    string entId = "convenience.chestkey";
                    EntitlementResult er = await entitlements.GrantAsync(entId, source + ":+" + grant.amount.ToString());
                    return er.Ok;
                }

                default:
                    // Unreachable: IsGameplaySafe already rejected unknown kinds. Fail-closed anyway.
                    return false;
            }
        }

        /// <summary>Map a currency-kind reward to its wallet Currency. Only the 3 server-owned
        /// currencies are valid here (Gold is in-battle/non-persistent and is NOT a reward kind, §9).</summary>
        public static Currency ToCurrency(RewardKind kind)
        {
            switch (kind)
            {
                case RewardKind.Silver: return Currency.Silver;
                case RewardKind.Gems:   return Currency.Gems;
                case RewardKind.PassXP: return Currency.PassXP;
                default: throw new ArgumentOutOfRangeException(nameof(kind), kind, "Not a wallet-currency reward kind.");
            }
        }
    }

    /// <summary>Immutable view of one quest's server-confirmed state (for UI; carries no payload, §12).</summary>
    public readonly struct QuestState
    {
        public readonly string QuestId;
        public readonly string ObjectiveKey;
        public readonly QuestCadence Cadence;
        public readonly int Progress;      // server-owned counter, clamped to [0, Target]
        public readonly int Target;        // QuestDef.targetCount
        public readonly bool Completed;    // Progress >= Target
        public readonly bool Claimed;      // reward granted (server-confirmed)

        public QuestState(string questId, string objectiveKey, QuestCadence cadence,
                          int progress, int target, bool completed, bool claimed)
        {
            QuestId = questId; ObjectiveKey = objectiveKey; Cadence = cadence;
            Progress = progress; Target = target; Completed = completed; Claimed = claimed;
        }

        public float Fraction => Target <= 0 ? 1f : Math.Min(1f, (float)Progress / Target);
    }

    /// <summary>Outcome of claiming a completed quest (UX only; no payload, §12).</summary>
    public readonly struct QuestClaimResult
    {
        public readonly bool Ok;
        public readonly string Error;   // terse code; null on success
        private QuestClaimResult(bool ok, string error) { Ok = ok; Error = error; }
        public static QuestClaimResult Success() => new QuestClaimResult(true, null);
        public static QuestClaimResult Failure(string e) => new QuestClaimResult(false, e ?? "unknown");

        public const string NotFound      = "not_found";
        public const string NotComplete   = "not_complete";
        public const string AlreadyClaimed = "already_claimed";
        public const string GrantRejected = "grant_rejected";   // server refused the (safe) grant
        public const string UnsafeReward  = "unsafe_reward";    // reward failed IsGameplaySafe (fail-closed)
    }

    /// <summary>
    /// Tracks daily/weekly quests over a QuestDef[] and claims their gameplay-safe rewards through the
    /// server. Progress is SERVER-OWNED in production (this service mirrors the server's counter so the
    /// UI can render it); play telemetry drives <see cref="ReportProgressAsync"/>. No quest gates play —
    /// there is no cost to start, advance, or claim (decision-log §1 CUT: no energy/stamina).
    ///
    /// Cadence boundaries (daily/weekly reset) anchor to <see cref="ITrustedClock"/> (server time),
    /// never the device clock, so dailies cannot be re-rolled by changing the system time.
    /// </summary>
    public sealed class QuestService
    {
        private readonly IServerEconomy _economy;
        private readonly IEntitlementService _entitlements;
        private readonly ITrustedClock _clock;
        private readonly GameAnalytics _analytics;

        // Live quest defs keyed by id (the active daily/weekly set the server issued for this period).
        private readonly Dictionary<string, QuestDef> _defs = new Dictionary<string, QuestDef>(StringComparer.Ordinal);

        // Mirror of the SERVER-owned progress/claim state (NOT an authority — adopted from the server).
        private readonly Dictionary<string, int> _progress = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly HashSet<string> _claimed = new HashSet<string>(StringComparer.Ordinal);

        // The cadence anchor of the currently-loaded set (UTC day for dailies / ISO week for weeklies),
        // used to detect a rollover so a stale set is not claimable across a reset.
        private int _dayAnchor = -1;     // days since epoch of the trusted "now" when loaded
        private int _weekAnchor = -1;    // ISO-week ordinal of the trusted "now" when loaded

        public QuestService(
            IServerEconomy economy,
            IEntitlementService entitlements,
            ITrustedClock clock = null,
            GameAnalytics analytics = null)
        {
            _economy = economy ?? throw new ArgumentNullException(nameof(economy));
            _entitlements = entitlements ?? throw new ArgumentNullException(nameof(entitlements));
            _clock = clock ?? new SystemTrustedClock();
            _analytics = analytics;
        }

        /// <summary>
        /// Load the active quest set (the daily/weekly defs the server issued for the current period).
        /// Stamps the trusted-time cadence anchors so a later rollover invalidates this set. Quests with
        /// a non-gameplay-safe reward are SKIPPED at load (fail-closed — they can never be claimed, §8).
        /// </summary>
        public void LoadActiveSet(IEnumerable<QuestDef> defs)
        {
            _defs.Clear();
            _progress.Clear();
            _claimed.Clear();

            if (defs != null)
            {
                foreach (var d in defs)
                {
                    if (d == null || string.IsNullOrEmpty(d.id)) continue;
                    // FAIL-CLOSED §8: refuse to even surface a quest whose reward is not gameplay-safe.
                    if (!MonetizationSafety.IsGameplaySafe(d.reward)) continue;
                    _defs[d.id] = d;
                    _progress[d.id] = 0;
                }
            }

            DateTimeOffset now = _clock.UtcNow;
            _dayAnchor = DayOrdinal(now);
            _weekAnchor = WeekOrdinal(now);
        }

        /// <summary>
        /// Adopt server-confirmed progress/claim state for a quest (e.g. after a profile read on login).
        /// SERVER WINS — this is a pure adopt, never client-authored advancement. Progress is clamped to
        /// the def's target; an unknown quest id is ignored.
        /// </summary>
        public void AdoptServerState(string questId, int serverProgress, bool serverClaimed)
        {
            if (string.IsNullOrEmpty(questId) || !_defs.TryGetValue(questId, out var def)) return;
            _progress[questId] = Clamp(serverProgress, 0, def.targetCount);
            if (serverClaimed) _claimed.Add(questId); else _claimed.Remove(questId);
        }

        /// <summary>
        /// Report observed play telemetry for an objective (e.g. "win_battles" +1). In production the
        /// SERVER increments the authoritative counter; here we advance the mirror and clamp to target.
        /// This NEVER costs the player anything — quests reward play, they don't gate it (decision-log §1).
        /// Returns the post-increment state of each matching quest. A rollover (trusted-time) since load
        /// is detected and reported via <see cref="IsCurrentPeriod"/>; stale sets are not advanced.
        /// </summary>
        public Task<IReadOnlyList<QuestState>> ReportProgressAsync(string objectiveKey, int delta = 1)
        {
            var touched = new List<QuestState>();
            if (string.IsNullOrEmpty(objectiveKey) || delta <= 0 || !IsCurrentPeriod)
                return Task.FromResult((IReadOnlyList<QuestState>)touched);

            foreach (var kv in _defs)
            {
                QuestDef def = kv.Value;
                if (!string.Equals(def.objectiveKey, objectiveKey, StringComparison.Ordinal)) continue;
                if (_claimed.Contains(def.id)) continue; // already done

                int cur = _progress.TryGetValue(def.id, out var p) ? p : 0;
                int next = Clamp(cur + delta, 0, def.targetCount);
                _progress[def.id] = next;
                touched.Add(Snapshot(def));
            }

            return Task.FromResult((IReadOnlyList<QuestState>)touched);
        }

        /// <summary>
        /// Claim a COMPLETED quest's reward through the server. Re-asserts gameplay-safety (fail-closed),
        /// grants via <see cref="RewardFulfillment"/>, and marks claimed ONLY if the server confirmed the
        /// grant. Idempotent: a second claim returns AlreadyClaimed and grants nothing. No cost to claim.
        /// </summary>
        public async Task<QuestClaimResult> ClaimAsync(string questId)
        {
            if (string.IsNullOrEmpty(questId) || !_defs.TryGetValue(questId, out var def))
                return QuestClaimResult.Failure(QuestClaimResult.NotFound);
            if (!IsCurrentPeriod)
                return QuestClaimResult.Failure(QuestClaimResult.NotComplete); // stale period: nothing to claim
            if (_claimed.Contains(questId))
                return QuestClaimResult.Failure(QuestClaimResult.AlreadyClaimed);

            int cur = _progress.TryGetValue(questId, out var p) ? p : 0;
            if (cur < def.targetCount)
                return QuestClaimResult.Failure(QuestClaimResult.NotComplete);

            // Re-assert safety at the claim boundary (defence in depth — GATE 4).
            if (!MonetizationSafety.IsGameplaySafe(def.reward))
                return QuestClaimResult.Failure(QuestClaimResult.UnsafeReward);

            string source = "quest:" + (def.objectiveKey ?? def.id);
            bool granted = await RewardFulfillment.TryGrantAsync(
                def.reward, _economy, _entitlements, source, _analytics);

            if (!granted)
                return QuestClaimResult.Failure(QuestClaimResult.GrantRejected);

            _claimed.Add(questId); // mark claimed ONLY after server-confirmed grant
            return QuestClaimResult.Success();
        }

        /// <summary>Current server-confirmed state of one quest (for UI). Empty struct if unknown.</summary>
        public QuestState GetState(string questId)
            => (!string.IsNullOrEmpty(questId) && _defs.TryGetValue(questId, out var def))
                ? Snapshot(def)
                : default;

        /// <summary>All currently-loaded quests' states (for the live-ops panel).</summary>
        public IReadOnlyList<QuestState> GetAll()
        {
            var list = new List<QuestState>(_defs.Count);
            foreach (var kv in _defs) list.Add(Snapshot(kv.Value));
            return list;
        }

        /// <summary>
        /// True iff the loaded set still belongs to the trusted-time current period (no daily/weekly
        /// rollover since LoadActiveSet). A stale set is neither advanceable nor claimable — the server
        /// would issue a fresh set; the client must reload rather than claim across a reset.
        /// </summary>
        public bool IsCurrentPeriod
        {
            get
            {
                if (_defs.Count == 0) return false;
                DateTimeOffset now = _clock.UtcNow;
                // A daily quest in the set requires the day anchor to still match; a weekly requires the
                // week anchor. We hold a mixed set, so require BOTH anchors valid for the kinds present.
                bool hasDaily = false, hasWeekly = false;
                foreach (var kv in _defs)
                {
                    if (kv.Value.cadence == QuestCadence.Daily) hasDaily = true;
                    else hasWeekly = true;
                }
                if (hasDaily && DayOrdinal(now) != _dayAnchor) return false;
                if (hasWeekly && WeekOrdinal(now) != _weekAnchor) return false;
                return true;
            }
        }

        // ----------------------------------------------------------------- helpers

        private QuestState Snapshot(QuestDef def)
        {
            int cur = _progress.TryGetValue(def.id, out var p) ? p : 0;
            bool completed = cur >= def.targetCount;
            bool claimed = _claimed.Contains(def.id);
            return new QuestState(def.id, def.objectiveKey, def.cadence,
                                  Clamp(cur, 0, def.targetCount), def.targetCount, completed, claimed);
        }

        private static int Clamp(int v, int lo, int hi) => v < lo ? lo : (v > hi ? hi : v);

        /// <summary>Whole UTC days since the Unix epoch — a stable daily-cadence ordinal.</summary>
        private static int DayOrdinal(DateTimeOffset now)
            => (int)(now.UtcDateTime.Date - DateTime.UnixEpoch).TotalDays;

        /// <summary>A stable weekly ordinal (epoch-anchored 7-day buckets). Independent of locale week
        /// rules so the boundary is deterministic and server-reproducible (DEFERRED: server supplies).</summary>
        private static int WeekOrdinal(DateTimeOffset now)
            => DayOrdinal(now) / 7;
    }
}

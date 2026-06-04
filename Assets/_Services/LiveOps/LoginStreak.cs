// BULWARK — LoginStreakService (live-ops retention). Phase 4 §13 4.5, §8/§10.
//
// WHAT THIS IS: a daily login streak with escalating, gameplay-safe rewards. "Today" is anchored to
// a TRUSTED, SERVER-OWNED clock (ITrustedClock from Quests.cs), never the device clock — so a streak
// cannot be faked or re-rolled by changing the system time. Streak length and last-claim day are
// SERVER-OWNED; this service mirrors the server's state for the UI and routes the reward grant
// through the server. The client never self-grants (§12 SERVER-AUTHORITATIVE).
//
// INVIOLABLE FAIRNESS:
//  * Every streak reward is a RewardGrant that MUST pass MonetizationSafety.IsGameplaySafe — cosmetic
//    / Silver / Gems / PassXP / CosmeticShards / ChestKey only. RewardKind has no power member, so a
//    streak can never grant stats/units/upgrades/ranked advantage (rule #1, GATE 4). An unsafe reward
//    is REFUSED (fail-closed) and the day stays unclaimed.
//  * NO coercive dark patterns: the streak NEVER gates play, costs nothing, and missing a day is not
//    punished beyond the streak counter resetting per the rules. There is no "pay to restore streak"
//    or any power lost — only cosmetic/currency cadence (decision-log §1; §10 ethical rules).
//  * All grants route through the Phase-3 server seams (IServerEconomy / IEntitlementService) via the
//    shared RewardFulfillment. No client-side balance math (§12).
//
// PURE C# (no UnityEngine) so it is CI-unit-testable (§12 meta = services, NOT the ECS battle sim).
// SCAFFOLD STATUS: authored, NOT compiled here (CI validates).
// DEFERRED: the REAL server round-trip + trusted-time source + analytics landing — no backend here.

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
    /// The escalating reward ladder for a login streak (§13 4.5). Each entry is a gameplay-safe
    /// RewardGrant for streak-day N (1-based). The ladder loops after its last entry so a long streak
    /// keeps paying out at the final, capped tier — there is no unbounded escalation (no P2W creep).
    /// All entries are validated gameplay-safe at construction; an unsafe entry is REJECTED.
    /// </summary>
    public sealed class LoginStreakLadder
    {
        private readonly RewardGrant[] _rungs;

        /// <summary>Build a ladder from day-1..day-N rewards. Throws if any reward is not gameplay-safe
        /// (fail-closed — an unsafe rung must never be configurable, §8). At least one rung is required.</summary>
        public LoginStreakLadder(IReadOnlyList<RewardGrant> rungs)
        {
            if (rungs == null || rungs.Count == 0)
                throw new ArgumentException("Login streak ladder needs at least one rung.", nameof(rungs));

            _rungs = new RewardGrant[rungs.Count];
            for (int i = 0; i < rungs.Count; i++)
            {
                if (!MonetizationSafety.IsGameplaySafe(rungs[i]))
                    throw new ArgumentException($"Login streak rung {i} is not gameplay-safe (§8/§9/§10).", nameof(rungs));
                _rungs[i] = rungs[i];
            }
        }

        public int Length => _rungs.Length;

        /// <summary>The gameplay-safe reward for a 1-based streak day. Days past the ladder length loop
        /// back to the final rung (capped escalation — never unbounded). Day &lt;= 0 returns rung 0.</summary>
        public RewardGrant RewardForDay(int streakDay)
        {
            if (_rungs.Length == 0) return default;
            if (streakDay <= 1) return _rungs[0];
            // Cap-and-loop at the final rung: once the ladder is exhausted, every further day pays the
            // top (capped) reward. This avoids any "the longer you pay, the more power" pattern.
            int idx = streakDay - 1;
            if (idx >= _rungs.Length) idx = _rungs.Length - 1;
            return _rungs[idx];
        }
    }

    /// <summary>Server-confirmed view of the streak (UI only; no payload, §12).</summary>
    public readonly struct LoginStreakState
    {
        public readonly int StreakDays;       // current consecutive-day count (server-owned)
        public readonly bool ClaimableToday;  // a reward is available for the trusted "today"
        public readonly int NextRewardDay;    // the streak-day the next claim pays out (== StreakDays+1 when claimable)

        public LoginStreakState(int streakDays, bool claimableToday, int nextRewardDay)
        {
            StreakDays = streakDays; ClaimableToday = claimableToday; NextRewardDay = nextRewardDay;
        }
    }

    /// <summary>Outcome of a daily login claim (UX only; no payload, §12).</summary>
    public readonly struct LoginClaimResult
    {
        public readonly bool Ok;
        public readonly int StreakDays;     // streak after this claim (== before on a no-op/failure)
        public readonly string Error;       // terse code; null on success
        private LoginClaimResult(bool ok, int streakDays, string error) { Ok = ok; StreakDays = streakDays; Error = error; }

        public static LoginClaimResult Success(int streakDays) => new LoginClaimResult(true, streakDays, null);
        public static LoginClaimResult Failure(int streakDays, string e) => new LoginClaimResult(false, streakDays, e ?? "unknown");

        public const string AlreadyClaimedToday = "already_claimed_today";
        public const string GrantRejected       = "grant_rejected";  // server refused the (safe) grant
        public const string UnsafeReward         = "unsafe_reward";   // reward failed IsGameplaySafe (fail-closed)
    }

    /// <summary>
    /// Daily login streak with escalating gameplay-safe rewards (§13 4.5). Cadence is trusted-time
    /// anchored (ITrustedClock = server time): one claim per trusted UTC day. Consecutive days advance
    /// the streak; a GAP of more than one day resets it to 1 (the streak rule). Server-owned state is
    /// mirrored here for the UI; the reward is granted through the server (no self-grant, §12).
    ///
    /// NO dark patterns: the streak never gates play, never charges to restore, and resets are the
    /// only consequence of a missed day (§10 ethical rules; decision-log §1).
    /// </summary>
    public sealed class LoginStreakService
    {
        private readonly LoginStreakLadder _ladder;
        private readonly IServerEconomy _economy;
        private readonly IEntitlementService _entitlements;
        private readonly ITrustedClock _clock;
        private readonly GameAnalytics _analytics;

        // Mirror of SERVER-owned streak state (NOT an authority — adopted from the server).
        private int _streakDays;            // current consecutive-day count
        private int _lastClaimDayOrdinal;   // trusted UTC day-ordinal of the last claim; -1 = never claimed

        public LoginStreakService(
            LoginStreakLadder ladder,
            IServerEconomy economy,
            IEntitlementService entitlements,
            ITrustedClock clock = null,
            GameAnalytics analytics = null)
        {
            _ladder = ladder ?? throw new ArgumentNullException(nameof(ladder));
            _economy = economy ?? throw new ArgumentNullException(nameof(economy));
            _entitlements = entitlements ?? throw new ArgumentNullException(nameof(entitlements));
            _clock = clock ?? new SystemTrustedClock();
            _analytics = analytics;
            _streakDays = 0;
            _lastClaimDayOrdinal = -1;
        }

        /// <summary>
        /// Adopt server-confirmed streak state (e.g. after a profile read on login). SERVER WINS — this
        /// is a pure adopt, never client-authored. <paramref name="lastClaimDayOrdinal"/> is the trusted
        /// UTC day-ordinal of the server's last recorded claim (-1 if never). Negative streaks clamp to 0.
        /// </summary>
        public void AdoptServerState(int streakDays, int lastClaimDayOrdinal)
        {
            _streakDays = streakDays < 0 ? 0 : streakDays;
            _lastClaimDayOrdinal = lastClaimDayOrdinal;
        }

        /// <summary>Current server-confirmed streak state for the trusted "today" (UI only).</summary>
        public LoginStreakState GetState()
        {
            int today = DayOrdinal(_clock.UtcNow);
            bool claimable = _lastClaimDayOrdinal != today; // not yet claimed this trusted day
            int prospectiveStreak = ProspectiveStreakFor(today);
            return new LoginStreakState(_streakDays, claimable, prospectiveStreak);
        }

        /// <summary>
        /// Claim today's login reward. Idempotent per trusted UTC day: a second claim the same day is a
        /// no-op (AlreadyClaimedToday). Advances the streak (consecutive day) or resets it to 1 (gap),
        /// then grants the ladder's gameplay-safe reward for the resulting streak-day through the server.
        /// State is committed ONLY if the server confirmed the grant. Costs the player nothing.
        /// </summary>
        public async Task<LoginClaimResult> ClaimDailyAsync()
        {
            int today = DayOrdinal(_clock.UtcNow);

            // One claim per trusted day (idempotent; no dark-pattern re-claim).
            if (_lastClaimDayOrdinal == today)
                return LoginClaimResult.Failure(_streakDays, LoginClaimResult.AlreadyClaimedToday);

            // Compute the streak this claim would establish (consecutive → +1; gap → reset to 1).
            int newStreak = ProspectiveStreakFor(today);

            RewardGrant reward = _ladder.RewardForDay(newStreak);

            // Re-assert safety at the claim boundary (defence in depth — GATE 4). The ladder already
            // validated every rung, but a malformed grant must still grant NOTHING (fail-closed).
            if (!MonetizationSafety.IsGameplaySafe(reward))
                return LoginClaimResult.Failure(_streakDays, LoginClaimResult.UnsafeReward);

            string source = "login_streak:day" + newStreak.ToString();
            bool granted = await RewardFulfillment.TryGrantAsync(
                reward, _economy, _entitlements, source, _analytics);

            if (!granted)
                return LoginClaimResult.Failure(_streakDays, LoginClaimResult.GrantRejected);

            // Commit the mirror ONLY after a server-confirmed grant (server is the authority).
            _streakDays = newStreak;
            _lastClaimDayOrdinal = today;
            return LoginClaimResult.Success(_streakDays);
        }

        // ----------------------------------------------------------------- helpers

        /// <summary>
        /// The streak that a claim on <paramref name="today"/> would produce, per the streak rule:
        ///  * never claimed before, or today is exactly one trusted day after the last claim → streak+1
        ///    (or 1 if there was no prior streak);
        ///  * a gap of more than one day (missed at least a full day) → reset to 1;
        ///  * already claimed today → unchanged (callers guard this before granting).
        /// </summary>
        private int ProspectiveStreakFor(int today)
        {
            if (_lastClaimDayOrdinal < 0) return 1;            // first ever claim
            if (today == _lastClaimDayOrdinal) return _streakDays <= 0 ? 1 : _streakDays; // already today
            if (today == _lastClaimDayOrdinal + 1) return (_streakDays <= 0 ? 0 : _streakDays) + 1; // consecutive
            return 1;                                          // gap > 1 day → reset
        }

        /// <summary>Whole UTC days since the Unix epoch — the trusted daily-cadence ordinal.</summary>
        private static int DayOrdinal(DateTimeOffset now)
            => (int)(now.UtcDateTime.Date - DateTime.UnixEpoch).TotalDays;
    }
}

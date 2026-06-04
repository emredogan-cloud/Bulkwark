// BULWARK — OPT-IN REWARDED ADS (Phase 4 §13 4.3, §9 "opt-in rewarded ads", §10 ethical rules).
//
// §10 / decision-log §1 (INVIOLABLE, CUT): NO interstitials. Ads are **OPT-IN REWARDED ONLY** — the
// player explicitly chooses to watch a full ad in exchange for a disclosed, gameplay-safe reward
// (e.g. ~10 Gems / Silver, §9). There is no auto-play, no mid-flow interruption, no forced view. This
// service has NO method that shows an ad without an explicit player-initiated request, by construction.
//
// FAIRNESS (GATE 4): the reward is a RewardGrant that MUST pass MonetizationSafety.IsGameplaySafe —
// currency / PassXP / shards only, NEVER power. RewardKind has no power member; an unsafe/malformed
// reward is REFUSED (fail-closed). Watching ads can never buy advantage — only the same earned
// currency play already grants (§9 lists rewarded ads as an *earned* gem source).
//
// RESPECT THE PLAYER (P4, §10 "no dark patterns"): a DAILY CAP bounds how many rewarded views are
// offered, so the loop is a light lever, never a grind/coercion. The cap is server-owned in production.
//
// SERVER-AUTHORITATIVE (§12, HARD RULE #1): the reward is granted SERVER-SIDE only after the ads SDK
// confirms a COMPLETED view (ad attribution); the client never self-grants from an unverified callback.
// Routed through the Phase-3 IServerEconomy. NO save-state logging (§12) — codes/scalars only.
//
// PURE C# seam (no UnityEngine / no ads SDK types leak across IRewardedAdProvider) so it is
// CI-unit-testable like IAP/Shop (§12 boundary: monetization = services, NOT the ECS battle sim).
// SCAFFOLD STATUS: authored, NOT compiled here. DEFERRED: the real rewarded-ads SDK adapter
// (AdMob/Unity LevelPlay/etc.) + the server-side completed-view attribution — none here.

using System;
using System.Threading.Tasks;
using Bulwark.Data;
using Bulwark.Services.Economy;
using Bulwark.Services.Analytics;

namespace Bulwark.Services.Monetization
{
    /// <summary>How a rewarded-ad view ended (from the SDK). Only Completed earns the reward.</summary>
    public enum AdViewOutcome : int
    {
        Completed = 0,     // the player watched to the rewarded threshold → eligible for the grant
        SkippedEarly = 1,  // closed before the rewarded threshold → NO reward (not a punishment, just no grant)
        NoFill = 2,        // no ad available right now (no inventory) → NO reward, try later
        Failed = 3,        // SDK/network error → NO reward
    }

    /// <summary>Result of an opt-in rewarded-ad SHOW from the SDK seam. Carries a verification token the
    /// SERVER validates for attribution (the client never trusts a raw "completed" flag — §12).</summary>
    public readonly struct AdViewResult
    {
        public readonly AdViewOutcome Outcome;
        public readonly string AttributionToken; // opaque SDK token; SERVER-validated for the grant (DEFERRED)

        public AdViewResult(AdViewOutcome outcome, string attributionToken)
        { Outcome = outcome; AttributionToken = attributionToken; }

        public static AdViewResult Done(string token) => new AdViewResult(AdViewOutcome.Completed, token);
        public static AdViewResult Not(AdViewOutcome outcome) => new AdViewResult(outcome, null);
    }

    /// <summary>
    /// The REWARDED-AD SDK seam. A platform adapter (AdMob / Unity LevelPlay / etc.) implements this; it
    /// MUST only ever present an ad in response to an explicit, player-initiated <see cref="ShowAsync"/>
    /// call (opt-in) — never an interstitial. Pure interface — no SDK types leak across, so the meta layer
    /// stays CI-testable. DEFERRED: the real SDK adapter + server-side completed-view attribution.
    /// </summary>
    public interface IRewardedAdProvider
    {
        /// <summary>True iff a rewarded ad is loaded and ready to show (so the UI can enable the opt-in button).</summary>
        bool IsAvailable { get; }

        /// <summary>Show the rewarded ad NOW — called only from an explicit player tap (opt-in). Resolves
        /// with the view outcome + an attribution token to be SERVER-validated. Never auto-invoked.</summary>
        Task<AdViewResult> ShowAsync(string placementId);
    }

    /// <summary>Why an opt-in watch did not grant (UX/audit; never a payload — §12).</summary>
    public enum RewardedAdReject : int
    {
        None = 0,
        NotAvailable,     // no ad loaded / no fill
        DailyCapReached,  // the respectful daily cap is hit — come back tomorrow (no coercion to pay)
        NotCompleted,     // the player skipped early / SDK failed → no reward (no penalty)
        UnsafeReward,     // the configured reward FAILED IsGameplaySafe — REFUSED (fail-closed, GATE 4)
        ServerRejected,   // the server refused the grant / attribution (validation/transport)
    }

    /// <summary>Outcome of an opt-in rewarded watch. No balance payload beyond the server-confirmed
    /// scalar the wallet already adopted (§12).</summary>
    public readonly struct RewardedAdResult
    {
        public readonly bool Ok;
        public readonly RewardKind Kind;     // what was granted (meaningful only when Ok)
        public readonly int Amount;          // granted amount (meaningful only when Ok)
        public readonly int RemainingToday;  // views still available today after this one (server-owned mirror)
        public readonly RewardedAdReject Reason;

        public RewardedAdResult(bool ok, RewardKind kind, int amount, int remainingToday, RewardedAdReject reason)
        { Ok = ok; Kind = kind; Amount = amount; RemainingToday = remainingToday; Reason = reason; }

        public static RewardedAdResult Granted(RewardKind kind, int amount, int remaining)
            => new RewardedAdResult(true, kind, amount, remaining, RewardedAdReject.None);
        public static RewardedAdResult Rejected(RewardedAdReject reason, int remaining)
            => new RewardedAdResult(false, default, 0, remaining, reason);
    }

    /// <summary>
    /// OPT-IN rewarded-ad service (§13 4.3). The player taps "watch for reward"; this asks the SDK to show
    /// ONE rewarded ad and, ONLY on a completed view, grants a disclosed gameplay-safe reward through the
    /// SERVER (IServerEconomy). A respectful daily cap bounds the offers. There is NO interstitial path and
    /// NO non-opt-in show — the type cannot present an ad except via WatchForRewardAsync (a player tap).
    /// </summary>
    public sealed class RewardedAdService
    {
        private readonly IRewardedAdProvider _ads;
        private readonly IServerEconomy _economy;
        private readonly GameAnalytics _analytics; // optional; minimized/non-PII events only (§12)

        // The disclosed reward per completed view (gameplay-safe; currency/PassXP/shards only). PROVISIONAL
        // (LSD-owned, §9 e.g. ~10 Gems), RC/SO-overridable; NEVER power (RewardKind has no power member).
        private readonly RewardGrant _rewardPerView;

        // Respectful daily cap (P4 §10 no-dark-patterns). Server-owned in production; mirrored here.
        private readonly int _dailyCap;
        private int _watchedToday;
        private int _dayAnchorOrdinal = -1; // trusted-UTC day-ordinal the count belongs to (rollover resets)

        public const string DefaultPlacement = "rewarded.default";

        public RewardedAdService(
            IRewardedAdProvider ads,
            IServerEconomy economy,
            RewardGrant rewardPerView,
            int dailyCap = 5,
            GameAnalytics analytics = null)
        {
            _ads = ads ?? throw new ArgumentNullException(nameof(ads));
            _economy = economy ?? throw new ArgumentNullException(nameof(economy));
            _analytics = analytics;
            _rewardPerView = rewardPerView;
            _dailyCap = dailyCap < 0 ? 0 : dailyCap;
        }

        /// <summary>True iff the opt-in button should be enabled: an ad is loaded AND the daily cap allows
        /// another view for the trusted "today". UX gate only — the server re-checks on grant.</summary>
        public bool CanWatch(int trustedDayOrdinal)
            => _ads.IsAvailable && RemainingToday(trustedDayOrdinal) > 0;

        /// <summary>Views still available for the trusted "today" (resets the mirror on a day rollover).</summary>
        public int RemainingToday(int trustedDayOrdinal)
        {
            if (trustedDayOrdinal != _dayAnchorOrdinal) { _dayAnchorOrdinal = trustedDayOrdinal; _watchedToday = 0; }
            int left = _dailyCap - _watchedToday;
            return left < 0 ? 0 : left;
        }

        /// <summary>Adopt the server-owned watched-today count (e.g. after a profile read). SERVER WINS.</summary>
        public void AdoptServerCount(int trustedDayOrdinal, int watchedToday)
        {
            _dayAnchorOrdinal = trustedDayOrdinal;
            _watchedToday = watchedToday < 0 ? 0 : watchedToday;
        }

        /// <summary>
        /// OPT-IN watch: the ONLY way to present a rewarded ad — call this from an explicit player tap.
        /// Flow: (1) respect the daily cap; (2) show ONE rewarded ad via the SDK seam; (3) on a COMPLETED
        /// view only, assert the reward IsGameplaySafe (fail-closed) and GRANT it server-side
        /// (IServerEconomy); (4) the client adopts only the server's confirmed balance and increments the
        /// (server-owned) daily count. A skip/no-fill/failure grants NOTHING and is not punished.
        /// <paramref name="trustedDayOrdinal"/> comes from the trusted clock (server time), never the device clock.
        /// DEFERRED: the live ads SDK + server-side completed-view attribution.
        /// </summary>
        public async Task<RewardedAdResult> WatchForRewardAsync(int trustedDayOrdinal, string placementId = DefaultPlacement)
        {
            int remaining = RemainingToday(trustedDayOrdinal);
            if (remaining <= 0)
                return RewardedAdResult.Rejected(RewardedAdReject.DailyCapReached, 0);
            if (!_ads.IsAvailable)
                return RewardedAdResult.Rejected(RewardedAdReject.NotAvailable, remaining);

            // GATE 4: the configured reward must be gameplay-safe BEFORE we even show the ad (fail-closed).
            if (!MonetizationSafety.IsGameplaySafe(_rewardPerView))
                return RewardedAdResult.Rejected(RewardedAdReject.UnsafeReward, remaining);

            // (2) Show ONE rewarded ad (player-initiated; opt-in). No interstitial, no auto-play.
            AdViewResult view = await _ads.ShowAsync(placementId ?? DefaultPlacement);
            if (view.Outcome != AdViewOutcome.Completed)
            {
                var reason = view.Outcome == AdViewOutcome.NoFill ? RewardedAdReject.NotAvailable
                                                                  : RewardedAdReject.NotCompleted;
                return RewardedAdResult.Rejected(reason, remaining); // skipped/failed → no reward, no penalty
            }

            // (3)+(4) Completed → grant the gameplay-safe reward SERVER-SIDE (currency/PassXP only). The
            // attribution token is what the SERVER validates in production; we pass it as the audit source.
            bool granted = await GrantRewardAsync(view.AttributionToken);
            if (!granted)
                return RewardedAdResult.Rejected(RewardedAdReject.ServerRejected, remaining);

            _watchedToday++; // mirror the server-owned count (server is the durable authority)
            int left = RemainingToday(trustedDayOrdinal);
            return RewardedAdResult.Granted(_rewardPerView.kind, _rewardPerView.amount, left);
        }

        // ------------------------------------------------------------------ internals

        /// <summary>Grant the per-view reward through the SERVER. Only the 3 server-owned currencies are
        /// valid for a rewarded ad (Silver/Gems/PassXP, §9); cosmetic/shard/chest-key ad rewards are out of
        /// scope at MVP (kept simple). NEVER grants power (no power kind). Re-asserts safety (defense in depth).</summary>
        private async Task<bool> GrantRewardAsync(string attributionToken)
        {
            RewardGrant g = _rewardPerView;
            if (!MonetizationSafety.IsGameplaySafe(g)) return false;       // fail-closed
            if (g.amount <= 0) return false;                              // a rewarded view must pay something

            Currency currency;
            switch (g.kind)
            {
                case RewardKind.Silver: currency = Currency.Silver; break;
                case RewardKind.Gems:   currency = Currency.Gems;   break;
                case RewardKind.PassXP: currency = Currency.PassXP; break;
                default: return false; // only wallet currencies are valid rewarded-ad payouts at MVP (fail-closed)
            }

            // The audit source is the OPT-IN, server-validated attribution (never a payload — §12).
            string source = "ad.optin." + (string.IsNullOrEmpty(attributionToken) ? "completed" : attributionToken);
            EconomyResult res = await _economy.GrantAsync(currency, g.amount, source);
            if (res.Ok) _analytics?.CurrencyGrant(currency, g.amount, "ad_optin"); // delta+code only (§12)
            return res.Ok;
        }
    }
}

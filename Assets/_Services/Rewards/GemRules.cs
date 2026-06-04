// BULWARK — GEM ECONOMY rules (§13 4.4, §9 "gems CANNOT"). Phase 4 meta/monetization layer.
//
// SCOPE (§9): Gems are the premium currency. They are EARNED (first-clear diminishing, quests,
// opt-in rewarded-ad watch, login streak, pass), STORED server-authoritatively (the client holds
// only a reconciled CRDT cache — Wallet.cs), and SPENT exclusively on cosmetics / battle-pass /
// convenience. This file ENCODES that economy and — critically — the INVIOLABLE "gems CANNOT"
// list as an explicit, testable predicate so GATE 4 can audit it.
//
// INVIOLABLE FAIRNESS (GATE 4): gems may NEVER buy raw power, uncapped upgrades, unit/commander
// power, ranked advantage, or fuel gacha-for-power (§9; decision-log §1 CUT). The single guard is
// Bulwark.Data.MonetizationSafety.IsGemSpendAllowed (an ALLOW-LIST: cosmetic./pass./convenience./
// chestskip./slot. only). GemSpend(sku) REFUSES anything that fails it, and IsProhibited(sku)
// mirrors the "gems CANNOT" list as a positive deny-check for clarity/regulatory display.
//
// SERVER-AUTHORITATIVE (§12, HARD RULE #1): the balance lives on the server. EARNING routes through
// IServerEconomy.GrantAsync; SPENDING routes a gem spend through IEntitlementService.PurchaseWith-
// GemsAsync (which itself debits Gems via IServerEconomy and records the entitlement). The client
// NEVER self-grants or self-debits gems — it adopts only the server's confirmed balance.
//
// 4 CURRENCIES ONLY (§9): Gold/Silver/Gems/PassXP. No new currency is introduced here. Gold is
// in-battle/non-persistent and is NEVER a gem source or sink.
//
// PURE C# (no UnityEngine) so this is CI-unit-testable like ServerEconomy/Wallet (§12 boundary:
// meta/monetization = services, NOT the ECS battle sim). NO save-state logging (§12).
// SCAFFOLD STATUS: authored, NOT compiled here (no Unity 6 / no BaaS — ADR-0-001 / ADR-2-001).
// DEFERRED: the REAL server round-trips (IServerEconomy grant/spend + IEntitlementService) land
// behind a BaaS adapter; opt-in rewarded-ad attribution lands behind the ads SDK — none here.

using System;
using System.Threading.Tasks;
using Bulwark.Data;
using Bulwark.Services.Economy;
using Bulwark.Services.Monetization;

namespace Bulwark.Services.Rewards
{
    /// <summary>
    /// The disclosed, server-authoritative gem SOURCES (§9 "Earned"). These are the ONLY ways a
    /// player obtains Gems. NONE of them is a paid pack here (IAP gem packs live in the §10 shop and
    /// route through IEntitlementService.RedeemIapReceiptAsync server-side). Every source GRANTS via
    /// IServerEconomy — the client never mints gems.
    /// </summary>
    public enum GemSource : int
    {
        FirstClear  = 0, // campaign first-clear — DIMINISHING per clear (anti-farm, §9)
        Quest       = 1, // daily/weekly QuestDef reward (§13 4.5)
        AdWatch     = 2, // OPT-IN rewarded ad only (decision-log §1: NO interstitials; rewarded ONLY)
        LoginStreak = 3, // daily login streak
        Pass        = 4, // battle-pass track reward (§13 4.2)
    }

    /// <summary>Why a gem SPEND was rejected (UX/audit; never a payload — §12).</summary>
    public enum GemSpendReject : int
    {
        None = 0,
        ProhibitedSku,    // failed the §9 "gems CANNOT" guard (raw power/upgrades/ranked/gacha) — REFUSED
        NonPositivePrice, // price <= 0
        InsufficientGems, // server-checked: not enough Gems
        ServerRejected,   // generic server-side rejection (validation/transport)
    }

    /// <summary>Outcome of a gem spend. Carries the granted entitlement id + a UX reject reason.
    /// NO balance payload beyond the server-confirmed scalar the wallet already adopted (§12).</summary>
    public readonly struct GemSpendResult
    {
        public readonly bool Ok;
        public readonly string EntitlementId;   // the cosmetic/pass/convenience sku that was granted
        public readonly GemSpendReject Reason;  // why a non-Ok happened (UX/audit only)

        public GemSpendResult(bool ok, string entitlementId, GemSpendReject reason)
        {
            Ok = ok; EntitlementId = entitlementId; Reason = reason;
        }

        public static GemSpendResult Success(string id) => new GemSpendResult(true, id, GemSpendReject.None);
        public static GemSpendResult Rejected(GemSpendReject reason) => new GemSpendResult(false, null, reason);
    }

    /// <summary>
    /// The §9 gem economy: EARN (server grant), STORE (server-authoritative; client CRDT cache via
    /// Wallet.cs), SPEND (cosmetics/pass/convenience ONLY — gated by MonetizationSafety). This type
    /// holds NO balance of its own — the authority is the server (IServerEconomy) and the client's
    /// reconciled view is the Wallet. It is a thin, trust-the-server policy façade.
    /// </summary>
    public sealed class GemRules
    {
        private readonly IServerEconomy _economy;       // server authority over the Gems balance (§12)
        private readonly IEntitlementService _entitlements; // routes gem spends -> entitlement (server)

        public GemRules(IServerEconomy economy, IEntitlementService entitlements)
        {
            _economy = economy ?? throw new ArgumentNullException(nameof(economy));
            _entitlements = entitlements ?? throw new ArgumentNullException(nameof(entitlements));
        }

        // =================================================================== "gems CANNOT" (§9)

        /// <summary>
        /// The §9 "gems CANNOT" deny-list, made EXPLICIT for clarity/regulatory display and GATE 4.
        /// A sku is PROHIBITED for gem spending iff it is NOT on the cosmetic/pass/convenience allow-
        /// list (MonetizationSafety.IsGemSpendAllowed). This is the same single guard, expressed as a
        /// positive prohibition: gems CANNOT buy raw power, uncapped upgrades, unit/commander power,
        /// ranked advantage, or fuel gacha-for-power. (Those sku families are simply NOT minted; the
        /// allow-list is the proof — a power sku can never match cosmetic./pass./convenience./
        /// chestskip./slot.)
        /// </summary>
        public static bool IsProhibited(string sku) => !MonetizationSafety.IsGemSpendAllowed(sku);

        /// <summary>The canonical reason a sku is prohibited (for clarity-mode / store copy / audit).
        /// Returns null when the sku IS allowed.</summary>
        public static string ProhibitionReason(string sku)
        {
            if (!IsProhibited(sku)) return null;
            // We do not parse the sku for a specific category — by §9 ANYTHING outside the allow-list
            // is power-adjacent and refused. One terse, non-PII code (no payload — §12).
            return "gems_cannot_buy_power"; // raw power / uncapped upgrades / unit-commander power / ranked / gacha
        }

        // =================================================================== EARN (server grant)

        /// <summary>
        /// EARN gems from a disclosed §9 source. The AMOUNT is computed by the caller (e.g. first-clear
        /// diminishing curve from EconomyConfig, quest reward, login-streak table) — this method does
        /// the server GRANT and tags the audit source. The client adopts only the server's NewBalance
        /// (via Wallet). DEFERRED: the live grant round-trip + (for AdWatch) the opt-in ad attribution.
        /// </summary>
        public Task<EconomyResult> EarnAsync(GemSource source, long amount)
        {
            if (amount <= 0) return Task.FromResult(EconomyResult.Failure(EconomyError.NonPositiveAmount));
            // Gems are server-owned; GrantAsync re-validates sanity/overflow and is the single mint path.
            return _economy.GrantAsync(Currency.Gems, amount, SourceTag(source));
        }

        /// <summary>Terse, non-PII audit tag for a gem source (analytics/audit — never a payload, §12).</summary>
        public static string SourceTag(GemSource source)
        {
            switch (source)
            {
                case GemSource.FirstClear:  return "gem.earn.first_clear";
                case GemSource.Quest:       return "gem.earn.quest";
                case GemSource.AdWatch:     return "gem.earn.ad_optin"; // rewarded, OPT-IN only (CUT: no interstitials)
                case GemSource.LoginStreak: return "gem.earn.login_streak";
                case GemSource.Pass:        return "gem.earn.pass";
                default:                    return "gem.earn.unknown";
            }
        }

        // =================================================================== SPEND (allow-list only)

        /// <summary>
        /// SPEND gems on a sku — the §9 sink. REFUSES (without any server call) any sku that fails the
        /// "gems CANNOT" guard, so a power/gacha spend can never even reach the server. An allowed sku
        /// is routed through IEntitlementService.PurchaseWithGemsAsync, which debits Gems via the
        /// server-authoritative IServerEconomy and records the entitlement. The client never self-debits.
        ///
        /// `gemPrice` is the disclosed gem price (from ShopItemDef.priceGems / a convenience table).
        /// DEFERRED: the live server debit + entitlement write (BaaS adapter behind IEntitlementService).
        /// </summary>
        public async Task<GemSpendResult> GemSpend(string sku, long gemPrice)
        {
            // GATE 4: reject the "gems CANNOT" list FIRST — fail closed, before any server interaction.
            if (IsProhibited(sku)) return GemSpendResult.Rejected(GemSpendReject.ProhibitedSku);
            if (gemPrice <= 0)      return GemSpendResult.Rejected(GemSpendReject.NonPositivePrice);

            PurchaseResult pr = await _entitlements.PurchaseWithGemsAsync(sku, gemPrice);
            if (pr.Ok) return GemSpendResult.Success(pr.EntitlementId);

            // Map the terse server code to a UX reason (no payload — §12). PurchaseWithGemsAsync
            // returns "gem_spend_not_allowed" if its own guard tripped (defense in depth) and
            // "insufficient_funds"/spend errors from IServerEconomy.
            var reason = pr.Error == "insufficient_funds" ? GemSpendReject.InsufficientGems
                       : pr.Error == "gem_spend_not_allowed" ? GemSpendReject.ProhibitedSku
                       : GemSpendReject.ServerRejected;
            return GemSpendResult.Rejected(reason);
        }

        // =================================================================== STORE (read cache)

        /// <summary>
        /// The client's last server-confirmed Gems balance (the CRDT-reconciled cache; server is the
        /// authority — §12). For UX gating only (e.g. "can afford"); the server re-checks on spend.
        /// </summary>
        public long CachedGems() => _economy.CachedBalance(Currency.Gems);

        /// <summary>UX affordability pre-check (NOT authoritative — the server re-validates on spend).</summary>
        public bool CanAfford(long gemPrice) => gemPrice > 0 && CachedGems() >= gemPrice;
    }
}

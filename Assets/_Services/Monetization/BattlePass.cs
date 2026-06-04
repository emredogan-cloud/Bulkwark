// BULWARK — BATTLE PASS S0 service (Phase 4 §13 4.2, §10, §9, §12).
// Dual-track (free + premium) SEASON pass, EARN-BY-PLAY. This is the meta/monetization layer
// (a pure-C# service, §12 boundary — NOT the ECS battle sim). It drives a BattlePassDef and
// routes EVERY mutation through the server-authority seams so the client never self-grants.
//
// INVIOLABLE FAIRNESS (this file is part of the GATE 4 audit surface):
//   • ZERO PAY-TO-WIN. The pass grants ONLY gameplay-safe RewardGrants (cosmetic / Silver / Gems /
//     PassXP / CosmeticShards / ChestKey — §8/§9). RewardKind has NO power member; there is nothing
//     to grant that confers stats/units/upgrades/ranked advantage. EVERY reward is re-checked with
//     Bulwark.Data.MonetizationSafety.IsGameplaySafe IMMEDIATELY BEFORE granting — reject otherwise.
//   • SERVER-AUTHORITATIVE (§12). Tier/PassXP progress is OWNED BY THE SERVER: it is READ from the
//     IServerEconomy wallet (PassXP) — never client-trusted. AddPassXp accrues PassXP THROUGH the
//     server (IServerEconomy.GrantAsync) and the client adopts only the server-confirmed balance.
//     Cosmetic rewards are granted via IEntitlementService.GrantAsync; currency/PassXP rewards via
//     IServerEconomy.GrantAsync. The client NEVER mutates a balance or an entitlement locally.
//   • PREMIUM is a one-time ENTITLEMENT ("pass.{seasonId}.premium"), bought with GEMS
//     (IEntitlementService.PurchaseWithGemsAsync — gated by §9 MonetizationSafety.IsGemSpendAllowed,
//     which permits "pass." SKUs) OR with money via a server-validated IAP receipt
//     (RedeemIapReceiptAsync). Buying premium unlocks COSMETICS + CURRENCY only — NEVER power (§10).
//   • NO save-state logging (§12): results carry terse CODES / scalars, never wallet/profile payloads.
//
// SCAFFOLD STATUS: authored, NOT compiled here (no Unity 6 / no BaaS — ADR-0-001 / ADR-2-001 /
// ADR-4-001 Phase 4 is AUTHOR-ONLY; runtime DEFERRED). Pure C# so it is CI-unit-testable.
// DEFERRED: the real server round-trip (BaaS adapter behind IBackendClient), live IAP receipt
// validation, and the CosmeticShards / ChestKey inventory backend (no shard/chest wallet exists at
// MVP — those grants route through the server entitlement seam as a placeholder; see integrationNotes).

using System;
using System.Threading.Tasks;
using Bulwark.Data;
using Bulwark.Services.Economy;

namespace Bulwark.Services.Monetization
{
    /// <summary>Why a battle-pass operation did not complete. UX/diagnostics only — NO payload (§12).</summary>
    public enum BattlePassRejectReason
    {
        None = 0,
        NoSeason,           // null/invalid BattlePassDef
        TierOutOfRange,     // requested tier < 1 or > tierCount
        TierNotReached,     // server PassXP has not unlocked this tier yet (earn-by-play)
        PremiumNotOwned,    // premium track claim attempted without the premium entitlement
        AlreadyClaimed,     // this (tier, track) reward was already granted (idempotency guard)
        UnsafeReward,       // reward failed MonetizationSafety.IsGameplaySafe — REFUSED (GATE 4)
        ServerRejected,     // server grant/spend/read rejected the operation
        NotAuthenticated,   // no server session / wallet read failed
    }

    /// <summary>Server-confirmed progress snapshot (READ-ONLY mirror of server state). NO payload (§12).</summary>
    public readonly struct BattlePassProgress
    {
        public readonly long PassXp;        // server-confirmed accrued PassXP (the authority's value)
        public readonly int CurrentTier;    // highest tier UNLOCKED by PassXp (0..tierCount), server-derived
        public readonly int TierCount;      // total tiers in the season
        public readonly bool PremiumOwned;  // server-confirmed premium entitlement ownership

        public BattlePassProgress(long passXp, int currentTier, int tierCount, bool premiumOwned)
        {
            PassXp = passXp;
            CurrentTier = currentTier;
            TierCount = tierCount;
            PremiumOwned = premiumOwned;
        }
    }

    /// <summary>Outcome of accruing PassXP (earn-by-play). The client adopts ONLY the server's value.</summary>
    public readonly struct PassXpResult
    {
        public readonly bool Ok;
        public readonly long NewPassXp;      // server-confirmed total PassXP after accrual
        public readonly int NewTier;         // server-derived tier unlocked at NewPassXp (0..tierCount)
        public readonly BattlePassRejectReason Reason;

        public PassXpResult(bool ok, long newPassXp, int newTier, BattlePassRejectReason reason)
        {
            Ok = ok; NewPassXp = newPassXp; NewTier = newTier; Reason = reason;
        }

        public static PassXpResult Success(long passXp, int tier) => new PassXpResult(true, passXp, tier, BattlePassRejectReason.None);
        public static PassXpResult Failure(BattlePassRejectReason r, long passXp = 0, int tier = 0) => new PassXpResult(false, passXp, tier, r);
    }

    /// <summary>Outcome of claiming a tier reward (one track). Confirmed ONLY if the server granted it.</summary>
    public readonly struct ClaimResult
    {
        public readonly bool Ok;
        public readonly int Tier;
        public readonly bool Premium;        // which track was claimed
        public readonly BattlePassRejectReason Reason;

        public ClaimResult(bool ok, int tier, bool premium, BattlePassRejectReason reason)
        {
            Ok = ok; Tier = tier; Premium = premium; Reason = reason;
        }

        public static ClaimResult Success(int tier, bool premium) => new ClaimResult(true, tier, premium, BattlePassRejectReason.None);
        public static ClaimResult Failure(int tier, bool premium, BattlePassRejectReason r) => new ClaimResult(false, tier, premium, r);
    }

    /// <summary>
    /// BATTLE PASS S0 service (§13 4.2). A thin, trust-the-server façade over a <see cref="BattlePassDef"/>:
    /// it READS server-owned PassXP to derive tier progress, ACCRUES PassXP by play through the server,
    /// and CLAIMS gameplay-safe tier rewards through the server seams. The premium track is a one-time
    /// server entitlement ("pass.{seasonId}.premium"). NO power is grantable — RewardKind has no power
    /// member and every grant re-passes MonetizationSafety.IsGameplaySafe (GATE 4). NO save-state logged.
    ///
    /// Idempotency: this service keeps an in-memory record of which (tier, track) rewards it has already
    /// claimed in THIS session to avoid a double-grant from a double-tap. The DURABLE record of truth is
    /// the SERVER (a real backend marks claimed tiers in the player profile so claims survive a
    /// reinstall and can't be re-farmed) — that persistence is DEFERRED (no backend here); see
    /// integrationNotes. The local guard is a UX/anti-double-tap convenience, never the authority.
    /// </summary>
    public sealed class BattlePassService
    {
        private readonly BattlePassDef _def;
        private readonly IEntitlementService _entitlements;
        private readonly IServerEconomy _economy;

        // OPTIONAL analytics (one consolidated pipeline, §12). Null is allowed (no-op). Currency events
        // carry only delta + reason CODE + currency NAME — never a balance (§12). May be null in tests.
        private readonly Bulwark.Services.Analytics.GameAnalytics _analytics;

        // SESSION-local anti-double-tap guard for claimed (tier, track). NOT the authority — the server
        // is. Keyed "{tier}:{F|P}". DEFERRED: durable server-side claimed-tier record (integrationNotes).
        private readonly System.Collections.Generic.HashSet<string> _claimed =
            new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);

        public BattlePassService(
            BattlePassDef def,
            IEntitlementService entitlements,
            IServerEconomy economy,
            Bulwark.Services.Analytics.GameAnalytics analytics = null)
        {
            _def = def ?? throw new ArgumentNullException(nameof(def));
            _entitlements = entitlements ?? throw new ArgumentNullException(nameof(entitlements));
            _economy = economy ?? throw new ArgumentNullException(nameof(economy));
            _analytics = analytics; // optional
        }

        /// <summary>The server entitlement id for this season's premium track (a §9-allowed "pass." SKU).</summary>
        public string PremiumEntitlementId => "pass." + (_def.seasonId ?? string.Empty) + ".premium";

        /// <summary>The season id this pass drives.</summary>
        public string SeasonId => _def.seasonId;

        // ----------------------------------------------------------------- read (server-owned progress)

        /// <summary>
        /// Read the SERVER-OWNED progress: the authoritative PassXP from the wallet, the tier it unlocks,
        /// and whether the premium entitlement is owned. PassXP is NEVER client-trusted — it is read from
        /// IServerEconomy.GetWalletAsync (the same server seam the Wallet adopts). NO payload logged (§12).
        /// </summary>
        public async Task<BattlePassProgress> GetProgressAsync()
        {
            int tierCount = TierCount;
            WalletSnapshot snap = await _economy.GetWalletAsync();
            long passXp = snap.Get(Currency.PassXP);
            if (passXp < 0) passXp = 0; // a bad server value can never unlock extra tiers (fail closed)

            int tier = TierForPassXp(passXp);
            bool premium = await _entitlements.OwnsAsync(PremiumEntitlementId);
            return new BattlePassProgress(passXp, tier, tierCount, premium);
        }

        // ----------------------------------------------------------------- earn-by-play (accrue PassXP)

        /// <summary>
        /// Accrue <paramref name="amount"/> PassXP from PLAY (earn-by-play, §10). PassXP is a server-owned
        /// currency (§9): the accrual round-trips through IServerEconomy.GrantAsync(Currency.PassXP, ...)
        /// and the client adopts ONLY the server-confirmed balance — it performs NO local PassXP math
        /// (HARD RULE #1). The returned tier is DERIVED from the server's confirmed PassXP. PassXP itself
        /// is gameplay-safe by construction (RewardKind.PassXP); accruing it confers no power (§9).
        /// </summary>
        /// <param name="source">An audit/analytics tag for WHERE the play-xp came from (e.g. "battle_win",
        /// "quest"). NOT logged as a save-state payload (§12); used only as the server `source` tag.</param>
        public async Task<PassXpResult> AddPassXp(long amount, string source)
        {
            if (_def == null || string.IsNullOrEmpty(_def.seasonId))
                return PassXpResult.Failure(BattlePassRejectReason.NoSeason);
            if (amount <= 0)
                return PassXpResult.Failure(BattlePassRejectReason.ServerRejected); // nothing to accrue

            // Sanity: PassXP accrual is itself a gameplay-safe grant (RewardKind.PassXP, no power, §9).
            var safety = new RewardGrant { kind = RewardKind.PassXP, amount = (int)Math.Min(amount, int.MaxValue) };
            if (!MonetizationSafety.IsGameplaySafe(in safety))
                return PassXpResult.Failure(BattlePassRejectReason.UnsafeReward);

            EconomyResult res = await _economy.GrantAsync(Currency.PassXP, amount, source ?? "pass_play");
            if (!res.Ok)
                return PassXpResult.Failure(MapEconomyError(res.Error), tier: 0);

            long newPassXp = res.NewBalance < 0 ? 0 : res.NewBalance;
            int newTier = TierForPassXp(newPassXp);

            _analytics?.CurrencyGrant(Currency.PassXP, amount, source ?? "pass_play");
            return PassXpResult.Success(newPassXp, newTier);
        }

        // ----------------------------------------------------------------- claim a tier reward

        /// <summary>
        /// CLAIM the reward for <paramref name="tier"/> on the requested track (<paramref name="premium"/>).
        /// Steps (all server-validated): (1) the tier is in range and REACHED by server-owned PassXP
        /// (earn-by-play); (2) for the premium track, the premium entitlement is OWNED (server read);
        /// (3) the specific RewardGrant for that tier+track PASSES MonetizationSafety.IsGameplaySafe —
        /// otherwise REFUSED (GATE 4); (4) the grant is committed via the SERVER seam appropriate to its
        /// kind (cosmetic → IEntitlementService.GrantAsync; currency/PassXP → IServerEconomy.GrantAsync;
        /// shards/chest-keys → server entitlement/inventory grant, DEFERRED). The FREE track auto-unlocks
        /// as tiers are reached (no entitlement needed); the PREMIUM track requires the premium purchase.
        /// The client adopts only the server's confirmed result and never self-grants (§12).
        /// </summary>
        public async Task<ClaimResult> ClaimTierAsync(int tier, bool premium)
        {
            if (_def == null || string.IsNullOrEmpty(_def.seasonId))
                return ClaimResult.Failure(tier, premium, BattlePassRejectReason.NoSeason);

            if (!TryGetTier(tier, out BattlePassTier tierDef))
                return ClaimResult.Failure(tier, premium, BattlePassRejectReason.TierOutOfRange);

            // Anti-double-tap (session). The durable claimed-record is server-side (DEFERRED).
            string claimKey = ClaimKey(tier, premium);
            if (_claimed.Contains(claimKey))
                return ClaimResult.Failure(tier, premium, BattlePassRejectReason.AlreadyClaimed);

            // (1) The tier must be REACHED by SERVER-OWNED PassXP (earn-by-play, never client-trusted).
            WalletSnapshot snap;
            try { snap = await _economy.GetWalletAsync(); }
            catch { return ClaimResult.Failure(tier, premium, BattlePassRejectReason.NotAuthenticated); }
            long passXp = snap.Get(Currency.PassXP);
            if (passXp < 0) passXp = 0;
            if (TierForPassXp(passXp) < tier)
                return ClaimResult.Failure(tier, premium, BattlePassRejectReason.TierNotReached);

            // (2) The PREMIUM track requires the server-confirmed premium entitlement.
            if (premium)
            {
                bool owns = await _entitlements.OwnsAsync(PremiumEntitlementId);
                if (!owns)
                    return ClaimResult.Failure(tier, premium, BattlePassRejectReason.PremiumNotOwned);
            }

            // (3) GATE 4: the specific reward MUST be gameplay-safe before ANY grant. Fail closed.
            RewardGrant reward = premium ? tierDef.premiumReward : tierDef.freeReward;
            if (!MonetizationSafety.IsGameplaySafe(in reward))
                return ClaimResult.Failure(tier, premium, BattlePassRejectReason.UnsafeReward);

            // (4) Commit the grant through the SERVER seam appropriate to its kind. The client never
            //     self-grants — it adopts only the server's confirmed result.
            bool granted = await GrantRewardAsync(reward, GrantSource(tier, premium));
            if (!granted)
                return ClaimResult.Failure(tier, premium, BattlePassRejectReason.ServerRejected);

            _claimed.Add(claimKey); // mark claimed for THIS session (server is the durable authority)
            return ClaimResult.Success(tier, premium);
        }

        // ----------------------------------------------------------------- buy the premium track

        /// <summary>
        /// BUY the one-time premium entitlement with GEMS (§10): routes through
        /// IEntitlementService.PurchaseWithGemsAsync, which spends gems via the server economy and is
        /// gated by §9 MonetizationSafety.IsGemSpendAllowed (the "pass." SKU is permitted; gems can buy
        /// the pass but the pass grants NO power). The price comes from the def (premiumPriceGems). The
        /// client adopts only the server's PurchaseResult — it never self-grants the entitlement (§12).
        /// </summary>
        public async Task<PurchaseResult> BuyPremiumAsync()
        {
            if (_def == null || string.IsNullOrEmpty(_def.seasonId))
                return PurchaseResult.Failure("no_season");

            long priceGems = _def.premiumPriceGems;
            if (priceGems < 0)
                return PurchaseResult.Failure("bad_price");

            PurchaseResult res = await _entitlements.PurchaseWithGemsAsync(PremiumEntitlementId, priceGems);
            if (res.Ok)
                _analytics?.CurrencySpend(Currency.Gems, priceGems, "pass_premium");
            return res;
        }

        /// <summary>
        /// BUY the one-time premium entitlement with MONEY (§10): submits the platform store receipt to
        /// IEntitlementService.RedeemIapReceiptAsync, which the SERVER validates before granting the
        /// entitlement (the client never trusts an unverified receipt — §12). Live receipt validation is
        /// DEFERRED (no backend). The product is cosmetic+currency only — NO power.
        /// </summary>
        public Task<PurchaseResult> BuyPremiumWithReceiptAsync(string platformReceipt)
        {
            if (_def == null || string.IsNullOrEmpty(_def.seasonId))
                return Task.FromResult(PurchaseResult.Failure("no_season"));
            if (string.IsNullOrEmpty(platformReceipt))
                return Task.FromResult(PurchaseResult.Failure("no_receipt"));

            return _entitlements.RedeemIapReceiptAsync(PremiumEntitlementId, platformReceipt);
        }

        // ----------------------------------------------------------------- internals (grant routing)

        /// <summary>
        /// Route a gameplay-safe RewardGrant to the correct SERVER seam by kind. Cosmetics → the
        /// entitlement service (server-validated ownership). Server-owned currencies (Silver/Gems/PassXP)
        /// → the economy seam. CosmeticShards / ChestKey have NO wallet at MVP (not in the Currency enum)
        /// — they are routed through the server ENTITLEMENT/inventory seam as a placeholder grant; the
        /// real shard/chest inventory backend is DEFERRED (integrationNotes). NEVER grants power — there
        /// is no power kind, and the caller already passed MonetizationSafety.IsGameplaySafe.
        /// </summary>
        private async Task<bool> GrantRewardAsync(RewardGrant g, string source)
        {
            switch (g.kind)
            {
                case RewardKind.Cosmetic:
                {
                    EntitlementResult r = await _entitlements.GrantAsync(g.cosmeticId, source);
                    return r.Ok;
                }

                case RewardKind.Silver:
                {
                    EconomyResult r = await _economy.GrantAsync(Currency.Silver, g.amount, source);
                    if (r.Ok) _analytics?.CurrencyGrant(Currency.Silver, g.amount, source);
                    return r.Ok;
                }

                case RewardKind.Gems:
                {
                    EconomyResult r = await _economy.GrantAsync(Currency.Gems, g.amount, source);
                    if (r.Ok) _analytics?.CurrencyGrant(Currency.Gems, g.amount, source);
                    return r.Ok;
                }

                case RewardKind.PassXP:
                {
                    EconomyResult r = await _economy.GrantAsync(Currency.PassXP, g.amount, source);
                    if (r.Ok) _analytics?.CurrencyGrant(Currency.PassXP, g.amount, source);
                    return r.Ok;
                }

                case RewardKind.CosmeticShards:
                case RewardKind.ChestKey:
                {
                    // No shard/chest WALLET exists at MVP (not a Currency, §9). Route through the server
                    // entitlement/inventory seam under a synthetic, gameplay-safe id so the grant is still
                    // SERVER-VALIDATED (client never self-grants). DEFERRED: a real shard/chest inventory
                    // backend that increments by g.amount (here the server records the grant event).
                    string invId = "inv." + g.kind.ToString().ToLowerInvariant() + "." + source;
                    EntitlementResult r = await _entitlements.GrantAsync(invId, source);
                    return r.Ok;
                }

                default:
                    // Unknown/added kind ⇒ NOT granted (fail closed — defends GATE 4 against future drift).
                    return false;
            }
        }

        // ----------------------------------------------------------------- internals (tier math)

        private int TierCount => _def.tierCount < 0 ? 0 : _def.tierCount;

        /// <summary>
        /// SERVER-derived tier unlocked by <paramref name="passXp"/> (earn-by-play). Tier N is unlocked
        /// once accrued PassXP reaches N * passXpPerTier. Clamped to [0, tierCount]. This is pure math on
        /// the SERVER-owned PassXP value — it adds no client trust (the input came from the server).
        /// </summary>
        private int TierForPassXp(long passXp)
        {
            int perTier = _def.passXpPerTier;
            if (perTier <= 0) return 0;            // misconfigured def ⇒ no tiers unlock (fail closed)
            if (passXp <= 0) return 0;

            long tier = passXp / perTier;          // integer floor: tier N at N*perTier
            int count = TierCount;
            if (tier < 0) return 0;
            if (tier > count) return count;
            return (int)tier;
        }

        /// <summary>Find the BattlePassTier whose `tier` == requested (1-based). False if out of range.</summary>
        private bool TryGetTier(int tier, out BattlePassTier result)
        {
            result = default;
            if (tier < 1 || tier > TierCount) return false;
            if (_def.tiers == null) return false;

            for (int i = 0; i < _def.tiers.Count; i++)
            {
                if (_def.tiers[i].tier == tier)
                {
                    result = _def.tiers[i];
                    return true;
                }
            }
            return false; // tier is in range but the def declares no row for it
        }

        private static string ClaimKey(int tier, bool premium) => tier.ToString() + ":" + (premium ? "P" : "F");

        private string GrantSource(int tier, bool premium)
            => "pass." + _def.seasonId + "." + (premium ? "premium" : "free") + ".tier" + tier;

        /// <summary>Map an economy error CODE to a UX reason. NO payload — codes only (§12).</summary>
        private static BattlePassRejectReason MapEconomyError(string economyError)
        {
            if (economyError == EconomyError.NotAuthenticated) return BattlePassRejectReason.NotAuthenticated;
            return BattlePassRejectReason.ServerRejected;
        }
    }
}

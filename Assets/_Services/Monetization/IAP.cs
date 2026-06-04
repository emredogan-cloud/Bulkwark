// BULWARK — IAP layer (meta/monetization). Phase 4 §13 4.3, §10 IAP + revenue architecture.
// The TRANSPARENT VALUE LADDER ($0.99 → $99.99) + the store seam. DIRECT, see-what-you-buy only:
// every tier is a fixed, fully-disclosed product (gem packs / cosmetics / pass / starter offer).
// NO loot boxes / random paid boxes (decision-log §1 CUT) — there is no randomized SKU here.
// NO price/fairness logic that creates P2W: tiers carry an honest "shown value" and a modest,
// transparent bonus% that rises with size (§10 "transparent, not predatory"); price NEVER buys power.
//
// SERVER-AUTHORITATIVE (§12): this layer NEVER grants an entitlement. It (a) defines the store seam
// (IStoreReceiptProvider) that the platform SDK fulfils, and (b) routes the resulting receipt to the
// SERVER via IEntitlementService.RedeemIapReceiptAsync for validation. The client never trusts an
// unverified receipt and never self-grants — the server validates the platform receipt and grants.
//
// PURE C# (no UnityEngine / no store SDK types in the seam) so it is CI-unit-testable (§12 boundary:
// monetization = services, NOT the ECS battle sim). A real Unity IAP / StoreKit / Play Billing
// adapter implements IStoreReceiptProvider at integration.
// SCAFFOLD STATUS: authored, NOT compiled here (no Unity / no store SDK / no backend).
// DEFERRED: the live store purchase + receipt round-trip AND the server-side receipt validation.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bulwark.Services.Monetization
{
    /// <summary>
    /// The result of asking the PLATFORM STORE to purchase a product: a receipt to be validated
    /// SERVER-side. <see cref="Receipt"/> is an opaque platform token (never trusted client-side).
    /// On user-cancel or store failure, Ok is false and the receipt is null (no entitlement granted).
    /// </summary>
    public readonly struct StorePurchase
    {
        public readonly bool Ok;
        public readonly string ProductId;  // store SKU that was purchased
        public readonly string Receipt;    // opaque platform receipt — validated by the SERVER, not here
        public readonly string Error;       // terse code (e.g. cancelled / unavailable); never a payload (§12)

        public StorePurchase(bool ok, string productId, string receipt, string error)
        { Ok = ok; ProductId = productId; Receipt = receipt; Error = error; }

        public static StorePurchase Success(string productId, string receipt) => new StorePurchase(true, productId, receipt, null);
        public static StorePurchase Failure(string productId, string error) => new StorePurchase(false, productId, null, error ?? "store_error");
    }

    /// <summary>
    /// The STORE SEAM. A platform adapter (Unity IAP / StoreKit / Play Billing) implements this; the
    /// IAP layer routes the receipt it returns to the server for validation. Pure interface — no SDK
    /// types leak across it, so the meta layer stays CI-testable. DEFERRED: the real SDK adapter.
    /// </summary>
    public interface IStoreReceiptProvider
    {
        /// <summary>Begin a platform purchase for <paramref name="productId"/>; resolve with a receipt
        /// to be SERVER-validated. The store, not this app, charges money (player-initiated only).</summary>
        Task<StorePurchase> PurchaseAsync(string productId);
    }

    /// <summary>
    /// No-op/simulated store so wiring + tests run without a platform SDK (DEFERRED). It returns a
    /// deterministic placeholder receipt for a known ladder SKU and a failure for an unknown one. It
    /// NEVER grants anything (only the server does, after validating this receipt).
    /// </summary>
    public sealed class StubStoreReceiptProvider : IStoreReceiptProvider
    {
        public Task<StorePurchase> PurchaseAsync(string productId)
        {
            if (string.IsNullOrEmpty(productId))
                return Task.FromResult(StorePurchase.Failure(productId, "no_product"));
            // Deterministic stub receipt — opaque, server-validated downstream (never trusted here).
            return Task.FromResult(StorePurchase.Success(productId, "stub-receipt:" + productId));
        }
    }

    /// <summary>
    /// One rung of the transparent value ladder (§10). A fixed, see-what-you-buy money product with a
    /// disclosed USD price, the gems it grants (for gem packs), and a transparent bonus%. NO randomness,
    /// NO power. The bonus% is the only "scaling" and it is honest (rises modestly with size), so a
    /// bigger spend is better value but NEVER buys advantage (gems can't buy power, §9).
    /// </summary>
    public readonly struct IapTier
    {
        public readonly string ProductId;  // store SKU, e.g. shop.gempack.medium
        public readonly int PriceUsdCents;  // disclosed price, $0.99 → $99.99
        public readonly int BaseGems;       // gems before bonus (for gem packs; 0 for non-currency tiers)
        public readonly int BonusGems;      // transparent bonus (rises modestly with size — not predatory)
        public readonly string ShownValueNote; // the honest "shown value" string (§10)

        public IapTier(string productId, int priceUsdCents, int baseGems, int bonusGems, string shownValueNote)
        {
            ProductId = productId;
            PriceUsdCents = priceUsdCents;
            BaseGems = baseGems;
            BonusGems = bonusGems;
            ShownValueNote = shownValueNote;
        }

        /// <summary>Total gems shown to the player (base + transparent bonus). Display value only —
        /// the SERVER credits gems after it validates the receipt; this is never client-authoritative.</summary>
        public int TotalGems => (BaseGems < 0 ? 0 : BaseGems) + (BonusGems < 0 ? 0 : BonusGems);
    }

    /// <summary>
    /// The IAP value ladder + purchase router. Holds the disclosed $0.99→$99.99 tiers (a transparent,
    /// non-predatory ladder) and routes a purchased receipt to the SERVER for validation via
    /// IEntitlementService.RedeemIapReceiptAsync. It NEVER grants an entitlement or credits a balance
    /// itself; the server does, after validating the receipt. No randomized/loot SKU exists here.
    /// </summary>
    public sealed class IapService
    {
        private readonly IStoreReceiptProvider _store;
        private readonly IEntitlementService _entitlements;
        private readonly List<IapTier> _ladder;

        public IapService(IStoreReceiptProvider store, IEntitlementService entitlements, IReadOnlyList<IapTier> ladder = null)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _entitlements = entitlements ?? throw new ArgumentNullException(nameof(entitlements));
            _ladder = new List<IapTier>(ladder ?? DefaultLadder());
        }

        /// <summary>The disclosed value ladder (§10), sorted small→large. Display + product registration.</summary>
        public IReadOnlyList<IapTier> Ladder => _ladder;

        /// <summary>Look up a ladder tier by store SKU (null if not a known ladder product).</summary>
        public IapTier? FindTier(string productId)
        {
            if (string.IsNullOrEmpty(productId)) return null;
            for (int i = 0; i < _ladder.Count; i++)
                if (string.Equals(_ladder[i].ProductId, productId, StringComparison.Ordinal)) return _ladder[i];
            return null;
        }

        /// <summary>
        /// Buy a money product: ask the STORE to charge (player-initiated), then route the receipt to
        /// the SERVER for validation + grant. The client never self-grants and never trusts an
        /// unverified receipt. On store-cancel/failure nothing is granted. DEFERRED: real store + server.
        /// </summary>
        public async Task<PurchaseResult> PurchaseAsync(string productId)
        {
            if (string.IsNullOrEmpty(productId)) return PurchaseResult.Failure("no_product");

            // 1) The STORE charges money (platform SDK). Player-initiated only — no interstitial/forced flow.
            StorePurchase purchase = await _store.PurchaseAsync(productId);
            if (!purchase.Ok || string.IsNullOrEmpty(purchase.Receipt))
                return PurchaseResult.Failure(purchase.Error ?? "store_failed");

            // 2) The SERVER validates the receipt and grants the entitlement (DEFERRED — no backend here).
            //    The client adopts only the server's answer; it never credits gems / unlocks itself.
            return await _entitlements.RedeemIapReceiptAsync(productId, purchase.Receipt);
        }

        /// <summary>
        /// The default transparent value ladder ($0.99→$99.99, §10). Illustrative/provisional content
        /// (LSD-owned, RC/SO-overridable at integration) — these are NOT balance/power values, only the
        /// honest shown-value rungs. Bonus% rises modestly with size (transparent, not predatory); no
        /// rung buys power. Gem amounts are DISPLAY values; the server is authoritative on credit.
        /// </summary>
        public static IReadOnlyList<IapTier> DefaultLadder() => new List<IapTier>
        {
            new IapTier("shop.gempack.starter", 99,    100,   0,   "100 Gems — starter value"),
            new IapTier("shop.gempack.small",   299,   320,   20,  "320 Gems (+20 bonus)"),
            new IapTier("shop.gempack.medium",  999,   1100,  100, "1,100 Gems (+100 bonus)"),
            new IapTier("shop.gempack.large",   2999,  3500,  500, "3,500 Gems (+500 bonus)"),
            new IapTier("shop.gempack.huge",    4999,  6000,  1000,"6,000 Gems (+1,000 bonus)"),
            new IapTier("shop.gempack.mega",    9999,  13000, 3000,"13,000 Gems (+3,000 bonus)"),
        };
    }
}

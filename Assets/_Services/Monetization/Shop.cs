// BULWARK — ShopService (meta/monetization layer). Phase 4 §13 4.3, §10 IAP + revenue architecture.
// DIRECT, see-what-you-buy shop over a catalog of ShopItemDef: a transparent value ladder
// (gem packs $0.99→$99.99), cosmetics/banners/emotes/bundles, the battle-pass premium, and a
// one-time first-purchase offer. NO loot boxes / random paid boxes (decision-log §1 CUT) — every
// product lists its exact contents up front. NO price/fairness logic that creates P2W.
//
// INVIOLABLE FAIRNESS (GATE 4): a catalog item may contain ONLY gameplay-safe RewardGrants —
// cosmetics / currency / pass-xp / convenience, NEVER power. Every item's contents are validated
// through MonetizationSafety.IsGameplaySafe BEFORE it is listed or purchasable; an item carrying
// anything else is REFUSED (it never reaches the player). The reward system has no power kind, so
// "power" is unrepresentable — but we still assert, fail-closed, so any future drift is caught.
//
// SERVER-AUTHORITATIVE (§12): the client never self-grants. Gem-priced purchases route through
// IEntitlementService.PurchaseWithGemsAsync (which spends via the Phase-3 IServerEconomy and is
// itself gated by MonetizationSafety.IsGemSpendAllowed, §9 "gems CANNOT"); money-priced purchases
// route a store receipt through IEntitlementService.RedeemIapReceiptAsync for SERVER-side validation.
//
// WEEKLY FEATURED ROTATION is a DATA concern (§10 "weekly featured cosmetics + bundles"): which SKUs
// are featured/limited-time is authored content (RC-driven SO list) — this service just lists what it
// is given and exposes a featured filter; it invents no FOMO mechanic (decision-log §1: no dark patterns).
//
// PURE C# (no UnityEngine in the service logic) so it is CI-unit-testable like the Phase-3 economy
// services (§12 boundary: meta/monetization = services, NOT the ECS battle sim).
// SCAFFOLD STATUS: authored, NOT compiled here (no Unity / no store SDK / no backend).
// DEFERRED: the live store receipt round-trip + the real server entitlement validation (no backend).

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bulwark.Data;
using Bulwark.Services.Analytics;

namespace Bulwark.Services.Monetization
{
    /// <summary>
    /// A transparent, listable shop row built from a <see cref="ShopItemDef"/>. Carries the item's
    /// id, display name, product type, the ONE price (gems XOR money), the shown-value note (§10),
    /// and the first-purchase flag — everything the storefront needs to render see-what-you-buy.
    /// Contents stay on the def; this is a display projection only.
    /// </summary>
    public readonly struct ShopListing
    {
        public readonly string Id;
        public readonly string DisplayName;
        public readonly ProductType ProductType;
        public readonly bool IsGemPriced;       // true ⇒ priced in Gems; false ⇒ priced in real money
        public readonly int PriceGems;          // valid iff IsGemPriced
        public readonly int PriceUsdCents;      // valid iff !IsGemPriced
        public readonly string ShownValueNote;  // the transparent "shown value" (§10 value ladder)
        public readonly bool FirstPurchaseOffer; // one-time conversion anchor (§10)

        public ShopListing(string id, string displayName, ProductType productType,
            bool isGemPriced, int priceGems, int priceUsdCents, string shownValueNote, bool firstPurchaseOffer)
        {
            Id = id;
            DisplayName = displayName;
            ProductType = productType;
            IsGemPriced = isGemPriced;
            PriceGems = priceGems;
            PriceUsdCents = priceUsdCents;
            ShownValueNote = shownValueNote;
            FirstPurchaseOffer = firstPurchaseOffer;
        }
    }

    /// <summary>Terse, non-sensitive shop error CODES (no payload/PII — §12 logging rule).</summary>
    public static class ShopError
    {
        public const string Unknown          = "unknown";
        public const string UnknownItem      = "unknown_item";       // sku not in catalog
        public const string ContainsPower    = "contains_power";     // an item content failed IsGameplaySafe (GATE 4)
        public const string BadPrice         = "bad_price";          // neither/both of gems & money set, or negative
        public const string AlreadyClaimed   = "already_claimed";    // first-purchase offer already taken
        public const string NoReceipt        = "no_receipt";         // money purchase with no store receipt
        public const string GemPathForMoney  = "gem_path_for_money_item"; // wrong purchase path for the price kind
        public const string MoneyPathForGem  = "money_path_for_gem_item";
        public const string PurchaseFailed   = "purchase_failed";    // entitlement/economy rejected the spend
    }

    /// <summary>
    /// Validates a <see cref="ShopItemDef"/> against the fairness rules ONCE, so the result can be
    /// reused for listing + purchase. An item is sellable iff (a) every content grant passes
    /// MonetizationSafety.IsGameplaySafe, and (b) it has EXACTLY ONE non-negative price (gems XOR money).
    /// </summary>
    public readonly struct CatalogValidation
    {
        public readonly bool Ok;
        public readonly string Error;       // null on success; a ShopError code otherwise
        public readonly bool IsGemPriced;   // meaningful only when Ok
        private CatalogValidation(bool ok, string error, bool isGemPriced) { Ok = ok; Error = error; IsGemPriced = isGemPriced; }
        public static CatalogValidation Good(bool isGemPriced) => new CatalogValidation(true, null, isGemPriced);
        public static CatalogValidation Bad(string error) => new CatalogValidation(false, error ?? ShopError.Unknown, false);
    }

    /// <summary>
    /// The shop façade (§13 4.3). Holds a catalog of ShopItemDef, lists transparent rows, and routes
    /// purchases to the SERVER-authoritative <see cref="IEntitlementService"/>. It NEVER grants an
    /// entitlement or mutates a balance itself. A catalog item that contains anything not
    /// gameplay-safe is rejected at validation and is never listed or sold (GATE 4 fairness guard).
    /// </summary>
    public sealed class ShopService
    {
        private readonly IEntitlementService _entitlements;
        private readonly GameAnalytics _analytics; // optional; minimized/non-PII events only (§12)

        // Catalog indexed by sku for O(1) purchase lookup. Authored content (the SO list), not invented here.
        private readonly Dictionary<string, ShopItemDef> _catalog = new Dictionary<string, ShopItemDef>(StringComparer.Ordinal);

        // Featured-this-week SKUs — a DATA concern (§10 weekly rotation), supplied by RC/content, not
        // synthesized here. Empty ⇒ nothing flagged featured. No FOMO mechanic is implied (decision-log §1).
        private readonly HashSet<string> _featured = new HashSet<string>(StringComparer.Ordinal);

        // First-purchase offer is ONE-TIME: once claimed, it can never be re-listed/re-bought. Server is
        // the real source of truth (entitlement ownership); this flag is a client-side UX/idempotency guard.
        private bool _firstPurchaseClaimed;

        public ShopService(IEntitlementService entitlements, GameAnalytics analytics = null)
        {
            _entitlements = entitlements ?? throw new ArgumentNullException(nameof(entitlements));
            _analytics = analytics;
        }

        /// <summary>
        /// Load/replace the catalog. Items that FAIL the fairness validation (contain power / bad price)
        /// are SILENTLY DROPPED from the sellable catalog — they never become listable or purchasable
        /// (fail-closed, GATE 4). Returns the SKUs that were rejected so a content/CI check can surface
        /// the authoring error. NOTE: this never throws on a bad item — it refuses to sell it.
        /// </summary>
        public IReadOnlyList<string> LoadCatalog(IEnumerable<ShopItemDef> items)
        {
            _catalog.Clear();
            var rejected = new List<string>();
            if (items == null) return rejected;

            foreach (var def in items)
            {
                if (def == null || string.IsNullOrEmpty(def.id)) continue;
                var v = Validate(def);
                if (!v.Ok) { rejected.Add(def.id); continue; } // refuse to list a power-bearing / mispriced item
                _catalog[def.id] = def;
            }
            return rejected;
        }

        /// <summary>Set the weekly featured set (a data concern, §10). Unknown SKUs are ignored.</summary>
        public void SetFeatured(IEnumerable<string> featuredSkus)
        {
            _featured.Clear();
            if (featuredSkus == null) return;
            foreach (var sku in featuredSkus)
                if (!string.IsNullOrEmpty(sku)) _featured.Add(sku);
        }

        /// <summary>All listable rows (transparent shown value). First-purchase offer is hidden once claimed.</summary>
        public IReadOnlyList<ShopListing> List()
        {
            var rows = new List<ShopListing>(_catalog.Count);
            foreach (var kv in _catalog)
            {
                var def = kv.Value;
                if (def.firstPurchaseOffer && _firstPurchaseClaimed) continue; // one-time offer already taken
                rows.Add(ToListing(def));
            }
            return rows;
        }

        /// <summary>Only the weekly-featured rows (§10 rotation), a display filter over the same catalog.</summary>
        public IReadOnlyList<ShopListing> ListFeatured()
        {
            var rows = new List<ShopListing>();
            foreach (var sku in _featured)
            {
                if (_catalog.TryGetValue(sku, out var def))
                {
                    if (def.firstPurchaseOffer && _firstPurchaseClaimed) continue;
                    rows.Add(ToListing(def));
                }
            }
            return rows;
        }

        /// <summary>True iff the one-time first-purchase offer is still available (client-side UX guard).</summary>
        public bool FirstPurchaseOfferAvailable => !_firstPurchaseClaimed;

        /// <summary>
        /// Purchase a GEM-priced item: routes the spend through the server via
        /// IEntitlementService.PurchaseWithGemsAsync (which spends Gems via IServerEconomy and enforces
        /// §9 "gems CANNOT"). The item MUST be gem-priced and fairness-valid. The client never grants.
        /// </summary>
        public async Task<PurchaseResult> PurchaseWithGemsAsync(string sku)
        {
            if (!_catalog.TryGetValue(sku, out var def)) return PurchaseResult.Failure(ShopError.UnknownItem);

            var v = Validate(def);
            if (!v.Ok) return PurchaseResult.Failure(v.Error);     // re-assert at point of sale (GATE 4)
            if (!v.IsGemPriced) return PurchaseResult.Failure(ShopError.GemPathForMoney);

            var offerGuard = GuardFirstPurchase(def);
            if (offerGuard != null) return PurchaseResult.Failure(offerGuard);

            // SERVER-authoritative: spend gems + grant entitlement on the server. Client adopts the answer.
            var result = await _entitlements.PurchaseWithGemsAsync(def.id, def.priceGems);
            if (result.Ok)
            {
                MarkClaimedIfFirstPurchase(def);
                // Currency event carries delta + reason CODE only — never a balance (§12).
                _analytics?.CurrencySpend(Currency.Gems, def.priceGems, "shop:" + def.id);
            }
            return result.Ok ? result : PurchaseResult.Failure(result.Error ?? ShopError.PurchaseFailed);
        }

        /// <summary>
        /// Purchase a MONEY-priced item: routes the platform store receipt to the SERVER via
        /// IEntitlementService.RedeemIapReceiptAsync for validation (DEFERRED — no backend here). The
        /// client never trusts an unverified receipt and never self-grants. The receipt is produced by
        /// the IAP layer's store seam (see IAP.cs); this method only forwards it for server validation.
        /// </summary>
        public async Task<PurchaseResult> PurchaseWithMoneyAsync(string sku, string platformReceipt)
        {
            if (!_catalog.TryGetValue(sku, out var def)) return PurchaseResult.Failure(ShopError.UnknownItem);

            var v = Validate(def);
            if (!v.Ok) return PurchaseResult.Failure(v.Error);     // re-assert at point of sale (GATE 4)
            if (v.IsGemPriced) return PurchaseResult.Failure(ShopError.MoneyPathForGem);
            if (string.IsNullOrEmpty(platformReceipt)) return PurchaseResult.Failure(ShopError.NoReceipt);

            var offerGuard = GuardFirstPurchase(def);
            if (offerGuard != null) return PurchaseResult.Failure(offerGuard);

            // SERVER validates the receipt then grants the entitlement (the client never self-grants).
            var result = await _entitlements.RedeemIapReceiptAsync(def.id, platformReceipt);
            if (result.Ok) MarkClaimedIfFirstPurchase(def);
            return result.Ok ? result : PurchaseResult.Failure(result.Error ?? ShopError.PurchaseFailed);
        }

        // ------------------------------------------------------------------ fairness + helpers

        /// <summary>
        /// THE catalog fairness guard (GATE 4): every content grant must pass IsGameplaySafe (no power),
        /// and the item must carry EXACTLY ONE non-negative price (gems XOR real money). Pure; reusable
        /// for both listing and the point-of-sale re-assert.
        /// </summary>
        public static CatalogValidation Validate(ShopItemDef def)
        {
            if (def == null || string.IsNullOrEmpty(def.id)) return CatalogValidation.Bad(ShopError.UnknownItem);

            // (a) NO POWER: every content reward must be gameplay-safe. RewardKind has no power member,
            // so this is total — but we assert, fail-closed, to catch any future drift (a malformed grant,
            // a non-positive amount, an empty cosmetic id). An item with ANY unsafe content is unsellable.
            if (def.contents != null)
            {
                for (int i = 0; i < def.contents.Count; i++)
                {
                    var grant = def.contents[i];
                    if (!MonetizationSafety.IsGameplaySafe(grant))
                        return CatalogValidation.Bad(ShopError.ContainsPower);
                }
            }

            // (b) EXACTLY ONE non-negative price. No P2W can come from price logic; the only rule here is
            // structural transparency (one price, shown value). Gems XOR money.
            bool gem = def.priceGems > 0;
            bool money = def.priceUsdCents > 0;
            if (def.priceGems < 0 || def.priceUsdCents < 0) return CatalogValidation.Bad(ShopError.BadPrice);
            if (gem == money) return CatalogValidation.Bad(ShopError.BadPrice); // need exactly one (both=0 or both>0 ⇒ bad)

            return CatalogValidation.Good(isGemPriced: gem);
        }

        private static ShopListing ToListing(ShopItemDef def)
        {
            bool gem = def.priceGems > 0;
            return new ShopListing(
                def.id, def.displayName, def.productType,
                isGemPriced: gem,
                priceGems: gem ? def.priceGems : 0,
                priceUsdCents: gem ? 0 : def.priceUsdCents,
                shownValueNote: def.shownValueNote,
                firstPurchaseOffer: def.firstPurchaseOffer);
        }

        /// <summary>Reject a re-purchase of an already-claimed one-time offer. Returns a code or null.</summary>
        private string GuardFirstPurchase(ShopItemDef def)
            => (def.firstPurchaseOffer && _firstPurchaseClaimed) ? ShopError.AlreadyClaimed : null;

        private void MarkClaimedIfFirstPurchase(ShopItemDef def)
        {
            if (def.firstPurchaseOffer) _firstPurchaseClaimed = true; // one-time (server remains source of truth)
        }
    }
}

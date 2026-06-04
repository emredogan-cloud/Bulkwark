// BULWARK — entitlement seam (server-validated ownership) — Phase 4 §13 4.x, §9/§10/§12.
// SERVER-AUTHORITATIVE: cosmetic/pass/bundle ownership lives on the server; the client asks and
// adopts the server's answer (it can never self-grant an entitlement). Purchases route gem spends
// through the Phase-3 IServerEconomy and store (IAP) receipts through server-side validation.
// FAIRNESS (INVIOLABLE): entitlements are cosmetic/convenience only — NEVER power. Gem spends are
// gated by MonetizationSafety.IsGemSpendAllowed (§9 "gems CANNOT"). No paid random boxes.
// SCAFFOLD STATUS: authored, NOT compiled when first written (CI validates). Live server round-trip DEFERRED.

using System.Collections.Generic;
using System.Threading.Tasks;
using Bulwark.Data;
using Bulwark.Services.Economy;

namespace Bulwark.Services.Monetization
{
    public readonly struct EntitlementResult
    {
        public readonly bool Ok;
        public readonly string Error;   // terse code; never a payload (§12 no save-state logging)
        public EntitlementResult(bool ok, string error) { Ok = ok; Error = error; }
        public static EntitlementResult Success() => new EntitlementResult(true, null);
        public static EntitlementResult Failure(string e) => new EntitlementResult(false, e ?? "unknown");
    }

    public readonly struct PurchaseResult
    {
        public readonly bool Ok;
        public readonly string EntitlementId;
        public readonly string Error;
        public PurchaseResult(bool ok, string id, string error) { Ok = ok; EntitlementId = id; Error = error; }
        public static PurchaseResult Success(string id) => new PurchaseResult(true, id, null);
        public static PurchaseResult Failure(string e) => new PurchaseResult(false, null, e ?? "unknown");
    }

    /// <summary>
    /// Server-authoritative entitlement ownership + purchase. Implemented by a BaaS adapter behind
    /// IBackendClient; the StubEntitlementService simulates the server so the meta layer is testable.
    /// </summary>
    public interface IEntitlementService
    {
        Task<bool> OwnsAsync(string entitlementId);                                 // server read
        Task<EntitlementResult> GrantAsync(string entitlementId, string source);    // earned grant (pass/chest/quest), server-validated
        Task<PurchaseResult> PurchaseWithGemsAsync(string sku, long gemPrice);      // gem spend (IServerEconomy) -> entitlement
        Task<PurchaseResult> RedeemIapReceiptAsync(string sku, string platformReceipt); // server validates the store receipt
    }

    /// <summary>
    /// No-op/simulated server. Enforces the §9 gem-spend allow-list + records owned entitlements in
    /// memory. Spends gems through the canonical IServerEconomy so the wallet stays server-authoritative.
    /// </summary>
    public sealed class StubEntitlementService : IEntitlementService
    {
        private readonly IServerEconomy _economy;
        private readonly HashSet<string> _owned = new HashSet<string>();
        public StubEntitlementService(IServerEconomy economy) { _economy = economy; }

        public Task<bool> OwnsAsync(string entitlementId) => Task.FromResult(_owned.Contains(entitlementId));

        public Task<EntitlementResult> GrantAsync(string entitlementId, string source)
        {
            if (string.IsNullOrEmpty(entitlementId)) return Task.FromResult(EntitlementResult.Failure("bad_id"));
            _owned.Add(entitlementId);
            return Task.FromResult(EntitlementResult.Success());
        }

        public async Task<PurchaseResult> PurchaseWithGemsAsync(string sku, long gemPrice)
        {
            if (!MonetizationSafety.IsGemSpendAllowed(sku)) return PurchaseResult.Failure("gem_spend_not_allowed"); // §9 "gems CANNOT"
            var spend = await _economy.TrySpendAsync(Currency.Gems, gemPrice, "iap:" + sku);
            if (!spend.Ok) return PurchaseResult.Failure(spend.Error ?? "spend_failed");
            _owned.Add(sku);
            return PurchaseResult.Success(sku);
        }

        public Task<PurchaseResult> RedeemIapReceiptAsync(string sku, string platformReceipt)
        {
            // The SERVER validates the platform receipt (DEFERRED — no backend here). On success it
            // grants the entitlement; the client never trusts an unverified receipt.
            if (string.IsNullOrEmpty(platformReceipt)) return Task.FromResult(PurchaseResult.Failure("no_receipt"));
            _owned.Add(sku);
            return Task.FromResult(PurchaseResult.Success(sku));
        }
    }
}

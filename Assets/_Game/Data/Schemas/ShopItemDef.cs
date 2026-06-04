// BULWARK — ShopItemDef ScriptableObject (Data). Phase 4 §13 4.3, §10.
// DIRECT, see-what-you-buy products (NO random paid boxes). A transparent value ladder
// ($0.99→$99.99) + a one-time first-purchase offer. Contents are gameplay-safe RewardGrants
// (cosmetics / gem packs / pass / banners / emotes) — NEVER power. Entitlements server-validated.
// SCAFFOLD STATUS: authored, NOT compiled when first written (CI validates).

using System.Collections.Generic;
using UnityEngine;

namespace Bulwark.Data
{
    [CreateAssetMenu(fileName = "ShopItemDef", menuName = "BULWARK/Data/ShopItemDef", order = 10)]
    public class ShopItemDef : ScriptableObject
    {
        [Header("Identity (§10)")]
        public string id;                 // e.g. shop.gempack.medium  /  cosmetic.bundle.ashen_s0
        public string displayName;
        public ProductType productType = ProductType.Cosmetic;

        [Header("Price (one of) — transparent value")]
        public int priceUsdCents;         // real-money price (0 if gem-priced)
        public int priceGems;             // gem price (0 if money-priced)
        [TextArea] public string shownValueNote; // the "shown value" (§10 transparent ladder)
        public bool firstPurchaseOffer;   // one-time conversion anchor (§10)

        [Header("Contents — gameplay-safe only (MonetizationSafety.IsGameplaySafe)")]
        public List<RewardGrant> contents = new List<RewardGrant>();
    }
}

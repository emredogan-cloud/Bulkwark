// BULWARK — monetization/live-ops shared types (Data layer). Phase 4 §13 4.1–4.5, §6/§8/§9/§10.
// This file ENCODES the inviolable fairness rules as types: rewards can ONLY be cosmetic /
// currency / convenience / PassXP — NEVER power (no stats/units/upgrades/ranked advantage).
// MonetizationSafety.IsGameplaySafe is the single guard every reward/shop/chest/pass passes.
// CUT (decision-log §1, HARD): loot boxes/gacha-for-power, interstitials, energy gates, P2W,
// paid random boxes without disclosed odds + dupe protection. 4 currencies ONLY (§9).
// SCAFFOLD STATUS: authored, NOT compiled here when first written (CI validates).

namespace Bulwark.Data
{
    /// <summary>Outfit-class tiers (§6) — recolor/material/trim/VFX prestige over a LOCKED silhouette.</summary>
    public enum OutfitTier : int { Standard = 0, Veteran = 1, Elite = 2, Legendary = 3, Mythic = 4 }

    /// <summary>Cosmetic rarity (§6) — purely visual prestige, NO stat or read advantage.</summary>
    public enum Rarity : int { Common = 0, Rare = 1, Epic = 2, Legendary = 3, Mythic = 4 }

    /// <summary>What a cosmetic changes (§6) — all visual-only; silhouette/size/hitbox/timing are LOCKED.</summary>
    public enum CosmeticVariant : int { ArmorRecolor = 0, ColorScheme = 1, WeaponSkin = 2, VfxRecolor = 3, Emote = 4, Banner = 5 }

    /// <summary>
    /// The ONLY reward kinds that may ship (§8/§9/§10). NOTE: there is deliberately NO "Power",
    /// "Upgrade", "Unit", or "StatBoost" kind — power is unrepresentable in the reward system.
    /// </summary>
    public enum RewardKind : int
    {
        Silver = 0,         // soft currency (§9)
        Gems = 1,           // premium currency — NEVER buys power (§9 "gems CANNOT")
        PassXP = 2,         // battle-pass track xp
        CosmeticShards = 3, // craft currency for cosmetics; dupes convert here (§8)
        Cosmetic = 4,       // a specific CosmeticDef (gameplay-safe, §6)
        ChestKey = 5,       // convenience: opens/queues an earned chest (no power)
        // (Gold is in-battle/non-persistent — not a reward currency.)
    }

    /// <summary>Shop product categories (§10) — direct, see-what-you-buy. No random paid boxes.</summary>
    public enum ProductType : int { Cosmetic = 0, GemPack = 1, BattlePassPremium = 2, CosmeticBundle = 3, Banner = 4, Emote = 5, StarterOffer = 6 }

    /// <summary>Chest tiers (§8) — names are cosmetic LABELS, distinct from the Silver/Gems currencies.</summary>
    public enum ChestTier : int { Wood = 0, Silver = 1, Gold = 2, Seasonal = 3 }

    public enum QuestCadence : int { Daily = 0, Weekly = 1 }

    /// <summary>A single gameplay-safe reward (currency/shards/cosmetic/passXP/convenience). Never power.</summary>
    [System.Serializable]
    public struct RewardGrant
    {
        public RewardKind kind;
        public int amount;          // for currencies/shards/passXP
        public string cosmeticId;   // for kind == Cosmetic (references a CosmeticDef.id)
    }

    /// <summary>
    /// The single structural fairness guard. Because RewardKind has no power member, this is
    /// total — but it is asserted everywhere (chests/shop/pass/quests) so any future drift
    /// (e.g., a non-positive amount, or a malformed grant) is caught. GATE 4 relies on it.
    /// </summary>
    public static class MonetizationSafety
    {
        /// <summary>True iff the grant is one of the allowed gameplay-safe kinds with sane data.</summary>
        public static bool IsGameplaySafe(in RewardGrant g)
        {
            switch (g.kind)
            {
                case RewardKind.Silver:
                case RewardKind.Gems:
                case RewardKind.PassXP:
                case RewardKind.CosmeticShards:
                case RewardKind.ChestKey:
                    return g.amount >= 0;
                case RewardKind.Cosmetic:
                    return !string.IsNullOrEmpty(g.cosmeticId);
                default:
                    return false; // unknown/added kind ⇒ NOT safe (fail closed — protects §13 GATE 4)
            }
        }

        /// <summary>§9 "gems CANNOT": gems may be spent ONLY on these. Power is never an option.</summary>
        public static bool IsGemSpendAllowed(string sku)
            => sku != null && (sku.StartsWith("cosmetic.") || sku.StartsWith("pass.") ||
                               sku.StartsWith("convenience.") || sku.StartsWith("chestskip.") || sku.StartsWith("slot."));
    }
}

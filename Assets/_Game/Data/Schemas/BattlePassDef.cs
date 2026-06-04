// BULWARK — BattlePassDef ScriptableObject (Data). Phase 4 §13 4.2, §10.
// Dual-track (free + premium) season pass, earn-by-play. Every tier reward is a gameplay-safe
// RewardGrant (cosmetic/currency/PassXP — NEVER power). Premium is a one-time entitlement
// (~$9.99 / ~950 Gems, §10); buying it grants cosmetics + currency, no power. Server-owned progress.
// SCAFFOLD STATUS: authored, NOT compiled when first written (CI validates).

using System.Collections.Generic;
using UnityEngine;

namespace Bulwark.Data
{
    [System.Serializable]
    public struct BattlePassTier
    {
        public int tier;               // 1..tierCount
        public RewardGrant freeReward;     // free-track reward (gameplay-safe)
        public RewardGrant premiumReward;  // premium-track reward (gameplay-safe)
    }

    [CreateAssetMenu(fileName = "BattlePassDef", menuName = "BULWARK/Data/BattlePassDef", order = 9)]
    public class BattlePassDef : ScriptableObject
    {
        [Header("Season (§10)")]
        public string seasonId;
        public string displayName;
        public int tierCount = 50;
        public int passXpPerTier = 1000;   // earn-by-play

        [Header("Premium upgrade (one-time entitlement; cosmetics+currency only — NO power)")]
        public int premiumPriceGems = 950;
        public int premiumPriceUsdCents = 999;

        [Header("Tracks — every reward must pass MonetizationSafety.IsGameplaySafe")]
        public List<BattlePassTier> tiers = new List<BattlePassTier>();
    }
}

// BULWARK — ChestDef ScriptableObject (Data). Phase 4 §13 4.4, §8 ethical chests.
// EARNED, DISCLOSED-ODDS, COSMETIC+CURRENCY-ONLY (no power), with timers + slot cap and
// duplicate→shard protection. NOT a paid random box (decision-log §1 CUT). Odds are explicit and
// queryable. Gems may only SKIP the timer (convenience), never buy power (§9).
// SCAFFOLD STATUS: authored, NOT compiled when first written (CI validates).

using System.Collections.Generic;
using UnityEngine;

namespace Bulwark.Data
{
    /// <summary>One disclosed drop slot: a gameplay-safe reward kind/rarity with a weight.</summary>
    [System.Serializable]
    public struct ChestDrop
    {
        public RewardKind kind;   // Silver/Gems/PassXP/CosmeticShards/Cosmetic — never power
        public Rarity rarity;     // for cosmetics/shards
        public int weight;        // relative odds (disclosed)
        public int amount;        // for currency/shards
        public string cosmeticId; // for kind == Cosmetic
    }

    [CreateAssetMenu(fileName = "ChestDef", menuName = "BULWARK/Data/ChestDef", order = 11)]
    public class ChestDef : ScriptableObject
    {
        [Header("Identity (§8)")]
        public string id;
        public string displayName;
        public ChestTier tier = ChestTier.Wood;
        [TextArea] public string sourceNote;   // e.g. "per-battle" / "daily/win-streak"

        [Header("Pacing (soft, free — NOT a paywall, §8)")]
        public int openMinutes = 15;           // timer; Gems may SKIP (convenience only)
        public int slotCap = 4;                // gentle pacing
        public bool dupeToShards = true;       // duplicate cosmetics convert to shards (§8)

        [Header("Disclosed odds — cosmetic+currency only (no power)")]
        public List<ChestDrop> drops = new List<ChestDrop>();

        /// <summary>Disclosed probability of a drop (weight / total). For UI odds display (regulatory + trust).</summary>
        public float OddsOf(int dropIndex)
        {
            int total = 0;
            for (int i = 0; i < drops.Count; i++) total += drops[i].weight < 0 ? 0 : drops[i].weight;
            if (total <= 0 || dropIndex < 0 || dropIndex >= drops.Count) return 0f;
            return (float)drops[dropIndex].weight / total;
        }
    }
}

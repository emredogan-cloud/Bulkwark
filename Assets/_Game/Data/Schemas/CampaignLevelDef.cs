// BULWARK — CampaignLevelDef ScriptableObject (Data). Phase 3 §13 3.3, §7 Classic.
// One Act-1 campaign level as DATA (~20 instances). Rewards use the §6/§9 diminishing-
// returns rule (first-clear gem = max(floor, base − dec×playCount)); the FORMULA lives in
// the mode/economy code, the per-level BASE lives here. Provisional values, LSD-owned (§16).
// SCAFFOLD STATUS: authored, NOT compiled (no Unity — ADR-0-001/-2-001).

using System.Collections.Generic;
using UnityEngine;

namespace Bulwark.Data
{
    /// <summary>One enemy spawn directive for a campaign level (PvE composition).</summary>
    [System.Serializable]
    public struct EnemyWave
    {
        public int unitIndex;     // index into the enemy faction's unit catalog
        public int count;
        public float spawnDelay;  // seconds after level start (or after prior wave)
    }

    [CreateAssetMenu(fileName = "CampaignLevel", menuName = "BULWARK/Data/CampaignLevel", order = 5)]
    public class CampaignLevelDef : ScriptableObject
    {
        [Header("Identity (§7 Classic / Act 1)")]
        public string id;
        public string displayName;
        public int index;                 // 1..~20
        public string mapId;              // references a MapDef.id (§11)
        public Faction enemyFaction = Faction.AshenHorde;

        [Header("PvE composition — PROVISIONAL")]
        public int enemyStartGold = 200;
        public List<EnemyWave> waves = new List<EnemyWave>();

        [Header("Rewards — first-clear gem BASE feeds the diminishing rule (§9). PROVISIONAL")]
        public int firstClearGemBase;     // gems on first clear; replays diminish (formula in code)
        public int silverReward;          // per-clear Silver (server-granted)
        public int passXpReward;          // per-clear PassXP

        [Header("Star objective (§7 replay payoff) — PROVISIONAL")]
        public float starClearTimeSeconds; // beat this for a star
        [TextArea] public string narrativeBlurb; // light narrative framing (§3 "light narrative")
    }
}

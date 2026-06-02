// BULWARK — UnitDef ScriptableObject schema (Data layer).
// Each field is a CANON-MECHANIC parameter (combat §4, economy/train §13 P1.1–1.2,
// mining §11), authored as DATA so the sim never hardcodes balance (§12, P1 task 2).
// VALUES live in the .asset instances (Assets/_Game/Data/Units/*.asset) and are
// PROVISIONAL / LSD-owned, telemetry-tunable (§16) — not asserted canon (§15.6).
// One ScriptableObject class per file (Unity requires filename == classname so
// hand-authored .asset instances resolve to this MonoScript).
// SCAFFOLD STATUS: authored, NOT compiled (no Unity — ADR-0-001/-002).

using UnityEngine;

namespace Bulwark.Data
{
    [CreateAssetMenu(fileName = "UnitDef", menuName = "BULWARK/Data/UnitDef", order = 0)]
    public class UnitDef : ScriptableObject
    {
        [Header("Identity (canon role/type/armor/faction — §5.1/§5.2)")]
        [Tooltip("Stable content id, e.g. ironpact_shieldman.")] public string id;
        public string displayName;
        public Faction faction = Faction.IronPact; // §5.1; existing P1 Iron Pact assets default correctly (0)
        public UnitRole role = UnitRole.Unset;
        public DamageType damageType = DamageType.Unset;
        public ArmorClass armorClass = ArmorClass.Unset;

        [Header("Combat stats — PROVISIONAL data (LSD-owned, FUN-gate/telemetry-tuned)")]
        public float maxHealth;
        public float moveSpeed;
        public float attackDamage;     // 'base' in the §4 modifier chain
        public float attackRange;
        public float attackInterval;   // seconds between swings

        [Header("Economy/training — §13 P1.1–1.2 (values PROVISIONAL)")]
        public int   goldCost;         // Gold to train (0 for the starting Miner if free)
        public float trainSeconds;     // queue/deploy time

        [Header("Mining — Miner role only (§11; 0 for non-miners)")]
        public float miningRatePerSec; // Gold/sec contributed while mining a node
    }
}

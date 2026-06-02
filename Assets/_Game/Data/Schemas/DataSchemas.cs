// BULWARK — Data schemas (Phase 0.3 scaffold): UnitDef / SpellDef / BalanceConfig.
// These are TYPED, CONTENT-EMPTY ScriptableObject containers. Roadmap §15 forbids
// inventing units/spells/currencies/balance now — so NO concrete units, NO numbers,
// NO balance values are authored here. Fields mirror canon STRUCTURE only
// (§4 combat model: type×armor matrix, roles; §5 spell draft) so Phase 1 can fill them.
// Values are consumed at runtime via the 3-tier resolver (RC → SO → literal, §9/§12).
// SCAFFOLD STATUS: authored, NOT compiled (no Unity 6 — ADR-0-001).

using System.Collections.Generic;
using UnityEngine;

namespace Bulwark.Data
{
    // Enum *labels* describe canon's combat-model AXES (§4). They are not content/balance.
    public enum DamageType { Unset = 0, A, B, C, D, E }   // 5 damage types (§4 5x4 matrix) — named in Phase 1
    public enum ArmorClass { Unset = 0, W, X, Y, Z }      // 4 armor classes (§4 5x4 matrix) — named in Phase 1
    public enum UnitRole   { Unset = 0, Miner, Frontline, Skirmisher, Ranged, Caster, Heavy, Flanker } // 7-role palette (§5.2)

    /// <summary>Schema for a single unit. Content-empty at P0; populated in Phase 1 under canon.</summary>
    [CreateAssetMenu(fileName = "UnitDef", menuName = "BULWARK/Data/UnitDef", order = 0)]
    public class UnitDef : ScriptableObject
    {
        [Tooltip("Stable content id. Empty at P0.")] public string id;
        public UnitRole role = UnitRole.Unset;
        public DamageType damageType = DamageType.Unset;
        public ArmorClass armorClass = ArmorClass.Unset;

        [Header("Stats — UNPOPULATED at P0 (Phase 1 fills under §4/§15)")]
        public float maxHealth;       // 0 until Phase 1
        public float moveSpeed;       // 0 until Phase 1
        public float attackDamage;    // 0 until Phase 1
        public float attackRange;     // 0 until Phase 1
        public float attackInterval;  // 0 until Phase 1
    }

    /// <summary>Schema for a draftable spell (§5.3 draft-3 loadout). Content-empty at P0.</summary>
    [CreateAssetMenu(fileName = "SpellDef", menuName = "BULWARK/Data/SpellDef", order = 1)]
    public class SpellDef : ScriptableObject
    {
        [Tooltip("Stable content id. Empty at P0.")] public string id;

        [Header("Tuning — UNPOPULATED at P0")]
        public float cooldown;        // 0 until Phase 1
        public float magnitude;       // 0 until Phase 1
        public float telegraphTime;   // 0 until Phase 1 (§4: every spell telegraphs/counters)
    }

    /// <summary>
    /// Schema for global balance + the type×armor counter matrix (§4). The matrix is
    /// DECLARED here (5 damage types × 4 armor classes) but UNPOPULATED — Phase 1 fills it.
    /// </summary>
    [CreateAssetMenu(fileName = "BalanceConfig", menuName = "BULWARK/Data/BalanceConfig", order = 2)]
    public class BalanceConfig : ScriptableObject
    {
        [Header("type x armor multipliers — UNPOPULATED at P0 (5x4, Phase 1 fills under §4)")]
        public float[] counterMatrix = new float[5 * 4]; // all 0 until Phase 1; index = (int)damageType*4 + (int)armorClass

        public List<UnitDef> units = new List<UnitDef>();   // empty at P0
        public List<SpellDef> spells = new List<SpellDef>(); // empty at P0

        public float GetMultiplier(DamageType dt, ArmorClass ac)
        {
            int di = (int)dt - 1, ai = (int)ac - 1;
            if (di < 0 || ai < 0) return 1f;
            int idx = di * 4 + ai;
            return (idx >= 0 && idx < counterMatrix.Length && counterMatrix[idx] != 0f) ? counterMatrix[idx] : 1f;
        }
    }
}

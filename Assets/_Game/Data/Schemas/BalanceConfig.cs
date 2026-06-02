// BULWARK — BalanceConfig ScriptableObject schema (Data layer).
// Holds the type×armor counter matrix (§4 5×4) + roster references. The matrix VALUES
// are PROVISIONAL / LSD-owned (§16) and live in the .asset; in Phase 1 only a FEW cells
// are non-neutral ("basic counters only", §13 P1.4) — the full matrix is Phase 2.2.
// SCAFFOLD STATUS: authored, NOT compiled (no Unity — ADR-0-001/-002).

using System.Collections.Generic;
using UnityEngine;

namespace Bulwark.Data
{
    [CreateAssetMenu(fileName = "BalanceConfig", menuName = "BULWARK/Data/BalanceConfig", order = 2)]
    public class BalanceConfig : ScriptableObject
    {
        // index = (damageType-1)*4 + (armorClass-1); 5 damage types × 4 armor classes = 20 cells.
        // Default/neutral multiplier is 1.0; 0.0 is treated as "unset → 1.0" by GetMultiplier.
        [Header("type×armor multipliers — PROVISIONAL; only 'basic counters' non-1 at P1 (§13 P1.4)")]
        public float[] counterMatrix = new float[5 * 4];

        public List<UnitDef> units = new List<UnitDef>();
        public List<SpellDef> spells = new List<SpellDef>(); // empty in P1 (spells = Phase 2)

        public float GetMultiplier(DamageType dt, ArmorClass ac)
        {
            int di = (int)dt - 1, ai = (int)ac - 1;
            if (di < 0 || ai < 0) return 1f;
            int idx = di * 4 + ai;
            return (idx >= 0 && idx < counterMatrix.Length && counterMatrix[idx] != 0f) ? counterMatrix[idx] : 1f;
        }
    }
}

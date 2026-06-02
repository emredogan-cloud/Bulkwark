// BULWARK — SpellDef ScriptableObject schema (Data layer).
// Schema only. SPELLS ARE PHASE 2 (§13 2.4 draft-3) — NO spell content is authored
// in Phase 1 (forbidden, §J of the Phase-1 prompt). This file exists from Phase 0 as a
// typed container; no SpellDef .asset instances ship in Phase 1.
// SCAFFOLD STATUS: authored, NOT compiled (no Unity — ADR-0-001/-002).

using UnityEngine;

namespace Bulwark.Data
{
    [CreateAssetMenu(fileName = "SpellDef", menuName = "BULWARK/Data/SpellDef", order = 1)]
    public class SpellDef : ScriptableObject
    {
        [Tooltip("Stable content id. Empty at P0/P1.")] public string id;

        [Header("Tuning — UNPOPULATED until Phase 2")]
        public float cooldown;
        public float magnitude;
        public float telegraphTime; // §5.3: every spell telegraphs/counters
    }
}

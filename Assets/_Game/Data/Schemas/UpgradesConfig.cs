// BULWARK — UpgradesConfig ScriptableObject (Data). Phase 3 §13 3.2, §3 capped progression.
// Per-unit upgrade tracks: each has a HARD CAP (maxLevel) and a rising Silver cost. Upgrades
// give bounded stat deltas — NEVER beyond the cap (anti-P2W, INVIOLABLE §15). Commander levels
// are likewise capped. The SERVER enforces caps + costs (§12); this data describes the curve.
// Provisional values, LSD-owned (§16). SCAFFOLD: authored, NOT compiled (ADR-0-001/-2-001).

using System.Collections.Generic;
using UnityEngine;

namespace Bulwark.Data
{
    /// <summary>One capped upgrade track (a unit's one stat). Cost rises with level; cap is hard.</summary>
    [System.Serializable]
    public struct UpgradeTrack
    {
        public string unitId;        // references UnitDef.id
        public UpgradeStat stat;
        public int maxLevel;         // HARD CAP (§3) — server-enforced
        public int baseCostSilver;   // cost of level 1
        public float costGrowth;     // multiplicative cost growth per level (e.g. 1.6)
        public float deltaPerLevel;  // additive stat delta per level (bounded by maxLevel)
    }

    [CreateAssetMenu(fileName = "UpgradesConfig", menuName = "BULWARK/Data/UpgradesConfig", order = 6)]
    public class UpgradesConfig : ScriptableObject
    {
        [Header("Per-unit upgrade tracks (CAPPED — §3/§15). PROVISIONAL")]
        public List<UpgradeTrack> tracks = new List<UpgradeTrack>();

        [Header("Commander levels (CAPPED) — PROVISIONAL")]
        public int commanderMaxLevel = 10;   // HARD CAP
        public int commanderBaseCostSilver = 200;
        public float commanderCostGrowth = 1.5f;
        // Commander leveling raises ABILITY availability/quality WITHIN the §6 power budget only —
        // it never lifts a commander above the ≤10–15% cap (server-enforced; ranked-normalized).
    }
}

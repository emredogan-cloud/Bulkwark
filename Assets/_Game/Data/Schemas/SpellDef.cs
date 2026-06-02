// BULWARK — SpellDef ScriptableObject schema (Data layer). Phase 2 §13 2.4 + §5.3.
// Draft-3 roguelite spells: a shared pool of ~12 owned spells, draft 3 per battle.
// CANON RULES encoded as fields: every spell has cooldown + charges, a TELEGRAPH, a
// COUNTER, and SYNERGY tags (§5.3) — "no un-counterable spell ships". Synergy example
// from canon: Chilled → Shatter (applyStatus=Chilled on one; synergyBonusVsStatus=Chilled
// on another). All numeric values are PROVISIONAL / LSD-owned (§15.6/§16); content stays
// within the §5.3 categories (Offensive/Control/Economy/Summon/Buff) — nothing invented.
// SCAFFOLD STATUS: authored, NOT compiled (no Unity — ADR-0-001/-002).

using UnityEngine;

namespace Bulwark.Data
{
    [CreateAssetMenu(fileName = "SpellDef", menuName = "BULWARK/Data/SpellDef", order = 1)]
    public class SpellDef : ScriptableObject
    {
        [Header("Identity (§5.3)")]
        [Tooltip("Stable content id, e.g. spell_freeze.")] public string id;
        public string displayName;
        public SpellCategory category = SpellCategory.Offensive;

        [Header("Cast — PROVISIONAL data")]
        public float cooldown;        // seconds between casts
        public int   charges = 1;     // §5.3 "cooldown + charge"
        public float telegraphTime;   // §5.3 telegraph window (MUST be > 0 — no un-counterable spell)

        [Header("Effect")]
        public TargetShape targetShape = TargetShape.Area;
        public float radius;          // for Area/Line
        public float magnitude;       // damage / heal / gold / buff strength (meaning per category)
        public float duration;        // status/buff duration (0 = instant)
        public StatusKind appliesStatus = StatusKind.None; // status inflicted/granted (e.g., Chilled)
        public int summonUnitIndex = -1;  // Summon category: index into the caster faction's unit catalog; -1 = none

        [Header("Synergy + counter (§5.3 — every spell is counterable)")]
        public StatusKind synergyBonusVsStatus = StatusKind.None; // e.g. Shatter bonus vs Chilled
        public float synergyMultiplier = 1f;                       // applied when target already has synergyBonusVsStatus
        [TextArea] public string counterNote;                      // how an opponent counters it (telegraph dodge / cleanse / spread / LoS)
    }
}

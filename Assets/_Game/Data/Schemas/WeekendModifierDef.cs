// BULWARK — WeekendModifierDef ScriptableObject (Data). Phase 4 §13 4.5, §7 (basic weekend modifiers ONLY).
// Data-driven rotating reward/rule knobs for a weekend — NOT the full event engine (Phase 7).
// Modifiers tune REWARD pacing or use EXISTING combat rules; they introduce NO new mechanics and
// NO power-for-pay. Reward multipliers are bounded + server-applied.
// SCAFFOLD STATUS: authored, NOT compiled when first written (CI validates).

using UnityEngine;

namespace Bulwark.Data
{
    [CreateAssetMenu(fileName = "WeekendModifierDef", menuName = "BULWARK/Data/WeekendModifierDef", order = 13)]
    public class WeekendModifierDef : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string description;

        [Header("Bounded reward knobs (data-only; no new mechanics, no pay-for-power)")]
        [Range(1f, 3f)] public float silverRewardMultiplier = 1f;
        [Range(1f, 3f)] public float passXpMultiplier = 1f;

        [Header("Optional EXISTING-rule modifier toggle (no new mechanics, §7)")]
        public string existingRuleKey;   // references a known combat rule (e.g. "endless_faster_waves"); empty = none
    }
}

// BULWARK — CommanderDef ScriptableObject schema (Data layer). Phase 2 §13 2.5 + §5.5/§6.
// Commander = identity, NOT power. MVP ships 1 commander per faction (2 total, §5.5) as
// "bones": exactly 1 ACTIVE + 1 PASSIVE ability, within a hard ≤10–15% power budget (§6 /
// prompt 2.5). Collectible commander ROSTER is Phase 7 (forbidden now). Ranked normalization
// is mandatory (§16) — represented here by a stub flag the sim honors.
// All numeric values PROVISIONAL / LSD-owned (§16); power budget is an INVIOLABLE cap.
// SCAFFOLD STATUS: authored, NOT compiled (no Unity — ADR-0-001/-002).

using UnityEngine;

namespace Bulwark.Data
{
    public enum CommanderActiveKind : int
    {
        None = 0,
        Rally = 1,      // temporary attack/move buff to nearby allies (Iron Pact identity: discipline)
        WarCry = 2,     // temporary speed/aggression burst (Ashen Horde identity: swarm)
    }

    public enum CommanderPassiveKind : int
    {
        None = 0,
        Quartermaster = 1, // small standing economy/training bonus
        Bloodthirst = 2,   // small standing combat-uptime bonus
    }

    [CreateAssetMenu(fileName = "CommanderDef", menuName = "BULWARK/Data/CommanderDef", order = 3)]
    public class CommanderDef : ScriptableObject
    {
        [Header("Identity (§5.5)")]
        public string id;
        public string displayName;
        public Faction faction = Faction.IronPact;

        [Header("Active ability (exactly one) — PROVISIONAL")]
        public CommanderActiveKind active = CommanderActiveKind.None;
        public float activeCooldown;   // seconds
        public float activeMagnitude;  // buff fraction (e.g. 0.15 = +15%) — see powerBudgetPct cap
        public float activeRadius;
        public float activeDuration;

        [Header("Passive ability (exactly one) — PROVISIONAL")]
        public CommanderPassiveKind passive = CommanderPassiveKind.None;
        public float passiveMagnitude; // standing bonus fraction

        [Header("Fairness caps (INVIOLABLE — §6)")]
        [Range(0f, 0.15f)] public float powerBudgetPct = 0.12f; // total commander power ≤10–15%
        public bool rankedNormalized = true;                    // ranked-normalization hook (stub honored by sim)
    }
}

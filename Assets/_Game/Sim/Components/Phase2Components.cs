// BULWARK — Phase 2 ECS component contract (SHARED). Roadmap §13 Phase 2 + §4/§5/§11.
// Integration backbone for the Phase-2 systems (Terrain, Formation, CounterMatrix, Spell,
// CommanderAbility, SquadAI, AIScheduler). EXTENDS Phase0/Phase1 components — those are
// reused, not redefined. Phase 2 POPULATES the neutral factor slots that Phase 1 plumbed:
//   • TerrainFactor (was 1.0) ← TerrainSystem (high ground/choke/cover/hazard, §4/§11)
//   • Positional   (was 1.0) ← PositionalSystem (flank 1.5 / back 2.0 geometry, §4)
// Canon discipline:
//   • §12 ECS boundary: plain IComponentData/IBufferElementData; input/draft UI = MonoBehaviour.
//   • §4 "one combat core": spells/terrain/positional feed the SAME §4 modifier chain; no fork.
//   • §5.3 "no un-counterable spell": every spell has a telegraph window + a counter (data).
//   • §6 commander power budget is an INVIOLABLE cap (≤10–15%).
//   • §15.6 no invented balance: numeric values come from DATA (UnitDef/SpellDef/CommanderDef/
//     MapDef/CounterMatrix.asset). Components carry values populated at load, not literals.
//   • Forbidden now (Phase 3+/7): meta/economy persistence, monetization, commander COLLECTION,
//     determinism/replays, 3rd faction, biomes. Do not add.
// SCAFFOLD STATUS: authored, NOT compiled (no Unity — ADR-0-001/-002/-1-001).

using Unity.Entities;
using Unity.Mathematics;
using Bulwark.Data;

namespace Bulwark.Sim
{
    // ---------------- 2.1 Terrain + positioning ----------------
    /// <summary>Unit facing (for flank/back geometry §4 and formation cohesion). Unit-normalized.</summary>
    public struct Facing : IComponentData { public float2 Dir; }

    /// <summary>
    /// A terrain feature entity (high ground / choke / cover / hazard, §4/§11). Effect
    /// multipliers are DATA (populated at map load from MapDef + provisional LSD-owned
    /// per-kind defaults), NOT literals baked into systems.
    /// </summary>
    public struct TerrainFeature : IComponentData
    {
        public TerrainKind Kind;   // Bulwark.Data.TerrainKind
        public float Width;
        public int Row;
        public float AttackMult;   // HighGround: occupant outgoing-damage bonus (>1)
        public float DefenseMult;  // Cover: occupant incoming-damage reduction (<1)
        public float MoveMult;     // Choke: throughput/speed reduction (<1)
        public float HazardDps;    // Hazard: damage/sec to occupants (>0)
    }

    /// <summary>Marks which terrain feature a unit currently occupies (Entity.Null = open ground).</summary>
    public struct TerrainOccupancy : IComponentData { public Entity Feature; }

    // ---------------- 2.2 Formations ----------------
    public enum FormationType : byte { Line = 0, Tight = 1, Loose = 2 }

    /// <summary>Squad-level formation order (one per squad entity). Anchor/Facing set by SquadAI/control.</summary>
    public struct SquadFormation : IComponentData
    {
        public int SquadId;
        public FormationType Type;
        public float2 Anchor;
        public float2 Facing;
    }

    /// <summary>A unit's slot within its squad formation (FormationSystem assigns world offsets).</summary>
    public struct FormationMember : IComponentData { public int SquadId; public int Slot; }

    // ---------------- 2.4 Spells (draft-3, synergy, telegraph/counter) ----------------
    /// <summary>Active status effects on a unit (Chilled/Burning/Poisoned/Stunned/Hasted/Raged...).</summary>
    public struct StatusEffect : IBufferElementData
    {
        public StatusKind Kind;   // Bulwark.Data.StatusKind
        public float Remaining;   // seconds left
        public float Magnitude;   // strength (DoT/sec, slow %, buff %)
    }

    /// <summary>One of a side's 3 drafted spells + its runtime cooldown/charges.</summary>
    public struct SpellSlot : IBufferElementData
    {
        public int SpellIndex;    // index into the shared SpellDef pool catalog
        public float Cooldown;    // seconds remaining
        public int ChargesLeft;
    }

    /// <summary>Singleton per side: holds the DynamicBuffer&lt;SpellSlot&gt; (the drafted 3).</summary>
    public struct SpellLoadoutTag : IComponentData { public int Team; }

    /// <summary>Pre-battle draft gate (§13 2.4 "draft 3"). Sim waits until both sides have drafted.</summary>
    public struct DraftState : IComponentData { public byte PlayerDone; public byte AiDone; }

    /// <summary>A queued cast request (player UI or SquadAI append; SpellSystem consumes).</summary>
    public struct SpellCastRequest : IBufferElementData
    {
        public int Team;
        public int SlotIndex;     // which of the side's 3 slots
        public float2 TargetPos;
        public Entity TargetEntity;
    }
    public struct SpellCastInboxTag : IComponentData { } // singleton holding DynamicBuffer<SpellCastRequest>

    /// <summary>
    /// A live telegraph: the counterplay window (§5.3). Spawned on cast; after Remaining≤0 the
    /// SpellSystem resolves the effect at Pos/Radius. Visible long enough to dodge/counter.
    /// </summary>
    public struct ActiveTelegraph : IComponentData
    {
        public int SpellIndex;
        public int CasterTeam;
        public float2 Pos;
        public float Radius;
        public float Remaining;   // telegraph time left before resolution
        public Entity TargetEntity;
    }

    // ---------------- 2.5 Commander (capped power, §6) ----------------
    /// <summary>
    /// One commander per side (the §5.5 MVP "bones"): exactly 1 active + 1 passive, within a
    /// hard power budget. Active is player/AI-triggered; passive is a standing buff. Magnitudes
    /// are clamped to PowerBudgetPct by CommanderAbilitySystem (INVIOLABLE fairness, §6).
    /// </summary>
    public struct CommanderRuntime : IComponentData
    {
        public int Team;
        public CommanderActiveKind Active;     // Bulwark.Data.CommanderActiveKind
        public CommanderPassiveKind Passive;   // Bulwark.Data.CommanderPassiveKind
        public float ActiveCooldown;
        public float ActiveTimer;              // seconds until ready
        public float ActiveMagnitude;
        public float ActiveRadius;
        public float ActiveDuration;
        public float PassiveMagnitude;
        public float PowerBudgetPct;           // ≤0.15 (§6) — enforced as a clamp
        public byte RankedNormalized;          // 1 = normalize in ranked (stub honored)
        public byte ActiveRequested;           // set by player UI / SquadAI; cleared on cast
    }

    // ---------------- 2.6 AI layering ----------------
    public enum SquadPosture : byte { Hold = 0, Advance = 1, FocusFire = 2, Retreat = 3 }

    /// <summary>Squad-level tactical layer (above the §13 P1.5 commander stance; below per-unit).</summary>
    public struct SquadAIState : IComponentData
    {
        public int SquadId;
        public int Team;
        public SquadPosture Posture;
        public Entity FocusTarget;  // for FocusFire
        public float ReevalCooldown;
    }

    /// <summary>
    /// Budgeted AI scheduler (§4 "budgeted scheduler"): processes at most BudgetPerFrame agents
    /// per frame, round-robin via Cursor, so AI cost stays frame-stable under load (§12 perf).
    /// </summary>
    public struct AISchedulerState : IComponentData { public int Cursor; public int BudgetPerFrame; }

    /// <summary>
    /// Multi-axis difficulty (§4 / §13 2.6): independent knobs, all data/LSD-owned. 1.0 = baseline.
    /// Distinct from Phase-1 Difficulty.Multiplier (which scales the combat chain); these bias AI.
    /// </summary>
    public struct DifficultyAxes : IComponentData
    {
        public float Eco;         // AI economy pacing
        public float Aggression;  // push vs defend bias
        public float Stats;       // AI unit stat scaling (kept modest; fairness §16)
        public float SpellAccess; // how readily AI drafts/casts spells
    }
}

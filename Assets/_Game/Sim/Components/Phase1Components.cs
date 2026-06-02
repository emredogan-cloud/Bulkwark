// BULWARK — Phase 1 ECS component contract (SHARED). Roadmap §13 Phase 1 + §4/§11.
// This is the integration backbone for all Phase-1 systems (Mining, Training, Targeting,
// Combat, StatueDamage, BasicAI, InfluenceMap). It EXTENDS Phase-0 SimComponents.cs
// (Position, Health, Team, AttackState, Movement, UnitTag) — those are reused, not redefined.
//
// Canon discipline encoded here:
//  • §12 ECS boundary: these are plain IComponentData; no UI/meta logic.
//  • §13 P1.4 modifier chain has FACTOR SLOTS for positional/terrain/level/difficulty, but
//    in Phase 1 they are NEUTRAL (1.0 / level 0). Flank/back geometry + terrain = Phase 2.1
//    (forbidden now) — the slots exist so Phase 2 populates them without re-plumbing.
//  • §15.6 no invented balance: numeric stats come from UnitDef/BalanceConfig DATA, not here.
// SCAFFOLD STATUS: authored, NOT compiled (no Unity — ADR-0-001/-002).

using Unity.Entities;
using Unity.Mathematics;
using Bulwark.Data;

namespace Bulwark.Sim
{
    // ---------------- Spatial: single horizontal front, 3 rows (§11) ----------------
    /// <summary>Row band 0..2 on the single front; same-row targeting preference (§13 P1.4).</summary>
    public struct Row : IComponentData { public int Index; }

    // ---------------- Economy (1.1) ----------------
    /// <summary>Per-side Gold balance (LOCAL battle economy only — NO meta/currency persistence, §J).</summary>
    public struct GoldStore : IComponentData { public int Team; public int Amount; }

    /// <summary>Tag: this unit is a Miner (mines Gold, weak combat — §5.2).</summary>
    public struct MinerTag : IComponentData { }

    /// <summary>Miner's current mining assignment + accumulator.</summary>
    public struct MiningState : IComponentData
    {
        public Entity Mine;       // Entity.Null when unassigned
        public float Accum;       // fractional Gold accumulator
        public float RatePerSec;  // from UnitDef.miningRatePerSec (data)
    }

    /// <summary>Fixed Gold node with a miner cap; contestable position (§11). OwnerTeam = -1 neutral.</summary>
    public struct MineNode : IComponentData
    {
        public int Capacity;      // miner cap
        public int Occupants;     // current miners assigned
        public float YieldPerSec; // node-side yield ceiling (data)
        public int OwnerTeam;     // -1 = contestable/neutral
    }

    // ---------------- Objective: Statue (1.1) ----------------
    public enum StatuePhase : byte { Intact = 0, Cracked = 1, Breaking = 2, Destroyed = 3 }

    /// <summary>Win/lose objective for a side.</summary>
    public struct StatueTag : IComponentData { public int Team; }

    /// <summary>
    /// Statue health + shield phase + readable damage states + recovered trickle-damage
    /// throttle (§11). Damage-state thresholds are PROVISIONAL data, LSD-owned.
    /// </summary>
    public struct StatueState : IComponentData
    {
        public float Health;
        public float MaxHealth;
        public float ShieldHealth;     // shield phase absorbs before Health
        public bool ShieldActive;
        public StatuePhase Phase;      // recomputed from Health fraction
        public float TrickleAccum;     // accumulates sub-threshold trickle
        public float TrickleThrottle;  // min interval/threshold before trickle applies (recovered statue math)
    }

    // ---------------- Combat profile (§13 P1.4 modifier chain) ----------------
    /// <summary>
    /// Per-unit combat identity for the modifier chain:
    /// round(base × (1 + Level×PerLevel)) × typeArmor × positional × terrain × difficulty.
    /// At P1: Level = 0 (progression = Phase 3), so the level factor = 1.
    /// </summary>
    public struct CombatProfile : IComponentData
    {
        public DamageType DamageType;
        public ArmorClass ArmorClass;
        public int Level;       // 0 at P1
        public float PerLevel;  // capped-upgrade slope (Phase 3); unused at P1
    }

    /// <summary>Positional factor SLOT. = 1.0 at P1 (flank/back geometry is Phase 2.1, forbidden now).</summary>
    public struct Positional : IComponentData { public float Multiplier; }

    /// <summary>Terrain factor SLOT. = 1.0 neutral at P1 (terrain is Phase 2.1, forbidden now).</summary>
    public struct TerrainFactor : IComponentData { public float Multiplier; }

    /// <summary>
    /// Counter-matrix singleton: 20 cells (5 damage × 4 armor), baked from BalanceConfig.
    /// Combat reads cell (damageType-1)*4 + (armorClass-1); 0 → treated as 1.0.
    /// "Basic counters only" at P1 (few non-1 cells); full matrix = Phase 2.2.
    /// </summary>
    public struct CounterCell : IBufferElementData { public float Multiplier; }
    public struct CounterMatrixTag : IComponentData { } // marks the singleton entity holding the buffer

    // ---------------- Targeting (1.4) ----------------
    /// <summary>Current target + re-eval timing + same-target stickiness bias (×1.2, §13 P1.4).</summary>
    public struct Targeting : IComponentData
    {
        public Entity Current;
        public float ReevalCooldown; // throttle re-acquire
        public float Stickiness;     // multiplier applied to the current target's score (e.g. 1.2)
    }

    // ---------------- Control (1.3) ----------------
    public struct PlayerControlled : IComponentData { }  // team tag: human side
    public struct Possessed : IComponentData { }         // unit currently possessed (player drives directly)
    public struct Squad : IComponentData { public int Id; } // squad grouping for select/command

    /// <summary>Order injected by the MonoBehaviour control shell (§12 boundary: input writes data, sim reads it).</summary>
    public struct ManualOrder : IComponentData
    {
        public float2 MovePos;
        public Entity AttackTarget;
        public byte HasMove;     // 0/1
        public byte HasAttack;   // 0/1
    }

    // ---------------- AI (1.5) ----------------
    public enum CommanderStance : byte { Defend = 0, Push = 1, Harass = 2 }

    public struct AIControlled : IComponentData { }  // team tag: AI side
    public struct AICommander : IComponentData
    {
        public int Team;
        public CommanderStance Stance;
        public float ReevalCooldown; // single-layer utility re-eval throttle (O(1), §13 P1.5)
    }

    /// <summary>Difficulty SLOT. = 1.0 at P1 (multi-axis difficulty = Phase 2.6).</summary>
    public struct Difficulty : IComponentData { public float Multiplier; }

    // ---------------- Match state ----------------
    public enum MatchOutcome : byte { Ongoing = 0, Victory = 1, Defeat = 2 }
    public struct MatchState : IComponentData { public MatchOutcome Outcome; }

    // ---------------- Training queue (1.2) ----------------
    /// <summary>A queued train order; resolves to a spawn when Remaining ≤ 0 and Gold was paid.</summary>
    public struct TrainOrder : IBufferElementData
    {
        public int UnitIndex;   // index into the side's authored UnitDef set
        public float Remaining; // train seconds left
        public byte Paid;       // gold already deducted (0/1)
    }
    public struct TrainQueueTag : IComponentData { public int Team; } // singleton per side holding the TrainOrder buffer
}

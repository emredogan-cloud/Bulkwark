// BULWARK — ECS battle-sim components (Phase 0.2 scaffold).
// Roadmap §12: ECS/DOTS is the BATTLE-SIM HOT PATH ONLY. No UI/meta logic here.
// SCAFFOLD STATUS: authored, NOT compiled — no Unity 6 / Entities toolchain in
// this environment (see docs/adr/ADR-0-001). A Unity dev opens and builds this.
// Minimal by design (roadmap §13 P0 risk: "ECS over-architecting").
// NO gameplay content / balance values live here (Phase 1+, and §15 forbids it now).

using Unity.Entities;
using Unity.Mathematics;

namespace Bulwark.Sim
{
    /// <summary>World-space position of a sim entity (2D front; z unused at MVP).</summary>
    public struct Position : IComponentData
    {
        public float2 Value;
    }

    /// <summary>Current/max hit points. Values come from data (UnitDef) at spawn, never hard-coded.</summary>
    public struct Health : IComponentData
    {
        public float Current;
        public float Max;
    }

    /// <summary>Faction allegiance. 0/1 are the two battle sides; resolved from match setup, not canon here.</summary>
    public struct Team : IComponentData
    {
        public int Id;
    }

    /// <summary>Per-unit attack timing/intent. Stats are injected from data, not authored in the sim.</summary>
    public struct AttackState : IComponentData
    {
        public Entity Target;     // Entity.Null when no valid target
        public float Cooldown;    // seconds remaining until next swing
        public float Range;       // engagement distance (from UnitDef)
        public float Damage;      // per-swing damage (from UnitDef)
        public float Interval;    // seconds between swings (from UnitDef)
    }

    /// <summary>Movement intent for the sim. Speed sourced from data; no pathing at P0 (single front).</summary>
    public struct Movement : IComponentData
    {
        public float Speed;       // units/sec (from UnitDef)
    }

    /// <summary>Tag marking a spawned combat unit (vs. environment/markers).</summary>
    public struct UnitTag : IComponentData { }
}

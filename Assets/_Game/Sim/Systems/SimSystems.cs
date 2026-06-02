// BULWARK — Phase 0.2 spike RETIRED. Roadmap §13 P1.4 / §4 / §12.
//
// This file previously held the Phase-0 MoveSystem + AttackSystem spike, whose target
// acquisition was an O(N^2) nearest-enemy scan flagged for replacement (§12 perf rule:
// targeting/AI must stay ~O(1) per unit). Phase 1 replaces that loop entirely:
//
//   • Target ACQUISITION         → TargetingSystem  (Assets/_Game/Sim/Systems/Targeting.cs),
//                                  bucketed via the influence map (NO all-pairs scan).
//   • Threat field               → InfluenceMapSystem (Assets/_Game/Sim/InfluenceMap.cs).
//   • MOVEMENT (toward target)   → MovementSystem   (Assets/_Game/Sim/Systems/Combat.cs),
//                                  reads Targeting.Current, honours PossessControl overrides.
//   • DAMAGE (modifier chain)    → CombatSystem     (Assets/_Game/Sim/Systems/Combat.cs),
//                                  basic counters + statue inbox + ECB destroy.
//
// The old MoveSystem and AttackSystem are intentionally DELETED — they are superseded and
// must not coexist (two movement/attack loops would double-step units and re-introduce the
// O(N^2) scan). AttackState.Target is no longer driven here; TargetingSystem owns acquisition
// via Targeting.Current (per the Phase-1 shared contract).
//
// This file is kept (rather than removed) so the Phase-0 path that referenced it stays valid
// and the retirement is documented in-tree. It declares no systems.
// SCAFFOLD STATUS: authored, NOT compiled (no Unity — ADR-0-001/-002).

namespace Bulwark.Sim
{
    // Intentionally empty. See file header: P0.2 MoveSystem/AttackSystem replaced by
    // InfluenceMapSystem + TargetingSystem + MovementSystem + CombatSystem in Phase 1.4.
}

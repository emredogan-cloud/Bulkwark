// BULWARK — Phase-1 control CONSUMER system (1.3). Roadmap §13 P1.3, §4 ("direct unit control"
// kept + extended), §12 (ECS owns gameplay sim; this is the sim side of the control hook).
//
// RESPONSIBILITY (§12 boundary): the MonoBehaviour shell (Assets/_Game/Control/BattleInput.cs) WRITES
// ManualOrder + the Possessed tag onto PlayerControlled units. This module READS those and resolves
// them into sim state that the movement / targeting / combat systems consume:
//   • Manual MOVE   : ManualOrder.HasMove==1 -> set a MoveDestination override (consumed by the §13 P1.4
//                     movement system) so the unit walks to MovePos instead of its auto destination.
//   • Manual ATTACK : ManualOrder.HasAttack==1 -> override Targeting.Current with AttackTarget. This takes
//                     PRECEDENCE over auto-targeting (§13 P1.4). We hold the pick via Targeting.Stickiness
//                     so the auto-targeter does not immediately re-evaluate over the player's pick.
//   • Auto-target STILL applies when there is no manual attack target (we never clear Targeting here;
//                     the influence-map targeter owns acquisition via Targeting.Current — contract rule).
//   • After applying, HasMove/HasAttack are CLEARED so each order fires exactly once.
//
// 1.3 <-> 1.4 HANDOFF CONTRACT (explicit, so manual moves never silently no-op):
//   MoveDestination{float2 Value; byte Active} is a STABLE control-module convention introduced under the
//   prompt's "introduce IF NEEDED" clause. To make the handoff verifiable from WITHIN this module:
//     (1) EnsureMoveDestinationSystem (a non-Burst structural pass in THIS file) ADDS MoveDestination to
//         every PlayerControlled UnitTag entity that lacks it, via an EntityCommandBuffer. Therefore by the
//         time orders are consumed, controllable units provably carry the component and HasMove is never
//         dropped for want of it.
//     (2) PossessControlSystem WRITES Value=MovePos, Active=1 when HasMove==1.
//     (3) The §13 P1.4 movement system READS MoveDestination: while Active==1 it drives the unit toward
//         Value; on ARRIVAL (within its own arrival radius) it MUST set Active=0 (clear the override) and
//         fall back to the auto destination. 1.3 never clears Active — ownership of clearing is 1.4's.
//   If 1.4 standardizes a different destination field later, this convention collapses into it; until then
//   the ensure-pass guarantees the component exists so the central deliverable (manual MovePos overrides
//   movement destination) cannot silently fail.
//
// CANON DISCIPLINE:
//   • Reuses existing Phase-1 components (ManualOrder/Possessed/Targeting). Invents NO mechanics (§15);
//     MoveDestination is a control-flow override, not a new gameplay mechanic/currency/unit.
//   • Per the shared contract, Phase 1 does NOT use AttackState.Target — acquisition lives in Targeting.
//   • No later-phase features (no formations/spells/commander logic). No balance numbers (§15.6); the
//     manual-hold duration is sourced from Targeting.Stickiness (contract field), not a hardcoded constant.
// SCAFFOLD STATUS: authored, NOT compiled (no Unity 6 / Entities here — ADR-0-001/-002).
// DEFERRED: arrival-radius / order-clear feel is GATE-1 on-device tuning (see deferredValidations).

using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Bulwark.Sim
{
    /// <summary>
    /// Player-issued move destination override (control-module convention, 1.3; see 1.3&lt;-&gt;1.4 handoff
    /// contract in PossessControl.cs header). When <see cref="Active"/> == 1 the §13 P1.4 movement system
    /// drives the unit toward <see cref="Value"/> instead of its auto destination, and CLEARS Active on
    /// arrival. Neutral default = 0 (follow auto destination).
    /// </summary>
    public struct MoveDestination : IComponentData
    {
        public float2 Value;
        public byte Active; // 0 = follow auto destination; 1 = honor Value (player order)
    }

    /// <summary>
    /// Structural guarantee for the 1.3&lt;-&gt;1.4 handoff: ensures every controllable unit
    /// (PlayerControlled + UnitTag) carries <see cref="MoveDestination"/> so PossessControlSystem can
    /// always set the override and HasMove is never silently dropped. Non-Burst because it performs
    /// structural changes via an EntityCommandBuffer (cannot be done inside the Burst order-consumer).
    /// Runs BEFORE PossessControlSystem so newly spawned units are covered the tick after spawn.
    /// </summary>
    [RequireMatchingQueriesForUpdate]
    [UpdateBefore(typeof(PossessControlSystem))]
    public partial struct EnsureMoveDestinationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            // Only run while there is at least one controllable unit lacking the override component.
            var builder = new Unity.Entities.EntityQueryBuilder(Unity.Collections.Allocator.Temp)
                .WithAll<PlayerControlled, UnitTag>()
                .WithNone<MoveDestination>();
            var query = state.GetEntityQuery(builder);
            state.RequireForUpdate(query);
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach (var (_, entity) in
                     SystemAPI.Query<RefRO<UnitTag>>()
                              .WithAll<PlayerControlled>()
                              .WithNone<MoveDestination>()
                              .WithEntityAccess())
            {
                // Neutral default (Active=0): unit follows its auto destination until a player order arrives.
                ecb.AddComponent(entity, new MoveDestination { Value = float2.zero, Active = 0 });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    /// <summary>
    /// Consumes ManualOrder for commanded / possessed units (§13 P1.3). Runs in the simulation group
    /// before the movement & targeting systems so this tick's orders are visible to them.
    /// </summary>
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    public partial struct PossessControlSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            // Only run when at least one order exists to consume.
            state.RequireForUpdate<ManualOrder>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (orderRW, entity) in
                     SystemAPI.Query<RefRW<ManualOrder>>()
                              .WithAll<UnitTag>()
                              .WithEntityAccess())
            {
                ManualOrder order = orderRW.ValueRO;
                bool changed = false;

                // ---- manual MOVE: override movement destination ----
                if (order.HasMove != 0)
                {
                    // EnsureMoveDestinationSystem (above, UpdateBefore) guarantees PlayerControlled UnitTag
                    // entities carry MoveDestination, so this override is applied — never silently dropped.
                    if (SystemAPI.HasComponent<MoveDestination>(entity))
                    {
                        var dest = SystemAPI.GetComponent<MoveDestination>(entity);
                        dest.Value = order.MovePos;
                        dest.Active = 1;
                        SystemAPI.SetComponent(entity, dest);

                        // Order applied — consume it.
                        order.HasMove = 0;
                        changed = true;
                    }
                    // If the component is somehow still absent (e.g., a non-PlayerControlled unit issued an
                    // order, or this is the very tick of spawn before the ensure-pass covers it) we DELIBERATELY
                    // leave HasMove==1 so the order is retried next tick instead of being lost.
                }

                // ---- manual ATTACK: override Targeting.Current (takes precedence over auto-target) ----
                if (order.HasAttack != 0)
                {
                    if (SystemAPI.HasComponent<Targeting>(entity))
                    {
                        var tgt = SystemAPI.GetComponent<Targeting>(entity);

                        // Only honor a still-valid (existing) enemy entity; otherwise leave auto-target alone.
                        if (order.AttackTarget != Entity.Null &&
                            SystemAPI.Exists(order.AttackTarget) &&
                            SystemAPI.HasComponent<Health>(order.AttackTarget))
                        {
                            tgt.Current = order.AttackTarget;
                            // Hold the player's pick using the contract's Stickiness window so the
                            // auto-targeter's own per-tick ReevalCooldown decrement does not immediately
                            // re-evaluate and stomp the manual pick. Value is data-fed (Targeting.Stickiness),
                            // never a hardcoded constant (§15.6). math.max preserves any longer pending hold.
                            tgt.ReevalCooldown = math.max(tgt.ReevalCooldown, tgt.Stickiness);
                            SystemAPI.SetComponent(entity, tgt);
                        }
                    }

                    order.HasAttack = 0;
                    // Consume the target reference once applied.
                    order.AttackTarget = Entity.Null;
                    changed = true;
                }

                if (changed)
                    orderRW.ValueRW = order;
            }
        }
    }
}

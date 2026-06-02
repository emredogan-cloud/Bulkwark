// BULWARK — Phase 1 MOVEMENT + COMBAT (roadmap §13 P1.4 "modifier chain; basic counters";
// §4 "Type×armor counter matrix (5×4)"; §11 single front / Statue objective; §12 ECS boundary).
//
// MovementSystem: closes on Targeting.Current until within AttackState.Range. If the unit is
//   Possessed or has a manual move order (ManualOrder.HasMove==1), PossessControl's destination
//   wins (§12 control boundary, §13 P1.3). Reads Targeting.Current — NOT AttackState.Target
//   (TargetingSystem owns acquisition; P0's O(N^2) AttackSystem.Target field is retired).
//
// CombatSystem: on AttackState.Cooldown<=0 and the target in Range, applies the §13 P1.4 chain:
//   dmg = round(base × (1 + Level×PerLevel)) × typeArmor × positional × terrain × difficulty
//   where base = AttackState.Damage and typeArmor = CounterCell[(dmgType-1)*4 + (armor-1)]
//   on the CounterMatrixTag singleton buffer (cell 0 → treated as 1.0).
//   At P1: Level=0 and Positional/Terrain/Difficulty=1.0 (neutral SLOTS — flank/back geometry
//   + terrain are Phase 2.1, FORBIDDEN now; we read the slots, we do NOT compute geometry).
//   If the target is a Statue (StatueTag), damage is appended to its StatueDamageInbox (the
//   StatueDamageSystem applies shield/phase math, §11) instead of touching Health directly.
//   Units reaching Health<=0 are destroyed via ECB.
//
// §15.6 no invented balance: base/range/interval come from AttackState (baked from UnitDef);
// the only literals here are NEUTRAL defaults (1.0) for the unused factor slots, per the rules.
// SCAFFOLD STATUS: authored, NOT compiled (no Unity — ADR-0-001/-002).

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Bulwark.Sim
{
    // StatueDamageInbox is declared ONCE in StatueDamage.cs (its draining owner); CombatSystem
    // below appends to that same buffer type. (Reconciliation: removed the duplicate declaration
    // that used to live here — a buffer element type may be declared only once.)

    /// <summary>
    /// Advances each unit toward Targeting.Current along the front until within
    /// AttackState.Range. A possessed unit, or one with an active manual move override
    /// (MoveDestination.Active==1, set by PossessControlSystem from ManualOrder), follows that
    /// destination instead (control boundary, §12 / §13 P1.3). On ARRIVAL the manual override is
    /// CLEARED here (Active=0) — per the 1.3<->1.4 handoff contract in PossessControl.cs, 1.4
    /// owns clearing — so the unit then falls back to auto-targeting.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(TargetingSystem))]
    public partial struct MovementSystem : ISystem
    {
        // Arrival radius for a manual MoveDestination order: the override is satisfied (and
        // cleared) once the unit is within this distance of the ordered point. NOT a combat/
        // balance value — it is the control-handoff arrival tolerance (§13 P1.3, DEFERRED feel).
        private const float k_ManualArriveRadius = 1e-3f;

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;

            foreach (var (pos, move, atk, tgt, e) in
                     SystemAPI.Query<RefRW<Position>, RefRO<Movement>, RefRO<AttackState>, RefRO<Targeting>>()
                              .WithAll<UnitTag>()
                              .WithEntityAccess())
            {
                // CONTROL OVERRIDE (§12 / §13 P1.3): PossessControlSystem (which runs BEFORE this
                // system) consumes ManualOrder and writes the player's move intent into
                // MoveDestination{Value,Active}. So we read that override here — NOT ManualOrder
                // (whose HasMove flag PossessControl already cleared this tick). A possessed unit,
                // or one with an active MoveDestination, is control-driven, not auto-targeted.
                bool hasDest = SystemAPI.HasComponent<MoveDestination>(e);
                bool manualMove = hasDest && SystemAPI.GetComponent<MoveDestination>(e).Active != 0;
                if (SystemAPI.HasComponent<Possessed>(e) || manualMove)
                {
                    if (manualMove)
                    {
                        var dest = SystemAPI.GetComponent<MoveDestination>(e);
                        StepToward(ref pos.ValueRW.Value, dest.Value, move.ValueRO.Speed, dt,
                                   stopDist: k_ManualArriveRadius);
                        // ARRIVAL → clear the override (1.4 owns clearing per the handoff contract),
                        // so the unit resumes auto-targeting next tick.
                        if (math.distance(pos.ValueRO.Value, dest.Value) <= k_ManualArriveRadius)
                        {
                            dest.Active = 0;
                            SystemAPI.SetComponent(e, dest);
                        }
                    }
                    // Pure-possess (no active move override) leaves position to the control shell.
                    continue;
                }

                // AUTO: close on the targeting-owned current target until inside attack range.
                Entity target = tgt.ValueRO.Current;
                if (target == Entity.Null || !SystemAPI.HasComponent<Position>(target))
                    continue;

                float2 targetPos = SystemAPI.GetComponent<Position>(target).Value;
                StepToward(ref pos.ValueRW.Value, targetPos, move.ValueRO.Speed, dt,
                           stopDist: atk.ValueRO.Range);
            }
        }

        private static void StepToward(ref float2 p, float2 dest, float speed, float dt, float stopDist)
        {
            float2 delta = dest - p;
            float dist = math.length(delta);
            if (dist > stopDist && dist > 1e-4f)
            {
                float step = math.min(speed * dt, dist - stopDist);
                if (step > 0f)
                    p += (delta / dist) * step;
            }
        }
    }

    /// <summary>
    /// Resolves attacks against Targeting.Current using the §13 P1.4 modifier chain.
    /// Statue targets receive damage via StatueDamageInbox; units take it on Health.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(MovementSystem))]
    public partial struct CombatSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;

            // Resolve the counter-matrix singleton buffer once (cell 0 → 1.0). Empty/absent → all 1.0.
            bool hasMatrix = SystemAPI.TryGetSingletonEntity<CounterMatrixTag>(out var matrixEntity);
            DynamicBuffer<CounterCell> matrix = default;
            if (hasMatrix && SystemAPI.HasBuffer<CounterCell>(matrixEntity))
                matrix = SystemAPI.GetBuffer<CounterCell>(matrixEntity);

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (atk, pos, profile, positional, terrain, difficulty, tgt, e) in
                     SystemAPI.Query<RefRW<AttackState>, RefRO<Position>, RefRO<CombatProfile>,
                                     RefRO<Positional>, RefRO<TerrainFactor>, RefRO<Difficulty>,
                                     RefRO<Targeting>>()
                              .WithAll<UnitTag>()
                              .WithEntityAccess())
            {
                // Tick the swing cooldown regardless of target state.
                atk.ValueRW.Cooldown = math.max(0f, atk.ValueRO.Cooldown - dt);

                Entity target = tgt.ValueRO.Current;
                if (target == Entity.Null || !SystemAPI.HasComponent<Position>(target))
                    continue;

                // In range?
                float2 targetPos = SystemAPI.GetComponent<Position>(target).Value;
                if (math.distance(pos.ValueRO.Value, targetPos) > atk.ValueRO.Range)
                    continue;

                // Off cooldown?
                if (atk.ValueRO.Cooldown > 0f)
                    continue;

                // ---- §13 P1.4 modifier chain ----
                // base × (1 + Level×PerLevel): Level=0 at P1 ⇒ factor 1, but plumbed for Phase 3.
                float levelScaled = atk.ValueRO.Damage *
                                    (1f + profile.ValueRO.Level * profile.ValueRO.PerLevel);
                float rounded = math.round(levelScaled);

                // typeArmor from the data-baked counter matrix ("basic counters only" at P1).
                float typeArmor = LookupTypeArmor(matrix, profile.ValueRO.DamageType,
                                                  ResolveTargetArmor(ref state, target));

                // Neutral SLOTS at P1 (do NOT compute flank/back/terrain geometry — Phase 2.1).
                float dmg = rounded
                            * typeArmor
                            * positional.ValueRO.Multiplier
                            * terrain.ValueRO.Multiplier
                            * difficulty.ValueRO.Multiplier;
                if (dmg < 0f) dmg = 0f;

                // ---- Apply ----
                if (SystemAPI.HasComponent<StatueTag>(target))
                {
                    // Statue: append to its inbox; StatueDamageSystem owns shield/phase math (§11).
                    if (SystemAPI.HasBuffer<StatueDamageInbox>(target))
                    {
                        var inbox = SystemAPI.GetBuffer<StatueDamageInbox>(target);
                        inbox.Add(new StatueDamageInbox { Amount = dmg });
                    }
                    else
                    {
                        // Inbox not yet attached: enqueue it via ECB so the hit isn't lost.
                        var added = ecb.AddBuffer<StatueDamageInbox>(target);
                        added.Add(new StatueDamageInbox { Amount = dmg });
                    }
                }
                else if (SystemAPI.HasComponent<Health>(target))
                {
                    var hp = SystemAPI.GetComponent<Health>(target);
                    hp.Current -= dmg;
                    ecb.SetComponent(target, hp);
                    if (hp.Current <= 0f)
                        ecb.DestroyEntity(target);
                }

                atk.ValueRW.Cooldown = atk.ValueRO.Interval; // reset swing timer
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        /// <summary>Armor class of the target if it has a CombatProfile; Unset (⇒ 1.0) otherwise.</summary>
        private static Bulwark.Data.ArmorClass ResolveTargetArmor(ref SystemState state, Entity target)
        {
            if (SystemAPI.HasComponent<CombatProfile>(target))
                return SystemAPI.GetComponent<CombatProfile>(target).ArmorClass;
            return Bulwark.Data.ArmorClass.Unset;
        }

        /// <summary>
        /// Counter-matrix lookup at index (damageType-1)*4 + (armorClass-1). Any Unset, out-of-
        /// range index, or a 0.0 cell is treated as the neutral 1.0 multiplier (matches
        /// BalanceConfig.GetMultiplier semantics).
        /// </summary>
        private static float LookupTypeArmor(DynamicBuffer<CounterCell> matrix,
                                             Bulwark.Data.DamageType dt,
                                             Bulwark.Data.ArmorClass ac)
        {
            int di = (int)dt - 1;
            int ai = (int)ac - 1;
            if (di < 0 || ai < 0) return 1f;
            if (!matrix.IsCreated || matrix.Length == 0) return 1f;
            int idx = di * 4 + ai;
            if (idx < 0 || idx >= matrix.Length) return 1f;
            float m = matrix[idx].Multiplier;
            return m != 0f ? m : 1f;
        }
    }
}

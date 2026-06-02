// BULWARK — Phase 1 TRAINING / queue / deploy system (roadmap §13 P1.2).
//   "One faction, 4 units; train/queue/deploy; 4 units (Miner + 3 roles)."
// This module turns queued TrainOrders (authored by the Control shell §13 P1.3 or the
// BasicAI commander §13 P1.5) into spawned sim units, paying LOCAL battle Gold (§13 P1.1)
// and deploying them at the side's spawn point onto the single 3-row front (§11, §13 P1.4).
//
// CANON DISCIPLINE encoded here:
//  • §12 ECS boundary: this is pure battle sim (ISystem/SystemAPI). NO UI, NO meta. Input/AI
//    only WRITE TrainOrder data via the EnqueueTrain helper; the sim READS and resolves it.
//  • §15.6 / Hard-Rule 4 NO hardcoded balance: every spawned stat (health, speed, damage,
//    range, interval, cost, train time, mining rate, damage type, armor class, role) is read
//    from UnitSpawnStats — a runtime value-copy of the authored UnitDef set. Code introduces
//    only NEUTRAL 1.0 factor-slot defaults (Positional/TerrainFactor) and Cooldown=0.
//  • Phase-1 scope (Hard-Rule 2): only the 4 authorized roles (Miner, Frontline, Skirmisher,
//    Ranged). NO progression (Level=0, PerLevel=0), NO terrain/positional bonus (slots=1.0),
//    NO formations, NO economy persistence (Gold is the in-battle GoldStore only).
//  • §12 perf rule: O(1) work per resolved order; we touch only the HEAD of each side's queue
//    per tick (FIFO train/deploy) — no all-pairs scans.
//  • Targeting (§13 P1.4): TargetingSystem OWNS acquisition via Targeting.Current. We seed
//    Targeting.Current = Entity.Null and DO NOT use AttackState.Target (per shared contract).
//  • Difficulty (§13 P1.5 / Phase 2.6 slot) is handled GLOBALLY, not per spawned unit.
//
// SHARED-CONTRACT INTEGRATION (review fix):
//  • UnitSpawnStats, UnitCatalogTag, TeamSpawnPoint are the SINGLE canonical convention types
//    declared in Assets/_Game/Sim/Systems/SimSystemOrder.cs and baked by BattleBootstrap.cs.
//    This file CONSUMES them (it must NOT redefine them — one definition per namespace).
//    UnitSpawnStats carries the sim-side mirror enums DamageTypeId/ArmorClassId/RoleId (their
//    int values are identical to Bulwark.Data.DamageType/ArmorClass/UnitRole, so a numeric
//    cast is exact). SpawnUnit casts via (int) and gates the miner on RoleId.Miner.
//  • RowCursor is this module's only OWNED type (module-local row bookkeeping, not canon).
//    BattleBootstrap.BuildSide bakes only TrainQueueTag + an empty TrainOrder buffer, so this
//    system LAZILY ensures RowCursor on each queue entity via the ECB the first time it is
//    seen without one — that is what makes the §13 P1.4 round-robin across rows 0..2 work.
//
// SYSTEM ORDER (SimSystemOrder.cs convention): every Phase-1 sibling pins itself into the
// SimulationSystemGroup between the SimPhaseBegin/SimPhaseEnd anchors. TrainingSystem is
// canonical slot 2 — it must run AFTER MiningSystem (which tops up GoldStore before payment)
// and BEFORE InfluenceMapSystem.
//
// SCAFFOLD STATUS: authored, NOT compiled (no Unity — ADR-0-001/-002). The neighbour system
// types (MiningSystem/InfluenceMapSystem) are authored by sibling modules; the [UpdateAfter]/
// [UpdateBefore] bindings resolve at Unity-runtime once those siblings are present (DEFERRED).

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Bulwark.Data;

namespace Bulwark.Sim
{
    // ---------------- Module-local row bookkeeping (NOT a canon mechanic) ----------------
    /// <summary>
    /// Round-robin row cursor, kept on each side's TrainQueueTag singleton entity so deployment
    /// spreads units across rows 0..2 deterministically WITHOUT a static (statics are unsafe in
    /// Burst/multi-world). This is the ONLY type Training.cs owns: the catalog/spawn convention
    /// types (UnitSpawnStats, UnitCatalogTag, TeamSpawnPoint) live in SimSystemOrder.cs and are
    /// baked by BattleBootstrap.cs. TrainingSystem lazily adds RowCursor when a baked queue lacks
    /// it, so no Bootstrap change is required.
    /// </summary>
    public struct RowCursor : IComponentData { public int Next; }

    // ---------------- Training system (§13 P1.2) ----------------
    /// <summary>
    /// Processes the HEAD TrainOrder of each side's queue (FIFO train/deploy):
    ///   1. If unpaid, try to deduct UnitSpawnStats[UnitIndex].GoldCost from that Team's
    ///      GoldStore; if Gold is insufficient, WAIT (the order stalls at the head — no spawn,
    ///      no partial charge) so economy pressure is real (§13 P1.1).
    ///   2. Once paid, count down Remaining by dt.
    ///   3. When Remaining ≤ 0, SPAWN the unit via ECB at the side's TeamSpawnPoint with all
    ///      stats sourced from data, then remove the head order.
    /// One unit resolves per side per tick at the head — train time gates throughput, matching
    /// the §13 P1.2 "queue" model. Structural changes (spawn / RowCursor add) go through an ECB.
    /// Not [BurstCompile]: it walks singletons + a DynamicBuffer and issues structural changes;
    /// kept plain ISystem like the Phase-0 AttackSystem. Per-tick cost is O(sides), ~O(1) (§12).
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SimPhaseBegin))]
    [UpdateBefore(typeof(SimPhaseEnd))]
    [UpdateAfter(typeof(MiningSystem))]        // GoldStore is topped up before we charge it (slot 1 → 2).
    [UpdateBefore(typeof(InfluenceMapSystem))] // newly-spawned units exist before the influence pass (slot 2 → 3).
    public partial struct TrainingSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;

            // Catalog is required to resolve any order; if not yet baked, nothing to do.
            if (!SystemAPI.HasSingleton<UnitCatalogTag>())
                return;
            Entity catalogEntity = SystemAPI.GetSingletonEntity<UnitCatalogTag>();
            DynamicBuffer<UnitSpawnStats> catalog = SystemAPI.GetBuffer<UnitSpawnStats>(catalogEntity);
            if (catalog.Length == 0)
                return;

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // One queue per side (TrainQueueTag is the singleton-per-side that holds TrainOrders).
            foreach (var (queueTag, queueEntity) in
                     SystemAPI.Query<RefRO<TrainQueueTag>>().WithEntityAccess())
            {
                int team = queueTag.ValueRO.Team;
                DynamicBuffer<TrainOrder> orders = SystemAPI.GetBuffer<TrainOrder>(queueEntity);
                if (orders.Length == 0)
                    continue;

                // FIFO: only the head order is in production at any time.
                TrainOrder head = orders[0];

                // Guard against an out-of-range UnitIndex (bad order) — drop it rather than spawn
                // garbage. Only the 4 authorized catalog entries are valid (Hard-Rule 2).
                if (head.UnitIndex < 0 || head.UnitIndex >= catalog.Length)
                {
                    orders.RemoveAt(0);
                    continue;
                }

                UnitSpawnStats stats = catalog[head.UnitIndex];

                // ---- Step 1: payment gate (LOCAL battle Gold, §13 P1.1) ----
                if (head.Paid == 0)
                {
                    if (!TryDeductGold(team, stats.GoldCost))
                    {
                        // Insufficient Gold: stall the head, leave Remaining untouched. Retried
                        // next tick once mining (§13 P1.1) tops up the GoldStore.
                        continue;
                    }
                    head.Paid = 1;
                    // trainSeconds takes effect only after payment (queue then deploy, §13 P1.2).
                    if (head.Remaining <= 0f)
                        head.Remaining = stats.TrainSeconds;
                }

                // ---- Step 2: train countdown ----
                head.Remaining -= dt;

                if (head.Remaining > 0f)
                {
                    // Still training: persist the mutated head and move on.
                    orders[0] = head;
                    continue;
                }

                // ---- Step 3: deploy (spawn) + pop the head ----
                int row = NextRow(ref ecb, queueEntity);
                SpawnUnit(ref ecb, in stats, team, row, ResolveSpawnPos(team));
                orders.RemoveAt(0);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        /// <summary>
        /// Deducts <paramref name="cost"/> from the matching side's GoldStore if affordable.
        /// Returns false (no mutation) when the side cannot pay or has no GoldStore yet.
        /// Uses SystemAPI exclusively — no SystemState needed (review: dropped unused param).
        /// </summary>
        private bool TryDeductGold(int team, int cost)
        {
            if (cost <= 0)
                return true; // free unit (e.g. starting Miner if goldCost=0) — nothing to deduct.

            foreach (var store in SystemAPI.Query<RefRW<GoldStore>>())
            {
                if (store.ValueRO.Team != team)
                    continue;
                if (store.ValueRO.Amount < cost)
                    return false; // insufficient — wait.
                store.ValueRW.Amount -= cost;
                return true;
            }
            return false; // no GoldStore for this side yet → treat as unable to pay.
        }

        /// <summary>
        /// Round-robin row 0..2 for this side, advancing the per-queue RowCursor (§13 P1.4).
        /// BattleBootstrap bakes only TrainQueueTag + TrainOrder, so the cursor is LAZILY ensured
        /// here: when absent, we deploy into row 0 and ADD a RowCursor (via the ECB) seeded to 1,
        /// so every subsequent spawn reads a real component and advances 1→2→0→1… across rows.
        /// </summary>
        private int NextRow(ref EntityCommandBuffer ecb, Entity queueEntity)
        {
            if (SystemAPI.HasComponent<RowCursor>(queueEntity))
            {
                var cursor = SystemAPI.GetComponentRW<RowCursor>(queueEntity);
                int row = ((cursor.ValueRO.Next % 3) + 3) % 3; // safe modulo into 0..2
                cursor.ValueRW.Next = row + 1;
                return row;
            }

            // First spawn for this (baked) queue: use row 0 and create the cursor for next time.
            ecb.AddComponent(queueEntity, new RowCursor { Next = 1 });
            return 0;
        }

        /// <summary>Looks up this side's TeamSpawnPoint; falls back to origin if none baked.</summary>
        private float2 ResolveSpawnPos(int team)
        {
            foreach (var sp in SystemAPI.Query<RefRO<TeamSpawnPoint>>())
            {
                if (sp.ValueRO.Team == team)
                    return sp.ValueRO.Pos;
            }
            return float2.zero;
        }

        /// <summary>
        /// Builds one combat unit from data. All numeric stats come from <paramref name="stats"/>
        /// (Hard-Rule 4); the only literals are NEUTRAL factor slots (1.0), Cooldown=0, Level=0,
        /// PerLevel=0 (no progression at P1), and the §13 P1.4 stickiness bias 1.2.
        /// The catalog's sim-side mirror enums (DamageTypeId/ArmorClassId/RoleId) share identical
        /// int values with the Bulwark.Data enums, so the (int) bridge cast is exact.
        /// </summary>
        private void SpawnUnit(ref EntityCommandBuffer ecb, in UnitSpawnStats stats,
                               int team, int row, float2 pos)
        {
            Entity e = ecb.CreateEntity();

            ecb.AddComponent<UnitTag>(e);
            ecb.AddComponent(e, new Team { Id = team });
            ecb.AddComponent(e, new Position { Value = pos });
            ecb.AddComponent(e, new Row { Index = row });
            ecb.AddComponent(e, new Health { Current = stats.MaxHealth, Max = stats.MaxHealth });
            ecb.AddComponent(e, new Movement { Speed = stats.MoveSpeed });

            // AttackState: Range/Damage(base)/Interval/Cooldown ONLY. Target is left at its
            // default (Entity.Null) — TargetingSystem owns acquisition via Targeting.Current
            // (shared contract: do NOT use AttackState.Target in Phase 1).
            ecb.AddComponent(e, new AttackState
            {
                Range = stats.AttackRange,
                Damage = stats.AttackDamage,
                Interval = stats.AttackInterval,
                Cooldown = 0f,
            });

            // §13 P1.4 modifier-chain identity. Level/PerLevel = 0 (progression is Phase 3).
            // Bridge the sim-side mirror enums to the Bulwark.Data enums via exact int cast.
            ecb.AddComponent(e, new CombatProfile
            {
                DamageType = (Bulwark.Data.DamageType)(int)stats.DamageType,
                ArmorClass = (Bulwark.Data.ArmorClass)(int)stats.ArmorClass,
                Level = 0,
                PerLevel = 0f,
            });

            // Neutral factor slots (1.0). Positional/terrain geometry = Phase 2.1, forbidden now.
            ecb.AddComponent(e, new Positional { Multiplier = 1f });
            ecb.AddComponent(e, new TerrainFactor { Multiplier = 1f });

            // Targeting seed: no current target; stickiness bias 1.2 (§13 P1.4).
            ecb.AddComponent(e, new Targeting
            {
                Current = Entity.Null,
                ReevalCooldown = 0f,
                Stickiness = 1.2f,
            });

            // Role-specific: Miners also mine Gold (§11, §13 P1.1). Only the Miner role gets the
            // mining components — the 3 combat roles do not. Gated on the catalog's RoleId enum.
            if (stats.Role == RoleId.Miner)
            {
                ecb.AddComponent<MinerTag>(e);
                ecb.AddComponent(e, new MiningState
                {
                    Mine = Entity.Null,
                    Accum = 0f,
                    RatePerSec = stats.MiningRatePerSec,
                });
            }
        }
    }

    // ---------------- Enqueue helper (Control §13 P1.3 + BasicAI §13 P1.5) ----------------
    /// <summary>
    /// Append-only train helpers. The MonoBehaviour Control shell and the BasicAI commander call
    /// these to QUEUE a train order (writing data only — §12 boundary). Spawning/payment is the
    /// TrainingSystem's job. UnitIndex must reference the baked UnitCatalogTag buffer order.
    /// </summary>
    public static class Training
    {
        /// <summary>
        /// Enqueue a train order onto a side's queue buffer directly (caller already has the
        /// DynamicBuffer&lt;TrainOrder&gt;). Remaining=0/Paid=0: the system fills train time on
        /// payment so cost/duration always come from current data (Hard-Rule 4).
        /// </summary>
        public static void EnqueueTrain(DynamicBuffer<TrainOrder> queue, int unitIndex)
        {
            queue.Add(new TrainOrder { UnitIndex = unitIndex, Remaining = 0f, Paid = 0 });
        }

        /// <summary>
        /// Enqueue a train order for <paramref name="team"/> by locating that side's
        /// TrainQueueTag singleton-per-side via the EntityManager. Convenience for callers that
        /// only know the team id. Returns false if no queue exists for the side.
        /// </summary>
        public static bool EnqueueTrain(EntityManager em, int team, int unitIndex)
        {
            var query = em.CreateEntityQuery(
                ComponentType.ReadOnly<TrainQueueTag>(),
                ComponentType.ReadWrite<TrainOrder>());
            using var entities = query.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                if (em.GetComponentData<TrainQueueTag>(entities[i]).Team != team)
                    continue;
                DynamicBuffer<TrainOrder> buffer = em.GetBuffer<TrainOrder>(entities[i]);
                EnqueueTrain(buffer, unitIndex);
                return true;
            }
            return false;
        }
    }
}

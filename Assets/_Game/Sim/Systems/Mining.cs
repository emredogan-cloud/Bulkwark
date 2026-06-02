// BULWARK — Phase 1.1 ECONOMY: MiningSystem (battle-sim hot path).
// Roadmap trace:
//  • §13 P1.1 "Economy + objective": mines (capped), miner mining, Gold — the
//    "mine→Gold→statue" loop, the FIRST half of the Phase-1 core loop.
//  • §11 "Gold mines": fixed nodes with a miner cap; contestable position
//    (OwnerTeam = -1 → any side may use spare capacity) → economy becomes a light
//    spatial contest, not a static tap.
//  • §3 "Performance-first systems": O(1)/unit. The assignment is CACHED on the
//    MiningState; an assigned miner is NEVER rescanned. Only an UNASSIGNED miner
//    does a one-shot nearest-node scan, and there are few miners relative to units.
//  • §12 ECS boundary: pure IComponentData sim, no UI/meta. GoldStore is LOCAL
//    battle economy ONLY — no meta/currency persistence (§J, §12 "NEVER log save
//    state"; server-authoritative wallet is Phase 3.1, forbidden now).
//  • §15.6 no invented balance: yield/rate/capacity all come from DATA
//    (UnitDef.miningRatePerSec → MiningState.RatePerSec; MineNode.YieldPerSec /
//    .Capacity baked from world data). This system contains NO numeric stats.
//
// MODULE-LOCAL CONVENTIONS (declared here, stable names — per brief "define them in
// your module, keep names stable"; NOT in the shared contract, NOT invented gameplay):
//  • MiningOccupancy : ICleanupComponentData {Entity Mine}
//        Death-reconciliation plumbing only. Mirrors MiningState.Mine while a miner is
//        assigned; survives the miner entity's destruction so the next tick's
//        authoritative recount no longer counts the dead miner (its slot is freed) and
//        we can strip the stale cleanup record. Sibling modules MUST NOT invent a
//        different occupancy-reclaim component — reuse this one.
//
// OCCUPANCY CORRECTNESS (fix for last-write-wins §3/§12):
//  MineNode.Occupants is NOT mutated incrementally through the ECB inside per-entity
//  loops (that collapses concurrent same-tick +1/-1 to last-write-wins, leaking or
//  over-committing capacity). Instead each tick we RECOUNT every node's Occupants
//  AUTHORITATIVELY by counting the live miners assigned to it (option (c)), then write
//  each node's Occupants exactly once. The node set is small and fixed (§11), so this
//  stays well within the §3 perf budget and is NOT an O(N^2) all-pairs unit scan.
//
// SCAFFOLD STATUS: authored, NOT compiled — no Unity 6 / Entities toolchain in this
// environment (ADR-0-001/-002). Runtime behavior is DEFERRED to a Unity build.

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Bulwark.Sim
{
    /// <summary>
    /// Cleanup record of a miner's mine-node occupancy. ICleanupComponentData survives
    /// the miner entity's destruction (e.g. killed in combat), letting MiningSystem
    /// reclaim the node's Occupants slot deterministically on the next tick rather than
    /// leaking capacity. Module-local convention (stable name). Mirrors MiningState.Mine
    /// while assigned; its sole purpose is post-death occupancy reconciliation.
    /// </summary>
    public struct MiningOccupancy : ICleanupComponentData { public Entity Mine; }

    /// <summary>
    /// Phase-1.1 economy core: assigns idle miners to the nearest free, side-legal mine
    /// node (capped, contestable), accrues each miner's data-driven rate against the
    /// node's shared yield ceiling, and banks whole Gold into the team's GoldStore.
    ///
    /// Occupancy is reconciled by an AUTHORITATIVE per-tick recount (count live assigned
    /// miners per node, write Occupants once), so concurrent same-tick claims/releases
    /// can never collapse under last-write-wins — capacity neither leaks nor overcommits.
    /// Surplus occupants on an over-capacity node are released so they re-scan (2a).
    /// </summary>
    [BurstCompile]
    public partial struct MiningSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;

            // Structural changes (add/remove MiningOccupancy cleanup component) are
            // deferred to one ECB playback at the end of the tick.
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // AUTHORITATIVE per-node occupancy tally for THIS tick. Built by counting the
            // miners we actually admit (capped at each node's Capacity), then written back
            // to MineNode.Occupants exactly ONCE — never round-tripped through the ECB in a
            // per-entity loop, so there is no last-write-wins collapse. Keyed by node entity.
            var occCount = new NativeHashMap<Entity, int>(16, Allocator.Temp);

            // ---------------------------------------------------------------
            // 1) RECLAIM dead miners: a miner entity was destroyed (combat death) but its
            //    MiningOccupancy cleanup component persisted (it has NO MiningState now).
            //    We do NOT decrement Occupants here — the authoritative recount below
            //    simply will not count this dead miner, so its slot is reclaimed for free
            //    (no decrement = no last-write-wins leak). We only strip the stale record.
            // ---------------------------------------------------------------
            foreach (var (occ, e) in
                     SystemAPI.Query<RefRO<MiningOccupancy>>()
                              .WithNone<MiningState>()
                              .WithEntityAccess())
            {
                ecb.RemoveComponent<MiningOccupancy>(e);
            }

            // ---------------------------------------------------------------
            // 2) MINERS: validate / cap-enforce / assign-if-idle (cached), then accrue.
            //    A single ordered pass maintains the running occCount tally so that:
            //      • admitted occupants never exceed a node's Capacity,
            //      • surplus occupants on an over-capacity node are released (2a),
            //      • two idle miners cannot both claim the last slot of one node this tick.
            // ---------------------------------------------------------------
            foreach (var (mining, pos, team, e) in
                     SystemAPI.Query<RefRW<MiningState>, RefRO<Position>, RefRO<Team>>()
                              .WithAll<MinerTag>()
                              .WithEntityAccess())
            {
                int myTeam = team.ValueRO.Id;
                Entity assigned = mining.ValueRO.Mine;

                // 2a) Validate an existing assignment; release a stale or OVER-CAPACITY
                //     one so we re-scan. Release cases:
                //       (i)  the node entity no longer exists (destroyed), or
                //       (ii) admitting this miner would push the node past Capacity — i.e.
                //            it filled (occCount already == Capacity) and we are a surplus
                //            occupant, so we are no longer a legitimate slot-holder.
                //     Admitted miners increment the running tally; released miners do not.
                if (assigned != Entity.Null)
                {
                    if (!SystemAPI.HasComponent<MineNode>(assigned))
                    {
                        // (i) Node gone — drop assignment + its cleanup record.
                        mining.ValueRW.Mine = Entity.Null;
                        if (SystemAPI.HasComponent<MiningOccupancy>(e))
                            ecb.RemoveComponent<MiningOccupancy>(e);
                        assigned = Entity.Null;
                    }
                    else
                    {
                        var nd = SystemAPI.GetComponent<MineNode>(assigned);
                        int admitted = occCount.TryGetValue(assigned, out int c) ? c : 0;
                        if (admitted < nd.Capacity)
                        {
                            // Legitimate occupant — admit and count the slot.
                            occCount[assigned] = admitted + 1;
                        }
                        else
                        {
                            // (ii) Over capacity — surplus occupant. Release so we re-scan;
                            //      do NOT count the slot (recount stays at/under Capacity).
                            mining.ValueRW.Mine = Entity.Null;
                            if (SystemAPI.HasComponent<MiningOccupancy>(e))
                                ecb.RemoveComponent<MiningOccupancy>(e);
                            assigned = Entity.Null;
                        }
                    }
                }

                // 2b) Assign an idle miner to the nearest FREE, side-legal node.
                //     One-shot scan only while unassigned (O(1)/unit steady state, §3/§12).
                //     Freeness is measured against the LIVE tally (occCount), so a node
                //     already filled to Capacity earlier this tick is correctly skipped —
                //     two idle miners cannot both grab the last slot (no overcommit).
                if (assigned == Entity.Null)
                {
                    Entity node = FindNearestFreeNode(ref state, ref occCount, pos.ValueRO.Value, myTeam);
                    if (node != Entity.Null)
                    {
                        // Claim a slot in the running tally (authoritative, not via ECB).
                        int admitted = occCount.TryGetValue(node, out int c) ? c : 0;
                        occCount[node] = admitted + 1;

                        mining.ValueRW.Mine = node;
                        mining.ValueRW.Accum = 0f;
                        // Stamp the cleanup record so a later death frees the slot via recount.
                        ecb.AddComponent(e, new MiningOccupancy { Mine = node });
                        assigned = node;
                    }
                    else
                    {
                        // No free node this tick; remain idle (no Gold accrues).
                        continue;
                    }
                }

                // 2c) ACCRUE: bounded by the lesser of the miner's data rate and the
                //     node's FAIR SHARE of its yield ceiling (occupants split it). This
                //     is the "capped node" rule (§11) — more miners on one node do not
                //     mint more than its YieldPerSec total. The divisor is the AUTHORITATIVE
                //     occupant count for this tick (occCount), not the stale MineNode field,
                //     so the yield ceiling is correct even under concurrent assignment.
                var occNode = SystemAPI.GetComponent<MineNode>(assigned);
                int occupants = occCount.TryGetValue(assigned, out int cc) ? cc : 1;
                occupants = math.max(1, occupants);
                float fairShare = occNode.YieldPerSec / occupants;
                float perSec = math.min(mining.ValueRO.RatePerSec, fairShare);

                float accum = mining.ValueRO.Accum + perSec * dt;
                int banked = (int)math.floor(accum);     // transfer whole units only
                accum -= banked;
                mining.ValueRW.Accum = accum;

                // 2d) BANK whole Gold into this miner's team GoldStore (local battle econ).
                if (banked > 0)
                    AddGold(ref state, myTeam, banked);
            }

            // ---------------------------------------------------------------
            // 3) WRITE BACK authoritative Occupants exactly ONCE per node. Nodes absent
            //    from the tally have zero admitted occupants this tick. This is the single
            //    point of truth for Occupants — no incremental ECB round-trips, so capacity
            //    can neither leak (under-count) nor overcommit (over-count).
            // ---------------------------------------------------------------
            foreach (var (node, e) in
                     SystemAPI.Query<RefRW<MineNode>>().WithEntityAccess())
            {
                int counted = occCount.TryGetValue(e, out int c) ? c : 0;
                // Defensive clamp: never exceed Capacity (admission above already enforces it).
                node.ValueRW.Occupants = math.min(counted, node.ValueRO.Capacity);
            }

            occCount.Dispose();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        /// <summary>
        /// Nearest mine node with a free capacity slot (per the LIVE per-tick tally) that
        /// this team may use: contestable (OwnerTeam == -1) OR owned by this team. Linear
        /// over NODES only (a small, fixed world set — §11 "fixed nodes"), and run only for
        /// idle miners, so this stays ~O(1) per unit per §3/§12 (NOT an all-pairs unit scan).
        /// Freeness is judged against occCount (this tick's admitted occupants), so a node
        /// filled earlier this same tick is not double-booked.
        /// </summary>
        private Entity FindNearestFreeNode(
            ref SystemState state, ref NativeHashMap<Entity, int> occCount, float2 from, int myTeam)
        {
            Entity best = Entity.Null;
            float bestSq = float.MaxValue;

            foreach (var (node, npos, e) in
                     SystemAPI.Query<RefRO<MineNode>, RefRO<Position>>()
                              .WithEntityAccess())
            {
                var nd = node.ValueRO;
                if (nd.OwnerTeam != -1 && nd.OwnerTeam != myTeam) continue; // not side-legal

                int admitted = occCount.TryGetValue(e, out int c) ? c : 0;
                if (admitted >= nd.Capacity) continue;                      // full this tick

                float d = math.distancesq(from, npos.ValueRO.Value);
                if (d < bestSq) { bestSq = d; best = e; }
            }
            return best;
        }

        /// <summary>Add banked Gold to the GoldStore that owns the given team.</summary>
        private void AddGold(ref SystemState state, int team, int amount)
        {
            foreach (var store in SystemAPI.Query<RefRW<GoldStore>>())
            {
                if (store.ValueRO.Team == team)
                {
                    store.ValueRW.Amount += amount;
                    return;
                }
            }
            // No GoldStore for this team → Gold is dropped (bootstrap responsibility to
            // create one GoldStore per side). DEFERRED validation in a Unity build.
        }
    }
}

// BULWARK — ECS battle-sim systems (Phase 0.2 scaffold): Move + Attack.
// Roadmap §13 P0.2: "one unit moves and attacks another; ≥200 units at a stable
// frame on a mid-range phone." This is the minimal spike to prove that loop; the
// 200-unit perf GATE cannot be measured in this environment (no Unity, no device
// — see docs/adr/ADR-0-001), so GATE 0.2 is reported BLOCKED, not PASS.
// Roadmap §12 perf rule: targeting/AI must stay ~O(1) per unit. The O(N^2) nearest
// search below is a P0 spike ONLY and is flagged for replacement by the influence-
// map/budgeted-scheduler approach (roadmap §4, §12) before any scale claim.
// SCAFFOLD STATUS: authored, NOT compiled. Non-deterministic by design at MVP
// (decision-log §3: determinism/replays deferred to Phase 7).

using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Bulwark.Sim
{
    /// <summary>Advances each unit toward its current target along the front.</summary>
    [BurstCompile]
    public partial struct MoveSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;

            foreach (var (pos, atk, move) in
                     SystemAPI.Query<RefRW<Position>, RefRO<AttackState>, RefRO<Movement>>()
                              .WithAll<UnitTag>())
            {
                Entity target = atk.ValueRO.Target;
                if (target == Entity.Null || !SystemAPI.HasComponent<Position>(target))
                    continue;

                float2 targetPos = SystemAPI.GetComponent<Position>(target).Value;
                float2 delta = targetPos - pos.ValueRO.Value;
                float dist = math.length(delta);

                // Stop at engagement range; otherwise close in.
                if (dist > atk.ValueRO.Range && dist > 1e-4f)
                    pos.ValueRW.Value += (delta / dist) * move.ValueRO.Speed * dt;
            }
        }
    }

    /// <summary>
    /// Acquires a target (nearest enemy) and applies damage on cooldown.
    /// NOTE: nearest-enemy scan here is O(N^2) — a P0 correctness spike, NOT the
    /// shippable targeting. Replace with influence-map + budgeted scheduler (§12)
    /// before asserting the 200-unit perf gate.
    /// </summary>
    public partial struct AttackSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach (var (atk, pos, team, self) in
                     SystemAPI.Query<RefRW<AttackState>, RefRO<Position>, RefRO<Team>>()
                              .WithAll<UnitTag>()
                              .WithEntityAccess())
            {
                // (Re)acquire target if missing or dead.
                if (atk.ValueRO.Target == Entity.Null ||
                    !SystemAPI.HasComponent<Health>(atk.ValueRO.Target))
                {
                    atk.ValueRW.Target = FindNearestEnemy(ref state, pos.ValueRO.Value, team.ValueRO.Id);
                }

                atk.ValueRW.Cooldown = math.max(0f, atk.ValueRO.Cooldown - dt);

                Entity target = atk.ValueRO.Target;
                if (target == Entity.Null || !SystemAPI.HasComponent<Health>(target))
                    continue;

                float2 targetPos = SystemAPI.GetComponent<Position>(target).Value;
                if (math.distance(pos.ValueRO.Value, targetPos) <= atk.ValueRO.Range &&
                    atk.ValueRO.Cooldown <= 0f)
                {
                    var hp = SystemAPI.GetComponent<Health>(target);
                    hp.Current -= atk.ValueRO.Damage;
                    ecb.SetComponent(target, hp);
                    atk.ValueRW.Cooldown = atk.ValueRO.Interval;

                    if (hp.Current <= 0f)
                    {
                        ecb.DestroyEntity(target);
                        atk.ValueRW.Target = Entity.Null;
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private Entity FindNearestEnemy(ref SystemState state, float2 from, int myTeam)
        {
            Entity best = Entity.Null;
            float bestSq = float.MaxValue;

            foreach (var (pos, team, e) in
                     SystemAPI.Query<RefRO<Position>, RefRO<Team>>()
                              .WithAll<UnitTag, Health>()
                              .WithEntityAccess())
            {
                if (team.ValueRO.Id == myTeam) continue;
                float d = math.distancesq(from, pos.ValueRO.Value);
                if (d < bestSq) { bestSq = d; best = e; }
            }
            return best;
        }
    }
}

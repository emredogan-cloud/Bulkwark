// BULWARK — Phase 1 TARGETING (roadmap §13 P1.4 "influence-map targeting"; §4 layered AI
// "influence-map units"; §11 single front + Statue objective; §12 perf rule: ~O(1) per unit
// — NO O(N^2) all-pairs scans).
//
// Per unit, throttled by Targeting.ReevalCooldown, picks the best enemy with a SAME-ROW
// preference (§11 "3 rows", §13 P1.4 front-line readability). To stay bounded we bucket
// enemies ONCE per re-eval frame into a (team,row,xbin) hash map (built from the same
// data the InfluenceMapSystem uses), then each re-evaluating unit only scans candidates in
// its target row's nearby X-bins — a small bounded neighbourhood, not all enemies. Stickiness
// (×Targeting.Stickiness, e.g. 1.2) biases the CURRENT target's score to prevent thrash/jitter
// (P1.4 risk: "targeting jitter").
//
// STATUE OBJECTIVE (§11): the enemy Statue is the win condition, so it MUST be acquirable as a
// target — otherwise the targeting->combat->StatueDamageInbox->StatueDamageSystem->MatchState
// chain is broken (units would only ever fight each other). Statues carry StatueTag{int Team}
// (NOT UnitTag — we do NOT change their archetype/contract), so a SECOND, bounded pass queries
// StatueTag explicitly and registers each statue (there are only 2 => O(1)) into the buckets as
// a LOW-PRIORITY candidate spanning ALL rows at its X-bin. Effect: a unit prefers a nearby enemy
// UNIT, but falls back to the enemy Statue when no enemy unit is in its neighbourhood — so units
// that have broken through close on and damage the enemy base, making the §11 objective reachable.
//
// CONTROL boundary (§12, §13 P1.3): units that are Possessed, or carry a manual AttackTarget
// (ManualOrder.HasAttack==1), are SKIPPED here — PossessControl owns their target. This system
// only writes Targeting.Current; MovementSystem/CombatSystem consume it.
// §15.6 no invented balance: NO numeric unit/combat stats here — only spatial scoring of
// existing Position/Team/Health data + the data-owned Stickiness slot.
// SCAFFOLD STATUS: authored, NOT compiled (no Unity — ADR-0-001/-002).

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Bulwark.Sim
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SimPhaseBegin))]
    [UpdateBefore(typeof(SimPhaseEnd))]
    [UpdateAfter(typeof(InfluenceMapSystem))]   // reads this tick's rebuilt buckets.
    [UpdateAfter(typeof(PossessControlSystem))] // honor manual target overrides before auto-acquire.
    [UpdateBefore(typeof(BasicAISystem))]       // §13 1.5 commander builds on the resolved targeting.
    public partial struct TargetingSystem : ISystem
    {
        // Re-eval cadence + scoring knobs. These are TUNING SLOTS for targeting behaviour
        // (engine-side), NOT unit/combat balance values — nothing in the damage chain reads them.
        private const float ReevalPeriod = 0.20f; // refresh ReevalCooldown when it hits 0
        private const int SearchBinRadius = 4;     // X-bins scanned each side of the unit's bin
        private const float SameRowBonus = 2.0f;   // multiplicative preference for SAME row (§13 P1.4)
        private const float AdjacentRowBonus = 1.0f; // neighbouring rows: no bonus (still eligible)
        // The enemy Statue is a FALLBACK objective: weighted well below any live enemy unit so units
        // engage units first and only converge on the base when no unit is in their neighbourhood
        // (§11 objective). It is a positional weight, not a combat/balance number (§15.6).
        private const float StatueBonus = 0.01f;

        // Bucket key: enemyTeam, row, xbin packed into one int for the hash map.
        private static int Key(int team, int row, int xbin, int xbins, int rows)
            => (team * rows + row) * xbins + xbin;

        private struct EnemyEntry
        {
            public Entity Entity;
            public float2 Pos;
            public int Team;
            public int Row;
            public byte IsStatue; // 1 => low-priority objective candidate (StatueTag), not a unit
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<InfluenceMapSingleton>(out var im) || im.Built == 0)
                return; // wait for the first influence-map rebuild

            float dt = SystemAPI.Time.DeltaTime;
            int xbins = im.XBins;
            int rows = im.Rows;

            // 1) Bucket all live units by (team,row,xbin) — single O(N) pass, no all-pairs.
            int unitCount = 0;
            foreach (var _ in SystemAPI.Query<RefRO<UnitTag>>())
                unitCount++;

            // Count statues too (there are only 2 — §11 "one Statue per side"); reserve
            // capacity for their all-row registration so the hash map never reallocates.
            int statueCount = 0;
            foreach (var _ in SystemAPI.Query<RefRO<StatueTag>>())
                statueCount++;

            // Nothing to fight and no objective to acquire => bail.
            if (unitCount == 0 && statueCount == 0)
                return;

            int capacity = unitCount + statueCount * rows; // each statue is registered in every row
            var buckets = new NativeParallelMultiHashMap<int, EnemyEntry>(
                math.max(capacity, 1), Allocator.Temp);

            // 1a) Units. RC-2: exclude MinerTag — miners are economy units, never combat TARGETS.
            foreach (var (pos, team, row, e) in
                     SystemAPI.Query<RefRO<Position>, RefRO<Team>, RefRO<Row>>()
                              .WithAll<UnitTag, Health>()
                              .WithNone<MinerTag>()
                              .WithEntityAccess())
            {
                int t = team.ValueRO.Id;
                int r = im.RowClamp(row.ValueRO.Index);
                int xb = im.XBinOf(pos.ValueRO.Value.x);
                buckets.Add(Key(t, r, xb, xbins, rows), new EnemyEntry
                {
                    Entity = e,
                    Pos = pos.ValueRO.Value,
                    Team = t,
                    Row = r,
                    IsStatue = 0
                });
            }

            // 1b) Statues (§11 win-condition objective). Bounded extra pass: only StatueTag
            //     entities (2 of them), explicitly queried so they need NO UnitTag. A statue
            //     spans the whole front, so register it in EVERY row at its X-bin — this makes it
            //     reachable as a fallback from any attacker row without forcing the statue to
            //     carry a Row component. Statues without a Position are skipped (can't be located).
            foreach (var (statueTag, pos, e) in
                     SystemAPI.Query<RefRO<StatueTag>, RefRO<Position>>()
                              .WithEntityAccess())
            {
                int t = statueTag.ValueRO.Team;
                int xb = im.XBinOf(pos.ValueRO.Value.x);
                var entry = new EnemyEntry
                {
                    Entity = e,
                    Pos = pos.ValueRO.Value,
                    Team = t,
                    Row = -1,        // spans all rows
                    IsStatue = 1
                };
                for (int r = 0; r < rows; r++)
                    buckets.Add(Key(t, r, xb, xbins, rows), entry);
            }

            // 2) For each unit due for re-eval, scan only its target-row neighbourhood.
            //    RC-2: exclude MinerTag — miners never ACQUIRE a combat target (stay on the mines, off the line).
            foreach (var (tgt, pos, team, row, e) in
                     SystemAPI.Query<RefRW<Targeting>, RefRO<Position>, RefRO<Team>, RefRO<Row>>()
                              .WithAll<UnitTag, Health>()
                              .WithNone<MinerTag>()
                              .WithEntityAccess())
            {
                // CONTROL boundary: PossessControl owns possessed / manually-ordered targets.
                if (SystemAPI.HasComponent<Possessed>(e))
                    continue;
                if (SystemAPI.HasComponent<ManualOrder>(e) &&
                    SystemAPI.GetComponent<ManualOrder>(e).HasAttack != 0)
                    continue;

                // STUN GATE (Spell.cs EDIT 2): a Stunned unit CANNOT ACT — it holds its current
                // target but does not spend cost re-acquiring (CombatSystem also skips its swing,
                // EDIT 3, so the stun fully gates the unit). Burst-safe: StatusQuery is
                // [BurstCompile]+pure; fetch the buffer by entity.
                if (SystemAPI.HasBuffer<StatusEffect>(e))
                {
                    var sbuf = SystemAPI.GetBuffer<StatusEffect>(e);
                    if (StatusQuery.IsStunned(in sbuf)) continue;
                }

                // Drop a dead/destroyed current target before throttling, so we re-acquire promptly.
                // A valid target is one we can still locate (Position) AND damage: either a live
                // unit (Health) or the enemy Statue (StatueTag, drained via StatueDamageInbox).
                Entity current = tgt.ValueRO.Current;
                bool currentValid = current != Entity.Null &&
                                    SystemAPI.HasComponent<Position>(current) &&
                                    (SystemAPI.HasComponent<Health>(current) ||
                                     SystemAPI.HasComponent<StatueTag>(current));
                if (!currentValid)
                    tgt.ValueRW.Current = Entity.Null;

                // Throttle re-eval (P1.4 anti-jitter). Tick the cooldown; only re-acquire at 0.
                float cd = tgt.ValueRO.ReevalCooldown - dt;
                if (cd > 0f && currentValid)
                {
                    tgt.ValueRW.ReevalCooldown = cd;
                    continue;
                }
                tgt.ValueRW.ReevalCooldown = ReevalPeriod;

                int myTeam = team.ValueRO.Id;
                int myRow = im.RowClamp(row.ValueRO.Index);
                float2 myPos = pos.ValueRO.Value;
                int myBin = im.XBinOf(myPos.x);
                float stickiness = math.max(tgt.ValueRO.Stickiness, 1f);

                Entity best = Entity.Null;
                float bestScore = float.MinValue;

                // Same-row first (highest preference), then adjacent rows — a small bounded set.
                for (int dr = -1; dr <= 1; dr++)
                {
                    int r = myRow + dr;
                    if (r < 0 || r >= rows) continue;
                    float rowBonus = (dr == 0) ? SameRowBonus : AdjacentRowBonus;

                    for (int db = -SearchBinRadius; db <= SearchBinRadius; db++)
                    {
                        int xb = myBin + db;
                        if (xb < 0 || xb >= xbins) continue;

                        // Enemies = every OTHER team's bucket in this cell (units AND the statue).
                        for (int et = 0; et < im.Teams; et++)
                        {
                            if (et == myTeam) continue;
                            int key = Key(et, r, xb, xbins, rows);
                            if (!buckets.TryGetFirstValue(key, out var cand, out var it))
                                continue;
                            do
                            {
                                float distSq = math.distancesq(myPos, cand.Pos);
                                // The Statue is a low-priority FALLBACK objective: weight it far
                                // below any live enemy unit (StatueBonus << row bonuses) so units
                                // engage units first and only pick the base when nothing else is
                                // near. Closer scores higher; 1/(1+d^2) keeps score bounded.
                                float weight = (cand.IsStatue != 0) ? StatueBonus : rowBonus;
                                float score = weight / (1f + distSq);
                                if (cand.Entity == current)
                                    score *= stickiness; // bias toward keeping the current target
                                if (score > bestScore)
                                {
                                    bestScore = score;
                                    best = cand.Entity;
                                }
                            } while (buckets.TryGetNextValue(out cand, ref it));
                        }
                    }
                }

                tgt.ValueRW.Current = best; // Entity.Null if no enemy unit and no enemy statue nearby
            }

            buckets.Dispose();
        }
    }
}

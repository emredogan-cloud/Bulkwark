// BULWARK — Phase 2.1 TERRAIN + POSITIONAL (roadmap §13 2.1; §4 "one combat core" — the SAME
// modifier chain Phase-1 CombatSystem implements; §11 "single front / terrain features /
// high ground / choke / cover / hazard"; §12 ECS boundary + perf: ~O(1) per unit).
//
// WHAT PHASE 2.1 DOES (and does NOT do)
// Phase 1 plumbed two NEUTRAL factor slots into the §4 chain and left them at 1.0:
//   dmg = round(base × (1 + Level×PerLevel)) × typeArmor × positional × terrain × difficulty
//                                                          ^^^^^^^^^^   ^^^^^^^
// This module POPULATES those two previously-neutral slots — it does NOT fork combat and does
// NOT change the formula in Combat.cs (Hard-Rule 3 / §4). It writes, per unit, each tick:
//   • TerrainFactor.Multiplier  ← the AttackMult of the terrain feature the unit OCCUPIES
//                                  (HighGround > 1; open ground = 1). TerrainSystem.
//   • Positional.Multiplier     ← the §4 flank/back geometry vs the unit's CURRENT target
//                                  (BACK → 2.0, FLANK → 1.5, FRONT → 1.0). PositionalSystem.
// Both multipliers are then consumed UNCHANGED by the existing CombatSystem (§4 chain).
//
// FacingSystem additionally maintains each unit's Facing.Dir (the geometry input both for
// PositionalSystem here and for FormationSystem/SquadAI later). Facing points toward the unit's
// current target, else its active move order, else the enemy end of the front.
//
// CANON DISCIPLINE
//   • §4 ONE combat core: terrain + positional feed the SAME chain; no fork, no formula change.
//   • §4 CANON CONSTANTS (cited): flank = 1.5, back = 2.0. The cone thresholds that decide which
//     band an angle falls in are PROVISIONAL (LSD-owned), flagged below, NOT asserted canon.
//   • §15.6 no invented balance: terrain MAGNITUDES (AttackMult/DefenseMult/MoveMult/HazardDps)
//     are DATA on the TerrainFeature component, set at map load from MapDef — never literals here.
//     The only literals in this file are the §4 canon flank/back constants and the geometric
//     cone thresholds (engine geometry, flagged provisional), plus the neutral 1.0 default.
//   • §12 ECS boundary: pure battle sim (ISystem/Burst). No UI/meta. ~O(1) per unit — features
//     are few and fixed, so iterating the small TerrainFeature set per unit is fine; there is NO
//     all-pairs unit scan (Hard-Rule 6 / §12 perf).
//   • Forbidden now (Phase 3+/7): no new mechanics/currencies/biomes; we only read the canon
//     TerrainKind set (HighGround/Choke/Cover/Hazard) and the canon §4 flank/back geometry.
//
// SCAFFOLD STATUS: authored, NOT compiled (no Unity 6 / Entities toolchain — ADR-0-001/-002/
// -1-001). Runtime behavior (exact cone feel, hazard tick feel) is DEFERRED to a Unity build.

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Bulwark.Sim
{
    // =====================================================================================
    // ENSURE-PASS — structural guarantee that Phase-2 combat-geometry slots exist on units.
    // -------------------------------------------------------------------------------------
    // Phase-1 Training.SpawnUnit adds Positional + TerrainFactor (the §4 slots) but NOT the
    // Phase-2 geometry components Facing / TerrainOccupancy (those are 2.1, introduced here).
    // Rather than edit the Phase-1 spawn path (Hard-Rule: describe edits, don't make them), we
    // mirror the EnsureMoveDestinationSystem pattern (PossessControl.cs): a non-Burst structural
    // pass ADDS the two components (neutral defaults) to any UnitTag entity lacking them, so the
    // Burst systems below provably find them. Runs FIRST in the 2.1 window.
    //
    // NOTE for the integration pass: the CLEANER home for this is Training.SpawnUnit adding
    // Facing{float2.zero} + TerrainOccupancy{Entity.Null} alongside the existing Positional/
    // TerrainFactor adds. That edit is described in integrationNotes; this ensure-pass is the
    // self-contained fallback so 2.1 cannot silently no-op on units spawned before it.
    // =====================================================================================

    /// <summary>
    /// Ensures every combat unit carries the Phase-2 steady-state components: the geometry
    /// components <see cref="Facing"/> and <see cref="TerrainOccupancy"/>, an empty
    /// <see cref="StatusEffect"/> buffer (Spell.cs EDIT 4 — so the status guards in Spell.cs /
    /// CommanderAbility.cs are steady-state no-ops), and a neutral per-unit
    /// <see cref="Difficulty"/> slot (A5 — so CombatSystem's per-unit RefRO&lt;Difficulty&gt;
    /// query is satisfiable and SquadAI's per-team Stats-axis write has a slot to scale). All are
    /// neutral defaults. Non-Burst: structural changes via ECB cannot run inside the Burst geometry
    /// systems. Runs before <see cref="FacingSystem"/>. (TrainingSystem.SpawnUnit /
    /// SpellSummon.SpawnFromCatalog also add StatusEffect + Difficulty at spawn for immediacy;
    /// this pass is the self-contained fallback so nothing silently no-ops.)
    /// </summary>
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(MovementSystem))]
    [UpdateBefore(typeof(FacingSystem))]
    public partial struct EnsurePhase2CombatComponentsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            // Only run while there is at least one unit missing one of the ensured components.
            var missingFacing = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<UnitTag>()
                .WithNone<Facing>()
                .Build(ref state);
            var missingOccupancy = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<UnitTag>()
                .WithNone<TerrainOccupancy>()
                .Build(ref state);
            var missingStatus = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<UnitTag>()
                .WithNone<StatusEffect>()
                .Build(ref state);
            var missingDifficulty = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<UnitTag>()
                .WithNone<Difficulty>()
                .Build(ref state);
            state.RequireAnyForUpdate(missingFacing, missingOccupancy, missingStatus, missingDifficulty);
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // Add Facing where absent (neutral zero direction; FacingSystem overwrites this tick).
            foreach (var (_, e) in
                     SystemAPI.Query<RefRO<UnitTag>>()
                              .WithNone<Facing>()
                              .WithEntityAccess())
            {
                ecb.AddComponent(e, new Facing { Dir = float2.zero });
            }

            // Add TerrainOccupancy where absent (Entity.Null = open ground; TerrainSystem sets it).
            foreach (var (_, e) in
                     SystemAPI.Query<RefRO<UnitTag>>()
                              .WithNone<TerrainOccupancy>()
                              .WithEntityAccess())
            {
                ecb.AddComponent(e, new TerrainOccupancy { Feature = Entity.Null });
            }

            // Spell.cs EDIT 4: add an empty StatusEffect buffer where absent, so the per-unit
            // status reads/writes in Spell.cs / CommanderAbility.cs / StatusQuery never need a
            // lazy structural add on the hot path (their "skip if no buffer" guards become
            // steady-state no-ops).
            foreach (var (_, e) in
                     SystemAPI.Query<RefRO<UnitTag>>()
                              .WithNone<StatusEffect>()
                              .WithEntityAccess())
            {
                ecb.AddBuffer<StatusEffect>(e);
            }

            // A5: add a neutral per-unit Difficulty slot where absent, so CombatSystem's per-unit
            // RefRO<Difficulty> query matches every unit (the chain's difficulty factor is live)
            // and SquadAI's per-team Stats-axis write has a slot to scale (fair §16, per-team only).
            foreach (var (_, e) in
                     SystemAPI.Query<RefRO<UnitTag>>()
                              .WithNone<Difficulty>()
                              .WithEntityAccess())
            {
                ecb.AddComponent(e, new Difficulty { Multiplier = 1f });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    // =====================================================================================
    // FacingSystem — maintain each unit's facing direction (geometry input for §4 positional).
    // =====================================================================================

    /// <summary>
    /// Sets each unit's <see cref="Facing"/>.Dir to the normalized direction it is "looking":
    ///   1. toward its current target (<see cref="Targeting"/>.Current) if it has one;
    ///   2. else toward its active move order (<see cref="MoveDestination"/>, when Active==1);
    ///   3. else toward the ENEMY END of the single front (derived from the authored front span
    ///      in <see cref="InfluenceMapSingleton"/> MinX/MaxX — Team 0 spawns left so it faces
    ///      +X/MaxX, Team 1 spawns right so it faces -X/MinX; §11 single-front geometry).
    /// Facing is used by <see cref="PositionalSystem"/> here (the TARGET's facing decides flank/
    /// back) and later by FormationSystem/SquadAI. ~O(1) per unit. No balance values (§15.6) — pure
    /// geometry over existing Position/Targeting/MoveDestination + the data-derived front span.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(EnsurePhase2CombatComponentsSystem))]
    [UpdateBefore(typeof(PositionalSystem))]
    public partial struct FacingSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Front span (for the enemy-end fallback). Absent/unbuilt → fall back to a neutral
            // +X span so Team 0 faces +X and Team 1 faces -X (matches team0StatueX < team1StatueX).
            bool hasIm = SystemAPI.TryGetSingleton<InfluenceMapSingleton>(out var im);
            float minX = hasIm ? im.MinX : 0f;
            float maxX = hasIm ? im.MaxX : 1f;

            foreach (var (facing, pos, team, tgt, e) in
                     SystemAPI.Query<RefRW<Facing>, RefRO<Position>, RefRO<Team>, RefRO<Targeting>>()
                              .WithAll<UnitTag>()
                              .WithEntityAccess())
            {
                float2 myPos = pos.ValueRO.Value;
                float2 look = float2.zero;
                bool haveLook = false;

                // 1) Toward current target.
                Entity target = tgt.ValueRO.Current;
                if (target != Entity.Null && SystemAPI.HasComponent<Position>(target))
                {
                    look = SystemAPI.GetComponent<Position>(target).Value - myPos;
                    haveLook = true;
                }

                // 2) Else toward an active manual move order.
                if (!haveLook && SystemAPI.HasComponent<MoveDestination>(e))
                {
                    var dest = SystemAPI.GetComponent<MoveDestination>(e);
                    if (dest.Active != 0)
                    {
                        look = dest.Value - myPos;
                        haveLook = true;
                    }
                }

                // 3) Else toward the enemy end of the front (Team 0 → +X end, Team 1 → -X end).
                if (!haveLook)
                {
                    float enemyEndX = (team.ValueRO.Id == 0) ? maxX : minX;
                    look = new float2(enemyEndX - myPos.x, 0f);
                    haveLook = true;
                }

                // Normalize; if degenerate (zero-length), KEEP the previous facing (do not zero it
                // out — a stale-but-valid heading is better geometry than (0,0) for the §4 cone).
                float len = math.length(look);
                if (len > 1e-5f)
                    facing.ValueRW.Dir = look / len;
                // else: leave facing.ValueRW.Dir unchanged.
            }
        }
    }

    // =====================================================================================
    // TerrainSystem — assign occupancy, populate the §4 TerrainFactor slot, apply Hazard ticks.
    // =====================================================================================

    /// <summary>
    /// For each unit:
    ///   • OCCUPANCY: find the <see cref="TerrainFeature"/> entity the unit currently stands in
    ///     (same Row, and |unit.x − feature.center.x| ≤ feature.Width/2). The feature's world
    ///     center is its <see cref="Position"/>.x; if a unit is in no feature it is on OPEN GROUND
    ///     (Feature = Entity.Null). Set <see cref="TerrainOccupancy"/>.Feature accordingly.
    ///   • §4 TERRAIN SLOT: write <see cref="TerrainFactor"/>.Multiplier from the occupied
    ///     feature's <see cref="TerrainFeature"/>.AttackMult (HighGround > 1; open ground = 1.0).
    ///     This is the OUTGOING-damage terrain factor the CombatSystem §4 chain already multiplies
    ///     by — we POPULATE the slot, we do NOT change the chain (Hard-Rule 3).
    ///   • HAZARD: if the occupied feature is a <see cref="Bulwark.Data.TerrainKind"/>.Hazard, apply
    ///     its HazardDps to the occupant's Health this tick (HazardDps × dt). Dead units are
    ///     destroyed via ECB (same convention as CombatSystem). See integrationNotes for the
    ///     alternative StatusEffect(Burning) routing.
    ///
    /// PERF (§12): the TerrainFeature set is small and FIXED per map (2–4 features, §11), so the
    /// per-unit inner loop over features is ~O(1). This is NOT an all-pairs unit scan (Hard-Rule 6).
    /// All terrain magnitudes are DATA on the feature components (set at map load), never literals.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(EnsurePhase2CombatComponentsSystem))]
    [UpdateBefore(typeof(CombatSystem))]
    public partial struct TerrainSystem : ISystem
    {
        // Snapshot of one terrain feature for the bounded per-unit lookup (built once per tick).
        private struct FeatureRec
        {
            public Entity Entity;
            public float CenterX;
            public float HalfWidth;
            public int Row;
            public float AttackMult;
            public Bulwark.Data.TerrainKind Kind;
            public float HazardDps;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;

            // 1) Snapshot the (few, fixed) terrain features once. A feature is located by its
            //    Position.x center; Width/Row come from the TerrainFeature data (map load).
            int featureCount = 0;
            foreach (var _ in SystemAPI.Query<RefRO<TerrainFeature>>())
                featureCount++;

            var features = new NativeList<FeatureRec>(math.max(featureCount, 1), Allocator.Temp);
            foreach (var (feat, pos, fe) in
                     SystemAPI.Query<RefRO<TerrainFeature>, RefRO<Position>>()
                              .WithEntityAccess())
            {
                features.Add(new FeatureRec
                {
                    Entity = fe,
                    CenterX = pos.ValueRO.Value.x,
                    HalfWidth = 0.5f * feat.ValueRO.Width,
                    Row = feat.ValueRO.Row,
                    AttackMult = feat.ValueRO.AttackMult,
                    Kind = feat.ValueRO.Kind,
                    HazardDps = feat.ValueRO.HazardDps,
                });
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // 2) Per unit: resolve occupancy, populate the §4 terrain slot, tick hazard.
            foreach (var (occ, terrain, pos, row, e) in
                     SystemAPI.Query<RefRW<TerrainOccupancy>, RefRW<TerrainFactor>,
                                     RefRO<Position>, RefRO<Row>>()
                              .WithAll<UnitTag>()
                              .WithEntityAccess())
            {
                float ux = pos.ValueRO.Value.x;
                int ur = row.ValueRO.Index;

                Entity occupied = Entity.Null;
                float attackMult = 1f; // OPEN GROUND default (neutral, §4 — not a balance literal).
                bool inHazard = false;
                float hazardDps = 0f;

                // Bounded scan over the small fixed feature set (~O(1), §12). First containing
                // feature in the unit's row wins (features in a row should not overlap by design;
                // if they do, authoring is at fault — flagged for the GATE-2 content pass).
                for (int i = 0; i < features.Length; i++)
                {
                    var f = features[i];
                    if (f.Row != ur) continue;
                    if (math.abs(ux - f.CenterX) > f.HalfWidth) continue;

                    occupied = f.Entity;
                    attackMult = f.AttackMult;
                    if (f.Kind == Bulwark.Data.TerrainKind.Hazard)
                    {
                        inHazard = true;
                        hazardDps = f.HazardDps;
                    }
                    break;
                }

                occ.ValueRW.Feature = occupied;

                // Populate the §4 OUTGOING-damage terrain slot (HighGround>1; open=1). The
                // CombatSystem chain multiplies by this UNCHANGED — we do not touch the formula.
                terrain.ValueRW.Multiplier = (attackMult > 0f) ? attackMult : 1f;

                // HAZARD tick (§4/§11): direct Health damage this tick. HazardDps is DATA on the
                // feature. Statues never carry a Row/UnitTag here, so only units are ticked.
                if (inHazard && hazardDps > 0f && SystemAPI.HasComponent<Health>(e))
                {
                    var hp = SystemAPI.GetComponent<Health>(e);
                    hp.Current -= hazardDps * dt;
                    ecb.SetComponent(e, hp);
                    if (hp.Current <= 0f)
                        ecb.DestroyEntity(e);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            features.Dispose();
        }
    }

    // =====================================================================================
    // PositionalSystem — populate the §4 positional slot from flank/back geometry vs the target.
    // =====================================================================================

    /// <summary>
    /// For each unit with a current target, computes the §4 positional multiplier from the angle
    /// between the attack direction (attacker → target) and the TARGET'S facing:
    ///   • target's BACK   → 2.0  (attacking from behind the target's heading)
    ///   • target's FLANK  → 1.5  (attacking from the side)
    ///   • target's FRONT  → 1.0  (attacking into the target's heading; no bonus)
    /// The 1.5 (flank) and 2.0 (back) values are §4 CANON CONSTANTS (cited). The cone thresholds
    /// that classify an angle into front/flank/back are PROVISIONAL engine geometry (LSD-owned,
    /// flagged — NOT asserted canon): FRONT = within ±60° of facing, BACK = within ±60° of the
    /// reverse, FLANK = the remaining side bands.
    ///
    /// The result is written to the ATTACKER'S <see cref="Positional"/>.Multiplier, which the
    /// CombatSystem §4 chain then multiplies by UNCHANGED (Hard-Rule 3 — populate the slot, do not
    /// fork combat). Units with no target reset to the neutral 1.0 so a stale bonus never persists.
    /// ~O(1) per unit (single target lookup; no all-pairs scan, §12).
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(FacingSystem))]
    [UpdateBefore(typeof(CombatSystem))]
    public partial struct PositionalSystem : ISystem
    {
        // §4 CANON CONSTANTS — the flank/back damage multipliers (cite §4). The only combat
        // literals permitted in this file (Hard-Rule 7 exception: canon-stated §4 multipliers).
        private const float k_FlankMult = 1.5f;
        private const float k_BackMult  = 2.0f;
        private const float k_FrontMult = 1.0f; // FRONT = neutral (no bonus); also the no-target default.

        // PROVISIONAL cone thresholds (engine geometry, LSD-owned — NOT canon; cone is "provisional"
        // per the module brief). Compared against the cosine of the angle between the target's
        // facing and the attacker→target direction. cos(60°) = 0.5.
        //   • dot >  +0.5  → attacker is roughly IN FRONT of the target's facing → FRONT.
        //   • dot <  -0.5  → attacker is roughly BEHIND the target's facing      → BACK.
        //   • otherwise (side bands)                                             → FLANK.
        private const float k_FrontConeCos = 0.5f;
        private const float k_BackConeCos  = -0.5f;

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (positional, pos, tgt, e) in
                     SystemAPI.Query<RefRW<Positional>, RefRO<Position>, RefRO<Targeting>>()
                              .WithAll<UnitTag>()
                              .WithEntityAccess())
            {
                Entity target = tgt.ValueRO.Current;

                // No target (or unlocatable) → neutral. Never leave a stale flank/back bonus.
                if (target == Entity.Null || !SystemAPI.HasComponent<Position>(target))
                {
                    positional.ValueRW.Multiplier = k_FrontMult;
                    continue;
                }

                float2 attackerPos = pos.ValueRO.Value;
                float2 targetPos = SystemAPI.GetComponent<Position>(target).Value;

                // Attack direction: attacker → target (the line the hit travels along).
                float2 toTarget = targetPos - attackerPos;
                float toLen = math.length(toTarget);

                // Coincident positions → no meaningful angle → FRONT/neutral.
                if (toLen <= 1e-5f)
                {
                    positional.ValueRW.Multiplier = k_FrontMult;
                    continue;
                }
                float2 attackDir = toTarget / toLen;

                // Target's facing (the heading the §4 flank/back is measured against). If the
                // target is not a Phase-2 unit (no Facing — e.g. a Statue objective), there is no
                // back/flank to exploit → FRONT/neutral. This keeps statue damage on the §4 chain
                // unchanged (positional = 1.0), matching Phase-1 behavior for objectives.
                if (!SystemAPI.HasComponent<Facing>(target))
                {
                    positional.ValueRW.Multiplier = k_FrontMult;
                    continue;
                }
                float2 targetFacing = SystemAPI.GetComponent<Facing>(target).Dir;
                if (math.lengthsq(targetFacing) <= 1e-10f)
                {
                    // Target has no resolved heading yet → cannot classify → neutral.
                    positional.ValueRW.Multiplier = k_FrontMult;
                    continue;
                }

                // Classify by the angle between the TARGET'S facing and the attack direction.
                // dot = cos(angle between target-facing and attacker→target).
                //   • Attacker in front of the target  ⇒ attackDir ≈ targetFacing  ⇒ dot ≈ +1.
                //   • Attacker behind the target        ⇒ attackDir ≈ -targetFacing ⇒ dot ≈ -1.
                float dot = math.dot(math.normalize(targetFacing), attackDir);

                float mult;
                if (dot > k_FrontConeCos)      mult = k_FrontMult; // FRONT (no bonus)
                else if (dot < k_BackConeCos)  mult = k_BackMult;  // BACK  (§4 canon 2.0)
                else                           mult = k_FlankMult; // FLANK (§4 canon 1.5)

                positional.ValueRW.Multiplier = mult;
            }
        }
    }
}

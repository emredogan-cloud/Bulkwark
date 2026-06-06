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
    [UpdateAfter(typeof(SimPhaseBegin))]
    [UpdateBefore(typeof(SimPhaseEnd))]
    [UpdateAfter(typeof(TargetingSystem))]
    public partial struct MovementSystem : ISystem
    {
        // Arrival radius for a manual MoveDestination order: the override is satisfied (and
        // cleared) once the unit is within this distance of the ordered point. NOT a combat/
        // balance value — it is the control-handoff arrival tolerance (§13 P1.3, DEFERRED feel).
        private const float k_ManualArriveRadius = 1e-3f;

        // §6 ADR-2-002 commander-buff clamp ceiling. StatusQuery clamps the COMBINED commander-
        // sourced buff fraction to this when reading move/damage multipliers. We use the canon §6
        // ceiling (0.15) — the SAME hard cap CommanderAbilitySystem.k_PowerBudgetCeiling clamps
        // PowerBudgetPct (and thus every commander magnitude) to. The per-unit CommanderRuntime is
        // a per-TEAM singleton not carried on the unit on the Burst hot path; clamping the combined
        // commander fraction to this ceiling guarantees the commander-attributable buff ≤ the §6
        // budget regardless. Spell buffs are read separately (uncapped §5.3 layer). This is the one
        // allowed literal (canon-stated §6 cap, cited) — NOT an invented balance value (§15.6).
        private const float k_CommanderBudgetCeiling = 0.15f; // §6 commander power cap (ADR-2-002).

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;

            // RC-5: enemy-front anchors (each side's statue position) for the organic no-target march-to-contact.
            float2 statuePos0 = default, statuePos1 = default; bool have0 = false, have1 = false;
            foreach (var (stag, spos) in SystemAPI.Query<RefRO<StatueTag>, RefRO<Position>>())
            {
                if (stag.ValueRO.Team == 0) { statuePos0 = spos.ValueRO.Value; have0 = true; }
                else if (stag.ValueRO.Team == 1) { statuePos1 = spos.ValueRO.Value; have1 = true; }
            }

            foreach (var (pos, move, atk, tgt, team, e) in
                     SystemAPI.Query<RefRW<Position>, RefRO<Movement>, RefRO<AttackState>, RefRO<Targeting>, RefRO<Team>>()
                              .WithAll<UnitTag>()
                              .WithEntityAccess())
            {
                // STATUS + TERRAIN move scaling (Spell.cs EDIT 1 + A2 Choke). A Stunned unit halts
                // entirely; otherwise the base speed is scaled by the status move multiplier
                // (Chilled slow / Hasted speed, StatusQuery) AND by the unit's occupied terrain
                // feature MoveMult (Choke < 1; open = 1). Both factors COMBINE into one effective
                // speed used by BOTH the manual and the auto branch (no fork). Burst-safe: the
                // StatusQuery helpers are [BurstCompile]+pure; we fetch buffers by entity.
                float spd = move.ValueRO.Speed;
                bool stunned = false;
                if (SystemAPI.HasBuffer<StatusEffect>(e))
                {
                    var sbuf = SystemAPI.GetBuffer<StatusEffect>(e);
                    if (StatusQuery.IsStunned(in sbuf)) stunned = true;          // EDIT 1: stunned ⇒ no move
                    // EDIT 1 + §6 ADR-2-002: spell Chilled/Hasted (uncapped §5.3 layer) × the
                    // CLAMPED commander Hasted contribution (≤ k_CommanderBudgetCeiling) → net speed.
                    else spd *= StatusQuery.MoveSpeedMultiplier(in sbuf, k_CommanderBudgetCeiling);
                }
                // A2 CHOKE: scale by the occupied feature's MoveMult (Choke < 1; open/absent = 1).
                if (!stunned) spd *= ResolveOccupiedMoveMult(ref state, e);

                // CONTROL OVERRIDE (§12 / §13 P1.3): PossessControlSystem (which runs BEFORE this
                // system) consumes ManualOrder and writes the player's move intent into
                // MoveDestination{Value,Active}. So we read that override here — NOT ManualOrder
                // (whose HasMove flag PossessControl already cleared this tick). A possessed unit,
                // or one with an active MoveDestination, is control-driven, not auto-targeted.
                bool hasDest = SystemAPI.HasComponent<MoveDestination>(e);
                bool manualMove = hasDest && SystemAPI.GetComponent<MoveDestination>(e).Active != 0;
                if (SystemAPI.HasComponent<Possessed>(e) || manualMove)
                {
                    if (manualMove && !stunned)
                    {
                        // RC-3: HALT the attack-move to FIGHT when a valid enemy is already within attack
                        // range — otherwise an active MoveDestination (SimAiDriver/HUD advance, or a
                        // formation slot) walks the unit PAST enemies to the statue without engaging.
                        // Reuses the unit's existing AttackState.Range + Targeting.Current (no new constant);
                        // CombatSystem resolves the swing, and the override is kept so the push resumes once
                        // the enemy dies/leaves.
                        Entity tcur = tgt.ValueRO.Current;
                        bool enemyInRange = tcur != Entity.Null
                            && SystemAPI.HasComponent<Position>(tcur)
                            && math.distance(pos.ValueRO.Value, SystemAPI.GetComponent<Position>(tcur).Value) <= atk.ValueRO.Range;
                        if (!enemyInRange)
                        {
                            var dest = SystemAPI.GetComponent<MoveDestination>(e);
                            StepToward(ref pos.ValueRW.Value, dest.Value, spd, dt,
                                       stopDist: k_ManualArriveRadius);
                            // ARRIVAL → clear the override (1.4 owns clearing per the handoff contract),
                            // so the unit resumes auto-targeting next tick.
                            if (math.distance(pos.ValueRO.Value, dest.Value) <= k_ManualArriveRadius)
                            {
                                dest.Active = 0;
                                SystemAPI.SetComponent(e, dest);
                            }
                        }
                        // else: enemy in range → hold station this tick; CombatSystem swings on Targeting.Current.
                    }
                    // Pure-possess (no active move override) leaves position to the control shell.
                    // A stunned manual-move unit holds station (no step) but keeps its override.
                    continue;
                }

                if (stunned) continue; // EDIT 1: stunned auto unit does not move this tick.

                // AUTO: close on the targeting-owned current target until inside attack range.
                Entity target = tgt.ValueRO.Current;
                if (target == Entity.Null || !SystemAPI.HasComponent<Position>(target))
                {
                    // RC-5: no acquired target → ORGANIC MARCH TO CONTACT toward the enemy statue at the unit's
                    // own MoveSpeed (replaces the SimAiDriver/auto-demo advance scaffold). Stop within the unit's
                    // existing AttackState.Range (no new constant) so it then acquires + attacks the statue/enemy.
                    // Miners stay on the mines.
                    if (!SystemAPI.HasComponent<MinerTag>(e))
                    {
                        int tid = team.ValueRO.Id;
                        if (tid == 0 && have1)
                            StepToward(ref pos.ValueRW.Value, statuePos1, spd, dt, stopDist: atk.ValueRO.Range);
                        else if (tid == 1 && have0)
                            StepToward(ref pos.ValueRW.Value, statuePos0, spd, dt, stopDist: atk.ValueRO.Range);
                    }
                    continue;
                }

                float2 targetPos = SystemAPI.GetComponent<Position>(target).Value;
                StepToward(ref pos.ValueRW.Value, targetPos, spd, dt,
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

        /// <summary>
        /// A2 CHOKE: move-speed multiplier from the terrain feature the unit currently OCCUPIES
        /// (TerrainOccupancy.Feature → TerrainFeature.MoveMult; Choke &lt; 1, open = 1). No
        /// occupancy / open ground (Entity.Null) / missing feature / non-positive MoveMult ⇒
        /// neutral 1.0. TerrainSystem owns occupancy; this only READS it (no geometry computed
        /// here). Burst-safe (pure SystemAPI component reads).
        /// </summary>
        private static float ResolveOccupiedMoveMult(ref SystemState state, Entity unit)
        {
            // CS0120 fix: this is a `static` helper, so it cannot touch the source-generated
            // instance __TypeHandle/__query fields that SystemAPI.* expands to. Use the
            // behavior-identical EntityManager component reads off the `ref SystemState` we
            // already receive (pure reads — same data, no generator involvement).
            if (!state.EntityManager.HasComponent<TerrainOccupancy>(unit)) return 1f;
            Entity feature = state.EntityManager.GetComponentData<TerrainOccupancy>(unit).Feature;
            if (feature == Entity.Null || !state.EntityManager.HasComponent<TerrainFeature>(feature)) return 1f;
            float mv = state.EntityManager.GetComponentData<TerrainFeature>(feature).MoveMult;
            return mv > 0f ? mv : 1f;
        }
    }

    /// <summary>
    /// Resolves attacks against Targeting.Current using the §13 P1.4 modifier chain.
    /// Statue targets receive damage via StatueDamageInbox; units take it on Health.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SimPhaseBegin))]
    [UpdateBefore(typeof(SimPhaseEnd))]
    [UpdateAfter(typeof(MovementSystem))]
    [UpdateAfter(typeof(TerrainSystem))]            // TerrainOccupancy set before the cover/hazard read (A1).
    [UpdateAfter(typeof(PositionalSystem))]         // §4 positional slot populated before the chain reads it.
    [UpdateAfter(typeof(TelegraphResolveSystem))]   // spell status/damage lands before this tick's combat.
    [UpdateAfter(typeof(CommanderAbilitySystem))]   // commander Raged/Hasted buffs felt by this tick's combat.
    public partial struct CombatSystem : ISystem
    {
        // §6 ADR-2-002 commander-buff clamp ceiling (mirrors MovementSystem.k_CommanderBudgetCeiling
        // and CommanderAbilitySystem.k_PowerBudgetCeiling): StatusQuery clamps the COMBINED
        // commander-sourced Raged fraction to this when computing the outgoing-damage multiplier, so
        // the commander-attributable damage buff is provably ≤ the canon §6 cap (0.15) while spell
        // Raged stays its own uncapped §5.3 layer. The one allowed literal (canon §6 cap, cited).
        private const float k_CommanderBudgetCeiling = 0.15f; // §6 commander power cap (ADR-2-002).

        // GATE1-balance (Option A — SUDDEN-DEATH ESCALATION). Build-1/2 device validation proved that targeting
        // (Option B) alone CANNOT break a balanced, continuously-reinforced contact line (0/20 organic at the
        // symmetric 5v5 equilibrium): every unit always has an enemy in attack range, so no clear lane ever opens.
        // Fix: after kEscalationStartSec, ALL combat damage ramps up linearly (capped), SYMMETRICALLY for both
        // teams. Casualties then outpace gold-limited reinforcement → the line thins → units (high StatueBonus)
        // pour through → a statue falls. The marginally-stronger side (jitter/position) wins → BOTH outcomes;
        // bounded match length (first contact ~44s, resolution ~110-140s). NOT a balance edge (no side favored).
        private const float kEscalationStartSec = 40f;  // overtime begins (after the economy/contact phase)
        private const float kEscalationRate     = 0.08f; // +8% base damage per second after the start
        private const float kEscalationCap      = 10f;   // hard ceiling on the multiplier (prevents runaway)

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;
            // ElapsedTime is the WORLD clock (frozen during the timeScale=0 menus, never rewinds). It is valid as
            // a per-MATCH clock here ONLY because this build runs exactly ONE match per app-launch — the sim builds
            // the world once and exposes no battle-reset (see MatchPresentation: "ONE battle per launch"), and the
            // validation force-stops + relaunches between matches. FRAGILE: if an in-process rematch/battle-reset
            // ever ships, match 2 would start near the escalation cap → switch this to a per-match start time
            // (store match-start ElapsedTime in the MatchState singleton and use the delta). Build-3 is unaffected.
            float matchEscalation = math.min(kEscalationCap,
                1f + math.max(0f, (float)SystemAPI.Time.ElapsedTime - kEscalationStartSec) * kEscalationRate);

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

                // STUN GATE (Spell.cs EDIT 3): a Stunned unit CANNOT ACT — no swing this tick.
                // Burst-safe: StatusQuery is [BurstCompile]+pure; we fetch the buffer by entity.
                if (SystemAPI.HasBuffer<StatusEffect>(e))
                {
                    var sbufStun = SystemAPI.GetBuffer<StatusEffect>(e);
                    if (StatusQuery.IsStunned(in sbufStun)) continue; // stunned: skip the swing
                }

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

                // typeArmor via the ONE shared §4 lookup (CounterMatrix.Lookup) — the SAME read
                // the Phase-2 SpellSystem uses, so there is a single type×armor path (no fork).
                float typeArmor = CounterMatrix.Lookup(matrix, profile.ValueRO.DamageType,
                                                       ResolveTargetArmor(ref state, target));

                // Raged (Spell.cs EDIT 3): outgoing-status factor multiplies INTO the §4 chain
                // (it joins the SAME chain — no fork). Neutral 1.0 on an empty/absent buffer.
                // §6 ADR-2-002: net = spell Raged (uncapped §5.3 layer) × the CLAMPED commander Raged
                // contribution (≤ k_CommanderBudgetCeiling), so commander damage buff ≤ §6 budget.
                float ragedMult = 1f;
                if (SystemAPI.HasBuffer<StatusEffect>(e))
                {
                    var sbufRaged = SystemAPI.GetBuffer<StatusEffect>(e);
                    ragedMult = StatusQuery.OutgoingDamageMultiplier(in sbufRaged, k_CommanderBudgetCeiling);
                }

                // TERRAIN COVER (A1, §4/§11): the §4 chain's attacker-side factors are
                // positional/terrain(attacker HighGround AttackMult, in TerrainFactor)/difficulty;
                // now also fold in the TARGET's Cover defense — its occupied TerrainFeature's
                // DefenseMult (Cover < 1; open = 1). Read the target's TerrainOccupancy.Feature →
                // TerrainFeature.DefenseMult; absent/open ⇒ neutral 1.0. Joins the SAME chain.
                float coverMult = ResolveTargetCover(ref state, target);

                // §4 modifier chain (Phase 2 populates the previously-neutral positional/terrain
                // slots via PositionalSystem/TerrainSystem; cover + raged join the same chain).
                float dmg = rounded
                            * typeArmor
                            * positional.ValueRO.Multiplier
                            * terrain.ValueRO.Multiplier
                            * difficulty.ValueRO.Multiplier
                            * coverMult
                            * ragedMult;
                if (dmg < 0f) dmg = 0f;

                // GATE1-balance (Option A): symmetric sudden-death ramp — applied to BOTH unit and statue damage
                // (computed before the target-type branch), identically for every team → fair. Breaks the
                // reinforcement=attrition equilibrium so matches resolve organically.
                dmg *= matchEscalation;

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
            // CS0120 fix: `static` helper — replace SystemAPI.* (which expands to instance
            // generator fields) with the behavior-identical EntityManager reads off `state`.
            if (state.EntityManager.HasComponent<CombatProfile>(target))
                return state.EntityManager.GetComponentData<CombatProfile>(target).ArmorClass;
            return Bulwark.Data.ArmorClass.Unset;
        }

        /// <summary>
        /// TARGET Cover defense factor (A1, §4/§11): if the target occupies a TerrainFeature with
        /// Cover (DefenseMult &lt; 1; open = 1), the incoming hit is scaled by that DefenseMult.
        /// Reads the target's TerrainOccupancy.Feature → TerrainFeature.DefenseMult. Any unit with
        /// no TerrainOccupancy, an open-ground (Entity.Null) feature, a missing TerrainFeature, or a
        /// non-positive DefenseMult ⇒ neutral 1.0. The attacker's HighGround AttackMult is already
        /// folded into TerrainFactor, so this adds ONLY the target-side Cover (no double counting).
        /// </summary>
        private static float ResolveTargetCover(ref SystemState state, Entity target)
        {
            // CS0120 fix: `static` helper — replace SystemAPI.* (which expands to instance
            // generator fields) with the behavior-identical EntityManager reads off `state`.
            if (!state.EntityManager.HasComponent<TerrainOccupancy>(target)) return 1f;
            Entity feature = state.EntityManager.GetComponentData<TerrainOccupancy>(target).Feature;
            if (feature == Entity.Null || !state.EntityManager.HasComponent<TerrainFeature>(feature)) return 1f;
            float def = state.EntityManager.GetComponentData<TerrainFeature>(feature).DefenseMult;
            return def > 0f ? def : 1f;
        }
    }
}

// BULWARK — Phase 2.6 SQUAD TACTICAL AI (roadmap §13 2.6 "layered AI"; §4 layered-but-O(1)
// AI; §16 fairness). The MIDDLE decision layer:
//
//   COMMANDER layer (§13 1.5 BasicAISystem) — picks a CommanderStance {Defend/Push/Harass}
//        and biases TRAINING for the AI side. (Read, not duplicated, here.)
//        ▲ ABOVE
//   SQUAD layer (THIS FILE) — per AI squad, picks a SquadPosture {Hold/Advance/FocusFire/
//        Retreat} from a cheap O(1) utility (local strength vs enemy, statue distance,
//        commander stance, terrain) and translates it into SquadFormation.Anchor/Type +
//        SquadAIState.FocusTarget. Acts on the multi-axis DifficultyAxes (Aggression/Eco/
//        SpellAccess/Stats).
//        ▼ BELOW
//   UNIT layer — there is NO bespoke unit AI: AI units reuse the SAME shared TargetingSystem
//        + Movement/Combat as the player. This file authors NO combat/targeting (§4 "one
//        combat core"); it only WRITES sim data (formation anchor/type, focus target, train
//        orders, spell cast requests, commander active request).
//
// PERF / SCHEDULER (§4 budgeted scheduler, §12 ~O(1)/agent)
// Re-evaluation is double-gated so AI cost is FRAME-STABLE:
//   1. AISchedulerSystem (runs just before this) advances a round-robin cursor; only
//      BudgetPerFrame agents are PERMITTED to think on any frame (AISchedulerSystem.MayThink).
//   2. Each permitted agent additionally respects its OWN SquadAIState.ReevalCooldown cadence.
// We enumerate the SquadAIState query in the SAME order the scheduler counted it (same
// archetype set, same frame), so a running ordinal is a stable per-agent index we can pass to
// MayThink WITHOUT adding an index field to the shared SquadAIState component. Per permitted
// agent the work is a handful of cached reads + comparisons — O(1), no all-pairs scan.
//
// CANON DISCIPLINE
//   • §12 ECS boundary: pure battle sim (ISystem). Squad layer WRITES sim data only; it never
//     forks combat, never moves units directly (Movement/Combat own that), never spends Gold
//     (TrainingSystem deducts at the queue head — we only append TrainOrders, like BasicAI).
//   • §15.6 / Hard-Rule: NO hardcoded balance. Unit costs/train times come from the
//     UnitSpawnStats catalog (data); difficulty is the DifficultyAxes singleton (data); spell
//     identity/telegraph/counter come from SpellSlot/SpellDef (data). The constants below are
//     AI PACING/fairness knobs (cadence, utility weights, the modest Stats cap) — they tune
//     fairness, not power, are symmetric, and are telemetry-tunable (§16) — kept as code
//     consts per the canon-review allowance, mirroring BasicAI.cs.
//   • §16 FAIRNESS: difficulty axes BIAS the AI's pacing/decisions; they do not let it cheat.
//     Stats scaling is hard-clamped to a small band (k_StatsAxisMaxBonus) and is applied via
//     the Difficulty.Multiplier SLOT that the §4 combat chain already reads — NOT a fork. AI
//     spell casts go through the SAME SpellCastRequest inbox + telegraph (§5.3) the player uses.
//   • §5.3 every spell counterable: we ONLY append a SpellCastRequest (a normal cast); the
//     shared SpellSystem spawns the telegraph + applies the counterable effect. We invent no
//     spell and bypass no telegraph.
//   • Forbidden now (Phase 3+/7): no economy persistence, no commander COLLECTION (we read the
//     one CommanderRuntime "bones"), no determinism/replays, no new mechanics.
//   • §15.10 no over-engineering: cheap utility (a few weighted signals + argmax), not GOAP/ML.
//
// ORDERING (SimSystemOrder.cs convention): between the Phase-1 anchors, AFTER AISchedulerSystem
// (cursor fresh) which itself is AFTER BasicAISystem (commander stance fresh) and BEFORE
// MovementSystem (posture biases this tick's movement). Train-order / spell-request / commander
// writes are consumed by their owning systems with standard one-tick latency.
//
// SCAFFOLD STATUS: authored, NOT compiled (no Unity 6 / Entities here — ADR-0-001/-002/-1-001).
// Runtime behavior (posture feel, frame-stability, spell timing) is DEFERRED (deferredValidations).

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Bulwark.Sim
{
    /// <summary>
    /// Squad tactical AI for the AIControlled side(s). For each scheduler-permitted, cadence-due
    /// <see cref="SquadAIState"/> it scores the four postures from a cheap O(1) utility, commits
    /// the best, and writes the squad's <see cref="SquadFormation"/> anchor/type + focus target.
    /// Difficulty axes (<see cref="DifficultyAxes"/>) bias training pace, spell casting, push/hold,
    /// and a small fair stats scaling. It micro-manages NOTHING: the shared Targeting/Movement/
    /// Combat own per-unit behavior for ALL teams (§4 one combat core).
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SimPhaseBegin))]
    [UpdateBefore(typeof(SimPhaseEnd))]
    [UpdateAfter(typeof(AISchedulerSystem))] // round-robin cursor is fresh before any agent thinks.
    [UpdateBefore(typeof(MovementSystem))]   // posture/anchor biases this tick's movement.
    public partial struct SquadAISystem : ISystem
    {
        // ---- AI PACING / fairness knobs (NOT unit/combat balance) ---------------------------
        // Per-agent reeval cadence (~1s, like BasicAI) — the scheduler caps HOW MANY think per
        // frame; this caps how OFTEN a given squad re-decides. Both are §4/§12 frame-stability,
        // not balance.
        private const float k_ReevalPeriod = 1.0f;

        // Posture stickiness so a squad doesn't thrash posture every tick under noise (mirrors
        // BasicAI's StanceStickiness). Fairness/feel, not power.
        private const float k_PostureStickiness = 0.10f;

        // Local neighbourhood radius (front units) used for the O(1) local-strength read. A
        // squad evaluates only units near its own anchor — NOT the whole board (§12). Spatial
        // engine knob, not balance.
        private const float k_LocalRadius = 8.0f;

        // How far the posture nudges the formation anchor along the squad's facing each reeval.
        // Advance pushes toward the enemy; Retreat falls back; Hold/FocusFire stay. Movement is
        // still executed by MovementSystem at the unit's data-driven speed — this only moves the
        // RALLY anchor, a tactical hint. Engine/feel knob, not balance.
        private const float k_AnchorAdvanceStep = 3.0f;
        private const float k_AnchorRetreatStep = 2.0f;

        // FAIRNESS CLAMP (§16): the Stats axis may grant at most this small combat bonus via the
        // shared Difficulty.Multiplier slot. Hard-clamped so a high difficulty biases PACING and
        // DECISIONS, not raw power — the AI never gets an unfair statline. Symmetric, modest.
        private const float k_StatsAxisMaxBonus = 0.15f; // ≤ +15% on the §4 chain's difficulty slot.

        // Economy pacing: Eco axis scales how aggressively the squad layer tops up the side's
        // training queue. Floor/cap keep it from flooding (back-pressure mirrors BasicAI).
        private const int k_BaseEcoQueueCap = 3; // max pending orders the squad layer will add toward.

        // Spell pacing: SpellAccess axis gates how readily the AI casts. Below this normalised
        // threshold the squad layer does not cast at all this reeval (fair handicap on low diff).
        private const float k_SpellAccessCastThreshold = 0.5f;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SquadAIState>();
            state.RequireForUpdate<AISchedulerState>();
            state.RequireForUpdate<MatchState>();
        }

        // Not [BurstCompile]: like BasicAISystem this performs EntityManager buffer lookups
        // (catalog, train queue, spell loadout, spell inbox) and singleton reads. It runs at
        // most BudgetPerFrame agents/frame on a ~1s cadence, so it is NOT on the hot path; the
        // hot path (Targeting/Movement/Combat) is Burst'd in its own files.
        public void OnUpdate(ref SystemState state)
        {
            // Freeze with the sim once the match is decided (MatchFlow owns outcome).
            if (SystemAPI.GetSingleton<MatchState>().Outcome != MatchOutcome.Ongoing)
                return;

            float dt = SystemAPI.Time.DeltaTime;

            // Total agents + the scheduler's published window (for the per-agent gate). Counting
            // here matches the scheduler's count (same query, same frame) so ordinals line up.
            int agentCount = 0;
            foreach (var _ in SystemAPI.Query<RefRO<SquadAIState>>())
                agentCount++;
            if (agentCount == 0)
                return;

            var sched = SystemAPI.GetSingleton<AISchedulerState>();

            // Difficulty axes (data singleton). Optional: if Bootstrap hasn't baked it, use a
            // neutral 1.0 baseline so the squad layer still runs fairly.
            DifficultyAxes axes = new DifficultyAxes { Eco = 1f, Aggression = 1f, Stats = 1f, SpellAccess = 1f };
            if (SystemAPI.HasSingleton<DifficultyAxes>())
                axes = SystemAPI.GetSingleton<DifficultyAxes>();

            // PHASE-2 TWO-FACTION: the unit catalog is now per-team; each agent resolves ITS OWN
            // side's roster inside the loop below (Eco training reads that faction's roles/costs).

            // Iterate agents in the SAME enumeration order the scheduler counted them, tracking a
            // running ordinal as the stable per-agent index for the round-robin gate.
            int ordinal = -1;
            foreach (var (squadRef, e) in
                     SystemAPI.Query<RefRW<SquadAIState>>().WithEntityAccess())
            {
                ordinal++;

                ref var squad = ref squadRef.ValueRW;

                // Always tick this agent's own cadence so it stays in phase even on frames the
                // scheduler skips it (cheap; no decision work happens unless BOTH gates pass).
                if (squad.ReevalCooldown > 0f)
                    squad.ReevalCooldown -= dt;

                // GATE 1 — budgeted scheduler: only BudgetPerFrame agents may think this frame.
                if (!AISchedulerSystem.MayThink(ordinal, sched.Cursor, sched.BudgetPerFrame, agentCount))
                    continue;

                // GATE 2 — per-agent cadence: don't re-decide faster than the reeval period.
                if (squad.ReevalCooldown > 0f)
                    continue;
                squad.ReevalCooldown = k_ReevalPeriod;

                // ---- Gather cheap O(1)-ish LOCAL signals around this squad's anchor ----------
                SquadSignals sig = GatherSquadSignals(ref state, in squad);

                // ---- Read the commander stance for this team (§13 2.6 ABOVE-layer input) -----
                CommanderStance stance = GetCommanderStance(ref state, squad.Team);

                // ---- O(1) utility: pick the best posture (with Aggression bias + stickiness) --
                Entity focus;
                SquadPosture posture = PickPosture(in sig, stance, axes.Aggression, squad.Posture, out focus);
                squad.Posture = posture;
                squad.FocusTarget = (posture == SquadPosture.FocusFire) ? focus : Entity.Null;

                // ---- Translate posture -> SquadFormation anchor/type (+ facing) --------------
                ApplyPostureToFormation(ref state, in squad, in sig);

                // ---- Difficulty-axis actions (all WRITE data only; owning systems resolve) ----
                ApplyStatsAxis(ref state, squad.Team, axes.Stats);                 // Stats → §4 difficulty slot (clamped).
                if (TryGetCatalog(ref state, squad.Team, out var catalog))         // this side's per-team roster.
                    ApplyEcoAxis(ref state, squad.Team, axes.Eco, in sig, catalog); // Eco → train pace (TrainOrders).
                ApplySpellAxis(ref state, in squad, in sig, axes.SpellAccess);      // SpellAccess → SpellCastRequests.
                ApplyAggressionToCommanderActive(ref state, squad.Team, posture, axes.Aggression); // optional active request.
            }
        }

        // ============================ SIGNALS (O(1) local read) ============================

        private struct SquadSignals
        {
            public float2 Anchor;        // squad rally anchor (from SquadFormation)
            public float2 Facing;        // squad facing (toward the enemy, set by control/this layer)
            public int OwnNear;          // own units within k_LocalRadius of the anchor
            public int EnemyNear;        // enemy units within k_LocalRadius of the anchor
            public Entity NearestEnemy;  // closest enemy unit to the anchor (FocusFire candidate)
            public float NearestEnemyDistSq;
            public float OwnStatueFrac;  // own statue health fraction (1 = full)
            public float EnemyStatueFrac;// enemy statue health fraction
            public float DistToEnemyStatueSq;
            public byte OnFavourableTerrain; // 1 if this squad's anchor sits on AttackMult>1 / DefenseMult<1 terrain
            public byte InHazard;            // 1 if anchor sits on Hazard terrain (HazardDps>0)
        }

        /// <summary>
        /// One bounded pass to read a handful of LOCAL counts around this squad's anchor. NOT an
        /// all-pairs scan: each unit is visited once and only distance-tested against THIS
        /// squad's single anchor (O(units) per permitted agent, and only BudgetPerFrame agents
        /// run per frame, so worst-case per-frame cost stays bounded — §12). Statues (2) and
        /// terrain features (≤4 per map, §11) are tiny fixed sets.
        /// </summary>
        private SquadSignals GatherSquadSignals(ref SystemState state, in SquadAIState squad)
        {
            var sig = new SquadSignals
            {
                Anchor = float2.zero,
                Facing = new float2(1f, 0f),
                OwnNear = 0,
                EnemyNear = 0,
                NearestEnemy = Entity.Null,
                NearestEnemyDistSq = float.MaxValue,
                OwnStatueFrac = 1f,
                EnemyStatueFrac = 1f,
                DistToEnemyStatueSq = float.MaxValue,
                OnFavourableTerrain = 0,
                InHazard = 0,
            };

            // Resolve this squad's formation anchor/facing (the spatial reference for the read).
            bool haveAnchor = false;
            foreach (var f in SystemAPI.Query<RefRO<SquadFormation>>())
            {
                if (f.ValueRO.SquadId != squad.SquadId) continue;
                sig.Anchor = f.ValueRO.Anchor;
                sig.Facing = math.lengthsq(f.ValueRO.Facing) > 1e-6f
                    ? math.normalize(f.ValueRO.Facing)
                    : new float2(1f, 0f);
                haveAnchor = true;
                break;
            }
            // No formation entity yet → fall back to the team spawn point as the anchor so the
            // squad still has a sensible reference (data-driven, baked by Bootstrap).
            if (!haveAnchor)
            {
                foreach (var sp in SystemAPI.Query<RefRO<TeamSpawnPoint>>())
                {
                    if (sp.ValueRO.Team != squad.Team) continue;
                    sig.Anchor = sp.ValueRO.Pos;
                    break;
                }
            }

            float radSq = k_LocalRadius * k_LocalRadius;

            // Local own/enemy unit counts + nearest enemy to the anchor.
            foreach (var (pos, team, e) in
                     SystemAPI.Query<RefRO<Position>, RefRO<Team>>()
                              .WithAll<UnitTag, Health>()
                              .WithEntityAccess())
            {
                float dSq = math.distancesq(pos.ValueRO.Value, sig.Anchor);
                if (dSq > radSq) continue;
                if (team.ValueRO.Id == squad.Team)
                {
                    sig.OwnNear++;
                }
                else
                {
                    sig.EnemyNear++;
                    if (dSq < sig.NearestEnemyDistSq)
                    {
                        sig.NearestEnemyDistSq = dSq;
                        sig.NearestEnemy = e;
                    }
                }
            }

            // Statue fractions + distance to the enemy statue (2 statues only — O(1)).
            foreach (var (stag, sstate, spos) in
                     SystemAPI.Query<RefRO<StatueTag>, RefRO<StatueState>, RefRO<Position>>())
            {
                float max = sstate.ValueRO.MaxHealth;
                float frac = max > 0f ? sstate.ValueRO.Health / max : 1f;
                if (stag.ValueRO.Team == squad.Team)
                {
                    sig.OwnStatueFrac = frac;
                }
                else
                {
                    sig.EnemyStatueFrac = frac;
                    sig.DistToEnemyStatueSq = math.distancesq(spos.ValueRO.Value, sig.Anchor);
                }
            }

            // Terrain read (≤4 features/map, §11): if any OWN unit near the anchor OCCUPIES a
            // feature, reflect its character into the signal (favourable offence/defence edge vs
            // a hazard zone). TerrainOccupancy.Feature is set by the sibling TerrainSystem; we
            // only READ it (we never compute terrain geometry here — that is TerrainSystem's job).
            // Using occupancy (rather than the feature's map X, which isn't on the component)
            // keeps this an O(1)-per-near-unit read with no fabricated positional math.
            foreach (var (occ, team, pos) in
                     SystemAPI.Query<RefRO<TerrainOccupancy>, RefRO<Team>, RefRO<Position>>()
                              .WithAll<UnitTag>())
            {
                if (team.ValueRO.Id != squad.Team) continue;
                if (math.distancesq(pos.ValueRO.Value, sig.Anchor) > radSq) continue;
                Entity feat = occ.ValueRO.Feature;
                if (feat == Entity.Null || !SystemAPI.HasComponent<TerrainFeature>(feat)) continue;
                var tf = SystemAPI.GetComponent<TerrainFeature>(feat);
                if (tf.HazardDps > 0f) sig.InHazard = 1;
                if (tf.AttackMult > 1f || (tf.DefenseMult > 0f && tf.DefenseMult < 1f))
                    sig.OnFavourableTerrain = 1;
            }

            return sig;
        }

        /// <summary>
        /// Reads the §13 1.5 commander stance for a team (Defend default if absent). Takes
        /// <c>ref SystemState state</c> because it uses SystemAPI.Query: the Entities source
        /// generator needs the state handle to update query handles + complete dependencies (SGSG0002).
        /// </summary>
        private CommanderStance GetCommanderStance(ref SystemState state, int team)
        {
            foreach (var cmd in SystemAPI.Query<RefRO<AICommander>>())
            {
                if (cmd.ValueRO.Team == team)
                    return cmd.ValueRO.Stance;
            }
            return CommanderStance.Defend;
        }

        // ============================ POSTURE UTILITY ============================

        /// <summary>
        /// Cheap O(1) utility: score the four postures from local strength, statue health,
        /// commander stance, terrain, and the Aggression axis; argmax wins with light stickiness
        /// to the current posture. This is a "handful of comparisons" (§4 O(1) squad layer), not
        /// search/planning (§15.10). Defend-equivalent (Hold) is the tie default → fair.
        /// </summary>
        private SquadPosture PickPosture(in SquadSignals sig, CommanderStance stance,
                                         float aggressionAxis, SquadPosture current, out Entity focus)
        {
            focus = sig.NearestEnemy;

            // Local army edge (counts only; +1 smoothing). >1 = we outnumber locally.
            float localRatio = (sig.OwnNear + 1f) / (sig.EnemyNear + 1f);

            // Commander stance bias (the ABOVE layer steers the squad layer, §13 2.6).
            float stancePush = stance == CommanderStance.Push ? 1f : (stance == CommanderStance.Harass ? 0.5f : 0f);
            float stanceHold = stance == CommanderStance.Defend ? 1f : 0f;

            // Aggression axis (data) scales the push/retreat thresholds. Clamped to a sane band so
            // even max difficulty stays fair (§16) — it tilts decisions, it does not force suicide
            // or grant power.
            float aggr = math.clamp(aggressionAxis, 0.5f, 1.5f);

            // RETREAT: badly outnumbered locally, OR standing in a hazard, OR own statue in danger
            // while we have no local edge → fall back and preserve the army.
            float retreat =
                  (localRatio < 0.6f ? (0.6f - localRatio) * 2f : 0f)
                + (sig.InHazard != 0 ? 0.75f : 0f)
                + ((sig.OwnStatueFrac < 0.4f && localRatio < 1f) ? 0.5f : 0f);
            retreat *= (2f - aggr); // lower aggression → more willing to retreat.

            // HOLD: rough parity, or commander says Defend, or we're on favourable terrain worth
            // keeping → consolidate on the anchor.
            float hold =
                  stanceHold
                + (localRatio >= 0.8f && localRatio <= 1.25f ? 0.5f : 0f)
                + (sig.OnFavourableTerrain != 0 ? 0.5f : 0f);

            // FOCUSFIRE: a clear local enemy target exists and we have at least parity → collapse
            // fire onto the nearest enemy (the shared Targeting still acquires per unit; FocusTarget
            // is a HINT the formation anchor expresses, not a combat override).
            float focusFire =
                  (sig.NearestEnemy != Entity.Null && localRatio >= 0.9f ? 0.6f : 0f)
                + (sig.EnemyNear >= 2 && localRatio >= 1f ? 0.4f : 0f);

            // ADVANCE: we have a local edge AND (commander pushes OR enemy statue is the prize) →
            // press toward the front/objective.
            float advance =
                  (localRatio > 1.1f ? (localRatio - 1f) : 0f)
                + stancePush
                + (1f - sig.EnemyStatueFrac) * 0.4f; // already chipping the base → keep advancing.
            advance *= aggr; // higher aggression → more willing to advance.

            // Stickiness to the current posture to avoid per-tick thrash.
            switch (current)
            {
                case SquadPosture.Hold:      hold      += k_PostureStickiness; break;
                case SquadPosture.Advance:   advance   += k_PostureStickiness; break;
                case SquadPosture.FocusFire: focusFire += k_PostureStickiness; break;
                case SquadPosture.Retreat:   retreat   += k_PostureStickiness; break;
            }

            // Argmax (Hold default on ties → fair, never reflexively aggressive).
            SquadPosture best = SquadPosture.Hold;
            float bestScore = hold;
            if (advance   > bestScore) { bestScore = advance;   best = SquadPosture.Advance; }
            if (focusFire > bestScore) { bestScore = focusFire; best = SquadPosture.FocusFire; }
            if (retreat   > bestScore) { bestScore = retreat;   best = SquadPosture.Retreat; }
            return best;
        }

        // ============================ POSTURE -> FORMATION ============================

        /// <summary>
        /// Express the chosen posture as SquadFormation data: nudge the Anchor along/against the
        /// squad facing and pick a FormationType. Movement to the anchor is executed by the shared
        /// MovementSystem at each unit's data-driven speed — we only set the tactical reference.
        ///   Advance   → step anchor toward the enemy, Line (front presence).
        ///   FocusFire → anchor toward the focus target, Tight (concentrate).
        ///   Retreat   → step anchor back, Loose (spread to reduce AoE/hazard exposure).
        ///   Hold      → keep anchor, Line.
        /// </summary>
        private void ApplyPostureToFormation(ref SystemState state, in SquadAIState squad, in SquadSignals sig)
        {
            foreach (var fRef in SystemAPI.Query<RefRW<SquadFormation>>())
            {
                ref var form = ref fRef.ValueRW;
                if (form.SquadId != squad.SquadId) continue;

                float2 facing = math.lengthsq(form.Facing) > 1e-6f
                    ? math.normalize(form.Facing)
                    : sig.Facing;

                switch (squad.Posture)
                {
                    case SquadPosture.Advance:
                        form.Anchor += facing * k_AnchorAdvanceStep;
                        form.Type = FormationType.Line;
                        break;

                    case SquadPosture.FocusFire:
                        // Bias the anchor toward the focus target if we have one.
                        if (squad.FocusTarget != Entity.Null &&
                            SystemAPI.HasComponent<Position>(squad.FocusTarget))
                        {
                            float2 tp = SystemAPI.GetComponent<Position>(squad.FocusTarget).Value;
                            float2 dir = tp - form.Anchor;
                            if (math.lengthsq(dir) > 1e-6f)
                            {
                                dir = math.normalize(dir);
                                form.Anchor += dir * k_AnchorAdvanceStep;
                                form.Facing = dir;
                            }
                        }
                        form.Type = FormationType.Tight;
                        break;

                    case SquadPosture.Retreat:
                        form.Anchor -= facing * k_AnchorRetreatStep;
                        form.Type = FormationType.Loose;
                        break;

                    default: // Hold
                        form.Type = FormationType.Line;
                        break;
                }
                return;
            }
        }

        // ============================ DIFFICULTY AXES (write data only) ============================

        /// <summary>
        /// STATS axis (§16 fairness): scale the AI side's PER-UNIT Difficulty.Multiplier SLOT — the
        /// SAME slot the §4 combat chain (Combat.cs's main per-unit query reads RefRO&lt;Difficulty&gt;)
        /// already multiplies by — by a SMALL, HARD-CLAMPED amount. We do NOT fork combat or touch
        /// unit stats; we nudge the existing difficulty factor and clamp the bonus to
        /// k_StatsAxisMaxBonus so the AI never gets an unfair statline. The write is PER-TEAM (only
        /// the AI team's own units), so it cannot scale the player — fair and symmetric.
        ///
        /// PER-UNIT Difficulty (A5, integration RESOLVED): every unit now carries a per-unit
        /// Difficulty{Multiplier=1} slot — added at spawn by SpellSummon.SpawnFromCatalog (the one
        /// path TrainingSystem.SpawnUnit delegates to), and by EnsurePhase2CombatComponentsSystem as
        /// a fallback. BattleBootstrap no longer bakes a GLOBAL Difficulty singleton (that would have
        /// been a second instance and made GetSingleton ambiguous). So Combat.cs's per-unit
        /// RefRO&lt;Difficulty&gt; query and THIS per-team write both resolve a live per-unit slot. We
        /// intentionally do NOT write a global singleton, because that would scale BOTH teams
        /// (unfair, §16) — this write is per-team (AI units only).
        /// </summary>
        private void ApplyStatsAxis(ref SystemState state, int team, float statsAxis)
        {
            // Map the axis (≈0.5..1.5, 1 = baseline) to a clamped multiplier in [1, 1+max].
            float bonus = math.clamp(statsAxis - 1f, 0f, k_StatsAxisMaxBonus);
            float mult = 1f + bonus;

            // Apply only to this team's units (fair, symmetric): the AI team's combat damage uses
            // its own per-unit Difficulty slot. Player units keep their own slot untouched.
            foreach (var (diff, unitTeam) in
                     SystemAPI.Query<RefRW<Difficulty>, RefRO<Team>>().WithAll<UnitTag>())
            {
                if (unitTeam.ValueRO.Id != team) continue;
                diff.ValueRW.Multiplier = mult; // clamped band only — INVIOLABLE fairness (§16).
            }
        }

        /// <summary>
        /// ECO axis: pace the AI side's TRAINING. Higher Eco → top the queue up toward a larger
        /// cap; lower Eco → fewer pending orders. We only APPEND TrainOrders (data) via Training's
        /// helper — TrainingSystem still performs the authoritative Gold deduction at the queue
        /// head (no pre-spend, no cheating). Costs/roles come from the data catalog (Hard-Rule 4).
        /// This composes with BasicAI's economy bias; the squad layer just adds gentle extra pace
        /// on higher difficulty. Back-pressure cap prevents flooding (§16 fair, not overwhelming).
        /// </summary>
        private void ApplyEcoAxis(ref SystemState state, int team, float ecoAxis,
                                  in SquadSignals sig, DynamicBuffer<UnitSpawnStats> catalog)
        {
            // Scale the queue cap by the Eco axis (clamped). 1.0 → base cap; higher → +1.
            float eco = math.clamp(ecoAxis, 0.5f, 1.5f);
            int queueCap = k_BaseEcoQueueCap + (eco > 1.15f ? 1 : 0);
            if (eco < 0.85f) queueCap = math.max(1, k_BaseEcoQueueCap - 1);

            // RC-1 (logic-only): SquadAI is miner-blind and could spend the opening gold on a combat unit before
            // BasicAI's miner-first opening runs (cooldown-bake race). Defer SquadAI's combat enqueue until this
            // side's miner floor is met, making BasicAI the sole opening-economy authority. Reuses the shared
            // BasicAISystem.TargetMiners floor + the existing FindRoleIndex/queue-walk — NO new balance constant.
            int idxMiner = FindRoleIndex(catalog, RoleId.Miner);
            int liveMiners = 0;
            if (idxMiner >= 0)
                foreach (var mt in SystemAPI.Query<RefRO<Team>>().WithAll<MinerTag>())
                    if (mt.ValueRO.Id == team) liveMiners++;

            // Find this side's queue + current length and projected unpaid cost (data costs).
            foreach (var (q, qe) in
                     SystemAPI.Query<RefRO<TrainQueueTag>>().WithEntityAccess())
            {
                if (q.ValueRO.Team != team) continue;
                if (!state.EntityManager.HasBuffer<TrainOrder>(qe)) continue;

                var orders = state.EntityManager.GetBuffer<TrainOrder>(qe);
                if (orders.Length >= queueCap)
                    return; // back-pressure: queue already at/over the eco cap → wait.

                // RC-1: below the miner floor → defer combat (count in-flight miner orders too); BasicAI buys miners.
                if (idxMiner >= 0)
                {
                    int inFlightMiners = 0;
                    for (int oi = 0; oi < orders.Length; oi++)
                        if (orders[oi].UnitIndex == idxMiner) inFlightMiners++;
                    if (liveMiners + inFlightMiners < BasicAISystem.TargetMiners)
                        return; // economy first: let BasicAI's miner-first opening run.
                }

                // Affordability (projected unpaid cost + this unit) against current Gold. We do
                // NOT pre-spend; this only avoids queuing what the side plausibly cannot fund.
                int gold = GetTeamGold(ref state, team);
                int projected = ProjectedUnpaidCost(orders, catalog);

                // Choose ONE unit to add this reeval, biased by local need:
                //   • If locally outnumbered → add a Frontline (hold the line).
                //   • Else if we have an edge & commander-ish push read (enemy statue chipped) →
                //     add a Skirmisher to press.
                //   • Else add a Ranged for poke. (All within the §5 roster roles; data-resolved.)
                RoleId want = (sig.EnemyNear > sig.OwnNear)
                    ? RoleId.Frontline
                    : (sig.EnemyStatueFrac < 1f ? RoleId.Skirmisher : RoleId.Ranged);

                int idx = FindRoleIndex(catalog, want);
                if (idx < 0) idx = FindRoleIndex(catalog, RoleId.Frontline); // fair fallback.
                if (idx < 0) return;

                int cost = catalog[idx].GoldCost;
                if (projected + cost > gold)
                    return; // can't plausibly fund → don't queue (no pre-spend).

                Training.EnqueueTrain(orders, idx); // append only; TrainingSystem pays at the head.
                return;
            }
        }

        /// <summary>
        /// SPELLACCESS axis (§5.3 / §13 2.4): gate how readily the AI casts. Above the access
        /// threshold, if a drafted SpellSlot is OFF cooldown and has charges and the local board
        /// warrants it, APPEND a SpellCastRequest to the shared inbox — the SAME path the player
        /// uses. We do NOT resolve the spell, spawn the telegraph, or apply effects (the sibling
        /// SpellSystem owns that), so every AI cast is counterable (telegraph window, §5.3). We
        /// invent no spell — only request a cast of one of the side's already-drafted 3.
        /// </summary>
        private void ApplySpellAxis(ref SystemState state, in SquadAIState squad,
                                    in SquadSignals sig, float spellAccessAxis)
        {
            float access = math.clamp(spellAccessAxis, 0f, 1.5f);
            if (access < k_SpellAccessCastThreshold)
                return; // low difficulty → AI rarely/never casts (fair handicap, §16).

            // Only cast when there is a worthwhile local situation: a cluster of enemies near the
            // anchor (good AoE/control target) OR the squad is in trouble (buff/retreat support).
            bool worthCasting = sig.EnemyNear >= 2 || sig.InHazard != 0 || sig.OwnStatueFrac < 0.5f;
            if (!worthCasting)
                return;

            // Find this side's drafted SpellSlot buffer (SpellLoadoutTag singleton-per-side).
            Entity loadoutEntity = Entity.Null;
            foreach (var (tag, le) in
                     SystemAPI.Query<RefRO<SpellLoadoutTag>>().WithEntityAccess())
            {
                if (tag.ValueRO.Team != squad.Team) continue;
                loadoutEntity = le;
                break;
            }
            if (loadoutEntity == Entity.Null || !state.EntityManager.HasBuffer<SpellSlot>(loadoutEntity))
                return;

            var slots = state.EntityManager.GetBuffer<SpellSlot>(loadoutEntity);

            // Pick the FIRST ready slot (off cooldown, charges left). The SpellSystem owns the
            // actual cooldown/charge decrement on cast; we read its state to avoid spamming a
            // request for a spell that isn't ready. O(≤3) — only 3 drafted spells (§13 2.4).
            int chosenSlot = -1;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].Cooldown <= 0f && slots[i].ChargesLeft > 0)
                {
                    chosenSlot = i;
                    break;
                }
            }
            if (chosenSlot < 0)
                return;

            // Find the shared spell-cast inbox singleton and append the request (data only).
            // Target the local enemy cluster: the nearest enemy's position is a sensible aim
            // point; the SpellSystem clamps/validates shape/radius from SpellDef.
            if (!SystemAPI.HasSingleton<SpellCastInboxTag>())
                return;
            Entity inboxEntity = SystemAPI.GetSingletonEntity<SpellCastInboxTag>();
            if (!state.EntityManager.HasBuffer<SpellCastRequest>(inboxEntity))
                return;

            float2 aim = sig.NearestEnemy != Entity.Null ? AnchorOfNearest(ref state, sig) : sig.Anchor;

            var inbox = state.EntityManager.GetBuffer<SpellCastRequest>(inboxEntity);
            inbox.Add(new SpellCastRequest
            {
                Team = squad.Team,
                SlotIndex = chosenSlot,
                TargetPos = aim,
                TargetEntity = sig.NearestEnemy, // SpellSystem decides if the spell uses entity vs pos.
            });
        }

        /// <summary>Resolve the nearest-enemy aim position (its current Position, else the anchor).</summary>
        private float2 AnchorOfNearest(ref SystemState state, in SquadSignals sig)
        {
            if (sig.NearestEnemy != Entity.Null && SystemAPI.HasComponent<Position>(sig.NearestEnemy))
                return SystemAPI.GetComponent<Position>(sig.NearestEnemy).Value;
            return sig.Anchor;
        }

        /// <summary>
        /// AGGRESSION axis → optionally REQUEST the commander's active (§13 2.5 "bones", §6). When
        /// the squad is advancing/focus-firing AND aggression is high AND the active is off
        /// cooldown, set CommanderRuntime.ActiveRequested. The sibling CommanderAbilitySystem
        /// owns the actual trigger, the ≤15% power clamp (§6), and the telegraph/effect — we ONLY
        /// flag the request (data), exactly like the player UI would.
        /// </summary>
        private void ApplyAggressionToCommanderActive(ref SystemState state, int team,
                                                       SquadPosture posture, float aggressionAxis)
        {
            bool pressing = posture == SquadPosture.Advance || posture == SquadPosture.FocusFire;
            if (!pressing || aggressionAxis < 1.1f)
                return;

            foreach (var crRef in SystemAPI.Query<RefRW<CommanderRuntime>>())
            {
                ref var cr = ref crRef.ValueRW;
                if (cr.Team != team) continue;
                if (cr.ActiveTimer > 0f) return;     // still on cooldown — CommanderAbilitySystem ticks it.
                cr.ActiveRequested = 1;              // flag only; the owning system triggers + clamps (§6).
                return;
            }
        }

        // ============================ CATALOG / GOLD HELPERS ============================

        // Uses SystemAPI.Query, so it takes <c>ref SystemState state</c>: the Entities source
        // generator needs the state handle to update query handles + complete dependencies (SGSG0002).
        private int GetTeamGold(ref SystemState state, int team)
        {
            foreach (var gs in SystemAPI.Query<RefRO<GoldStore>>())
            {
                if (gs.ValueRO.Team == team)
                    return gs.ValueRO.Amount;
            }
            return 0;
        }

        private static int ProjectedUnpaidCost(DynamicBuffer<TrainOrder> orders,
                                               DynamicBuffer<UnitSpawnStats> catalog)
        {
            int total = 0;
            for (int i = 0; i < orders.Length; i++)
            {
                if (orders[i].Paid != 0) continue;
                int idx = orders[i].UnitIndex;
                if (idx >= 0 && idx < catalog.Length)
                    total += catalog[idx].GoldCost;
            }
            return total;
        }

        // PHASE-2 TWO-FACTION: resolve THIS side's per-team catalog (Iron Pact vs Ashen Horde).
        private bool TryGetCatalog(ref SystemState state, int team, out DynamicBuffer<UnitSpawnStats> catalog)
        {
            catalog = default;
            if (!UnitCatalog.TryGetForTeam(state.EntityManager, team, out var catEntity))
                return false;
            if (!state.EntityManager.HasBuffer<UnitSpawnStats>(catEntity))
                return false;
            catalog = state.EntityManager.GetBuffer<UnitSpawnStats>(catEntity);
            return catalog.Length > 0;
        }

        /// <summary>Linear scan of the tiny catalog for the first index of a role (data-driven).</summary>
        private static int FindRoleIndex(DynamicBuffer<UnitSpawnStats> catalog, RoleId role)
        {
            for (int i = 0; i < catalog.Length; i++)
                if (catalog[i].Role == role)
                    return i;
            return -1;
        }
    }
}

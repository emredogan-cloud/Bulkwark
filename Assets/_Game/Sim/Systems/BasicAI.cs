// BULWARK — Phase-1 BASIC AI (roadmap §13 P1.5; §4 layered-but-O(1) AI; §5.1 the
// recovered reactive-threshold commander, modernized to a single-layer UTILITY pick).
//
// SCOPE (Phase 1, FUN GATE): drive the AIControlled side (Team 1) so it provides a
// FAIR fight — NOT optimal play. There is exactly ONE decision layer here:
//   * COMMANDER layer — on AICommander.ReevalCooldown (~1s) pick a CommanderStance
//     from a tiny utility score over cheap signals, then bias TRAINING for Team 1.
//   * UNIT layer — there is NO bespoke unit AI: AI units reuse the SAME shared
//     TargetingSystem + Combat/Movement as the player (they carry the standard
//     components via the Training spawn path). This file authors NO combat/targeting.
//
// CANON GUARDS (roadmap §15 + Phase-1 prompt §J):
//   * Single-layer utility only. The squad layer + budgeted scheduler + multi-axis
//     DIFFICULTY are Phase 2.6 — NOT here. We read Difficulty.Multiplier=1 only.
//   * No new mechanics/units/currencies. Only the 4 authorized P1 roles
//     (Miner, Frontline, Skirmisher, Ranged) resolved from the data catalog.
//   * NO hardcoded balance: gold costs / train times come from UnitSpawnStats (data).
//     The numbers below (target miner count, queue length, stance weights) are AI
//     PACING knobs, not unit/combat balance — they tune "fairness", not power, and
//     introduce no new currency/mechanic. They are kept conservative + symmetric.
//     They are telemetry-tunable (§16): LSD/telemetry may retune these fairness
//     coefficients; left as code consts for P1 per the canon-review allowance.
//   * O(1)/unit perf rule (§12): signal-gathering runs ONLY on a reeval tick (~1s),
//     not every frame, and is a single linear pass with no all-pairs (N^2) scan.
//
// SCAFFOLD STATUS: authored, NOT compiled — no Unity 6 / Entities toolchain here
// (ADR-0-001). Runtime behavior is DEFERRED to a Unity dev (see deferredValidations).

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Bulwark.Sim
{
    /// <summary>
    /// Phase-1 commander AI for the AIControlled side (Team 1). Single-layer utility:
    /// on its reeval cadence it scores three stances over a few cheap battle signals,
    /// commits the best one, and biases the Team-1 TrainQueue toward a fair, sensible
    /// 4-unit composition (economy first, then a frontline/skirmisher/ranged mix).
    ///
    /// It deliberately does NOT micro units, pre-deduct Gold, or run any per-unit
    /// combat logic — TargetingSystem + CombatSystem (shared) own that for ALL teams.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SimPhaseBegin))]
    [UpdateBefore(typeof(SimPhaseEnd))]
    [UpdateAfter(typeof(TargetingSystem))] // §13 1.5: commander builds on the shared targeting the AI's units use.
    [UpdateBefore(typeof(MovementSystem))] // stance/training bias settles before units move this tick.
    public partial struct BasicAISystem : ISystem
    {
        // ---- AI PACING knobs (fairness tuning, NOT unit/combat balance) ----------------
        // These are AI heuristic/pacing coefficients (cadence, economy floor, queue cap,
        // stickiness) — NOT unit/combat balance (damage/health/cost/range/speed, which
        // are read from the data catalog). They tune "fairness", not power, and are
        // telemetry-tunable (§16): kept as code consts for P1 per the canon-review
        // allowance. Switching them to a baked AIConfig singleton later would let LSD
        // retune fairness without recompiling.
        //
        // Reeval cadence: ~1s between commander decisions (§4 "adaptive yet O(1)").
        private const float ReevalPeriod = 1.0f;
        // Economy floor: keep a small, fixed miner count so the AI funds a fair fight
        // (miners first). Conservative — does not out-economy a reasonable player.
        private const int   TargetMiners = 3;
        // Back-pressure: never let the AI hoard a runaway queue (avoids "overwhelm").
        private const int   MaxQueueLength = 5;
        // Gold safety buffer: only enqueue a combat unit if the side can plausibly pay
        // for everything already queued PLUS this one (TrainingSystem still does the
        // authoritative deduction at the queue head; this just paces, no pre-spend).
        private const int   GoldHeadroom = 0;

        // Stickiness so the commander doesn't thrash stance every tick under noise.
        private const float StanceStickiness = 0.10f;

        public void OnCreate(ref SystemState state)
        {
            // Only run when an AI commander exists; bootstrap creates one for Team 1.
            state.RequireForUpdate<AICommander>();
            state.RequireForUpdate<UnitCatalogTag>();
            state.RequireForUpdate<MatchState>();
        }

        // NOTE: not [BurstCompile] — this system performs structural-free buffer
        // appends and state.EntityManager.HasComponent/HasBuffer lookups that are fine
        // unburst-ed at P1 (it runs at most once/sec). Combat/Targeting are the hot
        // path and ARE Burst'd in their own files.
        public void OnUpdate(ref SystemState state)
        {
            // Freeze once the match is decided (MatchFlow owns the final outcome).
            var match = SystemAPI.GetSingleton<MatchState>();
            if (match.Outcome != MatchOutcome.Ongoing)
                return;

            float dt = SystemAPI.Time.DeltaTime;

            // ---- Commander reeval gate: tick down each AI commander's cooldown -------
            foreach (var commander in SystemAPI.Query<RefRW<AICommander>>())
            {
                ref var cmd = ref commander.ValueRW;
                cmd.ReevalCooldown -= dt;
                if (cmd.ReevalCooldown > 0f)
                    continue;
                cmd.ReevalCooldown = ReevalPeriod;

                int team = cmd.Team;

                // ---- Gather cheap signals (single linear pass; reeval tick only) -----
                var sig = GatherSignals(ref state, team);

                // ---- Single-layer utility: score the 3 stances, pick the best -------
                cmd.Stance = PickStance(in sig, cmd.Stance);

                // ---- Bias training for this side under the chosen stance ------------
                BiasTraining(ref state, team, cmd.Stance, in sig);
            }
        }

        // ============================ SIGNAL GATHERING ============================

        private struct Signals
        {
            public int Gold;
            public int OwnUnits;       // own combat+miner units alive
            public int OwnMiners;      // own miners alive
            public int EnemyUnits;     // enemy units alive
            public int QueueLength;    // pending TrainOrders for this side
            public int QueuedCost;     // projected Gold cost of unpaid pending orders
            public float OwnStatueFrac;   // own statue Health / MaxHealth   (1 = full)
            public float EnemyStatueFrac; // enemy statue Health / MaxHealth (1 = full)
        }

        /// <summary>
        /// One linear pass over the small Phase-1 entity sets to read a handful of
        /// O(1)-per-entity counts. No all-pairs scan; runs only on a reeval tick.
        /// </summary>
        private Signals GatherSignals(ref SystemState state, int team)
        {
            var sig = new Signals
            {
                Gold = 0,
                OwnUnits = 0,
                OwnMiners = 0,
                EnemyUnits = 0,
                QueueLength = 0,
                QueuedCost = 0,
                OwnStatueFrac = 1f,
                EnemyStatueFrac = 1f
            };

            // Gold for this side.
            foreach (var gs in SystemAPI.Query<RefRO<GoldStore>>())
            {
                if (gs.ValueRO.Team == team) { sig.Gold = gs.ValueRO.Amount; break; }
            }

            // Unit + miner counts (own vs enemy). Plain counters — cheap, no pairs.
            foreach (var (unitTeam, e) in
                     SystemAPI.Query<RefRO<Team>>().WithAll<UnitTag>().WithEntityAccess())
            {
                if (unitTeam.ValueRO.Id == team)
                {
                    sig.OwnUnits++;
                    if (state.EntityManager.HasComponent<MinerTag>(e))
                        sig.OwnMiners++;
                }
                else
                {
                    sig.EnemyUnits++;
                }
            }

            // Statue health fractions (own + enemy).
            foreach (var (stag, sstate) in
                     SystemAPI.Query<RefRO<StatueTag>, RefRO<StatueState>>())
            {
                float max = sstate.ValueRO.MaxHealth;
                float frac = max > 0f ? sstate.ValueRO.Health / max : 1f;
                if (stag.ValueRO.Team == team) sig.OwnStatueFrac = frac;
                else sig.EnemyStatueFrac = frac;
            }

            // Pending queue length + projected unpaid cost for this side.
            foreach (var (q, e) in
                     SystemAPI.Query<RefRO<TrainQueueTag>>().WithEntityAccess())
            {
                if (q.ValueRO.Team != team) continue;
                if (!state.EntityManager.HasBuffer<TrainOrder>(e)) continue;

                var orders = state.EntityManager.GetBuffer<TrainOrder>(e);
                sig.QueueLength = orders.Length;
                sig.QueuedCost = ProjectedUnpaidCost(ref state, team, orders);
                break;
            }

            return sig;
        }

        /// <summary>
        /// Sum the data-driven Gold cost of orders not yet paid for, so pacing can
        /// avoid queueing more than the side can plausibly fund. Costs come from the
        /// data catalog (UnitSpawnStats.GoldCost) — never hardcoded.
        /// </summary>
        private int ProjectedUnpaidCost(ref SystemState state, int team, DynamicBuffer<TrainOrder> orders)
        {
            if (!TryGetCatalog(ref state, team, out var catalog))
                return 0;

            int total = 0;
            for (int i = 0; i < orders.Length; i++)
            {
                if (orders[i].Paid != 0) continue;           // already deducted
                int idx = orders[i].UnitIndex;
                if (idx >= 0 && idx < catalog.Length)
                    total += catalog[idx].GoldCost;
            }
            return total;
        }

        // ============================ STANCE UTILITY ============================

        /// <summary>
        /// Single-layer utility: three small scores from cheap signals, max wins,
        /// with light stickiness to the current stance to avoid per-tick thrash.
        /// This is intentionally a "handful of comparisons" (§4 O(1) commander), NOT
        /// GOAP/ML (§15.10 over-engineering prohibition).
        /// </summary>
        private CommanderStance PickStance(in Signals sig, CommanderStance current)
        {
            // Relative army strength (counts only — readability over precision at P1).
            // +1 smoothing so an empty board doesn't divide by zero.
            float armyRatio = (sig.OwnUnits + 1f) / (sig.EnemyUnits + 1f);

            // DEFEND: own statue hurt OR we're outnumbered → consolidate.
            float defend =
                  (1f - sig.OwnStatueFrac) * 1.5f          // the more hurt, the more defensive
                + (armyRatio < 1f ? (1f - armyRatio) : 0f); // outnumbered → defensive

            // PUSH: we have a real army edge AND enemy statue is reachable progress.
            float push =
                  (armyRatio > 1f ? (armyRatio - 1f) : 0f) * 1.0f
                + (1f - sig.EnemyStatueFrac) * 0.5f         // already chipping → keep pushing
                + sig.OwnStatueFrac * 0.25f;                // safe at home → free to commit

            // HARASS: rough parity / early game → poke and pressure economy.
            float harass =
                  (armyRatio >= 0.75f && armyRatio <= 1.5f ? 0.75f : 0f)
                + (sig.OwnStatueFrac > 0.66f && sig.EnemyStatueFrac > 0.66f ? 0.5f : 0f);

            // Stickiness bonus to whatever we're already doing.
            switch (current)
            {
                case CommanderStance.Defend: defend += StanceStickiness; break;
                case CommanderStance.Push:   push   += StanceStickiness; break;
                case CommanderStance.Harass: harass += StanceStickiness; break;
            }

            // Argmax (Defend default on ties → fair, never hyper-aggressive).
            CommanderStance best = CommanderStance.Defend;
            float bestScore = defend;
            if (push > bestScore)   { bestScore = push;   best = CommanderStance.Push; }
            if (harass > bestScore) { bestScore = harass; best = CommanderStance.Harass; }
            return best;
        }

        // ============================ TRAINING BIAS ============================

        /// <summary>
        /// Translate the chosen stance into a FAIR build order: keep a small miner
        /// economy first, then maintain a sensible frontline/skirmisher/ranged mix,
        /// stance-weighted. Paces by Gold (projected) + a hard queue cap so the AI
        /// never floods. Enqueues by appending a TrainOrder to this side's queue —
        /// TrainingSystem performs the authoritative Gold deduction at the head.
        /// </summary>
        private void BiasTraining(ref SystemState state, int team, CommanderStance stance, in Signals sig)
        {
            // Hard back-pressure: a full queue means "wait".
            if (sig.QueueLength >= MaxQueueLength)
                return;

            if (!TryGetCatalog(ref state, team, out var catalog))
                return;

            // Resolve the role -> catalog-index mapping at runtime (data-driven).
            // Role comparison uses the sim-side RoleId enum carried by UnitSpawnStats,
            // so which catalog index is which role is discovered at runtime, never
            // hardcoded to a fixed slot (HARD RULE 4).
            int idxMiner      = FindRoleIndex(catalog, RoleId.Miner);
            int idxFrontline  = FindRoleIndex(catalog, RoleId.Frontline);
            int idxSkirmisher = FindRoleIndex(catalog, RoleId.Skirmisher);
            int idxRanged     = FindRoleIndex(catalog, RoleId.Ranged);

            // ---- Economy first: top up miners up to the fair floor -----------------
            if (sig.OwnMiners < TargetMiners && idxMiner >= 0)
            {
                TryEnqueue(ref state, team, idxMiner, in sig, catalog);
                return; // one decision per reeval tick keeps pacing gentle
            }

            // ---- Combat mix under the chosen stance --------------------------------
            // Pick ONE combat role to add this tick, biased by stance. We rotate by a
            // cheap signal (own unit count) so the build self-balances over time
            // without storing per-role tallies — a fair, legible composition.
            int pick = ChooseCombatRole(stance, sig.OwnUnits, idxFrontline, idxSkirmisher, idxRanged);
            if (pick >= 0)
                TryEnqueue(ref state, team, pick, in sig, catalog);
        }

        /// <summary>
        /// Stance-weighted choice of a single combat role to queue. Rotation by a
        /// counter (own unit count) yields a steady frontline/skirmisher/ranged blend
        /// rather than spamming one unit — fair, not optimal.
        /// </summary>
        private int ChooseCombatRole(CommanderStance stance, int rotation,
                                     int idxFrontline, int idxSkirmisher, int idxRanged)
        {
            // Stance lightly reorders preference; all three remain in the mix so the
            // composition is always sensible and counter-able by the player.
            //   Defend  -> lead with Frontline (hold the line), then ranged, skirmisher.
            //   Push    -> lead with Skirmisher (close + commit), then frontline, ranged.
            //   Harass  -> lead with Ranged (poke), then skirmisher, frontline.
            int a, b, c;
            switch (stance)
            {
                case CommanderStance.Push:
                    a = idxSkirmisher; b = idxFrontline; c = idxRanged; break;
                case CommanderStance.Harass:
                    a = idxRanged; b = idxSkirmisher; c = idxFrontline; break;
                default: // Defend
                    a = idxFrontline; b = idxRanged; c = idxSkirmisher; break;
            }

            // Build the ordered candidate list, skipping roles absent from the catalog.
            // rotation % 3 spreads picks across the three preferred slots over time.
            int slot = ((rotation % 3) + 3) % 3;
            int first = slot == 0 ? a : (slot == 1 ? b : c);
            if (first >= 0) return first;
            if (a >= 0) return a;
            if (b >= 0) return b;
            if (c >= 0) return c;
            return -1;
        }

        /// <summary>
        /// Append a TrainOrder for this side IF projected unpaid cost (+ this unit's
        /// data cost) fits within available Gold (+ headroom). No pre-deduction:
        /// TrainingSystem deducts at the head when Paid==0 (HARD RULE: server/sim owns
        /// the spend; AI only paces). Returns nothing — best-effort, fair pacing.
        /// </summary>
        private void TryEnqueue(ref SystemState state, int team, int unitIndex,
                                in Signals sig, DynamicBuffer<UnitSpawnStats> catalog)
        {
            if (unitIndex < 0 || unitIndex >= catalog.Length)
                return;

            int cost = catalog[unitIndex].GoldCost;
            // Affordability gate (projected): don't queue what we can't plausibly fund.
            if (sig.QueuedCost + cost > sig.Gold + GoldHeadroom)
                return;

            float trainSeconds = catalog[unitIndex].TrainSeconds;

            // Find this side's queue entity + buffer and append.
            foreach (var (q, e) in
                     SystemAPI.Query<RefRO<TrainQueueTag>>().WithEntityAccess())
            {
                if (q.ValueRO.Team != team) continue;
                if (!state.EntityManager.HasBuffer<TrainOrder>(e)) continue;

                var orders = state.EntityManager.GetBuffer<TrainOrder>(e);
                orders.Add(new TrainOrder
                {
                    UnitIndex = unitIndex,
                    Remaining = trainSeconds, // data-driven (UnitSpawnStats), not hardcoded
                    Paid = 0                  // TrainingSystem deducts Gold at the head
                });
                return;
            }
        }

        // ============================ CATALOG HELPERS ============================

        // PHASE-2 TWO-FACTION: resolve THIS side's per-team catalog (Iron Pact vs Ashen Horde)
        // rather than a global singleton — TrainOrder.UnitIndex is side-relative. The AI side's
        // build-bias must read its own faction's roster (role indices differ per side).
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

        /// <summary>
        /// Linear scan of the (tiny, ==4) catalog for the first index of a role.
        /// Compares against the sibling-authored UnitSpawnStats.Role (Bulwark.Sim.RoleId
        /// enum) directly — no Bulwark.Data reference, no int-mirroring.
        /// </summary>
        private static int FindRoleIndex(DynamicBuffer<UnitSpawnStats> catalog, RoleId role)
        {
            for (int i = 0; i < catalog.Length; i++)
                if (catalog[i].Role == role)
                    return i;
            return -1;
        }
    }
}

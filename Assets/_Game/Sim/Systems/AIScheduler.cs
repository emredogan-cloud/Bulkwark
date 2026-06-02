// BULWARK — Phase 2.6 BUDGETED AI SCHEDULER (roadmap §13 2.6 "budgeted scheduler";
// §4 "adaptive yet O(1)" layered AI; §12 perf rule: AI/targeting ~O(1) per agent, the
// AI scheduler is budgeted so AI cost is FRAME-STABLE under load).
//
// WHAT THIS IS
// A single, tiny ISystem that owns the AISchedulerState{Cursor,BudgetPerFrame} singleton
// and advances a ROUND-ROBIN cursor over the AI's tactical agents (the SquadAIState set,
// §13 2.6). It does NOT itself touch squads or combat — it only moves the cursor and
// publishes the per-frame "thinking window" so the SquadAISystem (next in order) can ask,
// O(1) per agent, "may I re-evaluate this frame?".
//
// WHY A SHARED CURSOR (not a per-agent timer)
//   • §4/§12: the cost of AI MUST be bounded per frame regardless of how many squads exist.
//     A per-agent ReevalCooldown alone can let many agents re-eval on the SAME frame (a
//     spike). The scheduler caps re-evals to BudgetPerFrame agents/frame and rotates which
//     ones via Cursor, so the worst-case per-frame AI work is constant (BudgetPerFrame),
//     not O(agents). SquadAIState.ReevalCooldown still gates an agent's OWN cadence; the
//     scheduler gates HOW MANY may think at once (the two compose — see SquadAI.cs).
//   • The window is expressed as a half-open ordinal range [start, start+budget) over the
//     stable enumeration order of the SquadAIState query. Both this system and SquadAISystem
//     enumerate that SAME query (same archetype set, same frame) so a running ordinal is a
//     stable per-agent index they agree on WITHOUT storing an extra index field on the
//     component (we must not redefine SquadAIState — it is the shared Phase-2 contract).
//
// CANON DISCIPLINE
//   • §12 ECS boundary: pure battle sim (ISystem). No UI/meta. No structural changes.
//   • §15.6 NO invented balance: BudgetPerFrame is an AI PACING knob (frame-stability), not
//     unit/combat balance — it changes no damage/health/cost. It is DATA: read from the
//     AISchedulerState singleton (baked by Bootstrap), with only a safe positive fallback if
//     a non-positive budget was baked. Difficulty axes live in their own DifficultyAxes
//     singleton (consumed by SquadAI), not here.
//   • Forbidden now (Phase 3+/7): no determinism/replays guarantees, no new mechanics. The
//     round-robin is deterministic-in-ORDER only (frame stability), not numeric determinism.
//   • §15.10 no over-engineering: this is a counter + modulo, not a job scheduler framework.
//
// ORDERING (SimSystemOrder.cs convention): pinned between the Phase-1 anchors, AFTER the
// §13 1.5 BasicAISystem (the commander stance the squad layer reads, §13 2.6 "ABOVE
// BasicAI") and BEFORE MovementSystem (so squad postures bias movement the same tick).
// SquadAISystem pins itself [UpdateAfter(typeof(AISchedulerSystem))] so the cursor is fresh
// before any agent asks "may I think?".
//
// SCAFFOLD STATUS: authored, NOT compiled (no Unity 6 / Entities here — ADR-0-001/-002/-1-001).
// Runtime scheduling/frame-stability behavior is DEFERRED to a Unity dev (deferredValidations).

using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Bulwark.Sim
{
    /// <summary>
    /// Budgeted round-robin scheduler for the AI tactical layer (§13 2.6 / §4 / §12).
    /// Each frame it advances <see cref="AISchedulerState.Cursor"/> by
    /// <see cref="AISchedulerState.BudgetPerFrame"/> agents, wrapping over the count of
    /// <see cref="SquadAIState"/> agents, so over consecutive frames every agent is permitted
    /// to re-evaluate in turn while at most BudgetPerFrame do so on any single frame.
    /// Owns ONLY the cursor; the actual per-agent decision is in <see cref="SquadAISystem"/>.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SimPhaseBegin))]
    [UpdateBefore(typeof(SimPhaseEnd))]
    [UpdateAfter(typeof(BasicAISystem))]   // §13 2.6: squad layer is ABOVE the §13 1.5 commander stance.
    [UpdateBefore(typeof(MovementSystem))] // posture/anchor must be set before units move this tick.
    public partial struct AISchedulerSystem : ISystem
    {
        // Safe fallback budget if Bootstrap baked a non-positive value (frame-stability would
        // otherwise be undefined). This is a PACING floor, not balance — at least one agent
        // re-evals per frame so the AI never fully stalls. Telemetry-tunable (§16).
        private const int k_MinBudget = 1;

        public void OnCreate(ref SystemState state)
        {
            // Run only once the scheduler singleton exists (Bootstrap bakes it) AND while the
            // match is live (MatchFlow freezes the work systems on a decided match).
            state.RequireForUpdate<AISchedulerState>();
            state.RequireForUpdate<MatchState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Freeze with the rest of the sim once the match is decided (MatchFlow owns outcome).
            if (SystemAPI.GetSingleton<MatchState>().Outcome != MatchOutcome.Ongoing)
                return;

            // Count the tactical agents (SquadAIState) — a single O(agents) tally, no all-pairs
            // (§12). This is the round-robin modulus; with zero agents there is nothing to gate.
            int agentCount = 0;
            foreach (var _ in SystemAPI.Query<RefRO<SquadAIState>>())
                agentCount++;
            if (agentCount == 0)
                return;

            ref var sched = ref SystemAPI.GetSingletonRW<AISchedulerState>().ValueRW;
            int budget = sched.BudgetPerFrame > 0 ? sched.BudgetPerFrame : k_MinBudget;

            // The window this frame is the half-open ordinal range [Cursor, Cursor+budget)
            // wrapped over agentCount. Normalise the cursor into [0, agentCount) so the window
            // SquadAI evaluates this frame matches what we advance past for next frame.
            int start = ((sched.Cursor % agentCount) + agentCount) % agentCount;
            sched.Cursor = start;

            // Advance for next frame, capping the step at agentCount so a budget ≥ agentCount
            // (everyone re-evals every frame) wraps cleanly to the same start rather than
            // skipping agents. Round-robin fairness: each agent gets a turn within
            // ceil(agentCount / budget) frames.
            int step = math.min(budget, agentCount);
            sched.Cursor = (start + step) % agentCount;
        }

        // ============================ SHARED GATE HELPER ============================
        // SquadAISystem (which runs AFTER this system) calls this O(1) per agent to ask
        // "may I re-evaluate this frame?". It reads the AISchedulerState singleton the
        // scheduler just published and reconstructs the SAME half-open window that was in
        // effect this frame. The published Cursor is the NEXT frame's start =
        // (start + step) % agentCount with step = min(budget, agentCount), so this frame's
        // window is the 'step' ordinals ENDING at the published Cursor:
        //   start = (publishedCursor - step) mod agentCount,  window = [start, publishedCursor).
        // Callers pass the published cursor + budget + agentCount; no extra component field is
        // needed (we must not redefine the shared SquadAIState). See SquadAISystem.OnUpdate.

        /// <summary>
        /// True if the tactical agent with stable <paramref name="ordinal"/> was inside THIS
        /// frame's permitted round-robin window, given the post-advance scheduler
        /// <paramref name="publishedCursor"/>, the (data) <paramref name="budget"/>, and the
        /// <paramref name="agentCount"/> tactical agents. Half-open, wrapping. O(1).
        /// </summary>
        public static bool MayThink(int ordinal, int publishedCursor, int budget, int agentCount)
        {
            if (agentCount <= 0) return false;
            if (budget <= 0) budget = 1;
            int step = math.min(budget, agentCount);
            if (step >= agentCount) return true; // whole board re-evals this frame.

            int cursor = ((publishedCursor % agentCount) + agentCount) % agentCount;
            int start = ((cursor - step) % agentCount + agentCount) % agentCount;
            int o = ((ordinal % agentCount) + agentCount) % agentCount;

            // Forward (wrapping) distance from the window start to this ordinal; inside if < step.
            int forward = o - start;
            if (forward < 0) forward += agentCount;
            return forward < step;
        }
    }
}

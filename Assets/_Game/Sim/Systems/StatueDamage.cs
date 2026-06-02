// BULWARK — Phase 1.1 OBJECTIVE: StatueDamageSystem (battle-sim hot path).
// Roadmap trace:
//  • §13 P1.1 "Economy + objective": the Statue with readable DAMAGE STATES — the
//    SECOND half of the Phase-1 core loop ("...→ push → topple the statue", §3).
//  • §11 "Statue": the iconic win/lose objective; armored, with a SHIELD PHASE and
//    clear damage states (intact→cracked→breaking→destroyed) for a readable climax;
//    "throttles trickle damage (recovered statue math)".
//  • §11 "Readability (INVIOLABLE)": damage states must be clearly separable; the
//    phase is recomputed EVERY tick from the Health fraction so the visual read is
//    deterministic w.r.t. health, not w.r.t. hit timing — and stays correct even if a
//    sibling system mutates StatueState.Health outside the inbox (e.g. a future heal).
//  • §12 ECS boundary: pure sim, no UI. MatchState (singleton) is the sim-side
//    win/lose signal the MonoBehaviour shell reads (§12: sim writes data, UI reads it).
//  • §15.6 no invented balance: damage VALUES arrive via the StatueDamageInbox (written
//    by the Combat system); MaxHealth / ShieldHealth / TrickleThrottle come from DATA.
//    The phase fraction thresholds below are PROVISIONAL, LSD/ADR-owned placeholders
//    (flagged, not asserted canon) — they are the ones named in the module brief.
//    MIGRATION TRACKED: move k_IntactAbove / k_CrackedAbove to a ScriptableObject /
//    BalanceConfig field before they are asserted as canon (§15.6); no change required
//    for the Phase-1.1 scaffold.
//
// SCAFFOLD STATUS: authored, NOT compiled — no Unity 6 / Entities toolchain here
// (ADR-0-001/-002). Runtime behavior is DEFERRED to a Unity build.

using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Bulwark.Sim
{
    /// <summary>
    /// Per-statue inbox of incoming damage for THIS tick. The Combat system appends
    /// {Amount} elements (it does not mutate StatueState directly), so all statue
    /// damage is resolved in ONE place with consistent shield/throttle/phase rules.
    /// Declared in EXACTLY ONE compilation unit (here); also named in the shared
    /// contract — sibling modules MUST reuse this type, not redeclare it.
    /// </summary>
    public struct StatueDamageInbox : IBufferElementData { public float Amount; }

    /// <summary>
    /// Resolves all statue damage for the tick: sums the inbox, routes it through the
    /// SHIELD phase (if active) then Health, applies the recovered-statue TRICKLE
    /// THROTTLE so tiny continuous damage lands in readable discrete chunks, recomputes
    /// the readable StatuePhase EVERY tick from the Health fraction, and raises the match
    /// Outcome when a statue is destroyed.
    /// </summary>
    [BurstCompile]
    public partial struct StatueDamageSystem : ISystem
    {
        // PROVISIONAL readable-phase thresholds (fraction of MaxHealth). LSD/ADR-owned
        // per §15.6 — named in the Phase-1.1 brief, NOT asserted canon. Centralized here
        // so a designer move is a one-line edit (and later, data-driven via SO — migration
        // tracked in the file header).
        private const float k_IntactAbove  = 0.66f; // > → Intact
        private const float k_CrackedAbove = 0.33f; // > → Cracked, else Breaking (>0), else Destroyed

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Match outcome singleton must exist (created at bootstrap). If absent, there
            // is nothing to signal; DEFERRED validation in a Unity build.
            bool hasMatch = SystemAPI.HasSingleton<MatchState>();
            MatchState match = hasMatch ? SystemAPI.GetSingleton<MatchState>() : default;
            bool matchDirty = false;

            foreach (var (statueTag, statue, inbox) in
                     SystemAPI.Query<RefRO<StatueTag>, RefRW<StatueState>, DynamicBuffer<StatueDamageInbox>>())
            {
                ref var s = ref statue.ValueRW;

                // Already destroyed: drain the inbox and skip (idempotent, no re-trigger).
                if (s.Phase == StatuePhase.Destroyed)
                {
                    inbox.Clear();
                    continue;
                }

                // 1) SUM this tick's incoming damage.
                float incoming = 0f;
                for (int i = 0; i < inbox.Length; i++)
                    incoming += math.max(0f, inbox[i].Amount);
                inbox.Clear(); // consume the inbox regardless of how it resolves

                // 2) TRICKLE THROTTLE (recovered statue math, §11): accumulate damage and
                //    only RELEASE it once the running total crosses TrickleThrottle. This
                //    converts a stream of tiny hits into readable discrete chunks so the
                //    damage-state read does not flicker. A throttle of 0 disables it
                //    (every hit applies immediately). On a sub-threshold tick we apply
                //    nothing this tick, but we still fall through to the unconditional
                //    phase recompute (4) so the readable state stays consistent with Health.
                float applied = 0f;
                if (incoming > 0f)
                {
                    if (s.TrickleThrottle > 0f)
                    {
                        s.TrickleAccum += incoming;
                        if (s.TrickleAccum >= s.TrickleThrottle)
                        {
                            applied = s.TrickleAccum;     // release the whole accumulated chunk
                            s.TrickleAccum = 0f;
                        }
                        // else: below threshold — hold; applied stays 0 this tick.
                    }
                    else
                    {
                        applied = incoming;
                    }
                }

                // 3) SHIELD PHASE absorbs first (if active), then Health takes the rest.
                if (applied > 0f)
                {
                    if (s.ShieldActive && s.ShieldHealth > 0f)
                    {
                        float absorbed = math.min(s.ShieldHealth, applied);
                        s.ShieldHealth -= absorbed;
                        applied -= absorbed;
                        if (s.ShieldHealth <= 0f)
                        {
                            s.ShieldHealth = 0f;
                            s.ShieldActive = false;       // shield phase ends — readable transition
                        }
                    }
                    if (applied > 0f)
                        s.Health -= applied;
                }

                // 4) RECOMPUTE the readable damage state from the Health fraction —
                //    UNCONDITIONALLY, every tick (§11 readability + the doc's "every tick"
                //    contract). Cheap, and guards against any external Health mutation (e.g.
                //    a future heal / "recovered statue") so Phase never goes stale.
                s.Phase = ComputePhase(s.Health, s.MaxHealth);

                // 5) DESTROYED → raise the match outcome (§13 P1.1 win/lose).
                //    Team-0 (player) statue down → Defeat; Team-1 (AI) statue down → Victory.
                if (s.Phase == StatuePhase.Destroyed && hasMatch && match.Outcome == MatchOutcome.Ongoing)
                {
                    match.Outcome = statueTag.ValueRO.Team == 0
                        ? MatchOutcome.Defeat
                        : MatchOutcome.Victory;
                    matchDirty = true;
                }
            }

            if (matchDirty)
                SystemAPI.SetSingleton(match);
        }

        /// <summary>
        /// Maps a Health fraction to the readable StatuePhase. PROVISIONAL thresholds
        /// (§15.6, LSD/ADR-owned). MaxHealth &lt;= 0 is treated as already Destroyed.
        /// </summary>
        private static StatuePhase ComputePhase(float health, float maxHealth)
        {
            if (health <= 0f) return StatuePhase.Destroyed;
            if (maxHealth <= 0f) return StatuePhase.Destroyed;

            float frac = health / maxHealth;
            if (frac > k_IntactAbove)  return StatuePhase.Intact;
            if (frac > k_CrackedAbove) return StatuePhase.Cracked;
            return StatuePhase.Breaking; // (0, 0.33] — health>0 guaranteed above
        }
    }
}

// BULWARK — Phase-1 MATCH FLOW (assembly capstone). Roadmap §13 Phase 1 (GATE 1 micro-battle),
// §11 (Statue = win/lose objective with readable damage states), §3 (preserved
// mine→train→push→statue loop), §12 (ECS battle-sim boundary; no UI/meta logic here).
//
// RESPONSIBILITY
//  • OWN the MatchState singleton (create it if the Bootstrap somehow didn't).
//  • RESOLVE the win/lose condition from the two StatueState objectives:
//      - The PLAYER's statue (Team 0) destroyed  → Defeat.
//      - The AI's statue     (Team 1) destroyed  → Victory.
//    If StatueDamageSystem already set Outcome (it owns the Health<=0 edge), MatchFlow
//    FINALIZES/FREEZES that result rather than re-deriving it — first decisive result wins.
//  • Once Outcome != Ongoing, FREEZE the sim: disable every other Phase-1 sim system so no
//    further mining/training/targeting/AI/combat work runs after the match is decided. This
//    is the single authority that stops the tick at the gate (the micro-battle ends cleanly).
//
// NON-RESPONSIBILITY (canon discipline)
//  • No scoring/meta/economy persistence, no progression, no replays — all later-phase /
//    forbidden now (§13, §15.2/§15.3). Outcome is the ONLY thing this writes.
//  • No save-state logging (§4/§12 inviolable): this never serializes or logs match/save state.
//  • No balance values: it reads StatueState.Health (data-seeded at spawn), invents none (§15.6).
//
// ORDERING (see SimSystemOrder.cs)
//  Runs LAST among Phase-1 sim systems: after StatueDamageSystem (which may set Outcome on the
//  killing blow) and before the SimPhaseEnd anchor.
//
// FREEZE-SET SINGLE SOURCE OF TRUTH (DRY, §15)
//  The systems to silence are NOT re-listed here. They come from SimSystemOrder.WorkSystems
//  (the same canonical array the documented order uses) plus SimSystemOrder.RetiredPhase0Systems
//  (the superseded P0 spike, defensively). So the documented order, the marker class, and this
//  freeze set are ONE list — they cannot diverge. Adding BasicAISystem to WorkSystems (§13 1.5)
//  automatically makes the decided match freeze the AI commander too.
//
// MATCHING: full simple-type-name equality with word boundaries against the (possibly
//  decorated) SystemState.DebugName — NOT a substring Contains. A target only matches a
//  system whose undecorated type name equals it exactly, so e.g. "AttackSystem" can never
//  silence some future "FooAttackSystemBar".
//
// SCAFFOLD STATUS: authored, NOT compiled (no Unity 6 / Entities here — ADR-0-001/-002).
// DEFERRED (no runtime): end-to-end playthrough to Victory/Defeat, GATE-1 fun verdict.

using Unity.Collections;
using Unity.Entities;

namespace Bulwark.Sim
{
    /// <summary>
    /// Finalizes the match and freezes the sim once an outcome is reached.
    /// Ordered last in the Phase-1 tick: after StatueDamageSystem, before SimPhaseEnd.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(StatueDamageSystem))]
    [UpdateBefore(typeof(SimPhaseEnd))]
    public partial struct MatchFlowSystem : ISystem
    {
        // Player = Team 0 (PlayerControlled); AI = Team 1 (AIControlled). Per the shared contract.
        private const int PlayerTeam = 0;
        private const int AiTeam = 1;

        // Freeze targets, built ONCE in OnCreate from the single canonical order in
        // SimSystemOrder (WorkSystems + RetiredPhase0Systems). Stored as FixedStrings so the
        // freeze pass touches no managed string on the (rare) decisive tick. This list is
        // generated, never hand-maintained, so it cannot drift from the documented order.
        private NativeArray<FixedString64Bytes> _freezeTargets;

        public void OnCreate(ref SystemState state)
        {
            // MatchFlow OWNS the MatchState singleton. The Bootstrap normally creates it;
            // if it is missing (e.g. systems came up before bootstrap), create Ongoing so
            // downstream singleton lookups never throw. Structural change in OnCreate is fine.
            if (!SystemAPI.HasSingleton<MatchState>())
            {
                var e = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(e, new MatchState { Outcome = MatchOutcome.Ongoing });
            }

            BuildFreezeTargets();
        }

        public void OnDestroy(ref SystemState state)
        {
            if (_freezeTargets.IsCreated) _freezeTargets.Dispose();
        }

        /// <summary>
        /// Materialize the freeze set from the ONE canonical order (SimSystemOrder). Phase-1
        /// work systems come from WorkSystems; the superseded Phase-0 spike from
        /// RetiredPhase0Systems (defensive — Bootstrap already disables it at setup). Persistent
        /// allocation, disposed in OnDestroy. Names that exceed 63 bytes are skipped (won't fit
        /// a FixedString64Bytes) — no Phase-1 type name is anywhere near that long.
        /// </summary>
        private void BuildFreezeTargets()
        {
            var work = SimSystemOrder.WorkSystems;
            var retired = SimSystemOrder.RetiredPhase0Systems;
            int total = work.Length + retired.Length;

            _freezeTargets = new NativeArray<FixedString64Bytes>(total, Allocator.Persistent);
            int n = 0;
            for (int i = 0; i < work.Length; i++)
                _freezeTargets[n++] = new FixedString64Bytes(work[i]);
            for (int i = 0; i < retired.Length; i++)
                _freezeTargets[n++] = new FixedString64Bytes(retired[i]);
        }

        // Not [BurstCompile]: this method calls managed World/SystemHandle APIs to disable
        // sibling systems on the freeze edge. Hot per-unit work lives in the other systems.
        public void OnUpdate(ref SystemState state)
        {
            var match = SystemAPI.GetSingletonRW<MatchState>();

            // Already decided (this tick froze it, or StatueDamage set it on a prior tick):
            // ensure the freeze is applied and do nothing else. Freezing is idempotent.
            if (match.ValueRO.Outcome != MatchOutcome.Ongoing)
            {
                FreezeSim(ref state);
                return;
            }

            // Derive outcome from the objectives. StatueDamageSystem may already have set
            // Outcome on the killing blow; if so the early-return above handled it. Here we
            // also detect a Destroyed/Health<=0 statue defensively (idempotent with §11 system).
            MatchOutcome resolved = MatchOutcome.Ongoing;

            foreach (var (tag, st) in
                     SystemAPI.Query<RefRO<StatueTag>, RefRO<StatueState>>())
            {
                bool down = st.ValueRO.Phase == StatuePhase.Destroyed || st.ValueRO.Health <= 0f;
                if (!down) continue;

                if (tag.ValueRO.Team == PlayerTeam)
                {
                    resolved = MatchOutcome.Defeat;  // our statue fell — loss takes priority
                    break;
                }
                if (tag.ValueRO.Team == AiTeam)
                {
                    resolved = MatchOutcome.Victory; // keep scanning in case ours also fell this tick
                }
            }

            if (resolved != MatchOutcome.Ongoing)
            {
                match.ValueRW.Outcome = resolved;
                FreezeSim(ref state);
            }
        }

        /// <summary>
        /// Stops further sim work once the match is decided by disabling every Phase-1 work
        /// system in this world (the canonical SimSystemOrder set, incl. the §13 1.5
        /// BasicAISystem). MatchFlow and the SimPhaseBegin/End anchors stay enabled so the
        /// freeze remains latched (and re-entry keeps it frozen). Disabling is the §12-clean
        /// way to halt the ECS tick without tearing down entities (no save state, readable
        /// end-of-battle snapshot preserved for the gate review).
        /// </summary>
        private void FreezeSim(ref SystemState state)
        {
            var world = state.World;
            if (world == null || !world.IsCreated) return;

            // Walk the world's unmanaged systems once; disable any whose undecorated type name
            // EQUALS a freeze target (exact, word-boundary match — never a substring). ~O(system
            // count) on the decisive tick only — negligible, never on the per-unit hot path (§12).
            using var systems = world.Unmanaged.GetAllUnmanagedSystems(Allocator.Temp);
            for (int i = 0; i < systems.Length; i++)
            {
                ref SystemState s = ref world.Unmanaged.ResolveSystemStateRef(systems[i]);
                FixedString128Bytes name = s.DebugName; // unmanaged FixedString of the system type name
                for (int t = 0; t < _freezeTargets.Length; t++)
                {
                    if (TypeNameEquals(name, _freezeTargets[t]))
                    {
                        s.Enabled = false;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// True iff the UNDECORATED simple type name embedded in <paramref name="debugName"/>
        /// EQUALS <paramref name="simpleName"/> exactly. DebugName may be decorated
        /// ("Bulwark.Sim.CombatSystem", "CombatSystem (Bulwark.Sim)", a presentation suffix,
        /// etc.), so we isolate the type-name token and compare it for full equality — NOT a
        /// Contains substring (which would wrongly match "FooAttackSystemBar" against
        /// "AttackSystem"). Allocation-free: scans the FixedString bytes in place.
        ///
        /// Token rule: the type name is the maximal run of C#-identifier chars
        /// ([A-Za-z0-9_]) that ends the namespace-qualified name — i.e. the substring after
        /// the LAST '.' (namespace separator) and before the first non-identifier char
        /// (space, '(', '[', '&lt;', etc.) that follows it. We find that token's [start,end)
        /// span in debugName and require it to equal simpleName byte-for-byte with matching
        /// length (word boundaries on both ends).
        /// </summary>
        private static bool TypeNameEquals(in FixedString128Bytes debugName, in FixedString64Bytes simpleName)
        {
            int n = debugName.Length, m = simpleName.Length;
            if (m == 0 || m > n) return false;

            // Find the end of the leading type-name token: scan from index 0 to the first
            // non-identifier byte (that is where decoration like " (group)" begins).
            int tokEnd = 0;
            while (tokEnd < n && IsIdentByte(debugName[tokEnd])) tokEnd++;

            // If the type name was namespace-qualified before that token (e.g.
            // "Bulwark.Sim.CombatSystem"), the run above already stopped at the leading dot's
            // segment only if a dot appeared; handle dots by treating the LAST dot inside the
            // leading token-run as the start. Re-scan: find token start = byte after the last
            // '.' that occurs within [0, tokEnd2), where tokEnd2 extends the identifier run
            // across '.' separators too.
            int tokEnd2 = 0;
            while (tokEnd2 < n && (IsIdentByte(debugName[tokEnd2]) || debugName[tokEnd2] == (byte)'.'))
                tokEnd2++;

            int tokStart = 0;
            for (int i = 0; i < tokEnd2; i++)
                if (debugName[i] == (byte)'.') tokStart = i + 1;

            int tokLen = tokEnd2 - tokStart;
            if (tokLen != m) return false;

            for (int k = 0; k < m; k++)
                if (debugName[tokStart + k] != simpleName[k]) return false;

            return true;
        }

        // C# identifier byte: letters, digits, underscore. (ASCII — Entities type names are ASCII.)
        private static bool IsIdentByte(byte b)
        {
            return (b >= (byte)'A' && b <= (byte)'Z')
                || (b >= (byte)'a' && b <= (byte)'z')
                || (b >= (byte)'0' && b <= (byte)'9')
                || b == (byte)'_';
        }
    }
}

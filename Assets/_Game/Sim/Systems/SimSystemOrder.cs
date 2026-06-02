// BULWARK — Phase-1 SIM SYSTEM ORDER (canonical update order + ordering anchors).
// Roadmap §13 Phase 1 (1.1 economy/objective → 1.2 train → 1.4 targeting+combat →
// 1.5 AI), §12 (ECS battle-sim is the isolated hot path; targeting/AI stay ~O(1)/unit),
// §3 (preserved mine→train→push→statue loop; readable lane combat is inviolable).
//
// WHY THIS FILE EXISTS
// The Phase-1 systems live in SEPARATE files authored by sibling modules (Mining,
// Training, InfluenceMap, PossessControl, Targeting, BasicAI, Combat, StatueDamage).
// This file is the single place that DOCUMENTS and ANCHORS the canonical per-frame
// order so the assembly is deterministic-in-ordering (not deterministic-in-math —
// numeric determinism/replays are a Phase-7 gate, §12, and FORBIDDEN now).
//
// THE CANONICAL ORDER (one fixed-step sim tick):
//   1. MiningSystem        (§13 1.1) — miners on nodes accrue Gold into GoldStore.
//   2. TrainingSystem      (§13 1.2) — pay Gold, tick TrainOrder, spawn deployed units.
//   3. InfluenceMapSystem  (§13 1.4) — build the cheap row/lane influence buckets that
//                                       keep targeting+AI O(1)/unit (§12 perf rule).
//   4. PossessControlSystem(§13 1.3) — apply player ManualOrder/Possessed intent
//                                       (input → data; MonoBehaviour writes, sim reads).
//   5. TargetingSystem     (§13 1.4) — acquire/keep Targeting.Current using the influence
//                                       map + same-row preference + stickiness (×1.2).
//   6. BasicAISystem       (§13 1.5) — single-layer utility COMMANDER for Team 1: reads
//                                       gold/army/statue signals, picks a CommanderStance,
//                                       biases the AI TrainQueue. Depends on 1.4 (§13 1.5
//                                       builds on the shared targeting/combat the AI's
//                                       units use), so it runs AFTER TargetingSystem and
//                                       BEFORE MovementSystem. It is utility-only (no spells
//                                       /collectible commanders — those are forbidden now).
//   7. MovementSystem      (§4/§11)  — close toward Targeting.Current / order along the front.
//   8. CombatSystem        (§4)      — modifier chain: base × typeArmor × positional ×
//                                       terrain × difficulty (all factor slots 1.0 at P1);
//                                       writes StatueDamageInbox when a statue is the target.
//   9. StatueDamageSystem  (§11)     — drain StatueDamageInbox through shield→health,
//                                       recompute StatuePhase, throttle trickle; on
//                                       Health<=0 sets MatchState.Outcome.
//  10. MatchFlowSystem     (§13 P1)  — owns/finalizes MatchState; once Outcome != Ongoing,
//                                       FREEZES the sim (disables the systems above).
//
// HOW SIBLING SYSTEMS PIN THEMSELVES INTO THIS ORDER
// Each sibling ISystem declares, in ITS OWN file:
//     [UpdateInGroup(typeof(SimulationSystemGroup))]
//     [UpdateAfter(typeof(SimPhaseBegin))]
//     [UpdateBefore(typeof(SimPhaseEnd))]
// plus the pairwise [UpdateAfter]/[UpdateBefore] against its immediate neighbours above,
// e.g. TargetingSystem: [UpdateAfter(typeof(PossessControlSystem))]
//                       [UpdateBefore(typeof(BasicAISystem))].
// BasicAISystem (the §13 1.5 commander) pins itself between Targeting and Movement:
//                       [UpdateAfter(typeof(TargetingSystem))]
//                       [UpdateBefore(typeof(MovementSystem))].
// (BasicAISystem appends TrainOrder for Team 1; those queue writes are consumed by the
//  next tick's TrainingSystem — standard one-tick latency, no same-tick ordering need.)
// The two empty anchor systems below give every Phase-1 sim system a STABLE pair of
// bookends to order between, so the begin/end of the sim tick are explicit and a
// system added later cannot silently float outside the Phase-1 window.
//
// SINGLE SOURCE OF TRUTH FOR THE ORDER (§15 governance, DRY)
// SimSystemOrder.WorkSystems is the ONE canonical, ordered list of the Phase-1 WORK
// system type names (the systems MatchFlow freezes). MatchFlow derives its freeze set
// from this array (see MatchFlow.cs) so the documented order and the freeze set CANNOT
// drift. The per-name consts below are convenience handles onto that same array; they
// are documentation/lookup only — the actual scheduled order is enforced by each
// sibling's [UpdateAfter]/[UpdateBefore] attributes (which require typeof, not strings).
//
// SCAFFOLD STATUS: authored, NOT compiled (no Unity 6 / Entities here — ADR-0-001/-002).
// Ordering is authored; the actual scheduled order is a Unity-runtime concern (DEFERRED).

using Unity.Entities;
using Unity.Mathematics;

namespace Bulwark.Sim
{
    // ============================================================================
    // PHASE-1 ASSEMBLY CONVENTION COMPONENTS (shared sim contract)
    // ----------------------------------------------------------------------------
    // The shared contract permits the assembly to INTRODUCE these IF NEEDED, defined
    // in this module with STABLE names. They are CROSS-MODULE: the Bootstrap (control
    // layer) bakes them in, and sibling SIM systems consume them, so they must live in
    // the Bulwark.Sim assembly (the Bootstrap assembly references Sim, not vice versa).
    // No balance values live here (§15.6) — UnitSpawnStats is a pure value copy of
    // already-authored UnitDef DATA fields, baked once at battle setup.
    // ============================================================================

    /// <summary>
    /// Per-side spawn anchor on the single front (§11): where a side's freshly-trained
    /// units appear before they push. One per team (Team 0 player end, Team 1 AI end).
    /// </summary>
    public struct TeamSpawnPoint : IComponentData
    {
        public int Team;
        public float2 Pos;
    }

    /// <summary>
    /// Value copy of a UnitDef's spawn-relevant fields, baked into the catalog buffer so
    /// the sim can spawn units WITHOUT touching managed ScriptableObjects on the hot path
    /// (§12). Index in the buffer == TrainOrder.UnitIndex == index into the authored set.
    /// Values are DATA (copied from UnitDef), never authored here (§15.6).
    /// </summary>
    public struct UnitSpawnStats : IBufferElementData
    {
        public DamageTypeId DamageType; // mirror of Bulwark.Data.DamageType as a sim-side int
        public ArmorClassId ArmorClass; // mirror of Bulwark.Data.ArmorClass as a sim-side int
        public RoleId Role;             // mirror of Bulwark.Data.UnitRole as a sim-side int
        public float MaxHealth;
        public float MoveSpeed;
        public float AttackDamage;      // 'base' in the §4 modifier chain
        public float AttackRange;
        public float AttackInterval;
        public int GoldCost;
        public float TrainSeconds;
        public float MiningRatePerSec;
    }

    // Sim-side mirrors of the Data enums, kept byte-identical in value so a cast is exact.
    // Defined here (not Data) so the Sim hot path never depends on the managed Data enums by
    // reference; the Bootstrap copies the int value across at bake time.
    public enum DamageTypeId : int { Unset = 0, Melee = 1, Pierce = 2, Blunt = 3, Fire = 4, Poison = 5 }
    public enum ArmorClassId : int { Unset = 0, Light = 1, Shielded = 2, Heavy = 3, Unarmored = 4 }
    public enum RoleId : int { Unset = 0, Miner = 1, Frontline = 2, Skirmisher = 3, Ranged = 4, Caster = 5, Heavy = 6, Flanker = 7 }

    /// <summary>
    /// Singleton tag marking the entity that holds the DynamicBuffer&lt;UnitSpawnStats&gt;
    /// (the per-side unit roster baked from the authored UnitDef set). TrainingSystem reads it.
    /// </summary>
    public struct UnitCatalogTag : IComponentData { }

    // StatueDamageInbox (the per-statue damage inbox buffer CombatSystem appends and
    // StatueDamageSystem drains, §11/§12) is declared ONCE in StatueDamage.cs (its draining
    // owner). (Reconciliation: removed the duplicate declaration that used to live here — a
    // buffer element type may be declared only once.)

    /// <summary>
    /// Empty ordering ANCHOR that marks the START of the Phase-1 sim tick.
    /// Sibling Phase-1 systems pin themselves with [UpdateAfter(typeof(SimPhaseBegin))].
    /// Does no work — it exists purely so the per-frame order is explicit and stable.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    public partial struct SimPhaseBegin : ISystem
    {
        public void OnCreate(ref SystemState state) { }
        public void OnUpdate(ref SystemState state) { /* anchor only */ }
        public void OnDestroy(ref SystemState state) { }
    }

    /// <summary>
    /// Empty ordering ANCHOR that marks the END of the Phase-1 sim tick.
    /// Sibling Phase-1 systems pin themselves with [UpdateBefore(typeof(SimPhaseEnd))].
    /// MatchFlowSystem runs as late as possible by ordering [UpdateBefore(typeof(SimPhaseEnd))]
    /// AND [UpdateAfter] every other Phase-1 system (see MatchFlow.cs), so it observes the
    /// fully-resolved tick before freezing the match.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public partial struct SimPhaseEnd : ISystem
    {
        public void OnCreate(ref SystemState state) { }
        public void OnUpdate(ref SystemState state) { /* anchor only */ }
        public void OnDestroy(ref SystemState state) { }
    }

    /// <summary>
    /// Compile-time-stable ordering MARKER. Other modules read these constant System
    /// type names indirectly via [UpdateAfter]/[UpdateBefore]; this struct centralizes the
    /// canonical names so a sibling file never guesses a neighbour's type. It is NOT a
    /// system (no ISystem) — purely documentation surfaced in code so the order is
    /// discoverable from one place. Values are the EXACT system type names to reference.
    ///
    /// SINGLE SOURCE OF TRUTH: <see cref="WorkSystems"/> is the canonical ordered list of
    /// the Phase-1 work systems (everything before MatchFlow). MatchFlow builds its freeze
    /// set from this array (plus the retired Phase-0 spike names) so the documented order
    /// and the freeze set cannot diverge. The per-name consts are just handles into it.
    /// </summary>
    public static class SimSystemOrder
    {
        // Canonical Phase-1 sim system type names, in tick order. Sibling modules MUST
        // use these exact names in their [UpdateInGroup]/[UpdateAfter]/[UpdateBefore].
        public const string MiningSystem         = nameof(MiningSystem);
        public const string TrainingSystem       = nameof(TrainingSystem);
        public const string InfluenceMapSystem   = nameof(InfluenceMapSystem);
        public const string PossessControlSystem = nameof(PossessControlSystem);
        public const string TargetingSystem      = nameof(TargetingSystem);
        public const string BasicAISystem        = nameof(BasicAISystem); // §13 1.5 utility commander
        public const string MovementSystem       = nameof(MovementSystem);
        public const string CombatSystem         = nameof(CombatSystem);
        public const string StatueDamageSystem   = nameof(StatueDamageSystem);
        public const string MatchFlowSystem      = nameof(MatchFlowSystem);

        // Ordering anchors (this file).
        public const string PhaseBegin = nameof(SimPhaseBegin);
        public const string PhaseEnd   = nameof(SimPhaseEnd);

        /// <summary>
        /// THE canonical ordered list of Phase-1 WORK systems (steps 1–9 above). MatchFlow
        /// freezes exactly these on a decided match — it derives its freeze set from this
        /// array, so adding/renaming a Phase-1 system here updates the freeze set too and
        /// the two cannot drift (DRY, §15 governance). MatchFlowSystem itself is NOT listed:
        /// it stays enabled to latch the frozen result. The anchors stay enabled too.
        /// </summary>
        public static readonly string[] WorkSystems =
        {
            MiningSystem,
            TrainingSystem,
            InfluenceMapSystem,
            PossessControlSystem,
            TargetingSystem,
            BasicAISystem,
            MovementSystem,
            CombatSystem,
            StatueDamageSystem,
        };

        /// <summary>
        /// Phase-0 spike systems (SimSystems.cs) that are SUPERSEDED by the Phase-1 set:
        /// MoveSystem/AttackSystem use AttackState.Target and an O(N^2) nearest-enemy scan
        /// that the shared contract forbids in Phase 1 (targeting lives in Targeting.Current,
        /// §12 O(1) rule). The Bootstrap DISABLES these at world setup so they never co-run
        /// with Targeting/Movement/Combat (see BattleBootstrap.DisablePhase0SpikeSystems).
        /// They are also listed in MatchFlow's freeze set as a defensive belt-and-braces
        /// guard in case a different entry point left them enabled.
        /// </summary>
        public static readonly string[] RetiredPhase0Systems =
        {
            "MoveSystem",
            "AttackSystem",
        };
    }
}

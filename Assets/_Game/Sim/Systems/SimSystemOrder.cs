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
// THE CANONICAL ORDER (one fixed-step sim tick). Phase-1 systems are interleaved with the
// Phase-2 systems wired by the integration pass; the EXACT scheduled order is enforced by each
// system's [UpdateAfter]/[UpdateBefore] attributes (the hard constraints are documented inline).
// Listed in tick order:
//   CounterMatrixValidationSystem (§4)  — one-shot init sanity check on the 20-cell matrix; self-disables.
//   1.  MiningSystem        (§13 1.1) — miners on nodes accrue Gold into GoldStore.
//   2.  TrainingSystem      (§13 1.2) — pay Gold, tick TrainOrder, spawn deployed units (per-team catalog).
//   3.  InfluenceMapSystem  (§13 1.4) — build the cheap row/lane influence buckets (O(1)/unit, §12).
//   3a. SpellSlotCooldownSystem (§2.4) — tick each side's drafted SpellSlot.Cooldown (before AI reads readiness).
//   3b. CommanderAbilitySystem  (§2.5) — apply the §6-clamped passive + triggered active buffs (status).
//       AISchedulerSystem        (§2.6) — advance the budgeted round-robin AI cursor.
//       SquadAISystem            (§2.6) — per AI squad: posture utility → SquadFormation + train/spell/active requests.
//       SpellCastSystem          (§2.4) — drain the cast inbox (player + SquadAI), spawn telegraphs.
//   4.  PossessControlSystem(§13 1.3) — apply player ManualOrder/Possessed intent (input → data).
//   4a. FormationSystem      (§2.2)   — lay squad members into slots (MoveDestination override).
//   5.  TargetingSystem     (§13 1.4) — acquire/keep Targeting.Current (stunned units skip re-acquire).
//   6.  BasicAISystem       (§13 1.5) — single-layer utility commander stance for Team 1.
//   7.  MovementSystem      (§4/§11)  — close toward Targeting.Current / order; stun-halt + Chilled/Hasted
//                                       move + Choke MoveMult scale the effective speed (no fork).
//   7a. EnsurePhase2CombatComponentsSystem (§2.1) — structural: ensure Facing/TerrainOccupancy/
//                                       StatusEffect buffer/Difficulty slot on every unit.
//   7b. FacingSystem         (§2.1)   — the SINGLE writer of Facing.Dir (geometry input for positional).
//   7c. TerrainSystem        (§2.1)   — occupancy + populate the §4 TerrainFactor (HighGround) slot + Hazard tick.
//   7d. PositionalSystem     (§2.1)   — populate the §4 positional (flank 1.5 / back 2.0) slot vs target.
//   7e. TelegraphResolveSystem (§2.4) — resolve elapsed telegraphs (offense via the SAME §4 typeArmor read).
//   8.  CombatSystem        (§4)      — modifier chain: base × typeArmor × positional × terrain ×
//                                       difficulty × targetCover × ragedOut; statue → inbox.
//   8a. StatusEffectSystem   (§2.4)   — SINGLE decay/expiry owner; Burning/Poisoned DoT (statue → inbox).
//   9.  StatueDamageSystem  (§11)     — drain StatueDamageInbox (shield→health), recompute phase, set Outcome.
//  10.  MatchFlowSystem     (§13 P1)  — owns/finalizes MatchState; on Outcome != Ongoing FREEZES the sim.
//
// HARD ORDERING CONSTRAINTS (enforced by attributes; the rest is a guide):
//   • Facing BEFORE Positional; Positional AFTER Targeting and BEFORE Combat; Terrain BEFORE Combat.
//   • Commander buffs (CommanderAbility) + spell status (TelegraphResolve) applied BEFORE the Combat
//     that should feel them. StatusEffect ticks AFTER Combat (DoT coherent, then routed to statue inbox).
//   • AISchedulerSystem BEFORE SquadAISystem; SpellSlotCooldown BEFORE SquadAI/SpellCast; SpellCast
//     AFTER SquadAI (same-tick AI casts honored) and BEFORE TelegraphResolve. MatchFlow LAST.
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
    /// Tag marking an entity that holds a DynamicBuffer&lt;UnitSpawnStats&gt; (a per-side unit
    /// roster baked from one faction's authored UnitDef set). PHASE-2 TWO-FACTION CHANGE: this is
    /// now PER-TEAM (Iron Pact vs Ashen Horde) — there is ONE catalog entity per side, keyed by
    /// <see cref="Team"/>, instead of a single global singleton. Each side's TrainOrder.UnitIndex /
    /// SpellDef.summonUnitIndex is SIDE-RELATIVE (an index into THAT side's roster). Sim readers
    /// (TrainingSystem, BasicAI, SquadAI, SpellSummon) resolve the catalog for a specific team via
    /// <see cref="UnitCatalog.TryGetForTeam"/> — they must NOT use GetSingletonEntity (two exist).
    /// </summary>
    public struct UnitCatalogTag : IComponentData { public int Team; }

    /// <summary>
    /// Shared per-team unit-catalog lookup (Phase-2 two-faction change). Resolves the
    /// <see cref="UnitCatalogTag"/> entity (and its <see cref="UnitSpawnStats"/> roster buffer) for
    /// a given team. Kept here next to the catalog types so every sim reader resolves the correct
    /// side's roster through ONE place (DRY, §15). Side-relative indexing: the returned buffer is
    /// indexed by that side's TrainOrder.UnitIndex / SpellDef.summonUnitIndex. Implemented over
    /// <see cref="EntityManager"/> (NOT SystemAPI) so it is callable from any context — SystemAPI
    /// source-gen is only valid inside an ISystem body. Cached query; the ≤2 catalogs make it O(1).
    /// </summary>
    public static class UnitCatalog
    {
        /// <summary>
        /// Find the catalog entity for <paramref name="team"/> via the EntityManager (linear over
        /// the ≤2 catalog entities — O(1)). Returns false if no per-team catalog was baked.
        /// </summary>
        public static bool TryGetForTeam(EntityManager em, int team, out Entity catalogEntity)
        {
            var query = em.CreateEntityQuery(
                ComponentType.ReadOnly<UnitCatalogTag>(),
                ComponentType.ReadOnly<UnitSpawnStats>());
            using var ents = query.ToEntityArray(Unity.Collections.Allocator.Temp);
            for (int i = 0; i < ents.Length; i++)
            {
                if (em.GetComponentData<UnitCatalogTag>(ents[i]).Team != team) continue;
                catalogEntity = ents[i];
                return true;
            }
            catalogEntity = Entity.Null;
            return false;
        }
    }

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

        // Phase-2 sim system type names (tactical depth), wired into the order above by the
        // integration pass. Same convention — siblings reference these via typeof attributes.
        public const string EnsurePhase2CombatComponentsSystem = nameof(EnsurePhase2CombatComponentsSystem);
        public const string FacingSystem                = nameof(FacingSystem);                // §2.1 (single Facing writer)
        public const string TerrainSystem               = nameof(TerrainSystem);               // §2.1
        public const string PositionalSystem            = nameof(PositionalSystem);            // §2.1
        public const string FormationSystem             = nameof(FormationSystem);             // §2.2
        public const string CounterMatrixValidationSystem = nameof(CounterMatrixValidationSystem); // §2.2 (init-only)
        public const string SpellSlotCooldownSystem     = nameof(SpellSlotCooldownSystem);     // §2.4
        public const string SpellCastSystem             = nameof(SpellCastSystem);             // §2.4
        public const string TelegraphResolveSystem      = nameof(TelegraphResolveSystem);      // §2.4
        public const string StatusEffectSystem          = nameof(StatusEffectSystem);          // §2.4
        public const string CommanderAbilitySystem      = nameof(CommanderAbilitySystem);      // §2.5
        public const string AISchedulerSystem           = nameof(AISchedulerSystem);           // §2.6
        public const string SquadAISystem               = nameof(SquadAISystem);               // §2.6
        // Control/structural-ensure siblings (run inside the group but are setup, not "work"):
        public const string EnsureMoveDestinationSystem = nameof(EnsureMoveDestinationSystem); // §1.3 ensure-pass

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
            // Phase-1 work systems.
            MiningSystem,
            TrainingSystem,
            InfluenceMapSystem,
            PossessControlSystem,
            TargetingSystem,
            BasicAISystem,
            MovementSystem,
            CombatSystem,
            StatueDamageSystem,
            // Phase-2 work systems (tactical depth) — frozen with the rest on a decided match so no
            // spell/commander/AI/terrain work runs after the gate. CounterMatrixValidationSystem is
            // init-only (self-disables) and EnsureMoveDestination/EnsurePhase2 are structural-ensure
            // passes; they are intentionally NOT listed (nothing to freeze / they idle when no work remains).
            SpellSlotCooldownSystem,
            CommanderAbilitySystem,
            AISchedulerSystem,
            SquadAISystem,
            SpellCastSystem,
            FormationSystem,
            FacingSystem,
            TerrainSystem,
            PositionalSystem,
            TelegraphResolveSystem,
            StatusEffectSystem,
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

// BULWARK — Phase 2 FORMATIONS (roadmap §13 2.2 "formations"; §4 "positional combat" — this
// module supplies the squad GEOMETRY/positions that drive movement; the unit Facing it ultimately
// feeds the §4 PositionalSystem (2.1) through is OWNED by 2.1's FacingSystem — see FACING OWNERSHIP
// below; §11 single front / 3 rows; §12 ECS boundary + ~O(1)/agent perf).
//
// WHAT THIS SYSTEM DOES (§13 2.2)
// FormationSystem arranges each squad's member units into a shape around its SquadFormation.Anchor
// according to SquadFormation.Type:
//   • Line  — wide, shallow lateral FAN within each member's existing row (a broad front; spreads
//             damage intake laterally). NOTE: this fans units sideways in continuous space; it does
//             NOT re-band a unit between the §11 row bands — see ROW BAND below.
//   • Tight — clustered around the anchor (cohesion / mass; AoE-VULNERABLE by geometry).
//   • Loose — dispersed wide+deep (AoE-RESISTANT by geometry; thinner local presence).
// For every member NOT under direct player control, it WRITES the unit's computed slot into
// MoveDestination{Value=slot, Active=1} (the §13 P1.3 control-handoff override that the §13 P1.4
// MovementSystem already consumes — see PossessControl.cs handoff contract). It does NOT write
// Facing (see FACING OWNERSHIP), and it does NOT touch terrain, the counter matrix, the §4 damage
// chain, or any positional MULTIPLIER — only the formation POSITIONS that movement realizes.
//
// FACING OWNERSHIP (§13 2.1↔2.2 integration — the single Facing writer is FacingSystem)
// The §4 positional geometry input Facing.Dir is OWNED by 2.1's FacingSystem (Terrain.cs), which
// runs [UpdateAfter(MovementSystem)] / [UpdateBefore(PositionalSystem)] and UNCONDITIONALLY
// recomputes Facing.Dir every tick (toward current target → else active move order → else the
// enemy end of the front). FormationSystem runs [UpdateBefore(MovementSystem)], i.e. BEFORE
// FacingSystem, so any Facing this system wrote would be stomped before PositionalSystem reads it.
// To avoid a DEAD write and a false claim, FormationSystem does NOT write Facing. Pointing a
// formation member at SquadFormation.Facing is instead a branch that must live INSIDE FacingSystem
// (one writer, no merge race) — that exact edit is described in integrationNotes for the
// integration pass. Until that branch lands, a holding (non-engaged) formation member faces via
// FacingSystem's move-order branch (it walks toward its slot) then its enemy-end fallback.
//
// ROW BAND (§11 3-row band — Line fans LATERALLY, it does not move units between rows)
// The authoritative §11 row band is the discrete Row{int Index}(0..2) component that
// TargetingSystem (RowClamp) and TerrainSystem (per-row feature match) read. FormationSystem only
// writes a continuous-space MoveDestination (a world position movement realizes); it NEVER writes
// Row.Index. A continuous Y offset therefore cannot move a unit between §11 row bands for
// targeting/terrain purposes. So Line's lateral fan is exactly that — a lateral spread within the
// member's EXISTING row — not a cross-row re-banding. Re-banding a unit between rows is a
// structural/data change owned by the spawn / SquadAI (2.6) layer; see integrationNotes.
//
// CONTROL BOUNDARY (§12 / §13 P1.3): a unit that is Possessed, or that already carries an active
// MoveDestination (Active==1), is SKIPPED — a live override (player order via PossessControlSystem,
// or any future squad-handoff writer) wins over this tick's formation slot. FormationSystem only
// ever turns Active ON for its OWN slots; it never clears another writer's override. (In the
// current repo PossessControlSystem is the only writer that sets Active=1, on PlayerControlled+
// UnitTag units; the guard is intentionally writer-agnostic so a foreign active override is never
// stomped — broader than, and consistent with, the P1.3 contract.)
//
// WHY IT RE-ASSERTS EVERY TICK: MovementSystem CLEARS MoveDestination.Active to 0 on arrival
// (k_ManualArriveRadius, see Combat.cs / the 1.3<->1.4 handoff contract). A formation is a STANDING
// order, not a one-shot move, so this system re-writes the slot (and re-arms Active=1) each tick for
// its members. When the unit is already standing on its slot the re-asserted destination is its own
// position, so it holds station; when the anchor/facing moves (squad advances) the slot moves and
// the unit follows. Re-asserting is idempotent and O(1) per member.
//
// PERF (§12): one bounded pass to index squads (there are few squads) into a NativeHashMap keyed by
// SquadId, then ONE linear pass over members doing an O(1) map lookup + O(1) slot math. No O(N^2)
// all-pairs scan. Slot offsets are computed analytically from (Type, Slot) — no neighbour search.
//
// CANON DISCIPLINE
//   • §15.6 no invented balance: the only literals here are provisional SPACING GEOMETRY (rank/file
//     spacing, cluster scale) — layout distances, NOT combat/power values. Nothing in the §4 damage
//     chain reads them; they place units in space. Marked provisional / LSD-tunable.
//   • Invents NO new mechanic/unit/spell/currency (§15): reuses existing SquadFormation/FormationMember
//     (Phase2Components.cs) and the existing MoveDestination control override (PossessControl.cs).
//     Formations are a §13 2.2 canon feature.
//   • No later-phase features: no spells, no commander power, no terrain math, no economy. Just layout.
//   • §12 ECS boundary: this is an ISystem in Bulwark.Sim that only WRITES sim data (MoveDestination).
//     Squad Anchor/Facing/Type are set upstream by SquadAI (2.6) or the control shell.
// SCAFFOLD STATUS: authored, NOT compiled (no Unity 6 / Entities here — ADR-0-001/-002/-1-001).
// DEFERRED: slot spacing "feel" + arrival/hold jitter are GATE-2 on-device tuning (deferredValidations).

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Bulwark.Sim
{
    /// <summary>
    /// Phase-2 (§13 2.2) formation layout. For each <see cref="SquadFormation"/>, places its
    /// <see cref="FormationMember"/> units into per-<see cref="FormationType"/> slot offsets around
    /// the squad <see cref="SquadFormation.Anchor"/>, rotated to face <see cref="SquadFormation.Facing"/>.
    /// For members not under direct player control (and not already carrying a live override) it sets
    /// the §13 P1.3 <see cref="MoveDestination"/> override toward the slot. Pure POSITION layout: no
    /// terrain / positional multiplier / damage logic, and no <see cref="Facing"/> write — Facing is
    /// owned by 2.1's FacingSystem (see FACING OWNERSHIP in the file header / integrationNotes).
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SimPhaseBegin))]
    [UpdateBefore(typeof(SimPhaseEnd))]
    // Run with the control/order layer, BEFORE movement consumes the destination this tick.
    [UpdateAfter(typeof(PossessControlSystem))]
    [UpdateBefore(typeof(MovementSystem))]
    [RequireMatchingQueriesForUpdate]
    public partial struct FormationSystem : ISystem
    {
        // ---- PROVISIONAL SPACING GEOMETRY (layout distances, NOT combat/balance — §15.6 exception:
        //      these place units in space; nothing in the §4 chain reads them). LSD-tunable at GATE 2.
        // Along-facing rank depth and across-facing file spacing for ordered ranks (Line/Tight/Loose).
        private const float k_FileSpacing = 0.6f; // lateral gap between neighbouring files (across facing)
        private const float k_RankDepth   = 0.6f; // gap between successive ranks (along -facing, behind anchor)
        // Tight is a denser cluster; Loose multiplies spacing to disperse (AoE-resistant geometry).
        private const float k_TightScale = 0.7f;
        private const float k_LooseScale = 1.8f;
        // Width of a Tight/Loose rank (files per rank) before wrapping to the next rank behind.
        private const int   k_FilesPerRank = 4;

        public void OnCreate(ref SystemState state)
        {
            // Only run when there is at least one squad formation to lay out AND members to place.
            state.RequireForUpdate<SquadFormation>();
            state.RequireForUpdate<FormationMember>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // ---- PASS 1: index squads by SquadId (few squads → cheap). O(squads). ----
            // We snapshot the squad's layout inputs (Type/Anchor/Facing) so the member pass is a
            // pure O(1) hash lookup with no entity hops.
            int squadCount = SystemAPI.QueryBuilder().WithAll<SquadFormation>().Build().CalculateEntityCount();
            if (squadCount == 0)
                return;

            var squads = new NativeHashMap<int, SquadLayout>(squadCount, Allocator.Temp);
            foreach (var sf in SystemAPI.Query<RefRO<SquadFormation>>())
            {
                var s = sf.ValueRO;
                // Normalize the facing once; degenerate facing falls back to +X (down the front, §11).
                float2 facing = NormalizeOrDefault(s.Facing, new float2(1f, 0f));
                // First writer wins per SquadId (ids are expected unique; this is a defensive guard).
                squads.TryAdd(s.SquadId, new SquadLayout
                {
                    Type = s.Type,
                    Anchor = s.Anchor,
                    Facing = facing,
                });
            }

            // ---- PASS 2: place each member into its slot. O(members), O(1) per member. ----
            foreach (var (member, e) in
                     SystemAPI.Query<RefRO<FormationMember>>()
                              .WithAll<UnitTag>()
                              .WithEntityAccess())
            {
                if (!squads.TryGetValue(member.ValueRO.SquadId, out var layout))
                    continue; // member references a squad with no SquadFormation this tick — leave it.

                // CONTROL BOUNDARY (§12 / §13 P1.3): never override a unit that is Possessed or that
                // already carries a LIVE MoveDestination (Active==1). A live override wins over this
                // tick's formation slot — whether it is a player order (PossessControlSystem, the
                // only Active=1 writer in the current repo) or any future squad-handoff writer.
                // FormationSystem only turns Active ON for its own slots; it must not stomp another
                // owner's live override. The guard is intentionally writer-agnostic.
                if (SystemAPI.HasComponent<Possessed>(e))
                    continue;
                bool hasDest = SystemAPI.HasComponent<MoveDestination>(e);
                if (hasDest && SystemAPI.GetComponent<MoveDestination>(e).Active != 0)
                    continue; // a live override owns this unit this tick → skip.

                // Compute this slot's local offset (provisional GEOMETRY), then place it in the world.
                float2 localOffset = SlotOffsetLocal(layout.Type, member.ValueRO.Slot);
                float2 world = layout.Anchor + RotateToFacing(localOffset, layout.Facing);

                // STANDING ORDER (§13 2.2): write the slot as a MoveDestination override and re-arm
                // Active=1 every tick (MovementSystem clears it on arrival; a formation must persist).
                // If the unit lacks MoveDestination it is not a controllable archetype the movement
                // override path covers (see integrationNotes); there is nothing else to write here.
                if (hasDest)
                {
                    var dest = SystemAPI.GetComponent<MoveDestination>(e);
                    dest.Value = world;
                    dest.Active = 1;
                    SystemAPI.SetComponent(e, dest);
                }

                // NOTE: this system does NOT write Facing. Facing.Dir is owned by 2.1's FacingSystem
                // (single writer). Pointing a formation member at SquadFormation.Facing is a branch
                // that must live inside FacingSystem to survive to PositionalSystem — see the FACING
                // OWNERSHIP header note and integrationNotes for the exact edit.
            }

            squads.Dispose();
        }

        /// <summary>Snapshot of a squad's layout inputs, captured once per tick for O(1) member lookup.</summary>
        private struct SquadLayout
        {
            public FormationType Type;
            public float2 Anchor;
            public float2 Facing; // normalized
        }

        /// <summary>
        /// Provisional per-<see cref="FormationType"/> slot offset in the squad's LOCAL frame
        /// (+X = facing direction, +Y = "left" across the facing). Pure layout geometry (§15.6: not
        /// a combat value). The slot index fans out deterministically so members occupy stable seats.
        ///   • Line  — one wide rank fanned LATERALLY around the anchor line (broad, shallow front;
        ///             a sideways spread WITHIN the member's existing §11 row — see ROW BAND header).
        ///   • Tight — dense ranks clustered just behind the anchor (cohesion / mass).
        ///   • Loose — the same rank pattern but dispersed (wider files + deeper ranks).
        /// </summary>
        private static float2 SlotOffsetLocal(FormationType type, int slot)
        {
            if (slot < 0) slot = 0;

            switch (type)
            {
                case FormationType.Line:
                {
                    // Broad single front: alternate left/right of the anchor so the squad presents a
                    // wide, shallow line. Centered so the anchor sits mid-line. Pure LATERAL fan —
                    // no rank depth and no row-band change (Row.Index is untouched; see ROW BAND).
                    int signed = FanIndex(slot);            // 0, +1, -1, +2, -2, ...
                    float across = signed * k_FileSpacing * 2f;
                    float depth = 0f; // Line is shallow (no rank depth)
                    return new float2(-depth, across);
                }

                case FormationType.Tight:
                {
                    // Clustered ranks: files across, ranks stacked behind the anchor; small spacing.
                    int file = slot % k_FilesPerRank;
                    int rank = slot / k_FilesPerRank;
                    int signedFile = FanIndex(file);        // center files around the anchor line
                    float across = signedFile * k_FileSpacing * k_TightScale;
                    float depth = rank * k_RankDepth * k_TightScale; // ranks sit BEHIND the anchor (−facing)
                    return new float2(-depth, across);
                }

                case FormationType.Loose:
                default:
                {
                    // Dispersed: same rank/file pattern, larger spacing (AoE-resistant geometry).
                    int file = slot % k_FilesPerRank;
                    int rank = slot / k_FilesPerRank;
                    int signedFile = FanIndex(file);
                    float across = signedFile * k_FileSpacing * k_LooseScale;
                    float depth = rank * k_RankDepth * k_LooseScale;
                    return new float2(-depth, across);
                }
            }
        }

        /// <summary>
        /// Maps a non-negative seat index to a centered fan: 0→0, 1→+1, 2→−1, 3→+2, 4→−2, …
        /// so members spread symmetrically around the anchor line rather than all to one side.
        /// </summary>
        private static int FanIndex(int slot)
        {
            int step = (slot + 1) / 2;       // 0,1,1,2,2,3,3,...
            return (slot & 1) == 1 ? step : -step;
        }

        /// <summary>
        /// Rotates a LOCAL offset (+X along facing, +Y across-left) into world space given a
        /// normalized facing. left = perpendicular(facing). World = facing·local.x + left·local.y.
        /// </summary>
        private static float2 RotateToFacing(float2 local, float2 facing)
        {
            float2 left = new float2(-facing.y, facing.x); // 90° CCW perpendicular
            return facing * local.x + left * local.y;
        }

        /// <summary>Returns the normalized vector, or <paramref name="fallback"/> if it is ~zero.</summary>
        private static float2 NormalizeOrDefault(float2 v, float2 fallback)
        {
            float len = math.length(v);
            return len > 1e-4f ? v / len : fallback;
        }
    }
}

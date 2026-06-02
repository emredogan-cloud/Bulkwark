// BULWARK — Phase 2.2b: type×armor COUNTER MATRIX plumbing (roadmap §13 2.2, §4).
//
// SCOPE (Prompt module 2.2b): provide the SHARED matrix-lookup helper and a one-shot
// validation system for the CounterCell singleton buffer. This file authors NO matrix
// VALUES — the 5×4 = 20 multipliers live in DATA (CounterMatrix.asset, a Bulwark.Data.
// BalanceConfig, authored separately, §15.6 "no hardcoded balance"). It also does NOT fork
// the §4 modifier chain: CombatSystem (Combat.cs) already applies
//   round(base × (1 + Level×PerLevel)) × typeArmor × positional × terrain × difficulty
// and reads the CounterCell DynamicBuffer on the CounterMatrixTag singleton. This module's
// Lookup IS that one type×armor read, refactored into a single shared entry point so that
// CombatSystem and the Phase-2 SpellSystem resolve the matrix through ONE place (the
// integration pass wires the existing inline reader to call it — see integrationNotes).
//
// CANON DISCIPLINE:
//  • §4: the matrix is exactly 5 damage types × 4 armor classes = 20 cells; index =
//    (damageType-1)*4 + (armorClass-1). Matches Combat.cs LookupTypeArmor and
//    BalanceConfig.GetMultiplier byte-for-byte (Unset / out-of-range / 0.0 cell → 1.0).
//  • §13 2.2: "full type×armor counter matrix" — Phase 2 populates the remaining cells in
//    DATA; this code is value-agnostic (works for the P1 "basic counters only" buffer and
//    the full P2 buffer identically).
//  • §12 ECS boundary: pure sim, Burst-friendly, no UI/meta. Lookup is O(1) (single indexed
//    buffer read) — no scans, satisfies the §12 perf budget.
//  • ADR-1-001: Phase 2 is AUTHOR-ONLY, GATE 2 deferred — runtime assertion is DEFERRED.
//
// SCAFFOLD STATUS: authored, NOT compiled (no Unity — ADR-0-001/-002).

using Unity.Burst;
using Unity.Entities;

namespace Bulwark.Sim
{
    // CounterCell (IBufferElementData{float Multiplier}) and CounterMatrixTag (the singleton
    // marker) are declared ONCE in Phase1Components.cs and are REUSED here verbatim — not
    // redefined. This file only adds the shared lookup + a validation system over them.

    /// <summary>
    /// The ONE type×armor counter-matrix lookup (§4). Static + Burst-compatible so it can be
    /// called identically from CombatSystem (Combat.cs) and the Phase-2 SpellSystem inside a
    /// Burst job — no duplicated indexing math, no forked §4 chain.
    ///
    /// The matrix is a DynamicBuffer&lt;CounterCell&gt; on the CounterMatrixTag singleton, baked
    /// from CounterMatrix.asset (BalanceConfig). VALUES are data-owned; this is plumbing only.
    /// </summary>
    [BurstCompile]
    public static class CounterMatrix
    {
        /// <summary>Canon shape (§4): 5 damage types × 4 armor classes.</summary>
        public const int DamageTypeCount = 5;
        public const int ArmorClassCount = 4;
        public const int CellCount = DamageTypeCount * ArmorClassCount; // 20

        /// <summary>Neutral multiplier — applied for Unset / out-of-range / unset (0.0) cells.</summary>
        public const float Neutral = 1f;

        /// <summary>
        /// Flat cell index for a (damageType, armorClass) pair, per §4:
        /// idx = (damageType-1)*4 + (armorClass-1). Returns -1 if either enum is Unset (0) —
        /// callers treat -1 as "neutral, no lookup". Pure integer math; no balance values.
        /// </summary>
        public static int Index(Bulwark.Data.DamageType dt, Bulwark.Data.ArmorClass armor)
        {
            int di = (int)dt - 1;
            int ai = (int)armor - 1;
            if (di < 0 || ai < 0) return -1;
            int idx = di * ArmorClassCount + ai;
            if (idx < 0 || idx >= CellCount) return -1;
            return idx;
        }

        /// <summary>
        /// THE shared type×armor multiplier read (§4). Returns the multiplier at
        /// (dt-1)*4 + (armor-1). Any Unset enum, out-of-range index, empty/uncreated buffer, or
        /// a 0.0 ("unset") cell yields the neutral 1.0 — IDENTICAL semantics to
        /// BalanceConfig.GetMultiplier and the existing Combat.cs LookupTypeArmor (which the
        /// integration pass replaces with a call to this). NO §4 chain logic lives here: this
        /// returns ONLY the typeArmor factor; the caller multiplies it into the chain.
        /// </summary>
        public static float Lookup(DynamicBuffer<CounterCell> matrix,
                                   Bulwark.Data.DamageType dt,
                                   Bulwark.Data.ArmorClass armor)
        {
            if (!matrix.IsCreated || matrix.Length == 0) return Neutral;
            int idx = Index(dt, armor);
            if (idx < 0 || idx >= matrix.Length) return Neutral;
            float m = matrix[idx].Multiplier;
            return m != 0f ? m : Neutral;
        }
    }

    /// <summary>
    /// Phase-2.2 one-shot SANITY check on the CounterCell singleton buffer (§13 2.2, §4):
    /// asserts the data-baked matrix has exactly 20 cells (5×4). It authors NOTHING — values
    /// are LSD/data-owned (CounterMatrix.asset). On a wrong-sized or missing buffer it logs an
    /// error (editor/dev builds) and disables itself so it never spams. Runtime behavior is
    /// DEFERRED (ADR-1-001: Phase 2 author-only, GATE 2 deferred; no Unity toolchain here).
    ///
    /// Not [BurstCompile] on OnUpdate: it is a one-time validation that may call managed
    /// UnityEngine.Debug (Burst forbids managed logging). It does no per-frame work — it runs
    /// once, reports, then sets state.Enabled = false (the MatchFlow self-disable pattern).
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    [UpdateAfter(typeof(SimPhaseBegin))]
    public partial struct CounterMatrixValidationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            // Nothing required; we tolerate the singleton not yet existing and re-check on the
            // first OnUpdate so the baker/bootstrap has had a chance to create it.
        }

        public void OnUpdate(ref SystemState state)
        {
            // Run exactly once: validate, report, then self-disable (no per-frame cost, §12).
            state.Enabled = false;

            if (!SystemAPI.TryGetSingletonEntity<CounterMatrixTag>(out var matrixEntity))
            {
                LogMissing();
                return;
            }

            if (!SystemAPI.HasBuffer<CounterCell>(matrixEntity))
            {
                LogMissing();
                return;
            }

            var matrix = SystemAPI.GetBuffer<CounterCell>(matrixEntity);
            if (matrix.Length != CounterMatrix.CellCount)
                LogWrongSize(matrix.Length);
        }

        // ---- Reporting (DEFERRED: editor/dev only; no-op in a Burst/headless build) ----
        // Kept out of OnUpdate's hot path concerns; these are managed and only fire on a defect.

        private static void LogMissing()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.LogError(
                "[BULWARK §4/§13.2.2] CounterMatrix invalid: no CounterMatrixTag singleton with a " +
                "CounterCell buffer found. Combat/Spell type×armor lookups will fall back to neutral " +
                "1.0. Bake CounterMatrix.asset (BalanceConfig, 20 cells) onto the singleton. (DEFERRED " +
                "runtime validation — ADR-1-001.)");
#endif
        }

        private static void LogWrongSize(int actual)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.LogError(
                "[BULWARK §4/§13.2.2] CounterMatrix wrong size: expected " + CounterMatrix.CellCount +
                " cells (5 damage × 4 armor) but found " + actual + ". Out-of-range lookups fall back " +
                "to neutral 1.0; re-bake CounterMatrix.asset (BalanceConfig). (DEFERRED runtime " +
                "validation — ADR-1-001.)");
#endif
        }
    }
}

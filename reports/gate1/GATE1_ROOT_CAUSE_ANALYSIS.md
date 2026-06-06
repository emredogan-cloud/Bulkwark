# BULWARK — GATE 1 · Phase D: Root Cause Analysis

**Date:** 2026-06-06 · Inputs: `GATE1_TELEMETRY.md` (10 matches), `GATE1_PLAYTEST_OBSERVATIONS.md`,
`reports/asset-migration/PROJECT_STATE_ANALYSIS.md` §10 (the documented deferred GATE-1 bugs),
`reports/prephase5/GATE1_VALIDATION_REPORT.md`. **No fixes performed — analysis only.**
Classification: **CRITICAL** = blocks "fun"/the contest · **HIGH** = severe fun loss · **MEDIUM/LOW** = lesser.

---

## RC-1 — AI economy never starts  ·  **CRITICAL**
- **Symptom (evidence):** AI gold flat at **exactly 5** in 390/390 samples; **0 AI miners** (max 0.0/10);
  AI fields **exactly 1 unit (91% of samples)** and **never trains more** — no economy, no army growth.
- **Root cause:** the AI commander has **two competing training sources** (BasicAI utility AI vs SquadAI), which
  conflict so the AI never reliably enqueues a *miner* first — with no miner it earns nothing, so it can never
  afford anything else. (Documented: BasicAI/SquadAI dual `TrainOrder` source, `SquadAI.cs:~560`; PROJECT_STATE §10-1.)
- **Impacted systems:** `BasicAI`, `SquadAI`, `TrainingSystem`, the AI side of the economy loop.
- **Fix complexity:** **HIGH** (arbitrate a single AI training authority + an opening build order).
- **Expected fun impact:** **CRITICAL** — with no AI economy there is no opponent, hence no game.

## RC-2 — Miners die / are not replaced  ·  **CRITICAL**
- **Symptom:** player miners decay **2 → 0** over several matches; AI miners never exist. Economy starves on both
  sides (the player is only propped up by the auto-demo re-queuing miners).
- **Root cause:** (a) **miner targeting death** — `TargetingSystem` selects on `WithAll<UnitTag, Health>` with **no
  `MinerTag` exclusion**, so miners are treated as combatants, walk into harm, and die; (b) **no replacement** —
  no system maintains a live-miner floor against attrition. (PROJECT_STATE §10-2, §10-3.)
- **Impacted systems:** `TargetingSystem`, miner lifecycle / training, economy stability.
- **Fix complexity:** **MEDIUM–HIGH** (exclude `MinerTag` from targeting + a miner-floor maintainer).
- **Expected fun impact:** **HIGH** — even a "fixed" AI would re-collapse economically without this.

## RC-3 — No unit-vs-unit combat occurs  ·  **CRITICAL**
- **Symptom:** `AttackState.Target != Null` count = **0 across all 390 samples** — units never engage each other.
  The two small forces **walk past each other** and each chips the *opposing statue* (a separate damage path, not
  `AttackState`) → statue damage is **bidirectional** (player statue to ~459–472 in 9/10; AI statue to ~154).
- **Root cause:** **downstream of RC-1/RC-2** — with the AI stuck at 1 unit and no army, the two forces are too
  small/sparse to acquire each other and instead beeline to statues; the combat system is never meaningfully
  exercised because no two opposing *armies* ever coexist.
- **Impacted systems:** `TargetingSystem`, `CombatSystem` (starved of inputs), the entire core loop's payoff.
- **Fix complexity:** **LOW once RC-1/RC-2 are fixed** (combat likely works; it has no opponents to act on).
- **Expected fun impact:** **CRITICAL** — combat is the core fantasy and it is entirely absent.

## RC-4 — Matches decided by the test probe, not by play  ·  **HIGH**
- **Symptom:** 10/10 "Victory" via the `SimAiDriver` probe (statue HP → ≈ −99,900 at ~t115), identical timing.
- **Root cause:** `SimAiDriver` is a **debug/validation scaffold** that force-ends the match at a fixed time; it
  masks the fact that no organic win/lose condition is reached (a consequence of RC-1/RC-3).
- **Impacted systems:** `SimAiDriver` (scaffold), match-resolution observability.
- **Fix complexity:** **N/A** for the probe itself (remove once real play resolves); the real work is RC-1/RC-3.
- **Expected fun impact:** **HIGH** — there is no earned victory or possible defeat.

## RC-5 — Deterministic, agency-free walkover  ·  **HIGH**
- **Symptom:** all 10 matches ~identical (116.3 s ± 0.7, same outcome, same shape); no player choice changes the result.
- **Root cause:** **emergent from RC-1–RC-4** — with no opponent, no decision is rewarded or punished.
- **Impacted systems:** the whole match experience.
- **Fix complexity:** **dependent** (resolves when RC-1–RC-3 do).
- **Expected fun impact:** **HIGH** — no tension, no stakes, no replay variety.

## RC-6 — Long dead time / near-empty battlefield  ·  **MEDIUM**
- **Symptom:** ~63 s before anything touches a statue (9/10; ~78 s in m1); 2–3 small units on screen the whole match; no unit-vs-unit fighting at any point.
- **Root cause:** small armies (RC-1/RC-2) + slow first contact; pacing of the opening.
- **Impacted systems:** spawn cadence, economy ramp, map/timing tuning (tuning is **deferred** per the campaign).
- **Fix complexity:** **MEDIUM** (pacing/tuning — only after the structural RC-1–RC-3).
- **Expected fun impact:** **MEDIUM** — boring even once combat exists, unless pacing is addressed.

---

## Causal chain (summary)
`RC-1 (no AI economy)` + `RC-2 (miners die/unreplaced)` → AI has ~0 units → `RC-3 (no combat)` → no organic
decision → `RC-4 (probe-decided)` + `RC-5 (deterministic walkover)`; `RC-6 (dead time)` compounds the boredom.
**Three CRITICALs (RC-1/2/3), two HIGHs (RC-4/5), one MEDIUM (RC-6).** The two true structural roots are **RC-1
and RC-2** (the documented deferred bugs); everything else is downstream.

**Per the campaign: documented only. Nothing fixed. Fixes belong to a later, separately-authorized effort.**

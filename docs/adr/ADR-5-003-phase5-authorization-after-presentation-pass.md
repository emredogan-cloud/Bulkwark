# ADR-5-003 — Phase 5 Authorization After Successful Presentation Pass

- **Status:** **ACCEPTED — Option A (Production Presentation phase)** (owner decision, 2026-06-05). The requested
  presentation/professionalization work proceeds as a **Production Presentation phase — explicitly NOT roadmap
  Phase 5.** GATE 1 (FUN), GATE 5 (scale-or-stop), and the ADR-5-002 LTV floor remain **open/binding and unwaived**;
  roadmap Phase 5 (soft launch/scale) is **NOT begun** and nothing is mislabeled as soft-launch progress. No
  gameplay/economy/AI/balance/commander-budget/monetization/canon change. No fabricated PASS.
- **Date:** 2026-06-05
- **Phase:** 4/Presentation → 5 transition (roadmap Phase 5 = *Soft Launch & Tuning — SCALE-OR-STOP*)
- **Relates to:** ADR-5-001 (conditional Phase-5 authorization), ADR-5-002 (GATE-5 LTV floor — still PROPOSED),
  ADR-4-001, and the conditional-authorization lineage (ADR-0-002/1-001/2-001). Roadmap §13 (phase plan + gates),
  §15 (no fabrication / canon closed), §16 (governance).
- **Decided by (required):** repo owner / Game Director (+ Live-ops/Product for GATE-5 floor). The production
  agent **must not** self-authorize, waive a gate, or set un-owned values.

## Context — what changed
The **Presentation Pass succeeded**: BULWARK now launches through a real game flow (Splash → Main Menu → Mode
Select → Match → Victory/Defeat) with placeholder sprite art instead of debug primitives, device-validated on the
Redmi Note 11R (`reports/asset-migration/ASSET_INTEGRATION_REPORT.md`,
`reports/prephase5/PREPHASE5_FINAL_POLISH_REPORT.md`). This is the "successful Presentation Pass" this ADR is
named for. **Phases 0–4 are COMPLETE; GATE 4 (fairness) = PASS (static).**

## The problem this ADR must resolve (do NOT waive)
The Phase-5 authorization's stated **objective** is presentation/professionalization — *character differentiation,
battle HUD, audio framework, VFX framework, animation framework, presentation polish* — i.e. **new presentation
features.** Per the entry audit (`PHASE5_ENTRY_AUDIT.md` §12), this **contradicts** the roadmap on three binding
points:

1. **Roadmap Phase 5 ≠ the stated objective.** Roadmap Phase 5 is **"Soft Launch & Tuning (SCALE-OR-STOP)" —
   limited-geo release + live telemetry + RC tuning + perf/stability hardening, "tuning only — NO new features,"**
   validated by **GATE 5** (D1≥~40%, D7≥~18%, blended-D30 LTV ≥ target CPI). Building presentation **features** is
   the opposite of "tuning only, no new features."
2. **Roadmap Phase 5 is un-executable / un-fabricatable here.** It requires a real soft-launch population + live
   D1/D7/D30/LTV. There is none. The earlier session already concluded this and delivered a **readiness scaffold**
   only (`PHASE5_CONFLICT_REPORT.md`, `reports/phase-5/FINAL_REPORT.md`). **ADR-5-002's GATE-5 LTV floor is still
   PROPOSED with no values**, so GATE 5 is un-evaluable by definition.
3. **GATE 1 (FUN) = FAIL** (re-gated in V2, `GATE1_VALIDATION_REPORT.md`). The roadmap's binding philosophy is
   "**combat must be fun before meta is built**" and GATE 1 is "**binding: if not → kill/pivot**." Advancing into
   the meta/scale phase while the fun gate is failed contradicts the roadmap.

## Decision (proposed — requires owner choice)
**Do NOT begin roadmap Phase 5 (soft launch / scale) and do NOT relabel presentation work as roadmap Phase 5.**
The requested presentation/professionalization work is legitimate and **fairness-neutral** (it touches no
economy/AI/balance/commander-budget/monetization/canon), but it is a **"Production Presentation" scope — distinct
from roadmap Phase 5.** This ADR therefore **surfaces the contradiction and requests an owner decision** among:

- **Option A — Re-scope (recommended, executable now):** authorize a **Production Presentation phase** (the stated
  objective: character differentiation → battle HUD → audio → VFX → animation → polish), explicitly **NOT** roadmap
  Phase 5. GATE 1 (FUN) and GATE 5 remain open/binding; nothing is mislabeled as soft-launch/scale progress.
- **Option B — Address GATE 1 first:** fix the FUN re-gate (the deferred GATE-1 AI/miner issues) before any further
  roadmap-phase or meta work — honoring "combat must be fun before meta is built."
- **Option C — Acknowledge roadmap Phase 5 is telemetry-blocked:** treat roadmap Phase 5 as **readiness-only**
  (already delivered) and stop, pending a real soft launch + an Accepted ADR-5-002 floor.

(These are not mutually exclusive: A then B is viable — do presentation work now, fix GATE 1 next, leave roadmap
Phase 5 for a real soft launch.)

## Why Phase 5 (roadmap) can / cannot "now begin"
- **Can (entry):** Phase-5 *entry condition* (GATE 4 PASS) is met.
- **Cannot (execution):** the *deliverables + exit gate* (live telemetry, RC-from-data, GATE-5 LTV/retention)
  cannot be produced or fabricated here, and **GATE 1 (FUN) = FAIL** is unwaived. So roadmap Phase 5 **cannot
  legitimately execute**; only a readiness scaffold (done) or a re-scoped presentation phase (Option A) is real.

## Inherited validation debt (PRESERVED — not closed, not fabricated)
GATE 1 (FUN) = **FAIL** · GATE 2 (playtest) = DEFERRED · GATE 3 (server economy) = DEFERRED · GATE 4 (fairness) =
PASS (static) · GATE 5 (scale-or-stop) = DEFERRED/un-evaluable (no telemetry; LTV floor unset).

## Unresolved GATE-1 issues (remain deferred)
BasicAI vs SquadAI economy conflict; miner targeting death; miner replacement (no live miner floor). Documented
in `PHASE5_ENTRY_AUDIT.md` §6 / `PROJECT_STATE_ANALYSIS.md` §10. **Not fixed by this ADR.**

## Constraints that remain BINDING (none waived)
- **No fabricated runtime/telemetry/GATE evidence** (§15.8). No self-certified or invented PASS.
- **Roadmap Phase 5 = tuning only, no new features**; presentation features are NOT roadmap Phase 5.
- **GATE 1 (FUN), GATE 5 (scale-or-stop), ADR-5-002 LTV floor** remain open/binding; no gate silently closed.
- **Preserve:** economy balance, AI balance, commander power budgets, monetization/fairness rules, roadmap canon,
  decision log. **No `/future/*` import** unless the roadmap explicitly authorizes it.
- **Established workflow** (author → independent review → adversarial audit → repair → CI/CD → Android validation)
  applies to any authorized work; no shortcuts.

## Consequences
- **Accepted as A:** I proceed with the Production Presentation work (correctly labeled), under all binding
  constraints, stopping at its own checkpoints — roadmap Phase 5 / GATE 5 untouched.
- **Accepted as B:** I scope a FUN re-gate effort on the deferred GATE-1 issues (a gameplay change — needs its own
  authorization since the standing rule is "deferred GATE-1 issues remain deferred unless the roadmap requires").
- **Accepted as C / no decision:** work stops at this entry gate; roadmap Phase 5 stays readiness-only.

**Until the owner decides, work is STOPPED at the Phase-5 entry gate (per "stop at the first required gate and
report honestly"). No constraint is waived; no PASS is fabricated; no presentation work is mislabeled as roadmap
Phase 5.**
